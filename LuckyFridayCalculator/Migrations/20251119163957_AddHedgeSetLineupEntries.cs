using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuckyFridayCalculator.Migrations
{
    /// <inheritdoc />
    public partial class AddHedgeSetLineupEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HedgeSetLineupEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HedgeSetId = table.Column<int>(type: "int", nullable: false),
                    MemberId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HedgeSetLineupEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HedgeSetLineupEntries_HedgeSets_HedgeSetId",
                        column: x => x.HedgeSetId,
                        principalTable: "HedgeSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HedgeSetLineupEntries_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HedgeSetLineupEntries_HedgeSetId",
                table: "HedgeSetLineupEntries",
                column: "HedgeSetId");

            migrationBuilder.CreateIndex(
                name: "IX_HedgeSetLineupEntries_MemberId",
                table: "HedgeSetLineupEntries",
                column: "MemberId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HedgeSetLineupEntries");
        }
    }
}
