using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace practiceApp.Migrations
{
    /// <inheritdoc />
    public partial class AddRestoreCountToCategoryModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RestoreCount",
                table: "categoryModels",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RestoreCount",
                table: "categoryModels");
        }
    }
}
