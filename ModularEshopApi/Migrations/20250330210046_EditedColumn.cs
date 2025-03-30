using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModularEshopApi.Migrations
{
    /// <inheritdoc />
    public partial class EditedColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Password",
                table: "Users",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "Images",
                table: "Products",
                newName: "ImagePaths");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "Users",
                newName: "Password");

            migrationBuilder.RenameColumn(
                name: "ImagePaths",
                table: "Products",
                newName: "Images");
        }
    }
}
