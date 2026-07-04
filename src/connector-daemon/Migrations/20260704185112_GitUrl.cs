using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace connector_daemon.Migrations
{
    /// <inheritdoc />
    public partial class GitUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GitUrl",
                table: "JobEvents",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GitUrl",
                table: "JobEvents");
        }
    }
}
