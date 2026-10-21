using Microsoft.AspNetCore.Mvc;
using RumyBlog.API.Data;
using RumyBlog.API.DTOs;
using RumyBlog.API.DTOs.Auth;
using RumyBlog.API.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;



namespace RumyBlog.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        //AppDbContext sınıfını buraya enjekte ediyoruz "springteki dependency enjection mantığı"
        //_context üzerinden veri tabanı işlemlerini yapabilir hale geliriz
        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public IActionResult Register(RegisterDto dto)
        {
            //_context.Users.Any(...) sistemde daha önce bu mail adresi ile biri kayıt olmuş mu kontrolü
            var emailExists = _context.Users.Any(x => x.Email == dto.Email);

            if (emailExists)
            {
                return BadRequest("Bu email zaten kayıtlı");
            }

            //Dışarıdan gelen verileri (dto), veri tabanına kaydedilecek olan gerçek modele (User) aktarılır.
            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Password = HashPassword(dto.Password)
            };

            _context.Users.Add(user); //komutu kullanıcıyı henüz veri tabanına yazmaz, sadece EF Core'un takip listesine ekler.
            //EF Core'a şunu der: "Bak, elimde yeni bir veri var. Bunu takibe al ve veri tabanına eklenecekler listesine koy."

            _context.SaveChanges();//Bu satır çalıştığında EF Core bir SQL INSERT komutu oluşturur ve veriyi kalıcı olarak veri tabanına yazar.

            return Ok("Kullanıcı başarıyla kaydedildi");
        }

        //güvenlik açısında kullanıcının girdiği şifrenin aynı şekilde veri tabanına kaydedilmemesi lazım bu yüzden veri tabanında değişik şekilde görünür gerçek şifre görünmez
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        [HttpPost("refresh-token")]
        public IActionResult RefreshToken(string refreshToken)
        {
            var user = _context.Users.FirstOrDefault(x =>
                x.RefreshToken == refreshToken &&
                x.RefreshTokenExpireDate > DateTime.Now);

            if (user == null)
                return Unauthorized();

            var newToken = GenerateJwtToken(user);
            return Ok(new { token = newToken });
        }


        [HttpPost("login")]
        public IActionResult Login(LoginDto dto)
        {
            var user = _context.Users.FirstOrDefault(x => x.Email == dto.Email);
            if (user == null)
                return BadRequest("Email veya şifre hatalı");

            if (user.Password != HashPassword(dto.Password))
                return BadRequest("Email veya şifre hatalı");

            var accessToken = GenerateJwtToken(user);

            if (dto.RememberMe)
            {
                user.RefreshToken = Guid.NewGuid().ToString();
                user.RefreshTokenExpireDate = DateTime.Now.AddDays(7);
                _context.SaveChanges();
            }

            return Ok(new
            {
                token = accessToken,
                refreshToken = user.RefreshToken
            });
        }


        //jwt token üretimi ile ilgili kısım
        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new Claim(ClaimTypes.Email, user.Email),
    new Claim(ClaimTypes.Role, user.Role)
};



            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(
                    Convert.ToDouble(_configuration["Jwt:ExpireMinutes"])
                ),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
