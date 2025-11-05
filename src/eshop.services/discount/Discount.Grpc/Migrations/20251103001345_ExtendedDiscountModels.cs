using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Discount.Grpc.Migrations
{
    /// <inheritdoc />
    public partial class ExtendedDiscountModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicableCategories",
                table: "Coupon",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Coupon",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "MinimumPurchaseAmount",
                table: "Coupon",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<Guid>(
                name: "ProductId",
                table: "Coupon",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Coupon",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Coupon",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ApplicableCategories",
                table: "Codes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AutomaticType",
                table: "Codes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodeValue",
                table: "Codes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Codes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Codes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAutomatic",
                table: "Codes",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsStackable",
                table: "Codes",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "MaxCumulativeDiscountPercentage",
                table: "Codes",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "MinimumPurchaseAmount",
                table: "Codes",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Codes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Codes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TierRules",
                table: "Codes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Coupon",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApplicableCategories", "EndDate", "MinimumPurchaseAmount", "ProductId", "StartDate", "Status" },
                values: new object[] { null, null, 0.0, null, null, "Active" });

            migrationBuilder.UpdateData(
                table: "Coupon",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApplicableCategories", "EndDate", "MinimumPurchaseAmount", "ProductId", "StartDate", "Status" },
                values: new object[] { null, null, 0.0, null, null, "Active" });

            migrationBuilder.CreateIndex(
                name: "IX_Codes_CodeValue",
                table: "Codes",
                column: "CodeValue",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Codes_CodeValue",
                table: "Codes");

            migrationBuilder.DropColumn(
                name: "ApplicableCategories",
                table: "Coupon");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Coupon");

            migrationBuilder.DropColumn(
                name: "MinimumPurchaseAmount",
                table: "Coupon");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "Coupon");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Coupon");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Coupon");

            migrationBuilder.DropColumn(
                name: "ApplicableCategories",
                table: "Codes");

            migrationBuilder.DropColumn(
                name: "AutomaticType",
                table: "Codes");

            migrationBuilder.DropColumn(
                name: "CodeValue",
                table: "Codes");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Codes");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Codes");

            migrationBuilder.DropColumn(
                name: "IsAutomatic",
                table: "Codes");

            migrationBuilder.DropColumn(
                name: "IsStackable",
                table: "Codes");

            migrationBuilder.DropColumn(
                name: "MaxCumulativeDiscountPercentage",
                table: "Codes");

            migrationBuilder.DropColumn(
                name: "MinimumPurchaseAmount",
                table: "Codes");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Codes");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Codes");

            migrationBuilder.DropColumn(
                name: "TierRules",
                table: "Codes");
        }
    }
}
