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

        //get all categories
        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _databaseService.GetAllCategoriesAsync();
        }

        //create new category
        public async Task<Category?> CreateCategoryAsync(Category newCategory)
        {

            if (string.IsNullOrWhiteSpace(newCategory.Label))
            {
                throw new ArgumentException("Category Label cannot be empty");
            }



            return await _databaseService.CreateCategoryAsync(newCategory);
        }

        //delete category
        public async Task<bool> DeleteCategoryAsync(int id)
        {
            return await _databaseService.DeleteCategoryAsync(id);
        }


        public async Task<Category?> UpdateCategoryAsync(int id, Category updatedCategory)
        {
            return await _databaseService.UpdateCategoryAsync(id, updatedCategory);
        }


    }
}