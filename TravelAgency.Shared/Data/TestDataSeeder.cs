using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Models;

namespace TravelAgency.Shared.Data
{
    public class TestDataSeeder
    {
        private readonly AppDbContext _context;

        public TestDataSeeder(AppDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            await SeedRolesAsync();
            await SeedEmployeesAsync();
            await SeedMembersAsync();
            await SeedChatRoomsAsync();
            await SeedMessagesAsync();
            await SeedCityAsync();
            await SeedDistrictAsync();
            await SeedAttractionAsyync();
            await SeedRestaurantAsync();
            await SeedAccommodationAsync();
            await SeedTransportAsync();
            await SeedRegionAsync();
            await SeedTravelSupplierAsync();
            await SeedOfficialAccommodationAsync();
            await SeedOfficialRestaurantAsync();
            await SeedOfficialAttractionAsync();
            await SeedOfficialTravelAsync();
            await SeedOfficialTravelDetailAsync();
            await SeedOfficialTravelScheduleAsync();
            await SeedGroupTravelAsync();
            await SeedCustomTravelAsync();
            await SeedCustomTravelContentAsync();
            await SeedPermissionsAsync();
            await SeedRolePermissionsAsync();
        }

        private async Task SeedRolesAsync()
        {
            var predefinedRoles = new List<Role>
            {
                new() { RoleId = 1, RoleName = "系統管理員" },
                new() { RoleId = 2, RoleName = "人資主管" },
                new() { RoleId = 3, RoleName = "業務人員" },
                new() { RoleId = 4, RoleName = "客服人員" },
                new() { RoleId = 5, RoleName = "內容管理員" },
                new() { RoleId = 6, RoleName = "授權人" },
                new() { RoleId = 7, RoleName = "一般員工" }
            };

            var existingIds = _context.Roles.Select(r => r.RoleId).ToHashSet();
            var toAdd = predefinedRoles.Where(r => !existingIds.Contains(r.RoleId)).ToList();

            if (toAdd.Any())
            {
                await _context.Database.OpenConnectionAsync();
                await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT T_Role ON");

                _context.Roles.AddRange(toAdd);
                await _context.SaveChangesAsync();

                await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT T_Role OFF");
                await _context.Database.CloseConnectionAsync();
            }
        }

