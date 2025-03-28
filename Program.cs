using LaLigaTrackerBackend.Models;
using LaLigaTrackerBackend.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    EnvironmentName = Environments.Development
});

// Explicit port configuration
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenLocalhost(8080);
});

// Services configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// SQLite configuration
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=matches.db"));

var app = builder.Build();

// Middleware pipeline
app.UseCors("AllowAll");

// Database initialization
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// API Endpoints
var matches = app.MapGroup("/api/matches");

// GET all matches
matches.MapGet("/", async (AppDbContext db) => 
    await db.Matches.ToListAsync());

// GET match by ID
matches.MapGet("/{id}", async (int id, AppDbContext db) =>
{
    var match = await db.Matches.FindAsync(id);
    return match is null ? Results.NotFound() : Results.Ok(match);
});

// POST new match
matches.MapPost("/", async (Match newMatch, AppDbContext db) =>
{
    db.Matches.Add(newMatch);
    await db.SaveChangesAsync();
    return Results.Created($"/api/matches/{newMatch.Id}", newMatch);
});

// PUT update match
matches.MapPut("/{id}", async (int id, Match updatedMatch, AppDbContext db) =>
{
    var existingMatch = await db.Matches.FindAsync(id);
    if (existingMatch is null) return Results.NotFound();
    
    existingMatch.HomeTeam = updatedMatch.HomeTeam;
    existingMatch.AwayTeam = updatedMatch.AwayTeam;
    existingMatch.MatchDate = updatedMatch.MatchDate;
    
    await db.SaveChangesAsync();
    return Results.Ok(existingMatch);
});

// DELETE match
matches.MapDelete("/{id}", async (int id, AppDbContext db) =>
{
    var match = await db.Matches.FindAsync(id);
    if (match is null) return Results.NotFound();
    
    db.Matches.Remove(match);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// PATCH operations
matches.MapPatch("/{id}/goals", async (int id, AppDbContext db) =>
{
    var match = await db.Matches.FindAsync(id);
    if (match is null) return Results.NotFound();
    
    match.Goals++;
    await db.SaveChangesAsync();
    
    return Results.Ok(new { 
        Message = "Gol registrado correctamente",
        Goals = match.Goals 
    });
});

matches.MapPatch("/{id}/yellowcards", async (int id, AppDbContext db) =>
{
    var match = await db.Matches.FindAsync(id);
    if (match is null) return Results.NotFound();
    
    match.YellowCards++;
    await db.SaveChangesAsync();
    
    return Results.Ok(new { 
        Message = "Tarjeta amarilla registrada",
        YellowCards = match.YellowCards 
    });
});

matches.MapPatch("/{id}/redcards", async (int id, AppDbContext db) =>
{
    var match = await db.Matches.FindAsync(id);
    if (match is null) return Results.NotFound();
    
    match.RedCards++;
    await db.SaveChangesAsync();
    
    return Results.Ok(new { 
        Message = "Tarjeta roja registrada",
        RedCards = match.RedCards 
    });
});

matches.MapPatch("/{id}/extratime", async (int id, int minutes, AppDbContext db) =>
{
    var match = await db.Matches.FindAsync(id);
    if (match is null) return Results.NotFound();
    
    match.ExtraTimeMinutes = minutes;
    await db.SaveChangesAsync();
    
    return Results.Ok(new { 
        Message = $"Tiempo extra actualizado a {minutes} minutos",
        ExtraTime = match.ExtraTimeMinutes 
    });
});

// Error handling middleware
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex}");
        throw;
    }
});

app.Run();