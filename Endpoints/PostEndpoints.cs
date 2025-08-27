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

            // POST /posts
            endpoints.MapPost("/posts", async (Post postRequest, PostService postService) =>
            {
                try
                {
                    // Validate and create post
                    var createdPost = await postService.CreatePostAsync(postRequest);
                    if (createdPost == null)
                    {
                        return Results.Problem("Failed to create post.");
                    }
                    return Results.Created($"/posts/{createdPost.Id}", createdPost);
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(new { message = ex.Message });
                }
                catch (Exception ex)
                {
                    return Results.Problem($"An error occurred: {ex.Message}");
                }
            });

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
        }
    }
}