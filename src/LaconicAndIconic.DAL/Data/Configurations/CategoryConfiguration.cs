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
            new Category { Id = 1, Name = "Сніданки", CreatedAt = DateTime.UtcNow },
            new Category { Id = 2, Name = "Перші страви", CreatedAt = DateTime.UtcNow },
            new Category { Id = 3, Name = "Основні страви", CreatedAt = DateTime.UtcNow },
            new Category { Id = 4, Name = "Салати", CreatedAt = DateTime.UtcNow },
            new Category { Id = 5, Name = "Десерти", CreatedAt = DateTime.UtcNow },
            new Category { Id = 6, Name = "Закуски", CreatedAt = DateTime.UtcNow },
            new Category { Id = 7, Name = "Випічка", CreatedAt = DateTime.UtcNow },
            new Category { Id = 8, Name = "Вегетаріанські страви", CreatedAt = DateTime.UtcNow },
            new Category { Id = 9, Name = "Напої", CreatedAt = DateTime.UtcNow }
        );
    }
}
