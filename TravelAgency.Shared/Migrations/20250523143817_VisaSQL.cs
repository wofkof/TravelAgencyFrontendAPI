using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelAgency.Shared.Migrations
{
    /// <inheritdoc />
    public partial class VisaSQL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_T_Payment_T_DocumentOrderDetails_DocumentOrderId",
                table: "T_Payment");

            migrationBuilder.DropTable(
                name: "T_DocumentOrderDetails");

            migrationBuilder.DropTable(
                name: "T_DocumentApplicationForm");

            migrationBuilder.DropTable(
                name: "T_PickupInformation");

            migrationBuilder.DropTable(
                name: "T_PickupMethod");

            migrationBuilder.DropColumn(
                name: "PaymentDeadline",
                table: "T_Payment");

            migrationBuilder.RenameColumn(
                name: "PaymentMethod",
                table: "T_Payment",
                newName: "payment_method");

            migrationBuilder.RenameColumn(
                name: "PaymentId",
                table: "T_Payment",
                newName: "payment_id");

            migrationBuilder.RenameColumn(
                name: "DocumentOrderId",
                table: "T_Payment",
                newName: "order_form_id");

            migrationBuilder.RenameIndex(
                name: "IX_T_Payment_DocumentOrderId",
                table: "T_Payment",
                newName: "IX_T_Payment_order_form_id");

            migrationBuilder.AddColumn<int>(
                name: "document_menu_id",
                table: "T_Payment",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "T_DocumentMenu",
                columns: table => new
                {
                    menu_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    roc_passport_option = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    foreign_visa_option = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    application_type = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    processing_item = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    case_type = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    processing_days = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    document_validity_period = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    stay_duration = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    fee = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_DocumentMenu", x => x.menu_id);
                });

            migrationBuilder.CreateTable(
                name: "T_OrderForm",
                columns: table => new
                {
                    order_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    member_id = table.Column<int>(type: "int", nullable: false),
                    document_menu_id = table.Column<int>(type: "int", nullable: false),
                    departure_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    processing_quantity = table.Column<int>(type: "int", nullable: false),
                    chinese_surname = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    chinese_name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    english_surname = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    english_name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    birth_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    contact_person_name = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    contact_person_email = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    contact_person_phonenumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Pickup_method_option = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    mailing_city = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    mailing_detail_address = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    store_detail_address = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Tax_ID_option = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    company_name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    tax_id_number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    order_creation_time = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_OrderForm", x => x.order_id);
                    table.ForeignKey(
                        name: "FK_T_OrderForm_T_DocumentMenu_document_menu_id",
                        column: x => x.document_menu_id,
                        principalTable: "T_DocumentMenu",
                        principalColumn: "menu_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_T_OrderForm_T_Member_member_id",
                        column: x => x.member_id,
                        principalTable: "T_Member",
                        principalColumn: "MemberId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "T_CompletedOrderDetail",
                columns: table => new
                {
                    completed_order_detail_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    document_menu_id = table.Column<int>(type: "int", nullable: false),
                    order_form_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_CompletedOrderDetail", x => x.completed_order_detail_id);
                    table.ForeignKey(
                        name: "FK_T_CompletedOrderDetail_T_DocumentMenu_document_menu_id",
                        column: x => x.document_menu_id,
                        principalTable: "T_DocumentMenu",
                        principalColumn: "menu_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_T_CompletedOrderDetail_T_OrderForm_order_form_id",
                        column: x => x.order_form_id,
                        principalTable: "T_OrderForm",
                        principalColumn: "order_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_T_Payment_document_menu_id",
                table: "T_Payment",
                column: "document_menu_id");

            migrationBuilder.CreateIndex(
                name: "IX_T_CompletedOrderDetail_document_menu_id",
                table: "T_CompletedOrderDetail",
                column: "document_menu_id");

            migrationBuilder.CreateIndex(
                name: "IX_T_CompletedOrderDetail_order_form_id",
                table: "T_CompletedOrderDetail",
                column: "order_form_id");

            migrationBuilder.CreateIndex(
                name: "IX_T_OrderForm_document_menu_id",
                table: "T_OrderForm",
                column: "document_menu_id");

            migrationBuilder.CreateIndex(
                name: "IX_T_OrderForm_member_id",
                table: "T_OrderForm",
                column: "member_id");

            migrationBuilder.AddForeignKey(
                name: "FK_T_Payment_T_DocumentMenu_document_menu_id",
                table: "T_Payment",
                column: "document_menu_id",
                principalTable: "T_DocumentMenu",
                principalColumn: "menu_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_T_Payment_T_OrderForm_order_form_id",
                table: "T_Payment",
                column: "order_form_id",
                principalTable: "T_OrderForm",
                principalColumn: "order_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_T_Payment_T_DocumentMenu_document_menu_id",
                table: "T_Payment");

            migrationBuilder.DropForeignKey(
                name: "FK_T_Payment_T_OrderForm_order_form_id",
                table: "T_Payment");

            migrationBuilder.DropTable(
                name: "T_CompletedOrderDetail");

            migrationBuilder.DropTable(
                name: "T_OrderForm");

            migrationBuilder.DropTable(
                name: "T_DocumentMenu");

            migrationBuilder.DropIndex(
                name: "IX_T_Payment_document_menu_id",
                table: "T_Payment");

            migrationBuilder.DropColumn(
                name: "document_menu_id",
                table: "T_Payment");

            migrationBuilder.RenameColumn(
                name: "payment_method",
                table: "T_Payment",
                newName: "PaymentMethod");

            migrationBuilder.RenameColumn(
                name: "payment_id",
                table: "T_Payment",
                newName: "PaymentId");

            migrationBuilder.RenameColumn(
                name: "order_form_id",
                table: "T_Payment",
                newName: "DocumentOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_T_Payment_order_form_id",
                table: "T_Payment",
                newName: "IX_T_Payment_DocumentOrderId");

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentDeadline",
                table: "T_Payment",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "T_DocumentApplicationForm",
                columns: table => new
                {
                    ApplicationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MemberId = table.Column<int>(type: "int", nullable: false),
                    RegionId = table.Column<int>(type: "int", maxLength: 10, nullable: true),
                    ApplicationType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CaseType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "date", nullable: false),
                    Fee = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    ProcessingDays = table.Column<byte>(type: "tinyint", nullable: false),
                    ProcessingItem = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StayDuration = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_DocumentApplicationForm", x => x.ApplicationId);
                    table.ForeignKey(
                        name: "FK_T_DocumentApplicationForm_T_Member_MemberId",
                        column: x => x.MemberId,
                        principalTable: "T_Member",
                        principalColumn: "MemberId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_T_DocumentApplicationForm_T_Region_RegionId",
                        column: x => x.RegionId,
                        principalTable: "T_Region",
                        principalColumn: "RegionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "T_PickupInformation",
                columns: table => new
                {
                    PickupInfoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    City = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DetailedAddress = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    District = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_PickupInformation", x => x.PickupInfoId);
                });

            migrationBuilder.CreateTable(
                name: "T_PickupMethod",
                columns: table => new
                {
                    PickupMethodId = table.Column<byte>(type: "tinyint", nullable: false),
                    PickupMethodName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_PickupMethod", x => x.PickupMethodId);
                });

            migrationBuilder.CreateTable(
                name: "T_DocumentOrderDetails",
                columns: table => new
                {
                    DocumentOrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgencyCode = table.Column<int>(type: "int", nullable: false),
                    ApplicationId = table.Column<int>(type: "int", nullable: false),
                    PickupInfoId = table.Column<int>(type: "int", nullable: true),
                    PickupMethodId = table.Column<byte>(type: "tinyint", nullable: false),
                    ApplicationType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    BirthDate = table.Column<DateTime>(type: "date", nullable: false),
                    ChineseFirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ChineseLastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DepartureDate = table.Column<DateTime>(type: "date", nullable: false),
                    EnglishFirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EnglishLastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    ProcessingCount = table.Column<byte>(type: "tinyint", nullable: false),
                    RequiredData = table.Column<string>(type: "text", nullable: false),
                    SubmissionMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_DocumentOrderDetails", x => x.DocumentOrderId);
                    table.ForeignKey(
                        name: "FK_T_DocumentOrderDetails_T_Agency_AgencyCode",
                        column: x => x.AgencyCode,
                        principalTable: "T_Agency",
                        principalColumn: "AgencyCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_T_DocumentOrderDetails_T_DocumentApplicationForm_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "T_DocumentApplicationForm",
                        principalColumn: "ApplicationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_T_DocumentOrderDetails_T_PickupInformation_PickupInfoId",
                        column: x => x.PickupInfoId,
                        principalTable: "T_PickupInformation",
                        principalColumn: "PickupInfoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_T_DocumentOrderDetails_T_PickupMethod_PickupMethodId",
                        column: x => x.PickupMethodId,
                        principalTable: "T_PickupMethod",
                        principalColumn: "PickupMethodId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_T_DocumentApplicationForm_MemberId",
                table: "T_DocumentApplicationForm",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_T_DocumentApplicationForm_RegionId",
                table: "T_DocumentApplicationForm",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_T_DocumentOrderDetails_AgencyCode",
                table: "T_DocumentOrderDetails",
                column: "AgencyCode");

            migrationBuilder.CreateIndex(
                name: "IX_T_DocumentOrderDetails_ApplicationId",
                table: "T_DocumentOrderDetails",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_T_DocumentOrderDetails_PickupInfoId",
                table: "T_DocumentOrderDetails",
                column: "PickupInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_T_DocumentOrderDetails_PickupMethodId",
                table: "T_DocumentOrderDetails",
                column: "PickupMethodId");

            migrationBuilder.AddForeignKey(
                name: "FK_T_Payment_T_DocumentOrderDetails_DocumentOrderId",
                table: "T_Payment",
                column: "DocumentOrderId",
                principalTable: "T_DocumentOrderDetails",
                principalColumn: "DocumentOrderId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
