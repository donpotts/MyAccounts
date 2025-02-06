using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyAccounts.Migrations
{
    /// <inheritdoc />
    public partial class Budget : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BudgetAccount",
                columns: table => new
                {
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountName = table.Column<string>(type: "TEXT", nullable: false),
                    APR = table.Column<decimal>(type: "TEXT", nullable: true),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreditLimit = table.Column<decimal>(type: "TEXT", nullable: true),
                    MinPayment = table.Column<decimal>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetAccount", x => x.AccountId);
                });

            migrationBuilder.CreateTable(
                name: "BudgetMiscellanousExpense",
                columns: table => new
                {
                    ExpenseId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ExpenseType = table.Column<string>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    ExpenseDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetMiscellanousExpense", x => x.ExpenseId);
                });

            migrationBuilder.CreateTable(
                name: "TransacBudgetIncometion",
                columns: table => new
                {
                    IncomeId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Source = table.Column<string>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    IncomeDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransacBudgetIncometion", x => x.IncomeId);
                });

            migrationBuilder.CreateTable(
                name: "BudgetMonthlyBalance",
                columns: table => new
                {
                    BalanceId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    Month = table.Column<int>(type: "INTEGER", nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    Interest = table.Column<decimal>(type: "TEXT", nullable: false),
                    Balance = table.Column<decimal>(type: "TEXT", nullable: false),
                    Payment = table.Column<decimal>(type: "TEXT", nullable: false),
                    BudgetAccountAccountId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetMonthlyBalance", x => x.BalanceId);
                    table.ForeignKey(
                        name: "FK_BudgetMonthlyBalance_BudgetAccount_BudgetAccountAccountId",
                        column: x => x.BudgetAccountAccountId,
                        principalTable: "BudgetAccount",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BudgetTransaction",
                columns: table => new
                {
                    TransactionId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TransactionType = table.Column<string>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    BudgetAccountAccountId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetTransaction", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_BudgetTransaction_BudgetAccount_BudgetAccountAccountId",
                        column: x => x.BudgetAccountAccountId,
                        principalTable: "BudgetAccount",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BudgetMonthlyBalance_BudgetAccountAccountId",
                table: "BudgetMonthlyBalance",
                column: "BudgetAccountAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetTransaction_BudgetAccountAccountId",
                table: "BudgetTransaction",
                column: "BudgetAccountAccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BudgetMiscellanousExpense");

            migrationBuilder.DropTable(
                name: "BudgetMonthlyBalance");

            migrationBuilder.DropTable(
                name: "BudgetTransaction");

            migrationBuilder.DropTable(
                name: "TransacBudgetIncometion");

            migrationBuilder.DropTable(
                name: "BudgetAccount");
        }
    }
}
