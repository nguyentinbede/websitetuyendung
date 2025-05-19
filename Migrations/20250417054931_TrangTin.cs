using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TuyenDungFPT.Migrations
{
    /// <inheritdoc />
    public partial class TrangTin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChuDe",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenChuDe = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TenChuDeKhongDau = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChuDe", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BaiViet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChuDeId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TieuDe = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TieuDeKhongDau = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TomTat = table.Column<string>(type: "ntext", nullable: false),
                    NoiDung = table.Column<string>(type: "ntext", nullable: false),
                    NgayDang = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LuotXem = table.Column<int>(type: "int", nullable: false),
                    KiemDuyet = table.Column<bool>(type: "bit", nullable: false),
                    HienThi = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaiViet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BaiViet_ChuDe_ChuDeId",
                        column: x => x.ChuDeId,
                        principalTable: "ChuDe",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BaiViet_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BinhLuanBaiViet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BaiVietId = table.Column<int>(type: "int", nullable: false),
                    NguoiDungId = table.Column<int>(type: "int", nullable: false),
                    NoiDungBinhLuan = table.Column<string>(type: "ntext", nullable: false),
                    NgayDang = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LuotXem = table.Column<int>(type: "int", nullable: false),
                    KiemDuyet = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BinhLuanBaiViet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BinhLuanBaiViet_BaiViet_BaiVietId",
                        column: x => x.BaiVietId,
                        principalTable: "BaiViet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BinhLuanBaiViet_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BaiViet_ChuDeId",
                table: "BaiViet",
                column: "ChuDeId");

            migrationBuilder.CreateIndex(
                name: "IX_BaiViet_UserId",
                table: "BaiViet",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BinhLuanBaiViet_BaiVietId",
                table: "BinhLuanBaiViet",
                column: "BaiVietId");

            migrationBuilder.CreateIndex(
                name: "IX_BinhLuanBaiViet_UserId",
                table: "BinhLuanBaiViet",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BinhLuanBaiViet");

            migrationBuilder.DropTable(
                name: "BaiViet");

            migrationBuilder.DropTable(
                name: "ChuDe");
        }
    }
}
