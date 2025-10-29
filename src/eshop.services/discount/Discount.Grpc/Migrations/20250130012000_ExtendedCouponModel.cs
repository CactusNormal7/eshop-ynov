using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Discount.Grpc.Migrations
{
    /// <inheritdoc />
    public partial class ExtendedCouponModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Coupon",
                newName: "Coupons");

            migrationBuilder.AlterColumn<string>(
                name: "ProductName",
                table: "Coupons",
                type: "TEXT",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Coupons",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Coupons",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DiscountType",
                table: "Coupons",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "Percentage",
                table: "Coupons",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "MinimumAmount",
                table: "Coupons",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Coupons",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Coupons",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Coupons",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsStackable",
                table: "Coupons",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<double>(
                name: "MaxStackablePercentage",
                table: "Coupons",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RemainingUses",
                table: "Coupons",
                type: "INTEGER",
                nullable: false,
                defaultValue: -1);

            migrationBuilder.AddColumn<bool>(
                name: "IsAutomatic",
                table: "Coupons",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Coupons",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 1, 30, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Coupons",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_ProductName",
                table: "Coupons",
                column: "ProductName");

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_Category",
                table: "Coupons",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_Code",
                table: "Coupons",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_Status",
                table: "Coupons",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_Status_StartDate_EndDate",
                table: "Coupons",
                columns: new[] { "Status", "StartDate", "EndDate" });

            // Mise à jour des données existantes
            migrationBuilder.Sql(@"
                UPDATE Coupons 
                SET DiscountType = 0, 
                    Status = 0, 
                    IsStackable = 1, 
                    RemainingUses = -1, 
                    IsAutomatic = 1,
                    CreatedAt = datetime('now')
                WHERE DiscountType IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Coupons_Status_StartDate_EndDate",
                table: "Coupons");

            migrationBuilder.DropIndex(
                name: "IX_Coupons_Status",
                table: "Coupons");

            migrationBuilder.DropIndex(
                name: "IX_Coupons_Code",
                table: "Coupons");

            migrationBuilder.DropIndex(
                name: "IX_Coupons_Category",
                table: "Coupons");

            migrationBuilder.DropIndex(
                name: "IX_Coupons_ProductName",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "IsAutomatic",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "RemainingUses",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "MaxStackablePercentage",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "IsStackable",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "MinimumAmount",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "Percentage",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "DiscountType",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Coupons");

            migrationBuilder.AlterColumn<string>(
                name: "ProductName",
                table: "Coupons",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.RenameTable(
                name: "Coupons",
                newName: "Coupon");
        }
    }
}

