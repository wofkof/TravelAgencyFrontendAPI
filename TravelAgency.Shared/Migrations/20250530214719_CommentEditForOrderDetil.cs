using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelAgency.Shared.Migrations
{
    /// <inheritdoc />
    public partial class CommentEditForOrderDetil : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_T_Comment_T_Member_MemberId",
                table: "T_Comment");

            migrationBuilder.DropIndex(
                name: "IX_T_Comment_MemberId",
                table: "T_Comment");

            migrationBuilder.RenameColumn(
                name: "TravelType",
                table: "T_Comment",
                newName: "Category");

            migrationBuilder.RenameColumn(
                name: "TravelId",
                table: "T_Comment",
                newName: "OrderDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_T_Comment_MemberId_OrderDetailId",
                table: "T_Comment",
                columns: new[] { "MemberId", "OrderDetailId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_Comment_OrderDetailId",
                table: "T_Comment",
                column: "OrderDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_T_Comment_T_Member_MemberId",
                table: "T_Comment",
                column: "MemberId",
                principalTable: "T_Member",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_T_Comment_T_OrderDetail_OrderDetailId",
                table: "T_Comment",
                column: "OrderDetailId",
                principalTable: "T_OrderDetail",
                principalColumn: "OrderDetailId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_T_Comment_T_Member_MemberId",
                table: "T_Comment");

            migrationBuilder.DropForeignKey(
                name: "FK_T_Comment_T_OrderDetail_OrderDetailId",
                table: "T_Comment");

            migrationBuilder.DropIndex(
                name: "IX_T_Comment_MemberId_OrderDetailId",
                table: "T_Comment");

            migrationBuilder.DropIndex(
                name: "IX_T_Comment_OrderDetailId",
                table: "T_Comment");

            migrationBuilder.RenameColumn(
                name: "OrderDetailId",
                table: "T_Comment",
                newName: "TravelId");

            migrationBuilder.RenameColumn(
                name: "Category",
                table: "T_Comment",
                newName: "TravelType");

            migrationBuilder.CreateIndex(
                name: "IX_T_Comment_MemberId",
                table: "T_Comment",
                column: "MemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_T_Comment_T_Member_MemberId",
                table: "T_Comment",
                column: "MemberId",
                principalTable: "T_Member",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
