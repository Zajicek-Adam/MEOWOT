using MEOWOT.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MEOWOT.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<StoredFile> Files { get; set; }
        public DbSet<Thumbnail> Thumbnails{ get; set; }
        public DbSet<Gallery> Galleries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Thumbnail>().HasKey(t => new { t.FileId, t.Type });
            modelBuilder.Entity<StoredFile>().HasOne(x => x.Gallery).WithMany(x => x.Images).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Gallery>().HasMany(x => x.Images).WithOne(x => x.Gallery).OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<User>().HasOne(x => x.DefGallery).WithOne(x => x.GalleryUser);
        }
    }
}