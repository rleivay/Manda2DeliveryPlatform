using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Manda2.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixDriverRejectionReasonsSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "DriverRejectionReasons",
                newName: "DriverRejectionReasons",
                newSchema: "dsp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "DriverRejectionReasons",
                schema: "dsp",
                newName: "DriverRejectionReasons");
        }
    }
}
