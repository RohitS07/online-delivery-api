using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OnlineDelivery.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddReferenceData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'1000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NameEn = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameMr = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'1000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NameEn = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameMr = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Villages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'1000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CityId = table.Column<int>(type: "integer", nullable: false),
                    NameEn = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameMr = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DeliveryHours = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Villages", x => x.Id);
                    table.CheckConstraint("CK_Villages_DeliveryHours", "\"DeliveryHours\" >= 1");
                    table.ForeignKey(
                        name: "FK_Villages_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "IsActive", "NameEn", "NameMr" },
                values: new object[,]
                {
                    { 1, true, "Groceries", "किराणा" },
                    { 2, true, "Vegetables", "भाजीपाला" },
                    { 3, true, "Fruits", "फळे" },
                    { 4, true, "Dairy", "दुग्धजन्य पदार्थ" },
                    { 5, true, "Household", "घरगुती वस्तू" },
                    { 6, true, "Personal care", "वैयक्तिक काळजी" }
                });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "IsActive", "NameEn", "NameMr" },
                values: new object[,]
                {
                    { 1, true, "Pune", "पुणे" },
                    { 2, true, "Mumbai", "मुंबई" }
                });

            migrationBuilder.InsertData(
                table: "Villages",
                columns: new[] { "Id", "CityId", "DeliveryHours", "IsActive", "NameEn", "NameMr" },
                values: new object[,]
                {
                    { 1, 1, 3, true, "Mulshi", "मुळशी" },
                    { 2, 1, 3, true, "Jejuri", "जेजुरी" },
                    { 3, 2, 3, true, "Pali", "पाली" },
                    { 4, 2, 3, true, "Matheran", "माथेरान" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_NameEn",
                table: "Categories",
                column: "NameEn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cities_NameEn",
                table: "Cities",
                column: "NameEn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Villages_CityId_NameEn",
                table: "Villages",
                columns: new[] { "CityId", "NameEn" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Villages");

            migrationBuilder.DropTable(
                name: "Cities");
        }
    }
}