        private async Task SeedEmployeesAsync()
        {
            if (!_context.Employees.Any())
            {
                var roleId = _context.Roles.First().RoleId;

                _context.Employees.AddRange(
                    new Employee
                    {
                        RoleId = roleId,
                        Name = "測試員工",
                        Password = "Test@123",
                        Email = "employee@test.com",
                        Phone = "0900000000",
                        BirthDate = new DateTime(1990, 1, 1),
                        Note = "這是測試用員工"
                    },
                    new Employee
                    {
                        RoleId = 4,
                        Name = "客服人員",
                        Password = "Test@123",
                        Email = "T@gmail.com",
                        Phone = "0911111111",
                        BirthDate = new DateTime(1955, 1, 1),
                        Address = "高雄前金區"
                });

                await _context.SaveChangesAsync();
            }
        }
        private (string hash, string salt) HashPassword(string password)
        {
            using var hmac = new System.Security.Cryptography.HMACSHA512();
            var salt = Convert.ToBase64String(hmac.Key);
            var hash = Convert.ToBase64String(hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password)));
            return (hash, salt);
        }

        private async Task SeedMembersAsync()
        {
            if (!_context.Members.Any())
            {
                var (hash, salt) = HashPassword("Test@123");

                _context.Members.Add(new Member
                {
                    Name = "葉曄燁",
                    Email = "member1989@gmail.com",
                    Phone = "0925806525",
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    GoogleId = null,
                    IsBlacklisted = false,
                    Note = "這是測試用會員"
                });

                await _context.SaveChangesAsync();
            }
        }
        
        private async Task SeedChatRoomsAsync()
        {
            if (!_context.ChatRooms.Any())
            {
                var member = 11110;
                var employee = 1;

                _context.ChatRooms.Add(new ChatRoom
                {
                    MemberId = member,
                    EmployeeId = employee,
                    IsBlocked = false
                });
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedMessagesAsync()
        {
            if (!_context.Messages.Any())
            {
                var chatRoomId = _context.ChatRooms.First().ChatRoomId;
                _context.Messages.Add(new Message
                {
                    ChatRoomId = chatRoomId,
                    SenderType = SenderType.Employee,
                    SenderId = 1,
                    Content = "這是測試訊息",
                });
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedCityAsync()
        {
            if (!_context.Cities.Any())
            {
                _context.Cities.AddRange(
                    new City { CityName = "臺北市" },
                    new City { CityName = "新北市" }
                    );
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedDistrictAsync()
        {
            if (!_context.Districts.Any())
            {
                _context.Districts.AddRange(
                    new District
                    {
                        CityId = _context.Cities.First().CityId,
                        DistrictName = "信義區"
                    },
                    new District
                    {
                        CityId = _context.Cities.First().CityId,
                        DistrictName = "中山區"
                    },
                    new District
                    {
                        CityId = _context.Cities.Skip(1).First().CityId,
                        DistrictName = "淡水區"
                    }
                    );
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedAttractionAsyync()
        {
            if (!_context.Attractions.Any())
            {
                _context.Attractions.AddRange(
                    new Attraction
                    {
                        DistrictId = _context.Districts.First().DistrictId,
                        AttractionName = "國立國父紀念堂"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.First().DistrictId,
                        AttractionName = "臺北101購物中心"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(1).First().DistrictId,
                        AttractionName = "美麗華百樂園"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(2).First().DistrictId,
                        AttractionName = "淡水老街"
                    }
                    );
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedRestaurantAsync()
        {
            if (!_context.Restaurants.Any())
            {
                _context.Restaurants.AddRange(
                    new Restaurant
                    {
                        DistrictId = _context.Districts.First().DistrictId,
                        RestaurantName = "候布雄法式餐廳"
                    },
                    new Restaurant
                    {
                        DistrictId = _context.Districts.Skip(1).First().DistrictId,
                        RestaurantName = "晶華軒"
                    },
                    new Restaurant
                    {
                        DistrictId = _context.Districts.Skip(1).First().DistrictId,
                        RestaurantName = "頁小館"
                    },
                    new Restaurant
                    {
                        DistrictId = _context.Districts.Skip(2).First().DistrictId,
                        RestaurantName = "米特食堂"
                    }
                    );
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedAccommodationAsync()
        {
            if (!_context.Accommodations.Any())
            {
                _context.Accommodations.AddRange(
                    new Accommodation
                    {
                        DistrictId = _context.Districts.First().DistrictId,
                        AccommodationName = "誠品行旅"
                    },
                    new Accommodation
                    {
                        DistrictId = _context.Districts.Skip(1).First().DistrictId,
                        AccommodationName = "薇閣精品旅館-大直館"
                    },
                    new Accommodation
                    {
                        DistrictId = _context.Districts.Skip(2).First().DistrictId,
                        AccommodationName = "將捷金鬱金香酒店"
                    }
                    );
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedTransportAsync()
        {
            if (!_context.Transports.Any())
            {
                _context.Transports.AddRange(
                    new Transport { TransportMethod = "遊覽車" },
                    new Transport { TransportMethod = "租車" }
                    );
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedRegionAsync()
        {
            if (!_context.Regions.Any())
            {
                _context.Regions.AddRange(
                    new Region { Country = "日本",Name = "北海道" },
                    new Region { Country = "日本", Name = "東北" },
                    new Region { Country = "日本", Name = "關東" },
                    new Region { Country = "日本", Name = "沖繩" },
                    new Region { Country = "日本", Name = "近畿" },
                    new Region { Country = "日本", Name = "中國" },
                    new Region { Country = "日本", Name = "四國" },
                    new Region { Country = "日本", Name = "九州" },
                    new Region { Country = "日本", Name = "中部" }
                );
              await _context.SaveChangesAsync();
             }
         }
      
        private async Task SeedCustomTravelAsync()
        {
            if (!_context.CustomTravels.Any())
            {
                _context.CustomTravels.AddRange(
                    new CustomTravel 
                    { 
                        MemberId = _context.Members.First().MemberId,
                        ReviewEmployeeId =_context.Employees.First().EmployeeId,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        DepartureDate = DateTime.Parse("2025-05-11"),
                        EndDate = DateTime.Parse("2025-05-13"),
                        Days = 3,
                        People = 3,
                        TotalAmount = 30000,
                        Status = CustomTravelStatus.Pending,
                        Note = "測試行程",
                    },
                    new CustomTravel
                    {
                        MemberId = _context.Members.First().MemberId,
                        ReviewEmployeeId = _context.Employees.First().EmployeeId,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        DepartureDate = DateTime.Parse("2025-06-11"),
                        EndDate = DateTime.Parse("2025-06-12"),
                        Days = 2,
                        People = 3,
                        TotalAmount = 20000,
                        Status = CustomTravelStatus.Approved,
                        Note = "測試行程1",
                    },
                    new CustomTravel
                    {
                        MemberId = _context.Members.First().MemberId,
                        ReviewEmployeeId = _context.Employees.First().EmployeeId,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        DepartureDate = DateTime.Parse("2025-05-11"),
                        EndDate = DateTime.Parse("2025-05-11"),
                        Days = 1,
                        People = 2,
                        TotalAmount = 10000,
                        Status = CustomTravelStatus.Completed,
                        Note = "測試行程2",
                    },
                    new CustomTravel
                    {
                        MemberId = _context.Members.First().MemberId,
                        ReviewEmployeeId = _context.Employees.First().EmployeeId,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        DepartureDate = DateTime.Parse("2025-05-13"),
                        EndDate = DateTime.Parse("2025-05-13"),
                        Days = 1,
                        People = 3,
                        TotalAmount = 20000,
                        Status = CustomTravelStatus.Rejected,
                        Note = "測試行程3",
                    });
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedTravelSupplierAsync()
        {
            if (!_context.TravelSuppliers.Any())
            {
                _context.TravelSuppliers.AddRange(
                    new TravelSupplier
                    {
                        SupplierName = "測試飯店供應商",
                        SupplierType = SupplierType.Accommodation,
                        ContactName = "測試飯店聯絡人",
                        ContactPhone = "0900000000",
                        ContactEmail = "testAccomodation@gmail.com",
                        SupplierNote = "這是測試用飯店供應商"

                    },
                    new TravelSupplier
                    {
                        SupplierName = "測試餐廳供應商",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "測試餐廳聯絡人",
                        ContactPhone = "0900000001",
                        ContactEmail = "testRestaurant@gmail.com",
                        SupplierNote = "這是測試用餐廳供應商"
                    },
                    new TravelSupplier
                    {
                        SupplierName = "測試景點供應商",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "測試景點聯絡人",
                        ContactPhone = "0900000002",
                        ContactEmail = "testAttraction@gmail.com",
                        SupplierNote = "這是測試用景點供應商"
                    }
                    );
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedOfficialAccommodationAsync()
        {
            if (!_context.OfficialAccommodations.Any())
            {
                var Accommodation = _context.TravelSuppliers.FirstOrDefault(s => s.SupplierType == SupplierType.Accommodation);
                _context.OfficialAccommodations.AddRange(
                    new OfficialAccommodation
                    {
                        TravelSupplierId = Accommodation.TravelSupplierId,
                        RegionId = 1,
                        Name = "測試官方飯店",
                        Description = "這是測試用飯店",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    }
                    );
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedOfficialRestaurantAsync()
        {
            if (!_context.OfficialRestaurants.Any())
            {
                var Restaurant = _context.TravelSuppliers.FirstOrDefault(s => s.SupplierType == SupplierType.Restaurant);
                _context.OfficialRestaurants.AddRange(
                    new OfficialRestaurant
                    {
                        TravelSupplierId = Restaurant.TravelSupplierId,
                        RegionId = 1,
                        Name = "測試官方餐廳",
                        Description = "這是測試用餐廳",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    }
                    );
                await _context.SaveChangesAsync();
            }
        }
        private async Task SeedOfficialAttractionAsync()
        {
            if (!_context.OfficialAttractions.Any())
            {
                var Attraction = _context.TravelSuppliers.FirstOrDefault(s => s.SupplierType == SupplierType.Attraction);
                _context.OfficialAttractions.AddRange(
                    new OfficialAttraction
                    {
                        TravelSupplierId = Attraction.TravelSupplierId,
                        RegionId = 1,
                        Name = "測試官方景點1",
                        Description = "這是測試用景點1",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = Attraction.TravelSupplierId,
                        RegionId = 1,
                        Name = "測試官方景點2",
                        Description = "這是測試用景點2",
                        Longitude = 122.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = Attraction.TravelSupplierId,
                        RegionId = 2,
                        Name = "測試官方景點3",
                        Description = "這是測試用景點3",
                        Longitude = 123.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = Attraction.TravelSupplierId,
                        RegionId = 2,
                        Name = "測試官方景點4",
                        Description = "這是測試用景點4",
                        Longitude = 124.5654m,
                        Latitude = 25.0330m
                    }
                    );
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedOfficialTravelAsync()
        {
            if (!_context.OfficialTravels.Any())
            {
                _context.OfficialTravels.AddRange(
                    new OfficialTravel
                    {
                        CreatedByEmployeeId = _context.Employees.First().EmployeeId,
                        RegionId = _context.Regions.First().RegionId,
                        ItemId = 1,
                        Category = TravelCategory.Foreign,
                        Title = "國外旅行專案標題",
                        AvailableFrom = new DateTime(2025, 4, 10),
                        AvailableUntil = new DateTime(2025, 8, 10),
                        Description = "這是國外旅行專案",
                        TotalTravelCount = 1,
                        TotalDepartureCount = 10,
                        Days = 7,
                        CoverPath = null,
                        CreatedAt = new DateTime(2024, 8, 11),
                        UpdatedAt = new DateTime(2025, 1, 5),
                        Status = TravelStatus.Active
                    },
                    new OfficialTravel
                    {
                        CreatedByEmployeeId = _context.Employees.First().EmployeeId,
                        RegionId = _context.Regions.First().RegionId,
                        ItemId = 1,
                        Category = TravelCategory.Domestic,
                        Title = "國內旅行專案標題",
                        AvailableFrom = new DateTime(2025, 4, 10),
                        AvailableUntil = new DateTime(2025, 8, 10),
                        Description = "這是國內旅行專案",
                        TotalTravelCount = 1,
                        TotalDepartureCount = 10,
                        Days = 7,
                        CoverPath = null,
                        CreatedAt = new DateTime(2024, 8, 11),
                        UpdatedAt = new DateTime(2025, 1, 5),
                        Status = TravelStatus.Active
                    },
                    new OfficialTravel
                    {
                        CreatedByEmployeeId = _context.Employees.First().EmployeeId,
                        RegionId = _context.Regions.First().RegionId,
                        ItemId = 1,
                        Category = TravelCategory.CruiseShip,
                        Title = "郵輪旅行專案標題",
                        AvailableFrom = new DateTime(2025, 4, 10),
                        AvailableUntil = new DateTime(2025, 8, 10),
                        Description = "這是郵輪旅行專案",
                        TotalTravelCount = 1,
                        TotalDepartureCount = 10,
                        Days = 7,
                        CoverPath = null,
                        CreatedAt = new DateTime(2024, 8, 11),
                        UpdatedAt = new DateTime(2025, 1, 5),
                        Status = TravelStatus.Active
                    },
                    new OfficialTravel
                    {
                        CreatedByEmployeeId = _context.Employees.First().EmployeeId,
                        RegionId = _context.Regions.First().RegionId,
                        ItemId = 1,
                        Category = TravelCategory.Foreign,
                        Title = "國外旅行專案標題[隱藏]",
                        AvailableFrom = new DateTime(2025, 4, 10),
                        AvailableUntil = new DateTime(2025, 8, 10),
                        Description = "這是隱藏的國外旅行專案，你不應該看見它",
                        TotalTravelCount = 1,
                        TotalDepartureCount = 10,
                        Days = 7,
                        CoverPath = null,
                        CreatedAt = new DateTime(2024, 8, 11),
                        UpdatedAt = new DateTime(2025, 1, 5),
                        Status = TravelStatus.Hidden
                    },
                    new OfficialTravel
                    {
                        CreatedByEmployeeId = _context.Employees.First().EmployeeId,
                        RegionId = _context.Regions.First().RegionId,
                        ItemId = 1,
                        Category = TravelCategory.Foreign,
                        Title = "國外旅行專案標題[刪除]",
                        AvailableFrom = new DateTime(2025, 4, 10),
                        AvailableUntil = new DateTime(2025, 8, 10),
                        Description = "這是刪除的國外旅行專案，你不應該看見它",
                        TotalTravelCount = 1,
                        TotalDepartureCount = 10,
                        Days = 7,
                        CoverPath = null,
                        CreatedAt = new DateTime(2024, 8, 11),
                        UpdatedAt = new DateTime(2025, 1, 5),
                        Status = TravelStatus.Deleted
                    },
                    new OfficialTravel
                    {
                        CreatedByEmployeeId = _context.Employees.First().EmployeeId,
                        RegionId = _context.Regions.First().RegionId,
                        ItemId = 1,
                        Category = TravelCategory.Domestic,
                        Title = "國內旅行專案標題[隱藏]",
                        AvailableFrom = new DateTime(2025, 4, 10),
                        AvailableUntil = new DateTime(2025, 8, 10),
                        Description = "這是隱藏的國內旅行專案，你不應該看見它",
                        TotalTravelCount = 1,
                        TotalDepartureCount = 10,
                        Days = 7,
                        CoverPath = null,
                        CreatedAt = new DateTime(2024, 8, 11),
                        UpdatedAt = new DateTime(2025, 1, 5),
                        Status = TravelStatus.Hidden
                    },
                    new OfficialTravel
                    {
                        CreatedByEmployeeId = _context.Employees.First().EmployeeId,
                        RegionId = _context.Regions.First().RegionId,
                        ItemId = 1,
                        Category = TravelCategory.Domestic,
                        Title = "國內旅行專案標題[刪除]",
                        AvailableFrom = new DateTime(2025, 4, 10),
                        AvailableUntil = new DateTime(2025, 8, 10),
                        Description = "這是刪除的國內旅行專案，你不應該看見它",
                        TotalTravelCount = 1,
                        TotalDepartureCount = 10,
                        Days = 7,
                        CoverPath = null,
                        CreatedAt = new DateTime(2024, 8, 11),
                        UpdatedAt = new DateTime(2025, 1, 5),
                        Status = TravelStatus.Deleted
                    },
                    new OfficialTravel
                    {
                        CreatedByEmployeeId = _context.Employees.First().EmployeeId,
                        RegionId = _context.Regions.First().RegionId,
                        ItemId = 1,
                        Category = TravelCategory.CruiseShip,
                        Title = "郵輪旅行專案標題[隱藏]",
                        AvailableFrom = new DateTime(2025, 4, 10),
                        AvailableUntil = new DateTime(2025, 8, 10),
                        Description = "這是隱藏的郵輪旅行專案，你不應該看見它",
                        TotalTravelCount = 1,
                        TotalDepartureCount = 10,
                        Days = 7,
                        CoverPath = null,
                        CreatedAt = new DateTime(2024, 8, 11),
                        UpdatedAt = new DateTime(2025, 1, 5),
                        Status = TravelStatus.Hidden
                    },
                    new OfficialTravel
                    {
                        CreatedByEmployeeId = _context.Employees.First().EmployeeId,
                        RegionId = _context.Regions.First().RegionId,
                        ItemId = 1,
                        Category = TravelCategory.CruiseShip,
                        Title = "郵輪旅行專案標題[刪除]",
                        AvailableFrom = new DateTime(2025, 4, 10),
                        AvailableUntil = new DateTime(2025, 8, 10),
                        Description = "這是刪除的郵輪旅行專案，你不應該看見它",
                        TotalTravelCount = 1,
                        TotalDepartureCount = 10,
                        Days = 7,
                        CoverPath = null,
                        CreatedAt = new DateTime(2024, 8, 11),
                        UpdatedAt = new DateTime(2025, 1, 5),
                        Status = TravelStatus.Deleted
                    }
                    );
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedOfficialTravelDetailAsync()
        {
            if (!_context.OfficialTravelDetails.Any())
            {
                var ForeigntravelId = _context.OfficialTravels.FirstOrDefault(f => f.Category == TravelCategory.Foreign && f.Status == TravelStatus.Active).OfficialTravelId;
                var DomestictravelId = _context.OfficialTravels.FirstOrDefault(f => f.Category == TravelCategory.Domestic && f.Status == TravelStatus.Active).OfficialTravelId;
                var CruisetravelId = _context.OfficialTravels.FirstOrDefault(f => f.Category == TravelCategory.CruiseShip && f.Status == TravelStatus.Active).OfficialTravelId;
                _context.OfficialTravelDetails.AddRange(
                    new OfficialTravelDetail
                    {
                        OfficialTravelId = ForeigntravelId,
                        TravelNumber = 1,
                        AdultPrice = 10000,
                        ChildPrice = 5000,
                        BabyPrice = 0,
                        UpdatedAt = new DateTime(2025, 1, 5),
                        State = DetailState.Locked
                    },
                    new OfficialTravelDetail
                    {
                        OfficialTravelId = DomestictravelId,
                        TravelNumber = 1,
                        AdultPrice = 10000,
                        ChildPrice = 5000,
                        BabyPrice = 0,
                        UpdatedAt = new DateTime(2025, 1, 5),
                        State = DetailState.Locked
                    },
                    new OfficialTravelDetail
                    {
                        OfficialTravelId = CruisetravelId,
                        TravelNumber = 1,
                        AdultPrice = 10000,
                        ChildPrice = 5000,
                        BabyPrice = 0,
                        UpdatedAt = new DateTime(2025, 1, 5),
                        State = DetailState.Locked
                    }
                    );
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedOfficialTravelScheduleAsync()
        {
            if (!_context.OfficialTravelSchedules.Any())
            {
                var FDetailId = _context.OfficialTravelDetails.FirstOrDefault(f => f.OfficialTravel.Category == TravelCategory.Foreign && f.State == DetailState.Locked).OfficialTravelDetailId;
                var DDetailId = _context.OfficialTravelDetails.FirstOrDefault(f => f.OfficialTravel.Category == TravelCategory.Domestic && f.State == DetailState.Locked).OfficialTravelDetailId;
                var CDetailId = _context.OfficialTravelDetails.FirstOrDefault(f => f.OfficialTravel.Category == TravelCategory.CruiseShip && f.State == DetailState.Locked).OfficialTravelDetailId;
                _context.OfficialTravelSchedules.AddRange(
                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = FDetailId,
                        Day = 1,
                        Description = "這是國外旅行行程第一天的描述",
                        Breakfast = "這是國外旅行行程第一天的早餐",
                        Lunch = "這是國外旅行行程第一天的午餐",
                        Dinner = "這是國外旅行行程第一天的晚餐",
                        Hotel = "這是國外旅行行程第一天的飯店",
                        Attraction1 = _context.OfficialAttractions.First().AttractionId,
                        Attraction2 = null,
                        Attraction3 = null,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "這是國外旅行行程第一天的備註1",
                        Note2 = "這是國外旅行行程第一天的備註2"
                    },
                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = FDetailId,
                        Day = 2,
                        Description = "這是國外旅行行程第二天的描述",
                        Breakfast = "這是國外旅行行程第二天的早餐",
                        Lunch = "這是國外旅行行程第二天的午餐",
                        Dinner = "這是國外旅行行程第二天的晚餐",
                        Hotel = "這是國外旅行行程第二天的飯店",
                        Attraction1 = _context.OfficialAttractions.Skip(1).First().AttractionId,
                        Attraction2 = null,
                        Attraction3 = null,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "這是國外旅行行程第二天的備註1",
                        Note2 = "這是國外旅行行程第二天的備註2"
                    },
                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = DDetailId,
                        Day = 1,
                        Description = "這是國內旅行行程第一天的描述",
                        Breakfast = "這是國內旅行行程第一天的早餐",
                        Lunch = "這是國內旅行行程第一天的午餐",
                        Dinner = "這是國內旅行行程第一天的晚餐",
                        Hotel = "這是國內旅行行程第一天的飯店",
                        Attraction1 = _context.OfficialAttractions.Skip(2).First().AttractionId,
                        Attraction2 = null,
                        Attraction3 = null,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "這是國內旅行行程第一天的備註1",
                        Note2 = "這是國內旅行行程第一天的備註2"
                    },
                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = DDetailId,
                        Day = 2,
                        Description = "這是國內旅行行程第二天的描述",
                        Breakfast = "這是國內旅行行程第二天的早餐",
                        Lunch = "這是國內旅行行程第二天的午餐",
                        Dinner = "這是國內旅行行程第二天的晚餐",
                        Hotel = "這是國內旅行行程第二天的飯店",
                        Attraction1 = _context.OfficialAttractions.Skip(3).First().AttractionId,
                        Attraction2 = null,
                        Attraction3 = null,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "這是國內旅行行程第二天的備註1",
                        Note2 = "這是國內旅行行程第二天的備註2"
                    },
                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = CDetailId,
                        Day = 1,
                        Description = "這是郵輪旅行行程第一天的描述",
                        Breakfast = "這是郵輪旅行行程第一天的早餐",
                        Lunch = "這是郵輪旅行行程第一天的午餐",
                        Dinner = "這是郵輪旅行行程第一天的晚餐",
                        Hotel = "這是郵輪旅行行程第一天的飯店",
                        Attraction1 = _context.OfficialAttractions.First().AttractionId,
                        Attraction2 = null,
                        Attraction3 = null,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "這是郵輪旅行行程第一天的備註1",
                        Note2 = "這是郵輪旅行行程第一天的備註2"
                    },
                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = CDetailId,
                        Day = 2,
                        Description = "這是郵輪旅行行程第二天的描述",
                        Breakfast = "這是郵輪旅行行程第二天的早餐",
                        Lunch = "這是郵輪旅行行程第二天的午餐",
                        Dinner = "這是郵輪旅行行程第二天的晚餐",
                        Hotel = "這是郵輪旅行行程第二天的飯店",
                        Attraction1 = _context.OfficialAttractions.Skip(1).First().AttractionId,
                        Attraction2 = null,
                        Attraction3 = null,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "這是郵輪旅行行程第二天的備註1",
                        Note2 = "這是郵輪旅行行程第二天的備註2"
                    });
                     await _context.SaveChangesAsync();
            }
        }

        private async Task SeedCustomTravelContentAsync()
        {
            if (!_context.CustomTravelContents.Any())
            {
                _context.CustomTravelContents.AddRange(
                    new CustomTravelContent
                    {
                        CustomTravelId = _context.CustomTravels.First().CustomTravelId,
                        ItemId = _context.Restaurants.First().RestaurantId,
                        Category = TravelItemCategory.Restaurant,
                        Day = 1,
                        Time = "11:00",
                        AccommodationName = "測試"
                    },
                    new CustomTravelContent
                    {
                        CustomTravelId = _context.CustomTravels.First().CustomTravelId,
                        ItemId = _context.Attractions.First().AttractionId,
                        Category = TravelItemCategory.Attraction,
                        Day = 2,
                        Time = "13:00",
                        AccommodationName = "測試1"
                    },
                    new CustomTravelContent
                    {
                        CustomTravelId = _context.CustomTravels.First().CustomTravelId,
                        ItemId = _context.Accommodations.First().AccommodationId,
                        Category = TravelItemCategory.Accommodation,
                        Day = 3,
                        Time = "12:00",
                        AccommodationName = "測試2"
                    },
                    new CustomTravelContent
                    {
                        CustomTravelId = _context.CustomTravels.Skip(1).First().CustomTravelId,
                        ItemId = _context.Transports.First().TransportId,
                        Category = TravelItemCategory.Transport,
                        Day = 1,
                        Time = "12:00",
                        AccommodationName = "1測試"
                    },
                    new CustomTravelContent
                    {
                        CustomTravelId = _context.CustomTravels.Skip(1).First().CustomTravelId,
                        ItemId = _context.Accommodations.First().AccommodationId,
                        Category = TravelItemCategory.Accommodation,
                        Day = 2,
                        Time = "12:00",
                        AccommodationName = "1測試1"
                    },
                    new CustomTravelContent
                    {
                        CustomTravelId = _context.CustomTravels.Skip(2).First().CustomTravelId,
                        ItemId = _context.Restaurants.First().RestaurantId,
                        Category = TravelItemCategory.Restaurant,
                        Day = 1,
                        Time = "12:00",
                        AccommodationName = "2測試"
                    },
                    new CustomTravelContent
                    {
                        CustomTravelId = _context.CustomTravels.Skip(3).First().CustomTravelId,
                        ItemId = _context.Accommodations.First().AccommodationId,
                        Category = TravelItemCategory.Accommodation,
                        Day = 1,
                        Time = "12:00",
                        AccommodationName = "3測試"
                    }
                    );
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedGroupTravelAsync()
        {
            if (!_context.GroupTravels.Any())
            {
                var FDetailId = _context.OfficialTravelDetails.FirstOrDefault(f => f.OfficialTravel.Category == TravelCategory.Foreign && f.State == DetailState.Locked).OfficialTravelDetailId;
                var DDetailId = _context.OfficialTravelDetails.FirstOrDefault(f => f.OfficialTravel.Category == TravelCategory.Domestic && f.State == DetailState.Locked).OfficialTravelDetailId;
                var CDetailId = _context.OfficialTravelDetails.FirstOrDefault(f => f.OfficialTravel.Category == TravelCategory.CruiseShip && f.State == DetailState.Locked).OfficialTravelDetailId;
                _context.GroupTravels.AddRange(
                     new GroupTravel
                     {
                         OfficialTravelDetailId = FDetailId,
                         DepartureDate = new DateTime(2025, 4, 10),
                         ReturnDate = new DateTime(2025, 4, 17),
                         TotalSeats = 30,
                         SoldSeats = 0,
                         OrderDeadline = new DateTime(2025, 3, 1),
                         MinimumParticipants = 10,
                         GroupStatus = "開團",
                         CreatedAt = new DateTime(2024, 8, 11),
                         UpdatedAt = new DateTime(2025, 1, 5),
                         RecordStatus = "正常"

                     },
                     new GroupTravel
                     {
                         OfficialTravelDetailId = DDetailId,
                         DepartureDate = new DateTime(2025, 4, 10),
                         ReturnDate = new DateTime(2025, 4, 17),
                         TotalSeats = 30,
                         SoldSeats = 0,
                         OrderDeadline = new DateTime(2025, 3, 1),
                         MinimumParticipants = 10,
                         GroupStatus = "開團",
                         CreatedAt = new DateTime(2024, 8, 11),
                         UpdatedAt = new DateTime(2025, 1, 5),
                         RecordStatus = "正常"
                     },
                     new GroupTravel
                     {
                         OfficialTravelDetailId = CDetailId,
                         DepartureDate = new DateTime(2025, 4, 10),
                         ReturnDate = new DateTime(2025, 4, 17),
                         TotalSeats = 30,
                         SoldSeats = 0,
                         OrderDeadline = new DateTime(2025, 3, 1),
                         MinimumParticipants = 10,
                         GroupStatus = "開團",
                         CreatedAt = new DateTime(2024, 8, 11),
                         UpdatedAt = new DateTime(2025, 1, 5),
                         RecordStatus = "正常"
                     }
                     );
                await _context.SaveChangesAsync();
            }
        }
        //權限假資料
        private async Task SeedPermissionsAsync()
        {
            var predefinedPermissions = new List<Permission>
            {
                new() { PermissionId = 1, PermissionName = "查看會員", Caption = "可進入會員列表並查看基本資料" },
                new() { PermissionId = 2, PermissionName = "管理會員", Caption = "可新增、編輯、刪除會員資料" },
                new() { PermissionId = 3, PermissionName = "修改會員密碼", Caption = "可由後台變更會員密碼" },
                new() { PermissionId = 4, PermissionName = "查看參與人", Caption = "可瀏覽所有參與人資料" },
                new() { PermissionId = 5, PermissionName = "管理參與人", Caption = "可為會員新增/編輯/刪除參與人" },
                new() { PermissionId = 6, PermissionName = "查看員工", Caption = "可進入員工列表頁" },
                new() { PermissionId = 7, PermissionName = "管理員工", Caption = "可新增、編輯、刪除員工" },
                new() { PermissionId = 8, PermissionName = "管理角色", Caption = "可管理角色與其權限" },
                new() { PermissionId = 9, PermissionName = "設定角色權限", Caption = "可為角色指派權限" },
                new() { PermissionId = 10, PermissionName = "管理聊天室", Caption = "可檢視聊天室、發送訊息、關閉聊天室" },
                new() { PermissionId = 11, PermissionName = "查看公告", Caption = "可檢視公告內容" },
                new() { PermissionId = 12, PermissionName = "發布公告", Caption = "可新增或編輯公告內容" },
                new() { PermissionId = 13, PermissionName = "管理權限", Caption = "可 CRUD 權限表（Permission）" },
                new() { PermissionId = 14, PermissionName = "查看客製化行程", Caption = "可進入客製化列表並查看" },
                new() { PermissionId = 15, PermissionName = "管理客製化行程", Caption = "可新增、編輯、刪除客製化行程" },
                new() { PermissionId = 16, PermissionName = "查看官方行程", Caption = "可進入官方列表並查看" },
                new() { PermissionId = 17, PermissionName = "管理官方行程", Caption = "可新增、編輯、刪除官方行程" },
                new() { PermissionId = 18, PermissionName = "查看訂單", Caption = "可進入訂單列表並查看" },
                new() { PermissionId = 19, PermissionName = "管理訂單", Caption = "可新增、編輯、刪除管理訂單" },
                new() { PermissionId = 20, PermissionName = "查看首頁", Caption = "可進入首頁並查看" },
                new() { PermissionId = 21, PermissionName = "查看購物車", Caption = "可檢視購物車" },
                new() { PermissionId = 22, PermissionName = "管理購物車", Caption = "可新增、編輯、刪除購物車" }
            };

            var existingIds = _context.Permissions.Select(p => p.PermissionId).ToHashSet();
            var toAdd = predefinedPermissions.Where(p => !existingIds.Contains(p.PermissionId)).ToList();

            if (toAdd.Any())
            {
                await _context.Database.OpenConnectionAsync();
                await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT T_Permission ON");

                _context.Permissions.AddRange(toAdd);
                await _context.SaveChangesAsync();

                await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT T_Permission OFF");
                await _context.Database.CloseConnectionAsync();
            }
        }
        //角色對應權限假資料
        private async Task SeedRolePermissionsAsync()
        {
            var now = DateTime.Now;
            var mappings = new List<(int RoleId, int PermissionId)>
            {
                (1, 1), (1, 2), (1, 3), (1, 4), (1, 5), (1, 6), (1, 7), (1, 8), (1, 9),
                (1, 10), (1, 11), (1, 12), (1, 13), (1, 14), (1, 15), (1, 16), (1, 17), (1, 18), (1, 19), (1, 20), (1, 21), (1, 22),
                (2, 1), (2, 2), (2, 20),
                (3, 14), (3, 15), (3, 16), (3, 17), (3, 20),
                (4, 1), (4, 2), (4, 3), (4, 4), (4, 5), (4, 10), (4, 20),
                (5, 11), (5, 12), (5, 20),
                (6, 1), (6, 8), (6, 9), (6, 13), (6, 20),
                (7, 20)
            };

            var existing = _context.RolePermissions
                .Select(rp => new { rp.RoleId, rp.PermissionId })
                .ToHashSet();

            var toAdd = mappings
                .Where(m => !existing.Contains(new { m.RoleId, m.PermissionId }))
                .Select(m => new RolePermission
                {
                    RoleId = m.RoleId,
                    PermissionId = m.PermissionId,
                    CreatedAt = now
                })
                .ToList();

            if (toAdd.Any())
            {
                _context.RolePermissions.AddRange(toAdd);
                await _context.SaveChangesAsync();
            }
        }

    }
}
