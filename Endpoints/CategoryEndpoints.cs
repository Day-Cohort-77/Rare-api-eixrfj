using RareAPI.Models;
using RareAPI.Services;


namespace RareAPI.Endpoints
{
    public static class CategoryEndpoints
    {
        public static void MapCategoryEndpoints(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/Categories", async (DatabaseService databaseService) =>
            {
                try
                {
                    var categories = await databaseService.GetAllCategoriesAsync();
                    return Results.Ok(categories);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"An error occurred: {ex.Message}");
                }
            });

            endpoints.MapPost("/Categories", async (Category postRequest, DatabaseService databaseService) =>
            {
                try
                {
                    var newCategory = new Category
                    {
                        Label = postRequest.Label,
                    };
                    var createdCategory = await databaseService.CreateCategoryAsync(newCategory);
                    return Results.Created($"/Categories/{createdCategory?.Id}", createdCategory);

                }
                catch (Exception ex)
                {
                    return Results.Problem($"An error occurred: {ex.Message}");
                }
            });

            // DELETE /posts/{id}
            endpoints.MapDelete("/Categories/{id:int}", async (int id, DatabaseService databaseService) =>
            {
                try
                {
                    var deleted = await databaseService.DeleteCategoryAsync(id);
                    if (deleted)
                    {
                        return Results.NoContent();
                    }

                    return Results.NotFound(new { message = "Category not found" });
                }
                catch (Exception ex)
                {
                    return Results.Problem($"An error occurred: {ex.Message}");
                }
            });


        }
    }
}