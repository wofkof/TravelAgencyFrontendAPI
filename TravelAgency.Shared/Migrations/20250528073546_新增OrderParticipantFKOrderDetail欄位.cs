using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelAgency.Shared.Migrations
{
    /// <inheritdoc />
    public partial class 新增OrderParticipantFKOrderDetail欄位 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderDetailId",
                table: "T_OrderParticipant",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_T_OrderParticipant_OrderDetailId",
                table: "T_OrderParticipant",
                column: "OrderDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_T_OrderParticipant_T_OrderDetail_OrderDetailId",
                table: "T_OrderParticipant",
                column: "OrderDetailId",
                principalTable: "T_OrderDetail",
                principalColumn: "OrderDetailId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_T_OrderParticipant_T_OrderDetail_OrderDetailId",
                table: "T_OrderParticipant");

            migrationBuilder.DropIndex(
                name: "IX_T_OrderParticipant_OrderDetailId",
                table: "T_OrderParticipant");

            migrationBuilder.DropColumn(
                name: "OrderDetailId",
                table: "T_OrderParticipant");
        }
    }
}
