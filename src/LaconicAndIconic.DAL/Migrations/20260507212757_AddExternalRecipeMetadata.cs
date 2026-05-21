using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LaconicAndIconic.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalRecipeMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "Recipes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalSource",
                table: "Recipes",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_ExternalSource_ExternalId",
                table: "Recipes",
                columns: new[] { "ExternalSource", "ExternalId" },
                unique: true,
                filter: "\"ExternalSource\" IS NOT NULL AND \"ExternalId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Recipes_ExternalSource_ExternalId",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "ExternalSource",
                table: "Recipes");
        }
    }
}
