using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelAgency.Shared.Migrations
{
    /// <inheritdoc />
    public partial class ECPAY : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_T_OrderParticipant_IdNumber",
                table: "T_OrderParticipant");

            migrationBuilder.AddColumn<string>(
                name: "ECPayTradeNo",
                table: "T_Order",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MerchantTradeNo",
                table: "T_Order",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ECPayTradeNo",
                table: "T_Order");

            migrationBuilder.DropColumn(
                name: "MerchantTradeNo",
                table: "T_Order");

            migrationBuilder.CreateIndex(
                name: "IX_T_OrderParticipant_IdNumber",
                table: "T_OrderParticipant",
                column: "IdNumber",
                unique: true,
                filter: "[IdNumber] IS NOT NULL");
        }
    }
}
