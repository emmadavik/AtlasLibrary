using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdminDashbord.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Reminders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CompletedObjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    BorrowerName = table.Column<string>(type: "TEXT", nullable: false),
                    BorrowerEmail = table.Column<string>(type: "TEXT", nullable: false),
                    ObjectTitle = table.Column<string>(type: "TEXT", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    ReminderDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reminders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Summary = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SelectedObjectIds = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reminders");

            migrationBuilder.DropTable(
                name: "Reports");
        }
    }
}
