using LaconicAndIconic.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaconicAndIconic.DAL.Data.Configurations;

public class SharedListRecipeConfiguration : IEntityTypeConfiguration<SharedListRecipe>
{
    public void Configure(EntityTypeBuilder<SharedListRecipe> builder)
    {
        builder.HasKey(slr => new { slr.SharedListId, slr.RecipeId });

        builder.HasOne(slr => slr.SharedList)
            .WithMany(sl => sl.SharedListRecipes)
            .HasForeignKey(slr => slr.SharedListId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(slr => slr.Recipe)
            .WithMany(r => r.SharedListRecipes)
            .HasForeignKey(slr => slr.RecipeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
