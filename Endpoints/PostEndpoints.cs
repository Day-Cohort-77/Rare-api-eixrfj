using RareAPI.Models;
using RareAPI.Services;

namespace RareAPI.Endpoints
{
    public static class PostEndpoints
    {
        public static void MapPostEndpoints(this IEndpointRouteBuilder endpoints)
        {
            // GET /posts with optional expand=user or expand=category
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
                            p.Id,
                            p.Title,
                            p.Content,
                            p.Publication_Date,
                            p.Image_Url,
                            p.Approved,
                            p.Category_Id,
                            p.User_Id,
                            User = userDict.TryGetValue(p.User_Id, out var user) ? user : null
                        });
                        return Results.Ok(postsWithUser);
                    }

                    if (expand == "category")
                    {
                        var categories = await databaseService.GetAllCategoriesAsync();
                        var categoryDict = categories.ToDictionary(c => c.Id);
                        var postsWithCategory = posts.Select(p => new
                        {
                            p.Id,
                            p.Title,
                            p.Content,
                            p.Publication_Date,
                            p.Image_Url,
                            p.Approved,
                            p.Category_Id,
                            p.User_Id,
                            Category = categoryDict.TryGetValue(p.Category_Id, out var cat) ? cat : null
                        });
                        return Results.Ok(postsWithCategory);
                    }

                    return Results.Ok(posts);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"An error occurred: {ex.Message}");
                }
            });

            // GET /posts/{postId}
            endpoints.MapGet("/posts/{postId:int}", async (int postId, DatabaseService databaseService) =>
            {
                try
                {
                    var post = await databaseService.GetPostByIdAsync(postId);
                    if (post == null)
                    {
                        return Results.NotFound(new { message = "Post not found" });
                    }

                    User? user = null;
                    Category? category = null;
                    try
                    {
                        user = post.User_Id != 0 ? await databaseService.GetUserByIdAsync(post.User_Id) : null;
                    }
                    catch { user = null; }
                    try
                    {
                        if (post.Category_Id != 0)
                        {
                            var categories = await databaseService.GetAllCategoriesAsync();
                            category = categories.FirstOrDefault(c => c.Id == post.Category_Id);
                        }
                    }
                    catch { category = null; }

                    var expandedPost = new
                    {
                        Id = post.Id,
                        Title = post.Title,
                        Content = post.Content,
                        PublicationDate = post.Publication_Date,
                        ImageUrl = post.Image_Url,
                        Approved = post.Approved,
                        User = user != null ? new
                        {
                            id = user.Id,
                            name = $"{user.First_Name} {user.Last_Name}",
                            email = user.Email
                        } : null,
                        Category = category != null ? new
                        {
                            id = category.Id,
                            label = category.Label
                        } : null,
                        User_Id = post.User_Id,
                        Category_Id = post.Category_Id
                    };

                    return Results.Ok(expandedPost);
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

            // PUT /posts/{id}
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