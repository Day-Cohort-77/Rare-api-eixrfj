namespace RareAPI.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int Author_Id { get; set; }
        public int Post_Id { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
