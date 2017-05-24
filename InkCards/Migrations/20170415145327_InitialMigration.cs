using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace InkCards.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CardImpressions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BackMillisecondsSpent = table.Column<long>(nullable: false),
                    CardId = table.Column<Guid>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    FrontMillisecondsSpent = table.Column<long>(nullable: false),
                    GuessedCorrectly = table.Column<bool>(nullable: false),
                    TestedSide = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardImpressions", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CardImpressions");
        }
    }
}
