using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyAccounts.Migrations
{
    /// <inheritdoc />
    public partial class BudgetUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "BudgetCategory",
                table: "Category",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "BudgetMonth",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "BudgetExpense",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "BudgetAccount",
                table: "Account",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "Account",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BudgetCategory",
                table: "Category");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "BudgetExpense");

            migrationBuilder.DropColumn(
                name: "BudgetAccount",
                table: "Account");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "Account");

            migrationBuilder.AlterColumn<int>(
                name: "Name",
                table: "BudgetMonth",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");
        }
    }
}
