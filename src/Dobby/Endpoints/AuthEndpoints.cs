using Dobby.DTOs;
using Dobby.Services;
using Dobby.Data;
using Dobby.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Dobby.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/auth")
            .WithTags("Auth");

        group.MapPost("/register", RegisterAsync)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status409Conflict)
            .Produces<AuthResponseDto>(StatusCodes.Status201Created);

        group.MapPost("/login", LoginAsync)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<AuthResponseDto>(StatusCodes.Status200OK);

        return routes;
    }

    private static async Task<IResult> RegisterAsync(
        RegisterRequestDto dto,
        ITokenService tokenService,
        UrlDbContext db,
        IValidator<RegisterRequestDto> validator,
        CancellationToken ct
    )
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
    }

    private static async Task<IResult> LoginAsync(
            LoginRequestDto dto,
            IValidator<LoginRequestDto> validator,
            ITokenService tokenService,
            UrlDbContext db,
            CancellationToken ct
        )
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
    }
}
