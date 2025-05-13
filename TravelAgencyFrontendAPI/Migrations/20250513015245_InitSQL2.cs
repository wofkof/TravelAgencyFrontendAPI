using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelAgencyFrontendAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitSQL2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_T_MemberFavoriteTraveler_Email",
                table: "T_MemberFavoriteTraveler");

            migrationBuilder.DropIndex(
                name: "IX_T_MemberFavoriteTraveler_Phone",
                table: "T_MemberFavoriteTraveler");

            migrationBuilder.AddColumn<string>(
                name: "OrdererEmail",
                table: "T_Order",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OrdererName",
                table: "T_Order",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OrdererPhone",
                table: "T_Order",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "T_MemberFavoriteTraveler",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "T_MemberFavoriteTraveler",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "EmailVerificationCode",
                table: "T_Member",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailVerificationExpireTime",
                table: "T_Member",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCustomAvatar",
                table: "T_Member",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmailVerified",
                table: "T_Member",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ProfileImage",
                table: "T_Member",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_MemberFavoriteTraveler_Email",
                table: "T_MemberFavoriteTraveler",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_T_MemberFavoriteTraveler_Phone",
                table: "T_MemberFavoriteTraveler",
                column: "Phone",
                unique: true,
                filter: "[Phone] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_T_MemberFavoriteTraveler_Email",
                table: "T_MemberFavoriteTraveler");

            migrationBuilder.DropIndex(
                name: "IX_T_MemberFavoriteTraveler_Phone",
                table: "T_MemberFavoriteTraveler");

            migrationBuilder.DropColumn(
                name: "OrdererEmail",
                table: "T_Order");

            migrationBuilder.DropColumn(
                name: "OrdererName",
                table: "T_Order");

            migrationBuilder.DropColumn(
                name: "OrdererPhone",
                table: "T_Order");

            migrationBuilder.DropColumn(
                name: "EmailVerificationCode",
                table: "T_Member");

            migrationBuilder.DropColumn(
                name: "EmailVerificationExpireTime",
                table: "T_Member");

            migrationBuilder.DropColumn(
                name: "IsCustomAvatar",
                table: "T_Member");

            migrationBuilder.DropColumn(
                name: "IsEmailVerified",
                table: "T_Member");

            migrationBuilder.DropColumn(
                name: "ProfileImage",
                table: "T_Member");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "T_MemberFavoriteTraveler",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "T_MemberFavoriteTraveler",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_MemberFavoriteTraveler_Email",
                table: "T_MemberFavoriteTraveler",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_MemberFavoriteTraveler_Phone",
                table: "T_MemberFavoriteTraveler",
                column: "Phone",
                unique: true);
        }
    }
}
