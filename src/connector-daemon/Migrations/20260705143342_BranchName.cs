using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace connector_daemon.Migrations
{
    /// <inheritdoc />
    public partial class BranchName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Branch",
                table: "JobEvents",
                type: "TEXT",
                maxLength: 400,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Branch",
                table: "JobEvents");
        }
    }
}
