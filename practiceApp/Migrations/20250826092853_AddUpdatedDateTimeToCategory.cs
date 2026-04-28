using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace practiceApp.Migrations
{
    /// <inheritdoc />
    public partial class AddUpdatedDateTimeToCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDateTime",
                table: "categoryModels",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedDateTime",
                table: "categoryModels");
        }
    }
}
