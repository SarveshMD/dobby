using System.Data;
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

app.MapGet("/", () => "What a beautiful place... to be with friends");

app.MapPost("/api/urls", async (
    UrlDbContext db,
    CreateUrlItemDto dto,
    CancellationToken ct,
    CreateUrlItemDtoValidator validator,
    IUrlShortenerService urlShortener
) =>
{
    var validationResult = validator.Validate(dto);

    if (!validationResult.IsValid)
    {
        return Results.ValidationProblem(validationResult.ToDictionary());
    }
    await using var cmd = db.Database.GetDbConnection().CreateCommand();
    cmd.CommandText = "SELECT nextval('url_items_id_seq')::integer";

    if (cmd.Connection!.State != ConnectionState.Open)
    {
        await cmd.Connection.OpenAsync();
    }

    var id = Convert.ToInt32(await cmd.ExecuteScalarAsync());

    string encodedUrl = urlShortener.Encode(id);

    var urlItem = new UrlItem(
        dto.OriginalUrl,
        encodedUrl
    );

    db.UrlItems.Add(urlItem);
    await db.SaveChangesAsync();

    var res = ResponseUrlItemDto.FromEntity(urlItem);

    return Results.Created($"/{encodedUrl}", res);
});

app.Run();
