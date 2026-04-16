using LaconicAndIconic.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaconicAndIconic.DAL.Data.Configurations;

public class SharedListUserConfiguration : IEntityTypeConfiguration<SharedListUser>
{
    public void Configure(EntityTypeBuilder<SharedListUser> builder)
    {
        builder.HasKey(slu => new { slu.SharedListId, slu.UserId });

        builder.HasOne(slu => slu.SharedList)
            .WithMany(sl => sl.SharedListUsers)
            .HasForeignKey(slu => slu.SharedListId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(slu => slu.User)
            .WithMany(u => u.SharedListMemberships)
            .HasForeignKey(slu => slu.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
