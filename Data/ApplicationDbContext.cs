using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OurApp.Models;

namespace OurApp.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<Article> Articles { set; get; }
    public DbSet<Category> Categories { set; get; }
    public DbSet<Comment> Comments { set; get; }
    public DbSet<Subcategory> Subcategories { set; get; }
    public DbSet<ArticlesHistory> ArticlesHistories { set; get; }

    public DbSet<Bookmark> Bookmarks { set; get; }
    public DbSet<ArticleBookmark> ArticleBookmarks { set; get; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        base.OnModelCreating(modelBuilder);

        // definire primary key compus
        modelBuilder.Entity<ArticleBookmark>()
            .HasKey(ab => new { ab.Id, ab.ArticleId, ab.BookmarkId });


        // definire relatii cu modelele Bookmark si Article (FK)
        modelBuilder.Entity<ArticleBookmark>()
            .HasOne(ab => ab.Article)
            .WithMany(ab => ab.ArticleBookmarks)
            .HasForeignKey(ab => ab.ArticleId);

        modelBuilder.Entity<ArticleBookmark>()
            .HasOne(ab => ab.Bookmark)
            .WithMany(ab => ab.ArticleBookmarks)
            .HasForeignKey(ab => ab.BookmarkId);
    }

}

