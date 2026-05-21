using LaconicAndIconic.DAL.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LaconicAndIconic.DAL.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260521215000_CascadeDeleteSharedListRecipesForRecipes")]
    public partial class CascadeDeleteSharedListRecipesForRecipes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SharedListRecipes_Recipes_RecipeId",
                table: "SharedListRecipes");

            migrationBuilder.AddForeignKey(
                name: "FK_SharedListRecipes_Recipes_RecipeId",
                table: "SharedListRecipes",
                column: "RecipeId",
                principalTable: "Recipes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SharedListRecipes_Recipes_RecipeId",
                table: "SharedListRecipes");

            migrationBuilder.AddForeignKey(
                name: "FK_SharedListRecipes_Recipes_RecipeId",
                table: "SharedListRecipes",
                column: "RecipeId",
                principalTable: "Recipes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
