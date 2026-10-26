using Dobby.Data;
using Dobby.DTOs;
using Dobby.Services;
using Dobby.Models;
using Dobby.Extensions;

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

        group.MapDelete("/{urlIdString}", DeleteUrlAsync)
            .RequireAuthorization()
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

    private static async Task<IResult> DeleteUrlAsync(
        string urlIdString,
        ClaimsPrincipal principal,
        UrlDbContext db,
        IValidator<UrlIdDto> validator,
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

        return Results.NoContent();
    }

    private static async Task<IResult> RedirectAsync(
        string shortUrl,
        UrlDbContext db,
        IValidator<GetShortUrlRequest> validator,
        IUrlShortenerService urlShortener,
        CancellationToken ct
    )
    {
        var validationResult = await validator.ValidateAsync(new GetShortUrlRequest(shortUrl), ct);

        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        int key = urlShortener.Decode(shortUrl);

        var urlItem = await db.UrlItems
            .FindAsync(key, ct);

        if (urlItem is null)
        {
            return Results.NotFound(new
            {
                message = "Short URL was not found",
                shortUrl
            });
        }

        return Results.Redirect(urlItem.OriginalUrl);
    }
}
