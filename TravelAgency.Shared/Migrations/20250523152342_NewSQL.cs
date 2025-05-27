using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelAgency.Shared.Migrations
{
    /// <inheritdoc />
    public partial class NewSQL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "S_City",
                columns: table => new
                {
                    CityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CityName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_S_City", x => x.CityId);
                });

            migrationBuilder.CreateTable(
                name: "S_Transport",
                columns: table => new
                {
                    TransportId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransportMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_S_Transport", x => x.TransportId);
                });

            migrationBuilder.CreateTable(
                name: "T_Agency",
                columns: table => new
                {
                    AgencyCode = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgencyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ContactPerson = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ContactEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ContactPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ServiceDescription = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_Agency", x => x.AgencyCode);
                });

            migrationBuilder.CreateTable(
                name: "T_Countries",
                columns: table => new
                {
                    CountryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CountryName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Continent = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_Countries", x => x.CountryId);
                });

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
                name: "T_EmailVerificationCode",
                columns: table => new
                {
                    VerificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VerificationCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    VerificationType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()"),
                    ExpireAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_EmailVerificationCode", x => x.VerificationId);
                });

            migrationBuilder.CreateTable(
                name: "T_Member",
                columns: table => new
                {
                    MemberId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "11110, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Birthday = table.Column<DateTime>(type: "date", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true, defaultValue: "Other"),
                    IdNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PassportSurname = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PassportGivenName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PassportExpireDate = table.Column<DateTime>(type: "date", nullable: true),
                    Nationality = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DocumentType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "PASSPORT"),
                    DocumentNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PasswordSalt = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    GoogleId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RegisterDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()"),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    RememberToken = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    RememberExpireTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsBlacklisted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Note = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    ProfileImage = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsCustomAvatar = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    EmailVerificationCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EmailVerificationExpireTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsEmailVerified = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_Member", x => x.MemberId);
                });

            migrationBuilder.CreateTable(
                name: "T_Permission",
                columns: table => new
                {
                    PermissionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PermissionName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Caption = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_Permission", x => x.PermissionId);
                });

            migrationBuilder.CreateTable(
                name: "T_Region",
                columns: table => new
                {
                    RegionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_Region", x => x.RegionId);
                });

            migrationBuilder.CreateTable(
                name: "T_Role",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_Role", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "T_Sticker",
                columns: table => new
                {
                    StickerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_Sticker", x => x.StickerId);
                });

            migrationBuilder.CreateTable(
                name: "T_TravelSupplier",
                columns: table => new
                {
                    TravelSupplierId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SupplierType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ContactName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ContactPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SupplierNote = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_TravelSupplier", x => x.TravelSupplierId);
                });

            migrationBuilder.CreateTable(
                name: "S_District",
                columns: table => new
                {
                    DistrictId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CityId = table.Column<int>(type: "int", nullable: false),
                    DistrictName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_S_District", x => x.DistrictId);
                    table.ForeignKey(
                        name: "FK_S_District_S_City_CityId",
                        column: x => x.CityId,
                        principalTable: "S_City",
                        principalColumn: "CityId",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateTable(
                name: "T_MemberFavoriteTraveler",
                columns: table => new
                {
                    FavoriteTravelerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "20000, 1"),
                    MemberId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IdNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    BirthDate = table.Column<DateTime>(type: "date", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true, defaultValue: "Other"),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DocumentType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "PASSPORT"),
                    DocumentNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PassportSurname = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PassportGivenName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PassportExpireDate = table.Column<DateTime>(type: "date", nullable: true),
                    Nationality = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "GETDATE()"),
                    Note = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    IssuedPlace = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_MemberFavoriteTraveler", x => x.FavoriteTravelerId);
                    table.ForeignKey(
                        name: "FK_T_MemberFavoriteTraveler_T_Member_MemberId",
                        column: x => x.MemberId,
                        principalTable: "T_Member",
                        principalColumn: "MemberId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "T_Order",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MemberId = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Other"),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()"),
                    PaymentDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    InvoiceDeliveryEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    InvoiceOption = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Personal"),
                    InvoiceUniformNumber = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    InvoiceTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    InvoiceAddBillingAddr = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    InvoiceBillingAddress = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    OrdererName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OrdererPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OrdererEmail = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ECPayTradeNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    MerchantTradeNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_Order", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_T_Order_T_Member_MemberId",
                        column: x => x.MemberId,
                        principalTable: "T_Member",
                        principalColumn: "MemberId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "T_OrderForm",
                columns: table => new
                {
                    order_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    member_id = table.Column<int>(type: "int", nullable: false),
                    document_menu_id = table.Column<int>(type: "int", nullable: false),
                    departure_date = table.Column<DateTime>(type: "datetime", nullable: false),
                    processing_quantity = table.Column<int>(type: "int", nullable: false),
                    chinese_surname = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    chinese_name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    english_surname = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    english_name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    birth_date = table.Column<DateTime>(type: "datetime", nullable: false),
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
                    order_creation_time = table.Column<DateTime>(type: "datetime", nullable: false)
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
                name: "M_RolePermission",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_M_RolePermission", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_M_RolePermission_T_Permission_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "T_Permission",
                        principalColumn: "PermissionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_M_RolePermission_T_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "T_Role",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "T_Employee",
                columns: table => new
                {
                    EmployeeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BirthDate = table.Column<DateTime>(type: "date", nullable: true),
                    HireDate = table.Column<DateTime>(type: "date", nullable: false, defaultValueSql: "GETDATE()"),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    Note = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "Other"),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ImagePath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_Employee", x => x.EmployeeId);
                    table.ForeignKey(
                        name: "FK_T_Employee_T_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "T_Role",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "T_Official_Accommodation",
                columns: table => new
                {
                    AccommodationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TravelSupplierId = table.Column<int>(type: "int", nullable: true),
                    RegionId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_Official_Accommodation", x => x.AccommodationId);
                    table.ForeignKey(
                        name: "FK_T_Official_Accommodation_T_Region_RegionId",
                        column: x => x.RegionId,
                        principalTable: "T_Region",
                        principalColumn: "RegionId");
                    table.ForeignKey(
                        name: "FK_T_Official_Accommodation_T_TravelSupplier_TravelSupplierId",
                        column: x => x.TravelSupplierId,
                        principalTable: "T_TravelSupplier",
                        principalColumn: "TravelSupplierId");
                });

            migrationBuilder.CreateTable(
                name: "T_Official_Attraction",
                columns: table => new
                {
                    AttractionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TravelSupplierId = table.Column<int>(type: "int", nullable: true),
                    RegionId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_Official_Attraction", x => x.AttractionId);
                    table.ForeignKey(
                        name: "FK_T_Official_Attraction_T_Region_RegionId",
                        column: x => x.RegionId,
                        principalTable: "T_Region",
                        principalColumn: "RegionId");
                    table.ForeignKey(
                        name: "FK_T_Official_Attraction_T_TravelSupplier_TravelSupplierId",
                        column: x => x.TravelSupplierId,
                        principalTable: "T_TravelSupplier",
                        principalColumn: "TravelSupplierId");
                });

            migrationBuilder.CreateTable(
                name: "T_Official_Restaurant",
                columns: table => new
                {
                    RestaurantId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TravelSupplierId = table.Column<int>(type: "int", nullable: true),
                    RegionId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_Official_Restaurant", x => x.RestaurantId);
                    table.ForeignKey(
                        name: "FK_T_Official_Restaurant_T_Region_RegionId",
                        column: x => x.RegionId,
                        principalTable: "T_Region",
                        principalColumn: "RegionId");
                    table.ForeignKey(
                        name: "FK_T_Official_Restaurant_T_TravelSupplier_TravelSupplierId",
                        column: x => x.TravelSupplierId,
                        principalTable: "T_TravelSupplier",
                        principalColumn: "TravelSupplierId");
                });

            migrationBuilder.CreateTable(
                name: "T_Accommodation",
                columns: table => new
                {
                    AccommodationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DistrictId = table.Column<int>(type: "int", nullable: false),
                    AccommodationName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_Accommodation", x => x.AccommodationId);
                    table.ForeignKey(
                        name: "FK_T_Accommodation_S_District_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "S_District",
                        principalColumn: "DistrictId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "T_Attraction",
                columns: table => new
                {
                    AttractionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DistrictId = table.Column<int>(type: "int", nullable: false),
                    AttractionName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_Attraction", x => x.AttractionId);
                    table.ForeignKey(
                        name: "FK_T_Attraction_S_District_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "S_District",
                        principalColumn: "DistrictId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "T_Restaurant",
                columns: table => new
                {
                    RestaurantId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DistrictId = table.Column<int>(type: "int", nullable: false),
                    RestaurantName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_Restaurant", x => x.RestaurantId);
                    table.ForeignKey(
                        name: "FK_T_Restaurant_S_District_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "S_District",
                        principalColumn: "DistrictId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "T_OrderDetail",
                columns: table => new
                {
                    OrderDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()"),
                    StartDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_OrderDetail", x => x.OrderDetailId);
                    table.CheckConstraint("CK_OrderDetail_Price", "Price >= 0.00");
                    table.CheckConstraint("CK_OrderDetail_Quantity", "Quantity > 0");
                    table.ForeignKey(
                        name: "FK_T_OrderDetail_T_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "T_Order",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "T_OrderInvoices",
                columns: table => new
                {
                    InvoiceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    BuyerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    InvoiceItemDesc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    InvoiceType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "ElectronicInvoice"),
                    InvoiceStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    BuyerUniformNumber = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    InvoiceFileURL = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_OrderInvoices", x => x.InvoiceId);
                    table.CheckConstraint("CK_OrderInvoices_TotalAmount", "TotalAmount >= 0.00");
                    table.ForeignKey(
                        name: "FK_T_OrderInvoices_T_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "T_Order",
                        principalColumn: "OrderId");
                });

            migrationBuilder.CreateTable(
                name: "T_OrderParticipant",
                columns: table => new
                {
                    OrderParticipantId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BirthDate = table.Column<DateTime>(type: "date", nullable: false),
                    IdNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DocumentType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DocumentNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PassportSurname = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PassportGivenName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PassportExpireDate = table.Column<DateTime>(type: "date", nullable: true),
                    Nationality = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_OrderParticipant", x => x.OrderParticipantId);
                    table.ForeignKey(
                        name: "FK_T_OrderParticipant_T_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "T_Order",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateTable(
                name: "T_Payment",
                columns: table => new
                {
                    payment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    order_form_id = table.Column<int>(type: "int", nullable: false),
                    document_menu_id = table.Column<int>(type: "int", nullable: false),
                    payment_method = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_Payment", x => x.payment_id);
                    table.ForeignKey(
                        name: "FK_T_Payment_T_DocumentMenu_document_menu_id",
                        column: x => x.document_menu_id,
                        principalTable: "T_DocumentMenu",
                        principalColumn: "menu_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_T_Payment_T_OrderForm_order_form_id",
                        column: x => x.order_form_id,
                        principalTable: "T_OrderForm",
                        principalColumn: "order_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "T_Announcement",
                columns: table => new
                {
                    AnnouncementId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Published")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_Announcement", x => x.AnnouncementId);
                    table.ForeignKey(
                        name: "FK_T_Announcement_T_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "T_Employee",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "T_ChatRoom",
                columns: table => new
                {
                    ChatRoomId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    MemberId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()"),
                    IsBlocked = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    LastMessageAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_ChatRoom", x => x.ChatRoomId);
                    table.ForeignKey(
                        name: "FK_T_ChatRoom_T_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "T_Employee",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_T_ChatRoom_T_Member_MemberId",
                        column: x => x.MemberId,
                        principalTable: "T_Member",
                        principalColumn: "MemberId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "T_CustomTravel",
                columns: table => new
                {
                    CustomTravelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MemberId = table.Column<int>(type: "int", nullable: false),
                    ReviewEmployeeId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    DepartureDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Days = table.Column<int>(type: "int", nullable: false),
                    People = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    Note = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_CustomTravel", x => x.CustomTravelId);
                    table.ForeignKey(
                        name: "FK_T_CustomTravel_T_Employee_ReviewEmployeeId",
                        column: x => x.ReviewEmployeeId,
                        principalTable: "T_Employee",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_T_CustomTravel_T_Member_MemberId",
                        column: x => x.MemberId,
                        principalTable: "T_Member",
                        principalColumn: "MemberId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "T_OfficialTravel",
                columns: table => new
                {
                    OfficialTravelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedByEmployeeId = table.Column<int>(type: "int", nullable: false),
                    RegionId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AvailableFrom = table.Column<DateTime>(type: "datetime", nullable: true),
                    AvailableUntil = table.Column<DateTime>(type: "datetime", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TotalTravelCount = table.Column<int>(type: "int", nullable: true),
                    TotalDepartureCount = table.Column<int>(type: "int", nullable: true),
                    Days = table.Column<int>(type: "int", nullable: true),
                    CoverPath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_OfficialTravel", x => x.OfficialTravelId);
                    table.ForeignKey(
                        name: "FK_T_OfficialTravel_T_Employee_CreatedByEmployeeId",
                        column: x => x.CreatedByEmployeeId,
                        principalTable: "T_Employee",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_T_OfficialTravel_T_Region_RegionId",
                        column: x => x.RegionId,
                        principalTable: "T_Region",
                        principalColumn: "RegionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "T_CallLog",
                columns: table => new
                {
                    CallId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChatRoomId = table.Column<int>(type: "int", nullable: false),
                    CallerType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CallerId = table.Column<int>(type: "int", nullable: false),
                    ReceiverType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ReceiverId = table.Column<int>(type: "int", nullable: false),
                    CallType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    DurationInSeconds = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_CallLog", x => x.CallId);
                    table.ForeignKey(
                        name: "FK_T_CallLog_T_ChatRoom_ChatRoomId",
                        column: x => x.ChatRoomId,
                        principalTable: "T_ChatRoom",
                        principalColumn: "ChatRoomId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "T_Message",
                columns: table => new
                {
                    MessageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChatRoomId = table.Column<int>(type: "int", nullable: false),
                    SenderType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SenderId = table.Column<int>(type: "int", nullable: false),
                    MessageType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()"),
                    IsRead = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_Message", x => x.MessageId);
                    table.ForeignKey(
                        name: "FK_T_Message_T_ChatRoom_ChatRoomId",
                        column: x => x.ChatRoomId,
                        principalTable: "T_ChatRoom",
                        principalColumn: "ChatRoomId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "T_CustomTravelContent",
                columns: table => new
                {
                    ContentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomTravelId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Day = table.Column<int>(type: "int", nullable: false),
                    Time = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AccommodationName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_CustomTravelContent", x => x.ContentId);
                    table.ForeignKey(
                        name: "FK_T_CustomTravelContent_T_CustomTravel_CustomTravelId",
                        column: x => x.CustomTravelId,
                        principalTable: "T_CustomTravel",
                        principalColumn: "CustomTravelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "T_OfficialTravelDetail",
                columns: table => new
                {
                    OfficialTravelDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OfficialTravelId = table.Column<int>(type: "int", nullable: false),
                    TravelNumber = table.Column<int>(type: "int", nullable: true),
                    AdultPrice = table.Column<decimal>(type: "money", nullable: true),
                    ChildPrice = table.Column<decimal>(type: "money", nullable: true),
                    BabyPrice = table.Column<decimal>(type: "money", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    State = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_OfficialTravelDetail", x => x.OfficialTravelDetailId);
                    table.ForeignKey(
                        name: "FK_T_OfficialTravelDetail_T_OfficialTravel_OfficialTravelId",
                        column: x => x.OfficialTravelId,
                        principalTable: "T_OfficialTravel",
                        principalColumn: "OfficialTravelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "T_MessageMedia",
                columns: table => new
                {
                    MediaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageId = table.Column<int>(type: "int", nullable: false),
                    MediaType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DurationInSeconds = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_MessageMedia", x => x.MediaId);
                    table.ForeignKey(
                        name: "FK_T_MessageMedia_T_Message_MessageId",
                        column: x => x.MessageId,
                        principalTable: "T_Message",
                        principalColumn: "MessageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "T_GroupTravel",
                columns: table => new
                {
                    GroupTravelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OfficialTravelDetailId = table.Column<int>(type: "int", nullable: false),
                    DepartureDate = table.Column<DateTime>(type: "date", nullable: true),
                    ReturnDate = table.Column<DateTime>(type: "date", nullable: true),
                    TotalSeats = table.Column<int>(type: "int", nullable: true),
                    SoldSeats = table.Column<int>(type: "int", nullable: true),
                    OrderDeadline = table.Column<DateTime>(type: "date", nullable: true),
                    MinimumParticipants = table.Column<int>(type: "int", nullable: true),
                    GroupStatus = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "date", nullable: true, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    RecordStatus = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_GroupTravel", x => x.GroupTravelId);
                    table.ForeignKey(
                        name: "FK_T_GroupTravel_T_OfficialTravelDetail_OfficialTravelDetailId",
                        column: x => x.OfficialTravelDetailId,
                        principalTable: "T_OfficialTravelDetail",
                        principalColumn: "OfficialTravelDetailId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "T_OfficialTravelSchedule",
                columns: table => new
                {
                    OfficialTravelScheduleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OfficialTravelDetailId = table.Column<int>(type: "int", nullable: false),
                    Day = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Breakfast = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Lunch = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Dinner = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Hotel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Attraction1 = table.Column<int>(type: "int", nullable: true),
                    Attraction2 = table.Column<int>(type: "int", nullable: true),
                    Attraction3 = table.Column<int>(type: "int", nullable: true),
                    Attraction4 = table.Column<int>(type: "int", nullable: true),
                    Attraction5 = table.Column<int>(type: "int", nullable: true),
                    Note1 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Note2 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_OfficialTravelSchedule", x => x.OfficialTravelScheduleId);
                    table.ForeignKey(
                        name: "FK_T_OfficialTravelSchedule_T_OfficialTravelDetail_OfficialTravelDetailId",
                        column: x => x.OfficialTravelDetailId,
                        principalTable: "T_OfficialTravelDetail",
                        principalColumn: "OfficialTravelDetailId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "T_TravelRecord",
                columns: table => new
                {
                    TravelRecordId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupTravelId = table.Column<int>(type: "int", nullable: false),
                    TotalParticipants = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TotalOrders = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CompletionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_TravelRecord", x => x.TravelRecordId);
                    table.CheckConstraint("CK_TravelRecord_TotalAmount", "TotalAmount >= 0.00");
                    table.CheckConstraint("CK_TravelRecord_TotalOrders", "TotalOrders >= 0");
                    table.CheckConstraint("CK_TravelRecord_TotalParticipants", "TotalParticipants >= 0");
                    table.ForeignKey(
                        name: "FK_T_TravelRecord_T_GroupTravel_GroupTravelId",
                        column: x => x.GroupTravelId,
                        principalTable: "T_GroupTravel",
                        principalColumn: "GroupTravelId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_M_RolePermission_PermissionId",
                table: "M_RolePermission",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_S_District_CityId",
                table: "S_District",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_T_Accommodation_DistrictId",
                table: "T_Accommodation",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_T_Announcement_EmployeeId",
                table: "T_Announcement",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_T_Attraction_DistrictId",
                table: "T_Attraction",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_T_CallLog_ChatRoomId",
                table: "T_CallLog",
                column: "ChatRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_T_ChatRoom_EmployeeId_MemberId",
                table: "T_ChatRoom",
                columns: new[] { "EmployeeId", "MemberId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_ChatRoom_MemberId",
                table: "T_ChatRoom",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_T_Collect_MemberId_TravelType_TravelId",
                table: "T_Collect",
                columns: new[] { "MemberId", "TravelType", "TravelId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_Comment_MemberId",
                table: "T_Comment",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_T_CompletedOrderDetail_document_menu_id",
                table: "T_CompletedOrderDetail",
                column: "document_menu_id");

            migrationBuilder.CreateIndex(
                name: "IX_T_CompletedOrderDetail_order_form_id",
                table: "T_CompletedOrderDetail",
                column: "order_form_id");

            migrationBuilder.CreateIndex(
                name: "IX_T_CustomTravel_MemberId",
                table: "T_CustomTravel",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_T_CustomTravel_ReviewEmployeeId",
                table: "T_CustomTravel",
                column: "ReviewEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_T_CustomTravelContent_CustomTravelId",
                table: "T_CustomTravelContent",
                column: "CustomTravelId");

            migrationBuilder.CreateIndex(
                name: "IX_T_Employee_Email",
                table: "T_Employee",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_Employee_Phone",
                table: "T_Employee",
                column: "Phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_Employee_RoleId",
                table: "T_Employee",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_T_GroupTravel_OfficialTravelDetailId",
                table: "T_GroupTravel",
                column: "OfficialTravelDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_T_Member_Email",
                table: "T_Member",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_Member_Phone",
                table: "T_Member",
                column: "Phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_MemberFavoriteTraveler_Email",
                table: "T_MemberFavoriteTraveler",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_T_MemberFavoriteTraveler_IdNumber",
                table: "T_MemberFavoriteTraveler",
                column: "IdNumber",
                unique: true,
                filter: "[IdNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_T_MemberFavoriteTraveler_MemberId",
                table: "T_MemberFavoriteTraveler",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_T_MemberFavoriteTraveler_Phone",
                table: "T_MemberFavoriteTraveler",
                column: "Phone",
                unique: true,
                filter: "[Phone] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_T_Message_ChatRoomId",
                table: "T_Message",
                column: "ChatRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_T_MessageMedia_MessageId",
                table: "T_MessageMedia",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_T_Official_Accommodation_RegionId",
                table: "T_Official_Accommodation",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_T_Official_Accommodation_TravelSupplierId",
                table: "T_Official_Accommodation",
                column: "TravelSupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_T_Official_Attraction_RegionId",
                table: "T_Official_Attraction",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_T_Official_Attraction_TravelSupplierId",
                table: "T_Official_Attraction",
                column: "TravelSupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_T_Official_Restaurant_RegionId",
                table: "T_Official_Restaurant",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_T_Official_Restaurant_TravelSupplierId",
                table: "T_Official_Restaurant",
                column: "TravelSupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_T_OfficialTravel_CreatedByEmployeeId",
                table: "T_OfficialTravel",
                column: "CreatedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_T_OfficialTravel_RegionId",
                table: "T_OfficialTravel",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_T_OfficialTravelDetail_OfficialTravelId",
                table: "T_OfficialTravelDetail",
                column: "OfficialTravelId");

            migrationBuilder.CreateIndex(
                name: "IX_T_OfficialTravelSchedule_OfficialTravelDetailId",
                table: "T_OfficialTravelSchedule",
                column: "OfficialTravelDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_T_Order_MemberId",
                table: "T_Order",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_T_OrderDetail_OrderId",
                table: "T_OrderDetail",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_T_OrderForm_document_menu_id",
                table: "T_OrderForm",
                column: "document_menu_id");

            migrationBuilder.CreateIndex(
                name: "IX_T_OrderForm_member_id",
                table: "T_OrderForm",
                column: "member_id");

            migrationBuilder.CreateIndex(
                name: "IX_T_OrderInvoices_InvoiceNumber",
                table: "T_OrderInvoices",
                column: "InvoiceNumber",
                unique: true,
                filter: "[InvoiceNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_T_OrderInvoices_OrderId",
                table: "T_OrderInvoices",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_T_OrderParticipant_Email",
                table: "T_OrderParticipant",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_T_OrderParticipant_OrderId",
                table: "T_OrderParticipant",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_T_OrderParticipant_Phone",
                table: "T_OrderParticipant",
                column: "Phone",
                unique: true,
                filter: "[Phone] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_T_Payment_document_menu_id",
                table: "T_Payment",
                column: "document_menu_id");

            migrationBuilder.CreateIndex(
                name: "IX_T_Payment_order_form_id",
                table: "T_Payment",
                column: "order_form_id");

            migrationBuilder.CreateIndex(
                name: "IX_T_Restaurant_DistrictId",
                table: "T_Restaurant",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_T_TravelRecord_GroupTravelId",
                table: "T_TravelRecord",
                column: "GroupTravelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "M_RolePermission");

            migrationBuilder.DropTable(
                name: "S_Transport");

            migrationBuilder.DropTable(
                name: "T_Accommodation");

            migrationBuilder.DropTable(
                name: "T_Agency");

            migrationBuilder.DropTable(
                name: "T_Announcement");

            migrationBuilder.DropTable(
                name: "T_Attraction");

            migrationBuilder.DropTable(
                name: "T_CallLog");

            migrationBuilder.DropTable(
                name: "T_Collect");

            migrationBuilder.DropTable(
                name: "T_Comment");

            migrationBuilder.DropTable(
                name: "T_CompletedOrderDetail");

            migrationBuilder.DropTable(
                name: "T_Countries");

            migrationBuilder.DropTable(
                name: "T_CustomTravelContent");

            migrationBuilder.DropTable(
                name: "T_EmailVerificationCode");

            migrationBuilder.DropTable(
                name: "T_MemberFavoriteTraveler");

            migrationBuilder.DropTable(
                name: "T_MessageMedia");

            migrationBuilder.DropTable(
                name: "T_Official_Accommodation");

            migrationBuilder.DropTable(
                name: "T_Official_Attraction");

            migrationBuilder.DropTable(
                name: "T_Official_Restaurant");

            migrationBuilder.DropTable(
                name: "T_OfficialTravelSchedule");

            migrationBuilder.DropTable(
                name: "T_OrderDetail");

            migrationBuilder.DropTable(
                name: "T_OrderInvoices");

            migrationBuilder.DropTable(
                name: "T_OrderParticipant");

            migrationBuilder.DropTable(
                name: "T_Payment");

            migrationBuilder.DropTable(
                name: "T_Restaurant");

            migrationBuilder.DropTable(
                name: "T_Sticker");

            migrationBuilder.DropTable(
                name: "T_TravelRecord");

            migrationBuilder.DropTable(
                name: "T_Permission");

            migrationBuilder.DropTable(
                name: "T_CustomTravel");

            migrationBuilder.DropTable(
                name: "T_Message");

            migrationBuilder.DropTable(
                name: "T_TravelSupplier");

            migrationBuilder.DropTable(
                name: "T_Order");

            migrationBuilder.DropTable(
                name: "T_OrderForm");

            migrationBuilder.DropTable(
                name: "S_District");

            migrationBuilder.DropTable(
                name: "T_GroupTravel");

            migrationBuilder.DropTable(
                name: "T_ChatRoom");

            migrationBuilder.DropTable(
                name: "T_DocumentMenu");

            migrationBuilder.DropTable(
                name: "S_City");

            migrationBuilder.DropTable(
                name: "T_OfficialTravelDetail");

            migrationBuilder.DropTable(
                name: "T_Member");

            migrationBuilder.DropTable(
                name: "T_OfficialTravel");

            migrationBuilder.DropTable(
                name: "T_Employee");

            migrationBuilder.DropTable(
                name: "T_Region");

            migrationBuilder.DropTable(
                name: "T_Role");
        }
    }
}
