using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace connector_daemon.Migrations
{
    /// <inheritdoc />
    public partial class AdditionalTimestamps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EnqueuedAt",
                table: "JobEvents",
                type: "TEXT",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FinishedAt",
                table: "JobEvents",
                type: "TEXT",
                maxLength: 40,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnqueuedAt",
                table: "JobEvents");

            migrationBuilder.DropColumn(
                name: "FinishedAt",
                table: "JobEvents");
        }
    }
}
