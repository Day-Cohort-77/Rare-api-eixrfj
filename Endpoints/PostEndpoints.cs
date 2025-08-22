using RareAPI.Models;
using RareAPI.Services;

namespace RareAPI.Endpoints
{
    public static class PostEndpoints
    {
        public static void MapPostEndpoints(this IEndpointRouteBuilder endpoints)
        {
            // GET /posts
            endpoints.MapGet("/posts", async (DatabaseService databaseService) =>
            {
                try
                {
                    var posts = await databaseService.GetAllPostsAsync();
                    return Results.Ok(posts);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"An error occurred: {ex.Message}");
                }
            });

            // GET /posts/{id}
            // endpoints.MapGet("/posts/{id:int}", async (int id, DatabaseService databaseService) =>
            // {

            // });

            // POST /posts
            // endpoints.MapPost("/posts", async (Post postRequest, DatabaseService databaseService) =>
            // {
            //     try
            //     {
            //         var newPost = new Post
            //         {
            //             Title = postRequest.Title,
            //             Content = postRequest.Content,
            //             UserId = postRequest.UserId,
            //             IsPublished = postRequest.IsPublished
            //         };

            //     }
            //     catch (Exception ex)
            //     {
            //         return Results.Problem($"An error occurred: {ex.Message}");
            //     }
            // });

            // DELETE /posts/{id}
            endpoints.MapDelete("/posts/{id:int}", async (int id, DatabaseService databaseService) =>
            {
                try
                {
                    var deleted = await databaseService.DeletePostAsync(id);
                    if (deleted)
                    {
                        return Results.NoContent();
                    }

                    return Results.NotFound(new { message = "Post not found" });
                }
                catch (Exception ex)
                {
                    return Results.Problem($"An error occurred: {ex.Message}");
                }
            });

            // GET /users/{userId}/posts
            // endpoints.MapGet("/users/{userId:int}/posts", async (int userId, DatabaseService databaseService) =>
            // {

            // });
        }
    }
}