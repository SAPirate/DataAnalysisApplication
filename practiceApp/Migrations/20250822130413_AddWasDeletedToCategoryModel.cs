using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace practiceApp.Migrations
{
    /// <inheritdoc />
    public partial class AddWasDeletedToCategoryModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "WasDeleted",
                table: "categoryModels",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WasDeleted",
                table: "categoryModels");
        }
    }
}
