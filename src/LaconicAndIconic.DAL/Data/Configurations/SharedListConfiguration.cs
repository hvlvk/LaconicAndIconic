using LaconicAndIconic.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaconicAndIconic.DAL.Data.Configurations;

public class SharedListConfiguration : IEntityTypeConfiguration<SharedList>
{
    public void Configure(EntityTypeBuilder<SharedList> builder)
    {
        builder.HasKey(sl => sl.Id);

        builder.Property(sl => sl.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(sl => sl.Description)
            .HasMaxLength(500);

        builder.HasOne(sl => sl.Owner)
            .WithMany(u => u.OwnedSharedLists)
            .HasForeignKey(sl => sl.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
