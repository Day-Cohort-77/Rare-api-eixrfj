using RareAPI.Models;
using RareAPI.Services;

namespace RareAPI.Endpoints
{
    public static class PostEndpoints
    {
        public static void MapPostEndpoints(this IEndpointRouteBuilder endpoints)
        {

            // GET /posts with optional expand=user_id
            endpoints.MapGet("/posts", async (DatabaseService databaseService, HttpRequest request) =>
            {
                try
                {
                    var expand = request.Query["expand"].ToString();
                    var posts = await databaseService.GetAllPostsAsync();

                    if (expand == "user")
                    {
                        var users = await databaseService.GetAllUsersAsync();
                        var userDict = users.ToDictionary(u => u.Id);
                        var postsWithUser = posts.Select(p => new
                        {
                            Post = p,
                            User = userDict.ContainsKey(p.User_Id) ? userDict[p.User_Id] : null
                        });
                        return Results.Ok(postsWithUser);
                    }

                    return Results.Ok(posts);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"An error occurred: {ex.Message}");
                }
            });

            // GET /posts/{postId}
            endpoints.MapGet("/posts/{postId:int}", async (int postId, PostService postService) =>
            {
                try
                {
                    var post = await postService.GetPostByIdAsync(postId);
                    if (post == null)
                    {
                        return Results.NotFound(new { message = "Post not found" });
                    }
                    return Results.Ok(post);
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

            endpoints.MapPut("/posts/{id:int}", async (int id, Post updatedPost, PostService postService) =>
           {
               try
               {
                   var post = await postService.UpdatePostAsync(id, updatedPost);
                   if (post == null)
                       return Results.NotFound(new { message = "Post not found" });
                   return Results.Ok(post);
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