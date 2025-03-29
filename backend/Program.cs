using LaLigaTrackerBackend.Models;
using LaLigaTrackerBackend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    EnvironmentName = Environments.Development
});


builder.WebHost.ConfigureKestrel(serverOptions => 
{
    serverOptions.ListenLocalhost(8080);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "LaLiga Tracker API",
        Version = "v1",
        Description = "API para gestión de partidos de fútbol",
        Contact = new OpenApiContact
        {
            Name = "Tu Nombre",
            Email = "tu@email.com"
        }
    });
});

// Configuración CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=matches.db"));

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => 
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "LaLiga API v1");
    });
}

app.UseCors("AllowAll");

// Inicialización de la base de datos
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    
    // Datos iniciales de ejemplo
    if (!db.Matches.Any())
    {
        db.Matches.Add(new Match {
            HomeTeam = "Real Madrid",
            AwayTeam = "Barcelona",
            MatchDate = DateTime.Now.AddDays(7).ToString("yyyy-MM-dd")
        });
        await db.SaveChangesAsync();
    }
}

// Endpoints
var matches = app.MapGroup("/api/matches");

// GET todos los partidos
matches.MapGet("/", async (AppDbContext db) => 
    await db.Matches.ToListAsync())
    .WithTags("Partidos")
    .WithName("GetAllMatches")
    .WithOpenApi();

// GET partido por ID
matches.MapGet("/{id}", async (int id, AppDbContext db) =>
{
    var match = await db.Matches.FindAsync(id);
    return match is null ? Results.NotFound() : Results.Ok(match);
})
.WithTags("Partidos")
.WithName("GetMatchById")
.WithOpenApi();

// POST nuevo partido
matches.MapPost("/", async (Match newMatch, AppDbContext db) =>
{
    db.Matches.Add(newMatch);
    await db.SaveChangesAsync();
    return Results.Created($"/api/matches/{newMatch.Id}", newMatch);
})
.WithTags("Partidos")
.WithName("CreateMatch")
.WithOpenApi();

// PUT actualizar partido
matches.MapPut("/{id}", async (int id, Match updatedMatch, AppDbContext db) =>
{
    var existingMatch = await db.Matches.FindAsync(id);
    if (existingMatch is null) return Results.NotFound();
    
    existingMatch.HomeTeam = updatedMatch.HomeTeam;
    existingMatch.AwayTeam = updatedMatch.AwayTeam;
    existingMatch.MatchDate = updatedMatch.MatchDate;
    
    await db.SaveChangesAsync();
    return Results.Ok(existingMatch);
})
.WithTags("Partidos")
.WithName("UpdateMatch")
.WithOpenApi();

// DELETE partido
matches.MapDelete("/{id}", async (int id, AppDbContext db) =>
{
    var match = await db.Matches.FindAsync(id);
    if (match is null) return Results.NotFound();
    
    db.Matches.Remove(match);
    await db.SaveChangesAsync();
    return Results.NoContent();
})
.WithTags("Partidos")
.WithName("DeleteMatch")
.WithOpenApi();

// PATCH operations
matches.MapPatch("/{id}/goals", async (int id, AppDbContext db) =>
{
    var match = await db.Matches.FindAsync(id);
    if (match is null) return Results.NotFound();
    
    match.Goals++;
    await db.SaveChangesAsync();
    
    return Results.Ok(new { 
        Message = "Gol registrado correctamente",
        MatchId = match.Id,
        CurrentGoals = match.Goals
    });
})
.WithTags("Eventos")
.WithName("RegisterGoal")
.WithOpenApi();

matches.MapPatch("/{id}/yellowcards", async (int id, AppDbContext db) =>
{
    var match = await db.Matches.FindAsync(id);
    if (match is null) return Results.NotFound();
    
    match.YellowCards++;
    await db.SaveChangesAsync();
    
    return Results.Ok(new { 
        Message = "Tarjeta amarilla registrada",
        MatchId = match.Id,
        CurrentYellowCards = match.YellowCards
    });
})
.WithTags("Eventos")
.WithName("RegisterYellowCard")
.WithOpenApi();

matches.MapPatch("/{id}/redcards", async (int id, AppDbContext db) =>
{
    var match = await db.Matches.FindAsync(id);
    if (match is null) return Results.NotFound();
    
    match.RedCards++;
    await db.SaveChangesAsync();
    
    return Results.Ok(new { 
        Message = "Tarjeta roja registrada",
        MatchId = match.Id,
        CurrentRedCards = match.RedCards
    });
})
.WithTags("Eventos")
.WithName("RegisterRedCard")
.WithOpenApi();

matches.MapPatch("/{id}/extratime", async (int id, int minutes, AppDbContext db) =>
{
    var match = await db.Matches.FindAsync(id);
    if (match is null) return Results.NotFound();
    
    match.ExtraTimeMinutes = minutes;
    await db.SaveChangesAsync();
    
    return Results.Ok(new { 
        Message = $"Tiempo extra actualizado a {minutes} minutos",
        MatchId = match.Id,
        ExtraTimeMinutes = match.ExtraTimeMinutes
    });
})
.WithTags("Eventos")
.WithName("SetExtraTime")
.WithOpenApi();

// Middleware de manejo de errores
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex}");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("Ocurrió un error interno. Por favor intente nuevamente.");
    }
});

app.Run();