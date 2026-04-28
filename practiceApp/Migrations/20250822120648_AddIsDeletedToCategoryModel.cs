using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace practiceApp.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDeletedToCategoryModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "categoryModels",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "categoryModels");
        }
    }
}
