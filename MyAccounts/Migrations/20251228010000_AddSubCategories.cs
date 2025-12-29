using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyAccounts.Migrations
{
    /// <inheritdoc />
    public partial class AddSubCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add ParentCategoryId column
            migrationBuilder.AddColumn<long>(
                name: "ParentCategoryId",
                table: "Category",
                type: "bigint",
                nullable: true);

            // Drop the old unique index on Name
            migrationBuilder.DropIndex(
                name: "IX_Category_Name",
                table: "Category");

            // Create new composite unique index on (ParentCategoryId, Name)
            migrationBuilder.CreateIndex(
                name: "IX_Category_ParentCategoryId_Name",
                table: "Category",
                columns: new[] { "ParentCategoryId", "Name" },
                unique: true);

            // Create index for ParentCategoryId foreign key
            migrationBuilder.CreateIndex(
                name: "IX_Category_ParentCategoryId",
                table: "Category",
                column: "ParentCategoryId");

            // Add foreign key constraint
            migrationBuilder.AddForeignKey(
                name: "FK_Category_Category_ParentCategoryId",
                table: "Category",
                column: "ParentCategoryId",
                principalTable: "Category",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove foreign key constraint
            migrationBuilder.DropForeignKey(
                name: "FK_Category_Category_ParentCategoryId",
                table: "Category");

            // Drop the composite unique index
            migrationBuilder.DropIndex(
                name: "IX_Category_ParentCategoryId_Name",
                table: "Category");

            // Drop the ParentCategoryId index
            migrationBuilder.DropIndex(
                name: "IX_Category_ParentCategoryId",
                table: "Category");

            // Recreate the original unique index on Name
            migrationBuilder.CreateIndex(
                name: "IX_Category_Name",
                table: "Category",
                column: "Name",
                unique: true);

            // Remove ParentCategoryId column
            migrationBuilder.DropColumn(
                name: "ParentCategoryId",
                table: "Category");
        }
    }
}
