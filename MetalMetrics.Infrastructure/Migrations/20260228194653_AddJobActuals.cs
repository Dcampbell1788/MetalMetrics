using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetalMetrics.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJobActuals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobActuals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    JobId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ActualLaborHours = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LaborRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ActualMaterialCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ActualMachineHours = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MachineRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OverheadPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    TotalActualCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ActualRevenue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    EnteredBy = table.Column<string>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobActuals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobActuals_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobActuals_JobId",
                table: "JobActuals",
                column: "JobId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobActuals");
        }
    }
}
