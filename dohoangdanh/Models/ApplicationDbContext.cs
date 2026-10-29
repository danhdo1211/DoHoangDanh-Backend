using Microsoft.EntityFrameworkCore;

namespace dohoangdanh.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<AppRule> AppRules { get; set; }
        public DbSet<LocationLog> LocationLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<User>().HasIndex(u => u.Email).IsUnique();
        }
    }
}