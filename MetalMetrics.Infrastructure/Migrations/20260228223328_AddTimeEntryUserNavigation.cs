using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetalMetrics.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeEntryUserNavigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_JobTimeEntries_UserId",
                table: "JobTimeEntries",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobTimeEntries_AspNetUsers_UserId",
                table: "JobTimeEntries",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobTimeEntries_AspNetUsers_UserId",
                table: "JobTimeEntries");

            migrationBuilder.DropIndex(
                name: "IX_JobTimeEntries_UserId",
                table: "JobTimeEntries");
        }
    }
}
