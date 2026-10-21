namespace RumyBlog.API.DTOs
{
    public class UpdatePostDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public string? FileName { get; set; }

        // Post’u atan kullanıcının email’i
        public string UserEmail { get; set; }
    }
}
