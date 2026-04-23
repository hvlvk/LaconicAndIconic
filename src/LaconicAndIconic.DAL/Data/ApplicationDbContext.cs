using System.Reflection;
using LaconicAndIconic.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LaconicAndIconic.DAL.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<Rating> Ratings => Set<Rating>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<UserSubscription> UserSubscriptions => Set<UserSubscription>();
    public DbSet<SharedList> SharedLists => Set<SharedList>();
    public DbSet<SharedListUser> SharedListUsers => Set<SharedListUser>();
    public DbSet<SharedListRecipe> SharedListRecipes => Set<SharedListRecipe>();

    protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);
    builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());


    builder.Entity<Recipe>()
        .HasOne(r => r.Author)
        .WithMany()
        .HasForeignKey(r => r.AuthorId)
        .OnDelete(DeleteBehavior.Cascade);


    builder.Entity<Comment>()
        .HasOne(c => c.Author)
        .WithMany()
        .HasForeignKey(c => c.AuthorId)
        .OnDelete(DeleteBehavior.Cascade);


    builder.Entity<Rating>()
        .HasOne(r => r.User)
        .WithMany()
        .HasForeignKey(r => r.UserId)
        .OnDelete(DeleteBehavior.Cascade);


    builder.Entity<UserSubscription>()
        .HasOne(s => s.User)
        .WithMany()
        .HasForeignKey(s => s.UserId)
        .OnDelete(DeleteBehavior.Cascade);
        

    builder.Entity<SharedListUser>()
        .HasOne(slu => slu.User)
        .WithMany()
        .HasForeignKey(slu => slu.UserId)
        .OnDelete(DeleteBehavior.Cascade);
}

    public override int SaveChanges()
    {
        SetTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void SetTimestamps()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
