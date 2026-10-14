using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dobby.Migrations
{
    /// <inheritdoc />
    public partial class RemoveShortUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_url_items_short_url",
                table: "url_items");

            migrationBuilder.DropColumn(
                name: "short_url",
                table: "url_items");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "short_url",
                table: "url_items",
                type: "character(6)",
                fixedLength: true,
                maxLength: 6,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_url_items_short_url",
                table: "url_items",
                column: "short_url",
                unique: true);
        }
    }
}
