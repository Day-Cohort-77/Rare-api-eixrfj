using RareAPI.Models;

namespace RareAPI.Services
{
    public class CategoryService
    {
        private readonly DatabaseService _databaseService;

        public CategoryService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        // Business logic methods that use DatabaseService
        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _databaseService.GetAllCategoriesAsync();
        }


        public async Task<Category?> CreateCategoryAsync(Category newCategory)
        {
            // Add any business logic validation here
            if (string.IsNullOrWhiteSpace(newCategory.Label))
            {
                throw new ArgumentException("Category Label cannot be empty");
            }



            return await _databaseService.CreateCategoryAsync(newCategory);
        }



        public async Task<bool> DeleteCategoryAsync(int id)
        {
            return await _databaseService.DeleteCategoryAsync(id);
        }


    }
}