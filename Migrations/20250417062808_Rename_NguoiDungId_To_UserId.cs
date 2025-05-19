using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TuyenDungFPT.Migrations
{
    /// <inheritdoc />
    public partial class Rename_NguoiDungId_To_UserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BinhLuanBaiViet_Users_UserId",
                table: "BinhLuanBaiViet");

            migrationBuilder.DropColumn(
                name: "NguoiDungId",
                table: "BinhLuanBaiViet");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "BinhLuanBaiViet",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BinhLuanBaiViet_Users_UserId",
                table: "BinhLuanBaiViet",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BinhLuanBaiViet_Users_UserId",
                table: "BinhLuanBaiViet");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "BinhLuanBaiViet",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "NguoiDungId",
                table: "BinhLuanBaiViet",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_BinhLuanBaiViet_Users_UserId",
                table: "BinhLuanBaiViet",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
