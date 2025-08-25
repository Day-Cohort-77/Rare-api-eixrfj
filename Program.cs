using Microsoft.AspNetCore.Cors;
using RareAPI.Endpoints;
using RareAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add services to the DI container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddSingleton<PostService>();

var app = builder.Build();

// Use CORS
app.UseCors();

// Test database connection and print result
try
{
    using var testScope = app.Services.CreateScope();
    var dbService = testScope.ServiceProvider.GetRequiredService<DatabaseService>();
    using var conn = new Npgsql.NpgsqlConnection(dbService.GetType().GetField("_connectionString", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(dbService) as string);
    conn.Open();
    Console.WriteLine($"Successfully connected to database: {conn.Database}");
    conn.Close();
}
catch (Exception ex)
{
    Console.WriteLine($"Database connection failed: {ex.Message}");
    throw;
}

using (var scope = app.Services.CreateScope())
{
    var dbService = scope.ServiceProvider.GetRequiredService<DatabaseService>();
    await dbService.InitializeDatabaseAsync();
}

app.MapGet("/", () => "Welcome to Rare Publishing Platform API!");

app.MapAuthEndpoints();
app.MapPostEndpoints();
app.MapCategoryEndpoints();
app.Run();
