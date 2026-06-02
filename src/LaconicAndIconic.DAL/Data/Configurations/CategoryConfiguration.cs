using LaconicAndIconic.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaconicAndIconic.DAL.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(c => c.Name)
            .IsUnique();

        builder.HasData(
            new Category { Id = 1, Name = "Сніданки", CreatedAt = new DateTime(2026, 6, 1, 22, 49, 27, 117, DateTimeKind.Utc).AddTicks(3389) },
            new Category { Id = 2, Name = "Перші страви", CreatedAt = new DateTime(2026, 6, 1, 22, 49, 27, 117, DateTimeKind.Utc).AddTicks(3391) },
            new Category { Id = 3, Name = "Основні страви", CreatedAt = new DateTime(2026, 6, 1, 22, 49, 27, 117, DateTimeKind.Utc).AddTicks(3392) },
            new Category { Id = 4, Name = "Салати", CreatedAt = new DateTime(2026, 6, 1, 22, 49, 27, 117, DateTimeKind.Utc).AddTicks(3394) },
            new Category { Id = 5, Name = "Десерти", CreatedAt = new DateTime(2026, 6, 1, 22, 49, 27, 117, DateTimeKind.Utc).AddTicks(3395) },
            new Category { Id = 6, Name = "Закуски", CreatedAt = new DateTime(2026, 6, 1, 22, 49, 27, 117, DateTimeKind.Utc).AddTicks(3397) },
            new Category { Id = 7, Name = "Випічка", CreatedAt = new DateTime(2026, 6, 1, 22, 49, 27, 117, DateTimeKind.Utc).AddTicks(3398) },
            new Category { Id = 8, Name = "Вегетаріанські страви", CreatedAt = new DateTime(2026, 6, 1, 22, 49, 27, 117, DateTimeKind.Utc).AddTicks(3399) },
            new Category { Id = 9, Name = "Напої", CreatedAt = new DateTime(2026, 6, 1, 22, 49, 27, 117, DateTimeKind.Utc).AddTicks(3401) }
        );
    }
}
