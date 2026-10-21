using Dobby.Database;
using Dobby.DTOs;
using Dobby.Models;
using Dobby.Services;
using Dobby.Validators;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();


if (!builder.Environment.IsEnvironment("Testing"))
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<UrlDbContext>(optionsBuilder =>
    {
        optionsBuilder
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention();

        if (builder.Environment.IsDevelopment())
        {
            optionsBuilder
                .EnableSensitiveDataLogging()
                .LogTo(Console.WriteLine, LogLevel.Information);
        }
    });
}

var secretKeyString = builder.Configuration["UrlShortener:SecretKey"]
    ?? throw new InvalidOperationException("Skip32 secret key is missing from configuration");

builder.Services.AddSingleton<IUrlShortenerService, UrlShortener>(_ => new UrlShortener(secretKeyString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapGet("/", () => "What a beautiful place... to be with friends")
.Produces(StatusCodes.Status200OK);

app.MapPost("/api/urls", async (
    UrlDbContext db,
    CreateUrlItemDto dto,
    CancellationToken ct,
    IValidator<CreateUrlItemDto> validator,
    IUrlShortenerService urlShortener
) =>
{
    var validationResult = await validator.ValidateAsync(dto, ct);

    if (!validationResult.IsValid)
    {
        return Results.ValidationProblem(validationResult.ToDictionary());
    }

    var urlItem = new UrlItem(
        dto.OriginalUrl
    );

    db.UrlItems.Add(urlItem);
    await db.SaveChangesAsync(ct);

    string encodedUrl = urlShortener.Encode(urlItem.Id);

    var res = ResponseUrlItemDto.FromEntityAndShortUrl(urlItem, encodedUrl);

    return Results.Created($"/{encodedUrl}", res);
})
.ProducesValidationProblem()
.Produces<ResponseUrlItemDto>(StatusCodes.Status201Created);

app.MapGet("/{shortUrl}", async (
    string shortUrl,
    UrlDbContext db,
    IValidator<GetShortUrlRequest> validator,
    IUrlShortenerService urlShortener,
    CancellationToken ct
) =>
{
    Console.WriteLine($"ShortURL from URL: {shortUrl}");

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
})
.ProducesValidationProblem()
.Produces(StatusCodes.Status404NotFound)
.Produces(StatusCodes.Status307TemporaryRedirect);

app.MapPost("/api/auth/register", async (
    RegisterRequestDto dto,
    ITokenService tokenService,
    UrlDbContext db,
    IValidator<RegisterRequestDto> validator,
    CancellationToken ct
) =>
{
    var validationResult = await validator.ValidateAsync(dto, ct);

    if (!validationResult.IsValid)
    {
        return Results.ValidationProblem(validationResult.ToDictionary());
    }

    var usernameExists = await db.Users
        .AnyAsync(user => user.Username == dto.Username, ct);

    if (usernameExists)
    {
        return Results.Conflict("Username is taken");
    }

    string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

    var user = new User(
        dto.Username,
        passwordHash
    );

    db.Users.Add(user);
    await db.SaveChangesAsync(ct);

    string token = tokenService.GenerateToken(user);

    return Results.Created($"/users/{user.Id}", new AuthResponseDto
    (
        Message: "Registration Successful",
        Id: user.Id,
        Username: user.Username,
        Token: token
    ));
})
.ProducesValidationProblem()
.Produces(StatusCodes.Status409Conflict)
.Produces<AuthResponseDto>(StatusCodes.Status201Created);

app.MapPost("/api/auth/login", async (
    LoginRequestDto dto,
    IValidator<LoginRequestDto> validator,
    ITokenService tokenService,
    UrlDbContext db,
    CancellationToken ct
) =>
{
    var validationResult = await validator.ValidateAsync(dto, ct);

    if (!validationResult.IsValid)
    {
        return Results.ValidationProblem(validationResult.ToDictionary());
    }

    var user = await db.Users
        .FirstOrDefaultAsync(user => user.Username == dto.Username, ct);

    if (user is null)
    {
        return Results.Unauthorized();
    }

    bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

    if (!isPasswordValid)
    {
        return Results.Unauthorized();
    }

    string token = tokenService.GenerateToken(user);

    return Results.Ok(new AuthResponseDto
    (
        Message: "Login Successful",
        Id: user.Id,
        Username: user.Username,
        Token: token
    ));
})
.ProducesValidationProblem()
.Produces(StatusCodes.Status401Unauthorized)
.Produces<AuthResponseDto>(StatusCodes.Status200OK);

app.Run();

public partial class Program { }
