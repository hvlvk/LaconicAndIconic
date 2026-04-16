using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LaconicAndIconic.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddSharedListTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SharedLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OwnerId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedLists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SharedLists_AspNetUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SharedListRecipes",
                columns: table => new
                {
                    SharedListId = table.Column<int>(type: "integer", nullable: false),
                    RecipeId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedListRecipes", x => new { x.SharedListId, x.RecipeId });
                    table.ForeignKey(
                        name: "FK_SharedListRecipes_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SharedListRecipes_SharedLists_SharedListId",
                        column: x => x.SharedListId,
                        principalTable: "SharedLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SharedListUsers",
                columns: table => new
                {
                    SharedListId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedListUsers", x => new { x.SharedListId, x.UserId });
                    table.ForeignKey(
                        name: "FK_SharedListUsers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SharedListUsers_SharedLists_SharedListId",
                        column: x => x.SharedListId,
                        principalTable: "SharedLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 16, 21, 49, 15, 503, DateTimeKind.Utc).AddTicks(5958));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 16, 21, 49, 15, 503, DateTimeKind.Utc).AddTicks(5959));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 16, 21, 49, 15, 503, DateTimeKind.Utc).AddTicks(5960));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 16, 21, 49, 15, 503, DateTimeKind.Utc).AddTicks(5962));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 16, 21, 49, 15, 503, DateTimeKind.Utc).AddTicks(5963));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 16, 21, 49, 15, 503, DateTimeKind.Utc).AddTicks(5964));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 16, 21, 49, 15, 503, DateTimeKind.Utc).AddTicks(5965));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 16, 21, 49, 15, 503, DateTimeKind.Utc).AddTicks(5967));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 16, 21, 49, 15, 503, DateTimeKind.Utc).AddTicks(5968));

            migrationBuilder.CreateIndex(
                name: "IX_SharedListRecipes_RecipeId",
                table: "SharedListRecipes",
                column: "RecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedLists_OwnerId",
                table: "SharedLists",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedListUsers_UserId",
                table: "SharedListUsers",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SharedListRecipes");

            migrationBuilder.DropTable(
                name: "SharedListUsers");

            migrationBuilder.DropTable(
                name: "SharedLists");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 16, 17, 36, 1, 452, DateTimeKind.Utc).AddTicks(6750));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 16, 17, 36, 1, 452, DateTimeKind.Utc).AddTicks(6750));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 16, 17, 36, 1, 452, DateTimeKind.Utc).AddTicks(6750));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 16, 17, 36, 1, 452, DateTimeKind.Utc).AddTicks(6750));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 16, 17, 36, 1, 452, DateTimeKind.Utc).AddTicks(6750));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 16, 17, 36, 1, 452, DateTimeKind.Utc).AddTicks(6750));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 16, 17, 36, 1, 452, DateTimeKind.Utc).AddTicks(6800));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 16, 17, 36, 1, 452, DateTimeKind.Utc).AddTicks(6800));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 16, 17, 36, 1, 452, DateTimeKind.Utc).AddTicks(6800));
        }
    }
}
