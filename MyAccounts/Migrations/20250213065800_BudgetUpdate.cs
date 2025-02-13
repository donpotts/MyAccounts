using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyAccounts.Migrations
{
    /// <inheritdoc />
    public partial class BudgetUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BudgetMonthlyBalance_BudgetAccount_BudgetAccountAccountId",
                table: "BudgetMonthlyBalance");

            migrationBuilder.DropForeignKey(
                name: "FK_BudgetTransaction_BudgetAccount_BudgetAccountAccountId",
                table: "BudgetTransaction");

            migrationBuilder.DropColumn(
                name: "APR",
                table: "BudgetAccount");

            migrationBuilder.RenameColumn(
                name: "BudgetAccountAccountId",
                table: "BudgetTransaction",
                newName: "BudgetAccountId");

            migrationBuilder.RenameIndex(
                name: "IX_BudgetTransaction_BudgetAccountAccountId",
                table: "BudgetTransaction",
                newName: "IX_BudgetTransaction_BudgetAccountId");

            migrationBuilder.RenameColumn(
                name: "BudgetAccountAccountId",
                table: "BudgetMonthlyBalance",
                newName: "BudgetAccountId");

            migrationBuilder.RenameIndex(
                name: "IX_BudgetMonthlyBalance_BudgetAccountAccountId",
                table: "BudgetMonthlyBalance",
                newName: "IX_BudgetMonthlyBalance_BudgetAccountId");

            migrationBuilder.RenameColumn(
                name: "AccountId",
                table: "BudgetAccount",
                newName: "Id");

            migrationBuilder.AlterColumn<double>(
                name: "MinPayment",
                table: "BudgetAccount",
                type: "REAL",
                precision: 19,
                scale: 4,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AccountName",
                table: "BudgetAccount",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetMonthlyBalance_BudgetAccount_BudgetAccountId",
                table: "BudgetMonthlyBalance",
                column: "BudgetAccountId",
                principalTable: "BudgetAccount",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetTransaction_BudgetAccount_BudgetAccountId",
                table: "BudgetTransaction",
                column: "BudgetAccountId",
                principalTable: "BudgetAccount",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BudgetMonthlyBalance_BudgetAccount_BudgetAccountId",
                table: "BudgetMonthlyBalance");

            migrationBuilder.DropForeignKey(
                name: "FK_BudgetTransaction_BudgetAccount_BudgetAccountId",
                table: "BudgetTransaction");

            migrationBuilder.RenameColumn(
                name: "BudgetAccountId",
                table: "BudgetTransaction",
                newName: "BudgetAccountAccountId");

            migrationBuilder.RenameIndex(
                name: "IX_BudgetTransaction_BudgetAccountId",
                table: "BudgetTransaction",
                newName: "IX_BudgetTransaction_BudgetAccountAccountId");

            migrationBuilder.RenameColumn(
                name: "BudgetAccountId",
                table: "BudgetMonthlyBalance",
                newName: "BudgetAccountAccountId");

            migrationBuilder.RenameIndex(
                name: "IX_BudgetMonthlyBalance_BudgetAccountId",
                table: "BudgetMonthlyBalance",
                newName: "IX_BudgetMonthlyBalance_BudgetAccountAccountId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "BudgetAccount",
                newName: "AccountId");

            migrationBuilder.AlterColumn<decimal>(
                name: "MinPayment",
                table: "BudgetAccount",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "REAL",
                oldPrecision: 19,
                oldScale: 4,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AccountName",
                table: "BudgetAccount",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "APR",
                table: "BudgetAccount",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetMonthlyBalance_BudgetAccount_BudgetAccountAccountId",
                table: "BudgetMonthlyBalance",
                column: "BudgetAccountAccountId",
                principalTable: "BudgetAccount",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetTransaction_BudgetAccount_BudgetAccountAccountId",
                table: "BudgetTransaction",
                column: "BudgetAccountAccountId",
                principalTable: "BudgetAccount",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
