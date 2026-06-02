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
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(us => us.User)
            .WithMany(u => u.Followers)
            .HasForeignKey(us => us.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
