using RareAPI.Models;
using RareAPI.Services;

namespace RareAPI.Endpoints
{
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
        {
            // POST /register
            endpoints.MapPost("/register", async (User newUser, DatabaseService databaseService) =>
            {
                try
                {
                    // Check if user already exists
                    if (await databaseService.UserExistsAsync(newUser.Email))
                    {
                        return Results.BadRequest(new { message = "User with this email already exists" });
                    }

                    // Create new user
                    var createdUser = await databaseService.CreateUserAsync(newUser);
                    if (createdUser != null)
                    {
                        return Results.Created($"/users/{createdUser.Id}", createdUser);
                    }

                    return Results.BadRequest(new { message = "Failed to create user" });
                }
                catch (Exception ex)
                {
                    return Results.Problem($"An error occurred: {ex.Message}");
                }
            });

            // POST /login
            endpoints.MapPost("/login", async (LoginRequest loginRequest, DatabaseService databaseService) =>
            {
                try
                {
                    var (userId, storedPassword) = await databaseService.GetUserCredentialsAsync(loginRequest.Email);

                    if (userId.HasValue && storedPassword != null)
                    {
                        // Simple password comparison (in production, use proper password hashing)
                        if (storedPassword == loginRequest.Password)
                        {
                            return Results.Ok(new { valid = true, token = userId.Value });
                        }
                    }

                    return Results.Ok(new { valid = false, token = (int?)null });
                }
                catch (Exception ex)
                {
                    return Results.Problem($"An error occurred: {ex.Message}");
                }
            });

            // GET /users/{id}
            endpoints.MapGet("/users/{id:int}", async (int id, DatabaseService databaseService) =>
            {
                try
                {
                    var user = await databaseService.GetUserByIdAsync(id);
                    if (user != null)
                    {
                        return Results.Ok(user);
                    }
                    return Results.NotFound(new { message = "User not found" });
                }
                catch (Exception ex)
                {
                    return Results.Problem($"An error occurred: {ex.Message}");
                }
            });
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}