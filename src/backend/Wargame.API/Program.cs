using Wargame.Application.Interfaces;
using Wargame.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1. Ajouter le support des contrôleurs
builder.Services.AddControllers();

// 2. Configurer l'injection de dépendances pour le Repository
// On calcule le chemin relatif pour pointer vers data/units.json (qui est à la racine du monorepo)
var dataPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "data", "units.json"));
builder.Services.AddScoped<IUnitRepository>(provider => new JsonUnitRepository(dataPath));

// 3. Configurer les règles CORS pour autoriser l'application Vite / React
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173") 
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// 4. Activer les middlewares dans le bon ordre
app.UseCors("AllowReactApp");
app.MapControllers();

app.Run();