using Microsoft.EntityFrameworkCore;
using RumyBlog.API.Models;

namespace RumyBlog.API.Data
{
    //.NET uygulamasında veri tabanı işlemlerini yöneten EfCore yapısı
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Post> Posts { get; set; }
    }
}
