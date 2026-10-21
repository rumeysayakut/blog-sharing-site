namespace RumyBlog.API.DTOs.Auth
{

    //DTO = Data Transfer Object "Dış dünya ile konuşurken kullandığın veri paketi"
    public class LoginDto
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public bool RememberMe { get; set; }

    }
}
