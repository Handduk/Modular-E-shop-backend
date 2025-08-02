using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModularEshopApi.Migrations
{
    /// <inheritdoc />
    public partial class variantUpdate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Variant_Products_ProductId",
                table: "Variant");

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "Variant",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Variant_Products_ProductId",
                table: "Variant",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Variant_Products_ProductId",
                table: "Variant");

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "Variant",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_Variant_Products_ProductId",
                table: "Variant",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");
        }
    }
}
