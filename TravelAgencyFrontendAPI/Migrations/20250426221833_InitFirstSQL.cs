using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelAgencyFrontendAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitFirstSQL : Migration
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
                name: "T_Documents",
                columns: table => new
                {
                    DocumentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_Documents", x => x.DocumentId);
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
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IdNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PassportSurname = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PassportGivenName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PassportExpireDate = table.Column<DateTime>(type: "date", nullable: true),
                    Nationality = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DocumentType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DocumentNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PasswordSalt = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    GoogleId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RegisterDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    RememberToken = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    RememberExpireTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsBlacklisted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Note = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime", nullable: true)
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
                    PermissionName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
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
                name: "T_Requirements",
                columns: table => new
                {
                    RequirementId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequirementName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_Requirements", x => x.RequirementId);
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
                name: "T_VisaTypes",
                columns: table => new
                {
                    VisaTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VisaTypeName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_VisaTypes", x => x.VisaTypeId);
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
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IdNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BirthDate = table.Column<DateTime>(type: "date", nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DocumentNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PassportSurname = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PassportGivenName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PassportExpireDate = table.Column<DateTime>(type: "date", nullable: true),
                    Nationality = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETDATE()"),
                    Note = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Active")
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
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    OrderParticipantId = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()"),
                    ParticipantsCount = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_Order", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_T_Order_T_Member_MemberId",
                        column: x => x.MemberId,
                        principalTable: "T_Member",
                        principalColumn: "MemberId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "T_ResetPassword",
                columns: table => new
                {
                    TokenId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MemberId = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedTime = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()"),
                    ExpireTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_ResetPassword", x => x.TokenId);
                    table.ForeignKey(
                        name: "FK_T_ResetPassword_T_Member_MemberId",
                        column: x => x.MemberId,
                        principalTable: "T_Member",
                        principalColumn: "MemberId",
                        onDelete: ReferentialAction.Cascade);
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
                    Note = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
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
                name: "T_VisaInformation",
                columns: table => new
                {
                    VisaInfoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CountryId = table.Column<int>(type: "int", nullable: false),
                    VisaTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_VisaInformation", x => x.VisaInfoId);
                    table.ForeignKey(
                        name: "FK_T_VisaInformation_T_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "T_Countries",
                        principalColumn: "CountryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_T_VisaInformation_T_VisaTypes_VisaTypeId",
                        column: x => x.VisaTypeId,
                        principalTable: "T_VisaTypes",
                        principalColumn: "VisaTypeId",
                        onDelete: ReferentialAction.Cascade);
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
                name: "T_OrderParticipant",
                columns: table => new
                {
                    OrderParticipantId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
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
                name: "T_TravelRecord",
                columns: table => new
                {
                    TravelRecordId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalParticipants = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_TravelRecord", x => x.TravelRecordId);
                    table.ForeignKey(
                        name: "FK_T_TravelRecord_T_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "T_Order",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
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
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    IsBlocked = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    LastMessageAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_ChatRoom", x => x.ChatRoomId);
                    table.ForeignKey(
                        name: "FK_T_ChatRoom_T_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "T_Employee",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_T_ChatRoom_T_Member_MemberId",
                        column: x => x.MemberId,
                        principalTable: "T_Member",
                        principalColumn: "MemberId",
                        onDelete: ReferentialAction.Cascade);
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
                name: "IX_T_ChatRoom_EmployeeId",
                table: "T_ChatRoom",
                column: "EmployeeId");

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
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_MemberFavoriteTraveler_IdNumber",
                table: "T_MemberFavoriteTraveler",
                column: "IdNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_MemberFavoriteTraveler_MemberId",
                table: "T_MemberFavoriteTraveler",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_T_MemberFavoriteTraveler_Phone",
                table: "T_MemberFavoriteTraveler",
                column: "Phone",
                unique: true);

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
                name: "IX_T_OrderParticipant_Email",
                table: "T_OrderParticipant",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_OrderParticipant_IdNumber",
                table: "T_OrderParticipant",
                column: "IdNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_OrderParticipant_OrderId",
                table: "T_OrderParticipant",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_T_OrderParticipant_Phone",
                table: "T_OrderParticipant",
                column: "Phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_ResetPassword_MemberId",
                table: "T_ResetPassword",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_T_Restaurant_DistrictId",
                table: "T_Restaurant",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_T_TravelRecord_OrderId",
                table: "T_TravelRecord",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_T_VisaInformation_CountryId",
                table: "T_VisaInformation",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_T_VisaInformation_VisaTypeId",
                table: "T_VisaInformation",
                column: "VisaTypeId");
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
                name: "T_CustomTravelContent");

            migrationBuilder.DropTable(
                name: "T_Documents");

            migrationBuilder.DropTable(
                name: "T_GroupTravel");

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
                name: "T_OrderParticipant");

            migrationBuilder.DropTable(
                name: "T_Requirements");

            migrationBuilder.DropTable(
                name: "T_ResetPassword");

            migrationBuilder.DropTable(
                name: "T_Restaurant");

            migrationBuilder.DropTable(
                name: "T_Sticker");

            migrationBuilder.DropTable(
                name: "T_TravelRecord");

            migrationBuilder.DropTable(
                name: "T_VisaInformation");

            migrationBuilder.DropTable(
                name: "T_Permission");

            migrationBuilder.DropTable(
                name: "T_CustomTravel");

            migrationBuilder.DropTable(
                name: "T_Message");

            migrationBuilder.DropTable(
                name: "T_TravelSupplier");

            migrationBuilder.DropTable(
                name: "T_OfficialTravelDetail");

            migrationBuilder.DropTable(
                name: "S_District");

            migrationBuilder.DropTable(
                name: "T_Order");

            migrationBuilder.DropTable(
                name: "T_Countries");

            migrationBuilder.DropTable(
                name: "T_VisaTypes");

            migrationBuilder.DropTable(
                name: "T_ChatRoom");

            migrationBuilder.DropTable(
                name: "T_OfficialTravel");

            migrationBuilder.DropTable(
                name: "S_City");

            migrationBuilder.DropTable(
                name: "T_Member");

            migrationBuilder.DropTable(
                name: "T_Employee");

            migrationBuilder.DropTable(
                name: "T_Region");

            migrationBuilder.DropTable(
                name: "T_Role");
        }
    }
}
