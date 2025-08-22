namespace RareAPI.Models
{
    public class Post
    {
        public int Id { get; set; }
        public int User_Id { get; set; }
        public int Category_Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime Publication_Date { get; set; }
        public string Image_Url { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool? Approved { get; set; }
    }
}