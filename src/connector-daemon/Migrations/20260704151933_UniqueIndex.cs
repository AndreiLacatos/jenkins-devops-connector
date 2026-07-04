using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace connector_daemon.Migrations
{
    /// <inheritdoc />
    public partial class UniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_JobEvents_Name_Build_Commit_JobEvent",
                table: "JobEvents",
                columns: new[] { "Name", "Build", "Commit", "JobEvent" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JobEvents_Name_Build_Commit_JobEvent",
                table: "JobEvents");
        }
    }
}
