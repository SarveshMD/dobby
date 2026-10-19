using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dobby.Migrations
{
    /// <inheritdoc />
    public partial class CreateUserModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "user_id",
                table: "url_items",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_url_items_user_id",
                table: "url_items",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_url_items_users_user_id",
                table: "url_items",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_url_items_users_user_id",
                table: "url_items");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropIndex(
                name: "ix_url_items_user_id",
                table: "url_items");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "url_items");
        }
    }
}
