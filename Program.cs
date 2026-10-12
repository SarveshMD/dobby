using System.Data;
using Dobby.Database;
using Dobby.DTOs;
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
    CreateUrlItemDtoValidator validator
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

    return Results.Ok(id);
});

app.Run();
