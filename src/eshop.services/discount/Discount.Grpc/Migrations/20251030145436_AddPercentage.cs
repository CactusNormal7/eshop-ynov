using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Discount.Grpc.Migrations
{
    /// <inheritdoc />
    public partial class AddPercentage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Percentage",
                table: "Coupon",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.UpdateData(
                table: "Coupon",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Amount", "Percentage" },
                values: new object[] { 0.0, 30.0 });

            migrationBuilder.UpdateData(
                table: "Coupon",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Amount", "Percentage" },
                values: new object[] { 0.0, 50.0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Percentage",
                table: "Coupon");

            migrationBuilder.UpdateData(
                table: "Coupon",
                keyColumn: "Id",
                keyValue: 1,
                column: "Amount",
                value: 150.0);

            migrationBuilder.UpdateData(
                table: "Coupon",
                keyColumn: "Id",
                keyValue: 2,
                column: "Amount",
                value: 100.0);
        }
    }
}
