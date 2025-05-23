using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelAgency.Shared.Migrations
{
    /// <inheritdoc />
    public partial class VisaDeleteMemberId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_T_DocumentApplicationForm_T_Member_MemberId",
                table: "T_DocumentApplicationForm");

            migrationBuilder.DropIndex(
                name: "IX_T_DocumentApplicationForm_MemberId",
                table: "T_DocumentApplicationForm");

            migrationBuilder.DropColumn(
                name: "MemberId",
                table: "T_DocumentApplicationForm");

            migrationBuilder.AddColumn<int>(
                name: "MemberId",
                table: "T_DocumentOrderDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_T_DocumentOrderDetails_MemberId",
                table: "T_DocumentOrderDetails",
                column: "MemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_T_DocumentOrderDetails_T_Member_MemberId",
                table: "T_DocumentOrderDetails",
                column: "MemberId",
                principalTable: "T_Member",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_T_DocumentOrderDetails_T_Member_MemberId",
                table: "T_DocumentOrderDetails");

            migrationBuilder.DropIndex(
                name: "IX_T_DocumentOrderDetails_MemberId",
                table: "T_DocumentOrderDetails");

            migrationBuilder.DropColumn(
                name: "MemberId",
                table: "T_DocumentOrderDetails");

            migrationBuilder.AddColumn<int>(
                name: "MemberId",
                table: "T_DocumentApplicationForm",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_T_DocumentApplicationForm_MemberId",
                table: "T_DocumentApplicationForm",
                column: "MemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_T_DocumentApplicationForm_T_Member_MemberId",
                table: "T_DocumentApplicationForm",
                column: "MemberId",
                principalTable: "T_Member",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
