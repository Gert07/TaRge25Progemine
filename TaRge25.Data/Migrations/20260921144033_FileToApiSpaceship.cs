using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaRge25Shop.Data.Migrations
{
    /// <inheritdoc />
    public partial class FileToApiSpaceship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileToApi",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExistingFilePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpaceshipId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileToApi", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileToApi");
        }
    }
}
