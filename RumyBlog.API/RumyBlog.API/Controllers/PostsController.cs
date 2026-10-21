using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RumyBlog.API.Data;
using RumyBlog.API.DTOs;
using RumyBlog.API.Models;
using System.Security.Claims;

namespace RumyBlog.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {

        private readonly AppDbContext _context; //Veritabanı işlemlerini yapmak için kullanılan kapıdır
        private readonly IWebHostEnvironment _env;

        public PostsController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [Authorize] //sadece giriş yapan kişiler için
        [HttpPost]
        public async Task<IActionResult> CreatePost([FromForm] CreatePostDto dto)

        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif",".webp",".pdf",".docx"};

            try
            {
                
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                    return Unauthorized(new { message = "Kullanıcı kimliği doğrulanamadı." });

                int userId = int.Parse(userIdClaim.Value);

                string? fileName = null;
                string? filePath = null;

               
                if (dto.File != null && dto.File.Length > 0)
                {
                    var extension = Path.GetExtension(dto.File.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                        return BadRequest(new { message = "Geçersiz dosya tipi. Sadece resim yükleyebilirsiniz." });

                    if (dto.File.Length > 300 * 1024 * 1024)
                        return BadRequest(new { message = "Dosya boyutu 300MB'dan büyük olamaz." });

                    var uploadsFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");

                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    fileName = $"{Guid.NewGuid()}{extension}";
                    filePath = Path.Combine("uploads", fileName);
                    var fullPath = Path.Combine(uploadsFolder, fileName);

                    using var stream = new FileStream(fullPath, FileMode.Create);
                    await dto.File.CopyToAsync(stream);
                }

                var post = new Post
                {
                    Title = dto.Title,
                    Content = dto.Content,
                    CreatedAt = DateTime.UtcNow,
                    UserId = userId,
                    FileName = fileName,
                    FilePath = filePath
                };

                await _context.Posts.AddAsync(post);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Post başarıyla oluşturuldu", postId = post.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "İşlem sırasında sunucu taraflı bir hata oluştu." });
            }
        }


        [Authorize]
        [HttpGet("{id}")]
        public IActionResult GetPostById(int id)
        {
            var post = _context.Posts
                .Include(p => p.User)
                .Where(p => p.Id == id)
                .Select(p => new PostUpdateDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Content = p.Content
                })
                .FirstOrDefault();

            if (post == null)
                return NotFound("Post bulunamadı");

            return Ok(post);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
                return NotFound();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (post.UserId != userId && role != "Admin")
                return Forbid();

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return Ok("Post silindi");
        }



        [Authorize(Roles = "Admin")]
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok("Kullanıcı silindi");
        }


        [Authorize]
        [HttpGet("my")]
        public IActionResult GetMyPosts()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var posts = _context.Posts
                .Where(p => p.UserId == userId)
                .Select(p => new PostListDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Content = p.Content,
                    FileName = p.FileName,
                    FilePath = p.FilePath,
                    UserEmail = p.User.Email
                })
                .ToList();

            return Ok(posts);
        }



        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(int id, [FromForm] CreatePostDto dto)
        {
            try
            {

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                    return Unauthorized(new { message = "Kullanıcı kimliği doğrulanamadı." });

                int userId = int.Parse(userIdClaim.Value);
                var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);

                if (post == null)
                    return NotFound("Post bulunamadı");

                if (post.UserId != userId)
                    return Forbid("Bu postu güncelleme yetkin yok");

                post.Title = dto.Title;
                post.Content = dto.Content;

                await _context.SaveChangesAsync();

                return Ok("Post güncellendi");
            }
            catch
            {
                return StatusCode(500, "Güncelleme sırasında hata oluştu");
            }
        }

        [HttpGet]
        public IActionResult GetAllPosts()
        {
            var posts = _context.Posts
                .Include(p => p.User)
                .Select(p => new PostListDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Content = p.Content,
                    FileName = p.FileName,
                    FilePath = p.FilePath, 
                    UserEmail = p.User.Email
                })
                .ToList();

            return Ok(posts);
        }


        [Authorize]
        [HttpPost("{id}/upload-exe")]
        public async Task<IActionResult> UploadExe(int id, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("Dosya seçilmedi");

                if (Path.GetExtension(file.FileName).ToLower() != ".exe")
                    return BadRequest("Sadece .exe dosyaları yüklenebilir");

                if (file.Length > 20 * 1024 * 1024)
                    return BadRequest("Dosya 20MB'dan büyük olamaz");

                var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);
                if (post == null)
                    return NotFound("Post bulunamadı");

                var exeFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "executables");

                if (!Directory.Exists(exeFolder))
                    Directory.CreateDirectory(exeFolder);

                var fileName = $"{Guid.NewGuid()}.exe";
                var fullPath = Path.Combine(exeFolder, fileName);

                await using var stream = new FileStream(fullPath, FileMode.Create);
                await file.CopyToAsync(stream);

                post.FileName = fileName;
                post.FilePath = Path.Combine("executables", fileName);

                await _context.SaveChangesAsync();

                return Ok("Exe dosyası yüklendi");
            }
            catch
            {
                return StatusCode(500, "Exe yüklenirken hata oluştu");
            }
        }

    }
}
