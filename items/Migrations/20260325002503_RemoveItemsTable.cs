using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace items.Migrations
{
    /// <inheritdoc />
    public partial class RemoveItemsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Author",
                table: "CartItems",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "CartItems",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Author",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "CartItems");
        }
    }
}
