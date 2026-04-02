# Feature: User Subscription (Following) Mechanism

## Summary
Implement a user following mechanism that enables users to follow other users. This requires a newly introduced join entity to represent the many-to-many relationship mapping an `ApplicationUser` (the follower) to another `ApplicationUser` (the user being followed). This feature sets up the foundational data layer schema required to query the followers and following lists for a given user.

## Affected Areas
- **Models/Entities**: 
  - `LaconicAndIconic.DAL/Entities/UserSubscription.cs` (New)
  - `LaconicAndIconic.DAL/Entities/ApplicationUser.cs`
- **Controllers/Routes**: None
- **Services**: None
- **Config**: 
  - `LaconicAndIconic.DAL/Data/Configurations/UserSubscriptionConfiguration.cs` (New)
  - `LaconicAndIconic.DAL/Data/ApplicationDbContext.cs`
- **Frontend**: None
- **Migrations**: Yes
- **Re-index needed**: No

## Risk Assessment
- **Self-referencing Many-to-Many**: Building a many-to-many relationship on a single table (`ApplicationUser` to `ApplicationUser`) can result in foreign key cyclic cascade delete paths in the underlying RDBMS (PostgreSQL). To prevent `multiple cascade paths` errors during migration and execution, one or both sides of the relationship must be configured with `DeleteBehavior.Restrict` (or equivalent).
- **Data Type Constraints**: The `ApplicationUser` entity derives from `IdentityUser<int>`. So standard integer-based (`int`) keys must be used for `FollowerId` and `UserId`.

## Steps

### Step 1: Create `UserSubscription` Entity and Update `ApplicationUser`
- **Files**: 
  - `LaconicAndIconic.DAL/Entities/UserSubscription.cs`
  - `LaconicAndIconic.DAL/Entities/ApplicationUser.cs`
- **Change**: 
  - Create the `UserSubscription` class with two `int` foreign keys: `FollowerId` and `UserId`. Add navigation properties for `Follower` and `User`.
  - Update `ApplicationUser` with two `ICollection<UserSubscription>` properties: `Followers` (where the current user is `UserId`) and `Following` (where the current user is `FollowerId`).
- **Test**: Verify the project compiles successfully with the new properties.
- **Commit message**: `feat(dal): create UserSubscription entity and navigation properties`

### Step 2: Configure Fluent API and `ApplicationDbContext`
- **Files**: 
  - `LaconicAndIconic.DAL/Data/Configurations/UserSubscriptionConfiguration.cs`
  - `LaconicAndIconic.DAL/Data/ApplicationDbContext.cs`
- **Change**: 
  - Add a `DbSet<UserSubscription>` to `ApplicationDbContext`.
  - Create the Fluent API configuration for `UserSubscription`: set a composite primary key using `FollowerId` and `UserId`.
  - Explicitly configure the relationships `HasOne(...).WithMany(...)` ensuring that `OnDelete(DeleteBehavior.Restrict)` is applied to at least one (ideally both) foreign keys to prevent cascade cycles.
- **Test**: Run a local compilation to guarantee valid EF Core references exist.
- **Commit message**: `feat(dal): add UserSubscription fluent API configurations`

### Step 3: Add and Apply Database Migration
- **Files**: New migration files in `LaconicAndIconic.DAL/Migrations/`
- **Change**: 
  - Execute EF Core CLI to generate a migration. (e.g. `dotnet ef migrations add AddUserSubscriptions --project LaconicAndIconic.DAL --startup-project LaconicAndIconic.Web`)
  - Update the local database to verify it applies without cyclic errors. (`dotnet ef database update --project LaconicAndIconic.DAL --startup-project LaconicAndIconic.Web`)
- **Test**: Verify the migration generation succeeds, and the `Database update` command succeeds against a local DB instance. Check that the `UserSubscriptions` table consists of the composite key and correct constraints.
- **Commit message**: `migration: add AddUserSubscriptions EF Core migration`