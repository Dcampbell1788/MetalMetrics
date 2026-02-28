using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetalMetrics.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJobAndJobEstimate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    JobNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CustomerName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Jobs_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JobEstimates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    JobId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EstimatedLaborHours = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LaborRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EstimatedMaterialCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EstimatedMachineHours = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MachineRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OverheadPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    TotalEstimatedCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    QuotePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EstimatedMarginPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    AIGenerated = table.Column<bool>(type: "INTEGER", nullable: false),
                    AIPromptSnapshot = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobEstimates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobEstimates_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobEstimates_JobId",
                table: "JobEstimates",
                column: "JobId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_TenantId_JobNumber",
                table: "Jobs",
                columns: new[] { "TenantId", "JobNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobEstimates");

            migrationBuilder.DropTable(
                name: "Jobs");
        }
    }
}
