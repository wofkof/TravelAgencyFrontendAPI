using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelAgency.Shared.Migrations
{
    /// <inheritdoc />
    public partial class InitSQL3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_T_OrderParticipant_T_Member_MemberId",
                table: "T_OrderParticipant");

            migrationBuilder.DropIndex(
                name: "IX_T_OrderParticipant_MemberId",
                table: "T_OrderParticipant");

            migrationBuilder.DropColumn(
                name: "MemberId",
                table: "T_OrderParticipant");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MemberId",
                table: "T_OrderParticipant",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_T_OrderParticipant_MemberId",
                table: "T_OrderParticipant",
                column: "MemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_T_OrderParticipant_T_Member_MemberId",
                table: "T_OrderParticipant",
                column: "MemberId",
                principalTable: "T_Member",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
