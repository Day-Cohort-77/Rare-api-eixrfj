using RareAPI.Models;

namespace RareAPI.Services
{
    public class PostService
    {
        private readonly DatabaseService _databaseService;

        public PostService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        // Business logic methods that use DatabaseService
        public async Task<List<Post>> GetAllPostsAsync()
        {
            return await _databaseService.GetAllPostsAsync();
        }

        public async Task<Post?> GetPostByIdAsync(int id)
        {
            return await _databaseService.GetPostByIdAsync(id);
        }

        public async Task<Post?> CreatePostAsync(Post newPost)
        {
            // Add any business logic validation here
            if (string.IsNullOrWhiteSpace(newPost.Title))
            {
                throw new ArgumentException("Post title cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(newPost.Content))
            {
                throw new ArgumentException("Post content cannot be empty");
            }

            return await _databaseService.CreatePostAsync(newPost);
        }

        public async Task<Post?> UpdatePostAsync(int id, Post updatedPost)
        {
            // Add any business logic validation here
            if (string.IsNullOrWhiteSpace(updatedPost.Title))
            {
                throw new ArgumentException("Post title cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(updatedPost.Content))
            {
                throw new ArgumentException("Post content cannot be empty");
            }

            return await _databaseService.UpdatePostAsync(id, updatedPost);
        }

        public async Task<bool> DeletePostAsync(int id)
        {
            return await _databaseService.DeletePostAsync(id);
        }

        public async Task<List<Post>> GetPostsByUserIdAsync(int userId)
        {
            return await _databaseService.GetPostsByUserIdAsync(userId);
        }

        public async Task<List<Post>> GetPublishedPostsAsync()
        {
            // This could be a more complex business logic method
            var allPosts = await _databaseService.GetAllPostsAsync();
            return allPosts.Where(p => p.IsPublished).ToList();
        }
    }
}