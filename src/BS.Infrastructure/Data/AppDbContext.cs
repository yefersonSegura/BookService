using BS.Domain.Entity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BS.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Book> Books { get; set; }
    public DbSet<Author> Authors { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasOne(b => b.Author)
                .WithMany(a => a.Books)
                .HasForeignKey(b => b.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.Isbn).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.CoverUrl).HasMaxLength(500);
            entity.Property(e => e.PageCount).HasColumnName("PageNumber");
        });

        modelBuilder.Entity<Author>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
        });
    }
}
