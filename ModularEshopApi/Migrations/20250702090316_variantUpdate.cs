using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModularEshopApi.Migrations
{
    /// <inheritdoc />
    public partial class variantUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Variants",
                table: "Products");

            migrationBuilder.CreateTable(
                name: "Variant",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Price = table.Column<decimal>(type: "TEXT", nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", nullable: true),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Variant", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Variant_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Variant_ProductId",
                table: "Variant",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Variant");

            migrationBuilder.AddColumn<string>(
                name: "Variants",
                table: "Products",
                type: "TEXT",
                nullable: true);
        }
    }
}
