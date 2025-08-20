namespace Rare.Models
{
    public class Posts
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime PublicationDate { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool Approved { get; set; } 
    }
}