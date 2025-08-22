namespace RareAPI.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int UserId { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public bool IsPublished { get; set; } = false;

        // Navigation property (optional - for when you want to include user info)
        public User? User { get; set; }
    }
}