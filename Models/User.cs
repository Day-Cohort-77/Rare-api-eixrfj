namespace RareAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string First_Name { get; set; } = string.Empty;
        public string Last_Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Profile_Image_Url { get; set; } = string.Empty;
        public DateTime Created_On { get; set; }
        public bool Active { get; set; } = true;
    }
}