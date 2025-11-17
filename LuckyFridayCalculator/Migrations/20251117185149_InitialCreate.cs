using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LuckyFridayCalculator.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Fridays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    BetDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalOddsHongKong = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TotalOddsInternational = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TotalDeposit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Dog = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fridays", x => x.Id);
                    table.CheckConstraint("CK_Friday_TotalDeposit", "[TotalDeposit] > 0 AND [TotalDeposit] < 8100000");
                    table.ForeignKey(
                        name: "FK_Fridays_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HedgeSets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FridayId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Budget = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HedgeSets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HedgeSets_Fridays_FridayId",
                        column: x => x.FridayId,
                        principalTable: "Fridays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LineupEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MemberId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FridayId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LineupEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LineupEntries_Fridays_FridayId",
                        column: x => x.FridayId,
                        principalTable: "Fridays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LineupEntries_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SingleBets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    MatchStartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MatchEndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OddsHongKong = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    OddsInternational = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    FridayId = table.Column<int>(type: "int", nullable: true),
                    HedgeSetId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SingleBets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SingleBets_Fridays_FridayId",
                        column: x => x.FridayId,
                        principalTable: "Fridays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SingleBets_HedgeSets_HedgeSetId",
                        column: x => x.HedgeSetId,
                        principalTable: "HedgeSets",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Accounts",
                columns: new[] { "Id", "Title" },
                values: new object[,]
                {
                    { 1, "Account 1" },
                    { 2, "Account 2" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Fridays_AccountId",
                table: "Fridays",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_HedgeSets_FridayId",
                table: "HedgeSets",
                column: "FridayId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LineupEntries_FridayId",
                table: "LineupEntries",
                column: "FridayId");

            migrationBuilder.CreateIndex(
                name: "IX_LineupEntries_MemberId",
                table: "LineupEntries",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_SingleBets_FridayId",
                table: "SingleBets",
                column: "FridayId");

            migrationBuilder.CreateIndex(
                name: "IX_SingleBets_HedgeSetId",
                table: "SingleBets",
                column: "HedgeSetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LineupEntries");

            migrationBuilder.DropTable(
                name: "SingleBets");

            migrationBuilder.DropTable(
                name: "Members");

            migrationBuilder.DropTable(
                name: "HedgeSets");

            migrationBuilder.DropTable(
                name: "Fridays");

            migrationBuilder.DropTable(
                name: "Accounts");
        }
    }
}
