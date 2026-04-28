using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace practiceApp.Migrations
{
    /// <inheritdoc />
    public partial class AddDeletedDateTimeToCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDateTime",
                table: "categoryModels",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedDateTime",
                table: "categoryModels");
        }
    }
}
