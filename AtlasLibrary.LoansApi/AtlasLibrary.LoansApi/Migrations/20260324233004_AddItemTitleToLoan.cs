using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AtlasLibrary.LoansApi.Migrations
{
    /// <inheritdoc />
    public partial class AddItemTitleToLoan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ItemTitle",
                table: "Loans",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ItemTitle",
                table: "Loans");
        }
    }
}
