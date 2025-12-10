using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APG.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Resources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    BusinessUnitId = table.Column<int>(type: "int", nullable: false),
                    JobType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Seniority = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CurrentClient = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CurrentMission = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DailyCostRate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DailySellRate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MarginRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    HireDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Manager = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Resources_BusinessUnits_BusinessUnitId",
                        column: x => x.BusinessUnitId,
                        principalTable: "BusinessUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectResources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    ResourceId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ResourceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DailyCostRate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DailySellRate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GrossMarginAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GrossMarginPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    NetMarginPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    BilledDays = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectResources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectResources_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectResources_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResourceHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResourceId = table.Column<int>(type: "int", nullable: false),
                    ChangeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ChangeDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    OldValuesJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    NewValuesJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    OldDailyCostRate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    NewDailyCostRate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    OldDailySellRate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    NewDailySellRate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    OldMarginRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    NewMarginRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    ImpactNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ChangedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResourceHistories_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectResources_ProjectId",
                table: "ProjectResources",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectResources_ProjectId_ResourceId",
                table: "ProjectResources",
                columns: new[] { "ProjectId", "ResourceId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectResources_ResourceId",
                table: "ProjectResources",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectResources_Status",
                table: "ProjectResources",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceHistories_ChangedAt",
                table: "ResourceHistories",
                column: "ChangedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceHistories_ChangeType",
                table: "ResourceHistories",
                column: "ChangeType");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceHistories_ResourceId",
                table: "ResourceHistories",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_BusinessUnitId",
                table: "Resources",
                column: "BusinessUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_Email",
                table: "Resources",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Resources_Name",
                table: "Resources",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_Status",
                table: "Resources",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectResources");

            migrationBuilder.DropTable(
                name: "ResourceHistories");

            migrationBuilder.DropTable(
                name: "Resources");
        }
    }
}
