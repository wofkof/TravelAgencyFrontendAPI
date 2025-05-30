using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelAgency.Shared.Migrations
{
    /// <inheritdoc />
    public partial class 新增OrderParticipantMemberFavoriteTravelerId欄位 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FavoriteTravelerId",
                table: "T_OrderParticipant",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FavoriteTravelerId",
                table: "T_OrderParticipant");
        }
    }
}
