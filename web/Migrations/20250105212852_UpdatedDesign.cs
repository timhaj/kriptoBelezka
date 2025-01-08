using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace web.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedDesign : Migration
    {
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Asset",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(30,12)");
            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Transakcija",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(30,12)");
            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "Transakcija",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(30,12)");
        }

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Asset",
                type: "decimal(30,12)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Transakcija",
                type: "decimal(30,12)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "Transakcija",
                type: "decimal(30,12)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }
    }
}
