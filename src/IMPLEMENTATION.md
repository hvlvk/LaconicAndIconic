# Implementation Draft

This document contains the exact implementation and tests for the `User Subscription (Following) Mechanism` feature based on `PLAN.md`.

## Step 1: Create `UserSubscription` Entity and Update `ApplicationUser`

**File**: `LaconicAndIconic.DAL/Entities/UserSubscription.cs`
Creates the Join table entity to map followers and following.

```csharp
namespace LaconicAndIconic.DAL.Entities;

public class UserSubscription
{
    public int FollowerId { get; set; }
    public ApplicationUser Follower { get; set; } = null!;

    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
}
```

**File**: `LaconicAndIconic.DAL/Entities/ApplicationUser.cs`
Update the `ApplicationUser` entity to include the `Followers` and `Following` collections.

```csharp
using Microsoft.AspNetCore.Identity;

namespace LaconicAndIconic.DAL.Entities;

public class ApplicationUser : IdentityUser<int>
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? ProfilePicturePath { get; set; }

    public ICollection<Recipe> Recipes { get; } = [];
    public ICollection<Rating> Ratings { get; } = [];
    public ICollection<Comment> Comments { get; } = [];
    
    public ICollection<UserSubscription> Followers { get; } = [];
    public ICollection<UserSubscription> Following { get; } = [];
}
```

## Step 2: Configure Fluent API and `ApplicationDbContext`

**File**: `LaconicAndIconic.DAL/Data/Configurations/UserSubscriptionConfiguration.cs`
Enforces composite primary keys and sets `OnDelete(DeleteBehavior.Restrict)` to block circular cascade cycles in PostgreSQL.

```csharp
using LaconicAndIconic.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaconicAndIconic.DAL.Data.Configurations;

public class UserSubscriptionConfiguration : IEntityTypeConfiguration<UserSubscription>
{
    public void Configure(EntityTypeBuilder<UserSubscription> builder)
    {
        builder.HasKey(us => new { us.FollowerId, us.UserId });

        builder.HasOne(us => us.Follower)
            .WithMany(u => u.Following)
            .HasForeignKey(us => us.FollowerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(us => us.User)
            .WithMany(u => u.Followers)
            .HasForeignKey(us => us.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

**File**: `LaconicAndIconic.DAL/Data/ApplicationDbContext.cs`
Add the `UserSubscriptions` DbSet mapping.

```csharp
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
    public DbSet<UserSubscription> UserSubscriptions => Set<UserSubscription>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
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
```

## Step 3: Add and Apply Database Migration

Execute the following terminal commands to create and apply the database migrations:

```powershell
# Restore packages & build to ensure there are no compilation errors
dotnet build LaconicAndIconic.sln

# Run Entity Framework Migrations Tool
dotnet ef migrations add AddUserSubscriptions --project LaconicAndIconic.DAL --startup-project LaconicAndIconic.Web

# Apply to Development Database Configured in appsettings.json (with secrets)
dotnet ef database update --project LaconicAndIconic.DAL --startup-project LaconicAndIconic.Web
```

*(No C# code updates required from our end for this step, it simply involves executing standard code-first CLI tools to complete the deployment loop shown in the `PLAN.md`.)*
