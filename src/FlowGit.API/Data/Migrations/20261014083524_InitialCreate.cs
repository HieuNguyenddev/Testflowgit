using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FlowGit.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false, collation: "Vietnamese_100_CI_AI"),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Name", "Price", "Quantity" },
                values: new object[,]
                {
                    { 1, "Bàn phím cơ", 1290000m, 15 },
                    { 2, "Chuột không dây", 450000m, 30 },
                    { 3, "Màn hình 27 inch", 5490000m, 8 },
                    { 4, "Tai nghe Bluetooth", 890000m, 20 },
                    { 5, "Laptop 14 inch", 18900000m, 5 },
                    { 6, "Ổ cứng SSD 1TB", 1750000m, 12 },
                    { 7, "Bàn phím số", 320000m, 18 },
                    { 8, "Chuột gaming", 690000m, 22 },
                    { 9, "Webcam Full HD", 550000m, 11 },
                    { 10, "Loa vi tính", 410000m, 16 },
                    { 11, "Bàn di chuột", 150000m, 40 },
                    { 12, "Cáp HDMI 2m", 120000m, 50 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
