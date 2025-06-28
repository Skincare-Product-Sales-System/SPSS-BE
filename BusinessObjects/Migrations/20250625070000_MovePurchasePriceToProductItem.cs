using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObjects.Migrations
{
    /// <inheritdoc />
    public partial class MovePurchasePriceToProductItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add PurchasePrice column to ProductItems table
            migrationBuilder.AddColumn<decimal>(
                name: "PurchasePrice",
                table: "ProductItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            // Update PurchasePrice in ProductItems based on the Product's PurchasePrice
            migrationBuilder.Sql(@"
                UPDATE pi
                SET pi.PurchasePrice = p.PurchasePrice
                FROM ProductItems pi
                INNER JOIN Products p ON pi.ProductId = p.Id
            ");

            // Remove PurchasePrice column from Products table
            migrationBuilder.DropColumn(
                name: "PurchasePrice",
                table: "Products");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Add PurchasePrice column back to Products table
            migrationBuilder.AddColumn<decimal>(
                name: "PurchasePrice",
                table: "Products",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            // Update PurchasePrice in Products based on the average PurchasePrice of ProductItems
            migrationBuilder.Sql(@"
                UPDATE p
                SET p.PurchasePrice = (
                    SELECT AVG(pi.PurchasePrice)
                    FROM ProductItems pi
                    WHERE pi.ProductId = p.Id
                )
                FROM Products p
            ");

            // Remove PurchasePrice column from ProductItems table
            migrationBuilder.DropColumn(
                name: "PurchasePrice",
                table: "ProductItems");
        }
    }
}