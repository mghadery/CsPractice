using flashcard.application.ServiceContracts;
using flashcard.domain.DTOs;
using flashcard.infrastructure.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata.Ecma335;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

InfraSetup.AddServices(builder.Services, builder.Configuration);

var app = builder.Build();

await InfraSetup.SeedData(app.Services);

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
    .AllowAnonymous();

app.MapPost("/Login", async ([FromBody] UserLoginDTO loginDTO, IAuthService authService) => await authService.Login(loginDTO))
    .AllowAnonymous();

app.MapGet("/home", (HttpContext context) =>
{
    var user = context.User;
    if (user?.Identity?.IsAuthenticated == true)
        return user.Identity.Name;
    else
        return "Not authenticated!";
})
    .RequireAuthorization("onlyadmin");

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.55);
}
