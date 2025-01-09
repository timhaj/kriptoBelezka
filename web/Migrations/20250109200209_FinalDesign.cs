﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace web.Migrations
{
    /// <inheritdoc />
    public partial class FinalDesign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "Transakcija",
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

            migrationBuilder.AddColumn<string>(
                name: "ApiKey",
                table: "Nastavitve",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Asset",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(30,12)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApiKey",
                table: "Nastavitve");

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "Transakcija",
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
                name: "Price",
                table: "Asset",
                type: "decimal(30,12)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }
    }
}
