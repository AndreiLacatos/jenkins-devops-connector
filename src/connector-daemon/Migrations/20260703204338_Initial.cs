using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace connector_daemon.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Build = table.Column<int>(type: "INTEGER", nullable: false),
                    Commit = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    JobEvent = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    Url = table.Column<string>(type: "TEXT", maxLength: 40, nullable: true),
                    RegisteredAt = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    SyncStatus = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobEvents", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobEvents");
        }
    }
}
