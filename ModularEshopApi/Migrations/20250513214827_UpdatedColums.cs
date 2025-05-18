using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModularEshopApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedColums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImagePaths",
                table: "Products",
                newName: "Images");

            migrationBuilder.RenameColumn(
                name: "ImagePaths",
                table: "Categorys",
                newName: "Image");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Images",
                table: "Products",
                newName: "ImagePaths");

            migrationBuilder.RenameColumn(
                name: "Image",
                table: "Categorys",
                newName: "ImagePaths");
        }
    }
}
