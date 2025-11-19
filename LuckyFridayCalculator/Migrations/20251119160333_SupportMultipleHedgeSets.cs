using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuckyFridayCalculator.Migrations
{
    /// <inheritdoc />
    public partial class SupportMultipleHedgeSets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_HedgeSets_FridayId",
                table: "HedgeSets");

            migrationBuilder.CreateIndex(
                name: "IX_HedgeSets_FridayId",
                table: "HedgeSets",
                column: "FridayId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_HedgeSets_FridayId",
                table: "HedgeSets");

            migrationBuilder.CreateIndex(
                name: "IX_HedgeSets_FridayId",
                table: "HedgeSets",
                column: "FridayId",
                unique: true);
        }
    }
}
