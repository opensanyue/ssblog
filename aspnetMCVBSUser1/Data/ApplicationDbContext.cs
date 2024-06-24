using aspnetMCVBSUser1.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using aspnetMCVBSUser1.Models.ViewModels;

namespace aspnetMCVBSUser1.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Post> Posts { get; set; } = default!;

        public DbSet<Category> Categorys { get; set; } = default!;
        public DbSet<aspnetMCVBSUser1.Models.ViewModels.PostImg> PostImg { get; set; } = default!;
    }
}
