using LaconicAndIconic.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaconicAndIconic.DAL.Data.Configurations;

public class SavedRecipeConfiguration : IEntityTypeConfiguration<SavedRecipe>
{
    public void Configure(EntityTypeBuilder<SavedRecipe> builder)
    {
        builder.HasKey(sr => sr.Id);

        builder.HasOne(sr => sr.User)
            .WithMany(u => u.SavedRecipes)
            .HasForeignKey(sr => sr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sr => sr.Recipe)
            .WithMany(r => r.SavedRecipes)
            .HasForeignKey(sr => sr.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(sr => new { sr.UserId, sr.RecipeId })
            .IsUnique();
    }
}

