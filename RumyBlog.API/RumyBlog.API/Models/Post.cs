namespace RumyBlog.API.Models
{
    public class Post
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        //dosya bilgilerinin tutulduğu kısım
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
      

        // Kullanıcı ilişkisi
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
