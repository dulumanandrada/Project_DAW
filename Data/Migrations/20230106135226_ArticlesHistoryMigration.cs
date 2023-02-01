using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OurApp.Data.Migrations
{
    public partial class ArticlesHistoryMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArticlesHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ArticleId = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    Content = table.Column<string>(type: "TEXT", nullable: true),
                    EditDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: true),
                    SubcategoryId = table.Column<int>(type: "INTEGER", nullable: true),
                    UserId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticlesHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArticlesHistories_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ArticlesHistories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ArticlesHistories_Subcategories_SubcategoryId",
                        column: x => x.SubcategoryId,
                        principalTable: "Subcategories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArticlesHistories_CategoryId",
                table: "ArticlesHistories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticlesHistories_SubcategoryId",
                table: "ArticlesHistories",
                column: "SubcategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticlesHistories_UserId",
                table: "ArticlesHistories",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArticlesHistories");
        }
    }
}
