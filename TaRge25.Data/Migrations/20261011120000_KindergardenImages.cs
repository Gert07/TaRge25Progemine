using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaRge25Shop.Data.Migrations
{
    public partial class KindergardenImages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "KindergardenId",
                table: "FileToDatabases",
                type: "uniqueidentifier",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KindergardenId",
                table: "FileToDatabases");
        }
    }
}
