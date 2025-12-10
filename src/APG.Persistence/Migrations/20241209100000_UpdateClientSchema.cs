using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APG.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateClientSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check if Sector column exists and drop it (handled by SQL migration 003)
            // This migration assumes SQL migrations 003-008 have been applied

            // Add SectorId if not exists (from migration 003)
            migrationBuilder.AddColumn<int>(
                name: "SectorId",
                table: "Clients",
                type: "int",
                nullable: true);

            // Make financial fields nullable (from migration 004)
            migrationBuilder.AlterColumn<decimal>(
                name: "DefaultTargetMarginPercent",
                table: "Clients",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 5,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "DefaultMinimumMarginPercent",
                table: "Clients",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 5,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "DiscountPercent",
                table: "Clients",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 5,
                oldScale: 2);

            migrationBuilder.AlterColumn<int>(
                name: "ForcedVacationDaysPerYear",
                table: "Clients",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            // Add TargetHourlyRate (from migration 008)
            migrationBuilder.AddColumn<decimal>(
                name: "TargetHourlyRate",
                table: "Clients",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            // Add foreign key for SectorId
            migrationBuilder.CreateIndex(
                name: "IX_Clients_SectorId",
                table: "Clients",
                column: "SectorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Sectors_SectorId",
                table: "Clients",
                column: "SectorId",
                principalTable: "Sectors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove foreign key
            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Sectors_SectorId",
                table: "Clients");

            // Remove index
            migrationBuilder.DropIndex(
                name: "IX_Clients_SectorId",
                table: "Clients");

            // Remove TargetHourlyRate
            migrationBuilder.DropColumn(
                name: "TargetHourlyRate",
                table: "Clients");

            // Remove SectorId
            migrationBuilder.DropColumn(
                name: "SectorId",
                table: "Clients");

            // Revert financial fields to non-nullable
            migrationBuilder.AlterColumn<decimal>(
                name: "DefaultTargetMarginPercent",
                table: "Clients",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 5,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "DefaultMinimumMarginPercent",
                table: "Clients",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 5,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "DiscountPercent",
                table: "Clients",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 5,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ForcedVacationDaysPerYear",
                table: "Clients",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            // Add back Sector string column
            migrationBuilder.AddColumn<string>(
                name: "Sector",
                table: "Clients",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }
    }
}
