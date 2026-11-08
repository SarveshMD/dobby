using Dobby.Data;
using Dobby.DTOs;
using Dobby.Services;
using Dobby.Models;
using Dobby.Extensions;

using StackExchange.Redis;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Dobby.Endpoints;

public static class UrlEndpoints
{
    public static IEndpointRouteBuilder MapUrlEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/urls")
            .WithTags("Urls");

        group.MapPost("", PostUrlAsync)
            .ProducesValidationProblem()
            .Produces<ResponseUrlItemDto>(StatusCodes.Status201Created);

        group.MapGet("/my", GetMyUrlsAsync)
            .RequireAuthorization()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<IEnumerable<ResponseUrlItemDto>>(StatusCodes.Status200OK);

        group.MapGet("/{urlIdString}", GetUrlAsync)
            .RequireAuthorization()
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status200OK);

        group.MapDelete("/{urlIdString}", DeleteUrlAsync)
            .RequireAuthorization()
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status204NoContent);

        routes.MapGet("/{shortUrl}", RedirectAsync)
            .WithTags("Redirect")
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status302Found);

        return routes;
    }

    private static async Task<IResult> PostUrlAsync(
        UrlDbContext db,
        CreateUrlItemDto dto,
        CancellationToken ct,
        IValidator<CreateUrlItemDto> validator,
        IUrlShortenerService urlShortener,
        ClaimsPrincipal user
    )
    {
        var validationResult = await validator.ValidateAsync(dto, ct);

        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        Guid? userId = null;

        if (user.Identity?.IsAuthenticated == true)
        {
            userId = user.GetUserId();
        }

        var urlItem = new UrlItem(
            dto.OriginalUrl,
            userId
        );

        db.UrlItems.Add(urlItem);
        await db.SaveChangesAsync(ct);

        string encodedUrl = urlShortener.Encode(urlItem.Id);

        var res = ResponseUrlItemDto.FromEntityAndShortUrl(urlItem, encodedUrl);

        return Results.Created($"/{encodedUrl}", res);
    }

    private static async Task<IResult> GetMyUrlsAsync(
        UrlDbContext db,
        ClaimsPrincipal principal,
        IUrlShortenerService urlShortenerService,
        CancellationToken ct
    )
    {
        var userId = principal.GetUserId();
        var user = await db.Users
            .AnyAsync(user => user.Id == userId, ct);

        if (!user)
        {
            return Results.Unauthorized();
        }

        var urlItems = await db.UrlItems
            .Where(x => x.UserId == userId)
            .ToListAsync(ct);

        var res = urlItems
            .OrderBy(x => x.CreatedAt)
            .Select(x =>
                ResponseUrlItemDto.FromEntityAndShortUrl(
                    x,
                    urlShortenerService.Encode(x.Id)
                )
            );

        return Results.Ok(res);
    }

    private static async Task<IResult> GetUrlAsync(
        string urlIdString,
        IValidator<UrlIdDto> validator,
        UrlDbContext db,
        ClaimsPrincipal principal,
        IUrlShortenerService urlShortenerService,
        CancellationToken ct
    )
    {
        var userId = principal.GetUserId();
        var user = await db.Users
            .AnyAsync(u => u.Id == userId, ct);

        if (!user)
        {
            return Results.Unauthorized();
        }

        var validationResult = await validator.ValidateAsync(new UrlIdDto(urlIdString), ct);

        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        int urlId = int.Parse(urlIdString);

        var urlItem = await db.UrlItems
            .Where(x => x.UserId == userId)
            .FirstOrDefaultAsync(x => x.Id == urlId, ct);

        if (urlItem is null)
        {
            return Results.NotFound(new
            {
                message = "UrlItem with the given UrlId was not found",
                urlId
            });
        }

        string shortUrl = urlShortenerService.Encode(urlId);

        var res = ResponseUrlItemDto.FromEntityAndShortUrl(urlItem, shortUrl);

        return Results.Ok(res);
    }

    private static async Task<IResult> DeleteUrlAsync(
        string urlIdString,
        ClaimsPrincipal principal,
        UrlDbContext db,
        IValidator<UrlIdDto> validator,
        IUrlShortenerService urlShortener,
        IDatabase redisDb,
        ILogger<Program> logger,
        CancellationToken ct
    )
    {
        var userId = principal.GetUserId();
        var user = await db.Users
            .AnyAsync(x => x.Id == userId, ct);

        if (!user)
        {
            return Results.Unauthorized();
        }

        var validationResult = await validator.ValidateAsync(new UrlIdDto(urlIdString), ct);

        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        int urlId = int.Parse(urlIdString);

        var urlItem = await db.UrlItems
            .Where(x => x.UserId == userId)
            .FirstOrDefaultAsync(x => x.Id == urlId, ct);

        if (urlItem is null)
        {
            return Results.NotFound(new
            {
                message = "UrlItem with the given UrlId was not found",
                urlId
            });
        }

        db.UrlItems.Remove(urlItem);
        await db.SaveChangesAsync(ct);

        string shortUrl = urlShortener.Encode(urlId);

        try
        {
            await redisDb.KeyDeleteAsync($"url:{shortUrl}");
        }
        catch (RedisException ex)
        {
            logger.LogWarning(ex, "Redis cache DELETE operation failed for shortUrl: {ShortUrl}.", shortUrl);
        }


        return Results.NoContent();
    }

    private static async Task<IResult> RedirectAsync(
        string shortUrl,
        UrlDbContext db,
        IValidator<GetShortUrlRequest> validator,
        IUrlShortenerService urlShortener,
        IDatabase redisDb,
        ILogger<Program> logger,
        CancellationToken ct
    )
    {
        var validationResult = await validator.ValidateAsync(new GetShortUrlRequest(shortUrl), ct);

        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        string cacheKey = $"url:{shortUrl}";

        var redisRes = RedisValue.Null;

        try
        {
            redisRes = await redisDb.StringGetAsync(cacheKey);
        }
        catch (RedisException ex)
        {
            logger.LogWarning(ex, "Redis cache GET operation failed for shortUrl: {ShortUrl}. Falling back to DB.", shortUrl);
        }

        if (!redisRes.IsNullOrEmpty)
        {
            return Results.Redirect(redisRes.ToString());
        }

        int key = urlShortener.Decode(shortUrl);

        var urlItem = await db.UrlItems
            .FirstOrDefaultAsync(item => item.Id == key, ct);

        if (urlItem is null)
        {
            return Results.NotFound(new
            {
                message = "Short URL was not found",
                shortUrl
            });
        }

        try
        {
            await redisDb.StringSetAsync(cacheKey, urlItem.OriginalUrl, TimeSpan.FromHours(24));
        }
        catch (RedisException ex)
        {
            logger.LogWarning(ex, "Redis cache SET operation failed for shortUrl: {ShortUrl}", shortUrl);
        }

        return Results.Redirect(urlItem.OriginalUrl);
    }
}
