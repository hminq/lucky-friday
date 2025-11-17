using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuckyFridayCalculator.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFridayTotalDepositConstraintTo8100000 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop existing constraint
            migrationBuilder.Sql("ALTER TABLE [Fridays] DROP CONSTRAINT [CK_Friday_TotalDeposit];");
            
            // Add new constraint with updated limit
            migrationBuilder.Sql("ALTER TABLE [Fridays] ADD CONSTRAINT [CK_Friday_TotalDeposit] CHECK ([TotalDeposit] > 0 AND [TotalDeposit] < 8100000);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop new constraint
            migrationBuilder.Sql("ALTER TABLE [Fridays] DROP CONSTRAINT [CK_Friday_TotalDeposit];");
            
            // Restore old constraint
            migrationBuilder.Sql("ALTER TABLE [Fridays] ADD CONSTRAINT [CK_Friday_TotalDeposit] CHECK ([TotalDeposit] > 0 AND [TotalDeposit] < 8000000);");
        }
    }
}
