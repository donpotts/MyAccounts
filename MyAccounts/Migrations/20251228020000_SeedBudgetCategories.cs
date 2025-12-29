using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyAccounts.Migrations
{
    /// <inheritdoc />
    public partial class SeedBudgetCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert parent categories first
            var parentCategories = new (string Name, bool BudgetCategory)[]
            {
                ("Business", true),
                ("Cash", true),
                ("Food", true),
                ("Gifts", true),
                ("Hobbies", true),
                ("Housing", true),
                ("Medical", true),
                ("Personal Care", true),
                ("Shopping", true),
                ("Travel", true),
                ("Utilities", true),
                ("Tax", true)
            };

            foreach (var cat in parentCategories)
            {
                migrationBuilder.Sql($@"
                    INSERT INTO ""Category"" (""Name"", ""BudgetCategory"", ""ParentCategoryId"")
                    SELECT '{cat.Name}', {cat.BudgetCategory.ToString().ToLower()}, NULL
                    WHERE NOT EXISTS (SELECT 1 FROM ""Category"" WHERE ""Name"" = '{cat.Name}' AND ""ParentCategoryId"" IS NULL)
                ");
            }

            // Insert subcategories
            var subcategories = new (string Parent, string Name)[]
            {
                // Business
                ("Business", "Advertising"),
                ("Business", "Education"),
                ("Business", "Equipment"),
                ("Business", "Supplies"),
                ("Business", "Utilities"),

                // Cash
                ("Cash", "ATM Withdrawal"),
                ("Cash", "Cash Deposit"),
                ("Cash", "Cash Withdrawal"),

                // Food
                ("Food", "Dining Out"),
                ("Food", "Fast Food"),
                ("Food", "Groceries"),

                // Gifts
                ("Gifts", "Baby"),
                ("Gifts", "Birthday"),
                ("Gifts", "Cards"),
                ("Gifts", "Christmas"),
                ("Gifts", "Graduation"),
                ("Gifts", "Wrapping Paper"),
                ("Gifts", "Wedding"),

                // Hobbies
                ("Hobbies", "Card Making"),
                ("Hobbies", "Embroidery"),
                ("Hobbies", "Miscellaneous"),
                ("Hobbies", "Sewing"),

                // Housing
                ("Housing", "Insurance"),
                ("Housing", "Mortgage"),
                ("Housing", "Rent"),

                // Medical
                ("Medical", "Dental"),
                ("Medical", "Doctor"),
                ("Medical", "Prescription"),
                ("Medical", "Vision"),

                // Personal Care
                ("Personal Care", "Lotion"),
                ("Personal Care", "OTC Medications"),
                ("Personal Care", "Shampoo & Conditioner"),
                ("Personal Care", "Soap"),
                ("Personal Care", "Toothbrush & Toothpaste"),

                // Shopping
                ("Shopping", "Cleaning Supplies"),
                ("Shopping", "Clothing"),
                ("Shopping", "Electronics"),
                ("Shopping", "Kitchen Supplies"),
                ("Shopping", "Laundry Supplies"),
                ("Shopping", "Linens"),
                ("Shopping", "Stationary"),

                // Travel
                ("Travel", "Airline Tickets"),
                ("Travel", "Bus Tickets"),
                ("Travel", "Car Rental"),
                ("Travel", "Cruise Tickets"),
                ("Travel", "Hotel"),

                // Utilities
                ("Utilities", "Cable"),
                ("Utilities", "Cellular Service"),
                ("Utilities", "Electric"),
                ("Utilities", "Gas"),
                ("Utilities", "Trash"),
                ("Utilities", "Sewer"),
                ("Utilities", "Streaming Service"),
                ("Utilities", "Water"),

                // Tax
                ("Tax", "Income"),
                ("Tax", "Property"),
                ("Tax", "Sales")
            };

            foreach (var subcat in subcategories)
            {
                migrationBuilder.Sql($@"
                    INSERT INTO ""Category"" (""Name"", ""BudgetCategory"", ""ParentCategoryId"")
                    SELECT '{subcat.Name}', true, p.""Id""
                    FROM ""Category"" p
                    WHERE p.""Name"" = '{subcat.Parent}' AND p.""ParentCategoryId"" IS NULL
                    AND NOT EXISTS (
                        SELECT 1 FROM ""Category"" c
                        WHERE c.""Name"" = '{subcat.Name}'
                        AND c.""ParentCategoryId"" = p.""Id""
                    )
                ");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Delete subcategories first
            migrationBuilder.Sql(@"
                DELETE FROM ""Category"" WHERE ""ParentCategoryId"" IS NOT NULL
            ");

            // Delete parent categories
            var parentNames = new[] { "Business", "Cash", "Food", "Gifts", "Hobbies", "Housing",
                                       "Medical", "Personal Care", "Shopping", "Travel", "Utilities", "Tax" };
            foreach (var name in parentNames)
            {
                migrationBuilder.Sql($@"
                    DELETE FROM ""Category"" WHERE ""Name"" = '{name}' AND ""ParentCategoryId"" IS NULL
                ");
            }
        }
    }
}
