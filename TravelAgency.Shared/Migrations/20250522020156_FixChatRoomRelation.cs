using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelAgency.Shared.Migrations
{
    /// <inheritdoc />
    public partial class FixChatRoomRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_T_ChatRoom_EmployeeId",
                table: "T_ChatRoom");

            migrationBuilder.DropIndex(
                name: "IX_T_ChatRoom_MemberId",
                table: "T_ChatRoom");

            migrationBuilder.CreateIndex(
                name: "IX_T_ChatRoom_EmployeeId_MemberId",
                table: "T_ChatRoom",
                columns: new[] { "EmployeeId", "MemberId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_ChatRoom_MemberId",
                table: "T_ChatRoom",
                column: "MemberId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_T_ChatRoom_EmployeeId_MemberId",
                table: "T_ChatRoom");

            migrationBuilder.DropIndex(
                name: "IX_T_ChatRoom_MemberId",
                table: "T_ChatRoom");

            migrationBuilder.CreateIndex(
                name: "IX_T_ChatRoom_EmployeeId",
                table: "T_ChatRoom",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_T_ChatRoom_MemberId",
                table: "T_ChatRoom",
                column: "MemberId",
                unique: true);
        }
    }
}
