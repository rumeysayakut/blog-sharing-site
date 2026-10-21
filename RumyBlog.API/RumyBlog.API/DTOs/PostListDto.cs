namespace RumyBlog.API.DTOs
{
    public class PostListDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public string? FileName { get; set; }
        public string? FilePath { get; set; }

        // Post’u atan kullanıcının email’i postlar listelenirken her postun altına kullanıcısını yazacağız
        //bu yüzden burda eklemem
        public string UserEmail { get; set; }
    }
}
