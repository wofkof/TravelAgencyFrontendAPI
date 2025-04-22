using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelAgencyFrontendAPI.Migrations
{
    /// <inheritdoc />
    public partial class ADD_Collect_Comment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "T_Collect",
                columns: table => new
                {
                    CollectId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MemberId = table.Column<int>(type: "int", nullable: false),
                    TravelId = table.Column<int>(type: "int", nullable: false),
                    TravelType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_Collect", x => x.CollectId);
                    table.ForeignKey(
                        name: "FK_T_Collect_T_Member_MemberId",
                        column: x => x.MemberId,
                        principalTable: "T_Member",
                        principalColumn: "MemberId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "T_Comment",
                columns: table => new
                {
                    CommentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MemberId = table.Column<int>(type: "int", nullable: false),
                    TravelId = table.Column<int>(type: "int", nullable: false),
                    TravelType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Visible"),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_Comment", x => x.CommentId);
                    table.CheckConstraint("CK_Comment_Rating", "[Rating] BETWEEN 1 AND 5");
                    table.ForeignKey(
                        name: "FK_T_Comment_T_Member_MemberId",
                        column: x => x.MemberId,
                        principalTable: "T_Member",
                        principalColumn: "MemberId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_T_Collect_MemberId_TravelType_TravelId",
                table: "T_Collect",
                columns: new[] { "MemberId", "TravelType", "TravelId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_Comment_MemberId",
                table: "T_Comment",
                column: "MemberId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "T_Collect");

            migrationBuilder.DropTable(
                name: "T_Comment");
        }
    }
}
