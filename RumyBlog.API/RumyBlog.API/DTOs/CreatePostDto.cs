using Microsoft.AspNetCore.Http;

namespace RumyBlog.API.DTOs
{
    public class CreatePostDto
    {
        public string Title { get; set; }
        public string Content { get; set; }

        // kullanıcı görsel eklemek zorunda değil
        public IFormFile? File { get; set; }
    }
}
