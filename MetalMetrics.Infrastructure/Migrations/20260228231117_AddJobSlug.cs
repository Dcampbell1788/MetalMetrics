using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetalMetrics.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJobSlug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Jobs",
                type: "TEXT",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_TenantId_Slug",
                table: "Jobs",
                columns: new[] { "TenantId", "Slug" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Jobs_TenantId_Slug",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Jobs");
        }
    }
}
