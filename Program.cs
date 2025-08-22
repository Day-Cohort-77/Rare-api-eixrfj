using RareAPI.Endpoints;
using RareAPI.Services;

var builder = WebApplication.CreateBuilder(args);
// Add CORS policy to allow frontend on localhost:3000
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins("http://localhost:3000")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

// Add services to the DI container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddSingleton<PostService>();

var app = builder.Build();

// Use CORS policy
app.UseCors("AllowFrontend");

using (var scope = app.Services.CreateScope())
{
    var dbService = scope.ServiceProvider.GetRequiredService<DatabaseService>();
    await dbService.InitializeDatabaseAsync();
}

app.MapGet("/", () => "Welcome to Rare Publishing Platform API!");

app.MapAuthEndpoints();
app.MapPostEndpoints();

app.Run();
