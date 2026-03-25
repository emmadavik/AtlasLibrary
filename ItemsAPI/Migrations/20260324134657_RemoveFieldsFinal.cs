using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ItemsAPI.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFieldsFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Model",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Items");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "Items",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "Items",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
