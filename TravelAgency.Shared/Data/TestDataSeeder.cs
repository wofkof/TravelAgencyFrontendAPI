using Microsoft.EntityFrameworkCore;
using System.Net;
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
            await SeedMemberFavoriteTravelerAsync();
            await SeedAnnouncementAsync();
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
        //新增測試常用旅客
        private async Task SeedMemberFavoriteTravelerAsync()
        {
            if (!_context.MemberFavoriteTravelers.Any())
            {
                _context.MemberFavoriteTravelers.AddRange(
                    new MemberFavoriteTraveler
                    {
                        MemberId = 11110,
                        Name = "陳小華",
                        Phone = "0938987153",
                        IdNumber = "A228370009",
                        BirthDate = new DateTime(1996, 4, 1),
                        Gender = GenderType.Female,
                        Email = "Hua27@gmail.com",
                        DocumentType = DocumentType.ID_CARD_TW,
                        Nationality = "TW"

                    },
                    new MemberFavoriteTraveler
                    {
                        MemberId = 11110,
                        Name = "葉宣",
                        Phone = "0926733461",
                        IdNumber = "S245876990",
                        BirthDate = new DateTime(2003, 3, 16),
                        Gender = GenderType.Female,
                        Email = "Xuan2003@gmail.com",
                        DocumentType = DocumentType.ID_CARD_TW,
                        Nationality = "TW"
                    },
                    new MemberFavoriteTraveler
                    {
                        MemberId = 11110,
                        Name = "葉廷",
                        Phone = "0938992091",
                        IdNumber = "S136756120",
                        BirthDate = new DateTime(2003, 3, 17),
                        Gender = GenderType.Male,
                        Email = "Ting0317@gmail.com",
                        DocumentType = DocumentType.ID_CARD_TW,
                        Nationality = "TW"
                    }
                    );
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
                    new City { CityName = "新北市" },
                    new City { CityName = "臺南市" },
                    new City { CityName = "高雄市" },
                    new City { CityName = "嘉義市" },
                    new City { CityName = "嘉義縣" },
                    new City { CityName = "屏東縣" },
                    new City { CityName = "新竹市" },
                    new City { CityName = "新竹縣" },
                    new City { CityName = "桃園市" },
                    new City { CityName = "臺中市" },
                    new City { CityName = "苗栗縣" },
                    new City { CityName = "南投縣" },
                    new City { CityName = "彰化縣" },
                    new City { CityName = "雲林縣" },
                    new City { CityName = "基隆市" },
                    new City { CityName = "宜蘭縣" },
                    new City { CityName = "花蓮縣" },
                    new City { CityName = "臺東縣" },
                    new City { CityName = "澎湖縣" },
                    new City { CityName = "金門縣" },
                    new City { CityName = "連江縣" }
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
                        CityId = _context.Cities.First().CityId,
                        DistrictName = "中正區"
                    },
                    new District
                    {
                        CityId = _context.Cities.First().CityId,
                        DistrictName = "松山區"
                    },
                    new District
                    {
                        CityId = _context.Cities.First().CityId,
                        DistrictName = "大同區"
                    },
                    new District
                    {
                        CityId = _context.Cities.First().CityId,
                        DistrictName = "大安區"
                    },
                    new District
                    {
                        CityId = _context.Cities.First().CityId,
                        DistrictName = "萬華區"
                    },
                    new District
                    {
                        CityId = _context.Cities.First().CityId,
                        DistrictName = "士林區"
                    },
                    new District
                    {
                        CityId = _context.Cities.First().CityId,
                        DistrictName = "北投區"
                    },
                    new District
                    {
                        CityId = _context.Cities.First().CityId,
                        DistrictName = "內湖區"
                    },
                    new District
                    {
                        CityId = _context.Cities.First().CityId,
                        DistrictName = "南港區"
                    },
                    new District
                    {
                        CityId = _context.Cities.First().CityId,
                        DistrictName = "文山區"
                    },
                    new District
                    {
                        CityId = _context.Cities.Skip(1).First().CityId,
                        DistrictName = "淡水區"
                    },
                    new District
                    {
                        CityId = _context.Cities.Skip(1).First().CityId,
                        DistrictName = "板橋區"
                    },
                    new District
                    {
                        CityId = _context.Cities.Skip(1).First().CityId,
                        DistrictName = "永和區"
                    },
                    new District
                    {
                        CityId = _context.Cities.Skip(1).First().CityId,
                        DistrictName = "烏來區"
                    },
                    new District
                    {
                        CityId = _context.Cities.Skip(1).First().CityId,
                        DistrictName = "三峽區"
                    },
                    new District
                    {
                        CityId = _context.Cities.Skip(1).First().CityId,
                        DistrictName = "樹林區"
                    },
                    new District
                    {
                        CityId = _context.Cities.Skip(1).First().CityId,
                        DistrictName = "鶯歌區"
                    },
                    new District
                    {
                        CityId = _context.Cities.Skip(1).First().CityId,
                        DistrictName = "三重區"
                    },
                    new District
                    {
                        CityId = _context.Cities.Skip(1).First().CityId,
                        DistrictName = "新莊區"
                    },
                    new District
                    {
                        CityId = _context.Cities.Skip(1).First().CityId,
                        DistrictName = "三芝區"
                    },
                    new District
                    {
                        CityId = _context.Cities.Skip(2).First().CityId,
                        DistrictName = "中西區"
                    },
                    new District
                    {
                        CityId = _context.Cities.Skip(2).First().CityId,
                        DistrictName = "東區"
                    },
                    new District
                    {
                        CityId = _context.Cities.Skip(2).First().CityId,
                        DistrictName = "南區"
                    },
                    new District
                    {
                        CityId = _context.Cities.Skip(2).First().CityId,
                        DistrictName = "安平區"
                    },
                    new District
                    {
                        CityId = _context.Cities.Skip(2).First().CityId,
                        DistrictName = "永康區"
                    },
                    new District
                    {
                        CityId = _context.Cities.Skip(2).First().CityId,
                        DistrictName = "仁德區"
                    },
                    new District
                    {
                        CityId = _context.Cities.Skip(2).First().CityId,
                        DistrictName = "鹽水區"
                    },
                    new District
                    {
                        CityId = _context.Cities.Skip(2).First().CityId,
                        DistrictName = "七股區"
                    },
                    new District
                    {
                        CityId = _context.Cities.Skip(2).First().CityId,
                        DistrictName = "北區"
                    },
                    new District
                    {
                        CityId = _context.Cities.Skip(2).First().CityId,
                        DistrictName = "善化區"
                    },
                    new District
                    {
                        CityId = _context.Cities.Skip(3).First().CityId,
                        DistrictName = "前金區"
                    },
                    new District
                    {
                        CityId = _context.Cities.Skip(3).First().CityId,
                        DistrictName = "鹽埕區"
                    },
                    new District
                    {
                        CityId = _context.Cities.Skip(3).First().CityId,
                        DistrictName = "鼓山區"
                    },
                    new District
                    {
                        CityId = _context.Cities.Skip(3).First().CityId,
                        DistrictName = "旗津區"
                    },
                    new District
                    {
                        CityId = _context.Cities.Skip(3).First().CityId,
                        DistrictName = "左營區"
                    },
                    new District
                    {
                        CityId = _context.Cities.Skip(3).First().CityId,
                        DistrictName = "三民區"
                    },
                    new District
                    {
                        CityId = _context.Cities.Skip(3).First().CityId,
                        DistrictName = "美濃區"
                    },
                    new District
                    {
                        CityId = _context.Cities.Skip(3).First().CityId,
                        DistrictName = "旗山區"
                    },
                    new District
                    {
                        CityId = _context.Cities.Skip(3).First().CityId,
                        DistrictName = "鳳山區"
                    },
                    new District
                    {
                        CityId = _context.Cities.Skip(3).First().CityId,
                        DistrictName = "橋頭區"
                    },
                    new District
                    {
                        CityId = _context.Cities.Skip(3).First().CityId,
                        DistrictName = "小港區"
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
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(22).First().DistrictId,
                        AttractionName = "赤崁樓"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(22).First().DistrictId,
                        AttractionName = "原林百貨"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(22).First().DistrictId,
                        AttractionName = "台南市美術館2館"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(22).First().DistrictId,
                        AttractionName = "神農街"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(22).First().DistrictId,
                        AttractionName = "水仙宮"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(22).First().DistrictId,
                        AttractionName = "臺灣祀典武廟"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(22).First().DistrictId,
                        AttractionName = "國定古蹟臺南地方法院(司法博物館)"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(22).First().DistrictId,
                        AttractionName = "臺灣府城隍廟"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(23).First().DistrictId,
                        AttractionName = "南紡購物中心"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(23).First().DistrictId,
                        AttractionName = "台南文化創意產業園區"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(23).First().DistrictId,
                        AttractionName = "知事官邸生活館"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(23).First().DistrictId,
                        AttractionName = "東門巴克禮紀念教會"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(24).First().DistrictId,
                        AttractionName = "藍晒圖文創園區"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(24).First().DistrictId,
                        AttractionName = "水交社文化園區"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(24).First().DistrictId,
                        AttractionName = "黃金海岸"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(24).First().DistrictId,
                        AttractionName = "鯤喜灣文化園區"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(24).First().DistrictId,
                        AttractionName = "臺南市客家文化會館"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(24).First().DistrictId,
                        AttractionName = "黑橋牌香腸博物館"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(25).First().DistrictId,
                        AttractionName = "安平古堡"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(25).First().DistrictId,
                        AttractionName = "億載金城 (二鯤鯓砲台)"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(25).First().DistrictId,
                        AttractionName = "知事官邸生活館"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(25).First().DistrictId,
                        AttractionName = "原英商德記洋行"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(25).First().DistrictId,
                        AttractionName = "安平蚵灰窯文化館"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(25).First().DistrictId,
                        AttractionName = "台南運河博物館"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(25).First().DistrictId,
                        AttractionName = "孔廟文化園區「臺南孔子廟」"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(25).First().DistrictId,
                        AttractionName = "安平樹屋"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(25).First().DistrictId,
                        AttractionName = "觀夕平台"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(25).First().DistrictId,
                        AttractionName = "安平老街(延平老街)"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(25).First().DistrictId,
                        AttractionName = "安平天后宮(開臺天后宮)"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(25).First().DistrictId,
                        AttractionName = "台南運河遊船"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(25).First().DistrictId,
                        AttractionName = "安平遊憩碼頭"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(25).First().DistrictId,
                        AttractionName = "虱目魚主題館"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(26).First().DistrictId,
                        AttractionName = "四草綠色隧道"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(26).First().DistrictId,
                        AttractionName = "鹿耳門天后宮"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(26).First().DistrictId,
                        AttractionName = "台江文化中心"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(26).First().DistrictId,
                        AttractionName = "國立臺灣歷史博物館"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(26).First().DistrictId,
                        AttractionName = "布袋嘴寮代天府(百年魚木)"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(27).First().DistrictId,
                        AttractionName = "奇美博物館"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(27).First().DistrictId,
                        AttractionName = "十鼓文創園區"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(27).First().DistrictId,
                        AttractionName = "台南都會公園"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(27).First().DistrictId,
                        AttractionName = "亞力山大蝴蝶生態教育農場"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(27).First().DistrictId,
                        AttractionName = "臺南家具產業博物館"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(29).First().DistrictId,
                        AttractionName = "七股鹽山"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(29).First().DistrictId,
                        AttractionName = "紅樹林賞鳥區"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(29).First().DistrictId,
                        AttractionName = "黑面琵鷺生態展示館（原黑面琵鷺研究及保育管理中心）"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(29).First().DistrictId,
                        AttractionName = "七股觀海樓"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(29).First().DistrictId,
                        AttractionName = "曾文溪口濕地"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(29).First().DistrictId,
                        AttractionName = "南灣碼頭遊憩區"
                    },
                    new Attraction
                    {
                        DistrictId = _context.Districts.Skip(30).First().DistrictId,
                        AttractionName = "花園夜市"
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
                    },
                    new Restaurant
                    {
                        DistrictId = _context.Districts.Skip(22).First().DistrictId,
                        RestaurantName = "邱家小卷米粉"
                    },
                    new Restaurant
                    {
                        DistrictId = _context.Districts.Skip(22).First().DistrictId,
                        RestaurantName = "富盛號"
                    },
                    new Restaurant
                    {
                        DistrictId = _context.Districts.Skip(22).First().DistrictId,
                        RestaurantName = "保安路米糕"
                    },
                    new Restaurant
                    {
                        DistrictId = _context.Districts.Skip(22).First().DistrictId,
                        RestaurantName = "鼎富發豬油拌飯"
                    },
                    new Restaurant
                    {
                        DistrictId = _context.Districts.Skip(22).First().DistrictId,
                        RestaurantName = "矮仔成蝦仁飯"
                    },
                    new Restaurant
                    {
                        DistrictId = _context.Districts.Skip(22).First().DistrictId,
                        RestaurantName = "一味品碗粿魚羹"
                    },
                    new Restaurant
                    {
                        DistrictId = _context.Districts.Skip(22).First().DistrictId,
                        RestaurantName = "小豪洲沙茶爐"
                    },
                    new Restaurant
                    {
                        DistrictId = _context.Districts.Skip(22).First().DistrictId,
                        RestaurantName = "阿明豬心冬粉"
                    },
                    new Restaurant
                    {
                        DistrictId = _context.Districts.Skip(22).First().DistrictId,
                        RestaurantName = "集品蝦仁飯"
                    },
                    new Restaurant
                    {
                        DistrictId = _context.Districts.Skip(22).First().DistrictId,
                        RestaurantName = "度小月擔仔麵"
                    },
                    new Restaurant
                    {
                        DistrictId = _context.Districts.Skip(22).First().DistrictId,
                        RestaurantName = "阿堂鹹粥"
                    },
                    new Restaurant
                    {
                        DistrictId = _context.Districts.Skip(22).First().DistrictId,
                        RestaurantName = "老厝1933"
                    },
                    new Restaurant
                    {
                        DistrictId = _context.Districts.Skip(22).First().DistrictId,
                        RestaurantName = "懷舊小棧杏仁豆腐冰"
                    },
                    new Restaurant
                    {
                        DistrictId = _context.Districts.Skip(22).First().DistrictId,
                        RestaurantName = "冰鄉"
                    },
                    new Restaurant
                    {
                        DistrictId = _context.Districts.Skip(23).First().DistrictId,
                        RestaurantName = "周氏蝦捲台南總店"
                    },
                    new Restaurant
                    {
                        DistrictId = _context.Districts.Skip(23).First().DistrictId,
                        RestaurantName = "阿財牛肉湯"
                    },
                    new Restaurant
                    {
                        DistrictId = _context.Districts.Skip(23).First().DistrictId,
                        RestaurantName = "同記安平豆花"
                    },
                    new Restaurant
                    {
                        DistrictId = _context.Districts.Skip(23).First().DistrictId,
                        RestaurantName = "文章牛肉湯"
                    },
                    new Restaurant
                    {
                        DistrictId = _context.Districts.Skip(23).First().DistrictId,
                        RestaurantName = "屋裏的湯"
                    },
                    new Restaurant
                    {
                        DistrictId = _context.Districts.Skip(23).First().DistrictId,
                        RestaurantName = "王氏魚皮"
                    },
                    new Restaurant
                    {
                        DistrictId = _context.Districts.Skip(23).First().DistrictId,
                        RestaurantName = "捌伍貳冰室"
                    },
                    new Restaurant
                    {
                        DistrictId = _context.Districts.Skip(23).First().DistrictId,
                        RestaurantName = "滿玥軒"
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
                    },
                    new Accommodation
                    {
                        DistrictId = _context.Districts.Skip(22).First().DistrictId,
                        AccommodationName = "台南晶英酒店"
                    },
                    new Accommodation
                    {
                        DistrictId = _context.Districts.Skip(22).First().DistrictId,
                        AccommodationName = "台南大飯店"
                    },
                    new Accommodation
                    {
                        DistrictId = _context.Districts.Skip(22).First().DistrictId,
                        AccommodationName = "煙波大飯店台南館"
                    },
                    new Accommodation
                    {
                        DistrictId = _context.Districts.Skip(22).First().DistrictId,
                        AccommodationName = "康橋商旅"
                    },
                    new Accommodation
                    {
                        DistrictId = _context.Districts.Skip(22).First().DistrictId,
                        AccommodationName = "台南富驛時尚酒店"
                    },
                    new Accommodation
                    {
                        DistrictId = _context.Districts.Skip(23).First().DistrictId,
                        AccommodationName = "台南老爺行旅"
                    },
                    new Accommodation
                    {
                        DistrictId = _context.Districts.Skip(23).First().DistrictId,
                        AccommodationName = "台南遠東香格里拉"
                    },
                    new Accommodation
                    {
                        DistrictId = _context.Districts.Skip(23).First().DistrictId,
                        AccommodationName = "台糖長榮酒店"
                    },
                    new Accommodation
                    {
                        DistrictId = _context.Districts.Skip(23).First().DistrictId,
                        AccommodationName = "路徒行旅"
                    },
                    new Accommodation
                    {
                        DistrictId = _context.Districts.Skip(23).First().DistrictId,
                        AccommodationName = "塔木德酒店"
                    },
                    new Accommodation
                    {
                        DistrictId = _context.Districts.Skip(25).First().DistrictId,
                        AccommodationName = "VIVA漁樂活台南安平民宿"
                    },
                    new Accommodation
                    {
                        DistrictId = _context.Districts.Skip(25).First().DistrictId,
                        AccommodationName = "台南大員皇冠假日酒店"
                    },
                    new Accommodation
                    {
                        DistrictId = _context.Districts.Skip(25).First().DistrictId,
                        AccommodationName = "安平留飯店"
                    },
                    new Accommodation
                    {
                        DistrictId = _context.Districts.Skip(25).First().DistrictId,
                        AccommodationName = "臺邦商旅"
                    },
                    new Accommodation
                    {
                        DistrictId = _context.Districts.Skip(25).First().DistrictId,
                        AccommodationName = "福爾摩沙遊艇酒店"
                    },
                    new Accommodation
                    {
                        DistrictId = _context.Districts.Skip(25).First().DistrictId,
                        AccommodationName = "樹屋文旅"
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
                    new Transport { TransportMethod = "租車" },
                    new Transport { TransportMethod = "汽車" },
                    new Transport { TransportMethod = "騎車" },
                    new Transport { TransportMethod = "客運" },
                    new Transport { TransportMethod = "台鐵" },
                    new Transport { TransportMethod = "高鐵" },
                    new Transport { TransportMethod = "捷運" },
                    new Transport { TransportMethod = "自行車" },
                    new Transport { TransportMethod = "步行" },
                    new Transport { TransportMethod = "公車" },
                    new Transport { TransportMethod = "計程車" },
                    new Transport { TransportMethod = "接駁車" },
                    new Transport { TransportMethod = "包車服務" }
                    );
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedRegionAsync()
        {
            if (!_context.Regions.Any())
            {
                _context.Regions.AddRange(
                    new Region { Country = "日本", Name = "北海道" },
                    new Region { Country = "日本", Name = "東北" },
                    new Region { Country = "日本", Name = "關東" },
                    new Region { Country = "日本", Name = "沖繩" },
                    new Region { Country = "日本", Name = "近畿" },
                    new Region { Country = "日本", Name = "中國" },
                    new Region { Country = "日本", Name = "四國" },
                    new Region { Country = "日本", Name = "九州" },
                    new Region { Country = "日本", Name = "中部" },
                    new Region { Country = "台灣", Name = "北部" },
                    new Region { Country = "台灣", Name = "中部" },
                    new Region { Country = "台灣", Name = "東部" },
                    new Region { Country = "台灣", Name = "南部" },
                    new Region { Country = "台灣", Name = "離島" }
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
                        ReviewEmployeeId = _context.Employees.First().EmployeeId,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        DepartureDate = DateTime.Parse("2025-07-11"),
                        EndDate = DateTime.Parse("2025-07-12"),
                        Days = 2,
                        People = 3,
                        TotalAmount = 10000,
                        Status = CustomTravelStatus.Completed,
                        Note = "台南旅遊",
                    },
                    new CustomTravel
                    {
                        MemberId = _context.Members.First().MemberId,
                        ReviewEmployeeId = _context.Employees.First().EmployeeId,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        DepartureDate = DateTime.Parse("2025-06-22"),
                        EndDate = DateTime.Parse("2025-06-22"),
                        Days = 1,
                        People = 1,
                        TotalAmount = 1500,
                        Status = CustomTravelStatus.Rejected,
                        Note = "一人放空行程",
                    }
                    );
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
                        OfficialTravelId = ForeigntravelId,
                        TravelNumber = 2,
                        AdultPrice = 15000,
                        ChildPrice = 9000,
                        BabyPrice = 1000,
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
                        Description = "這是國外旅行行程1第一天的描述",
                        Breakfast = "這是國外旅行行程1第一天的早餐",
                        Lunch = "這是國外旅行行程1第一天的午餐",
                        Dinner = "這是國外旅行行程1第一天的晚餐",
                        Hotel = "這是國外旅行行程1第一天的飯店",
                        Attraction1 = _context.OfficialAttractions.First().AttractionId,
                        Attraction2 = null,
                        Attraction3 = null,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "這是國外旅行行程1第一天的備註1",
                        Note2 = "這是國外旅行行程1第一天的備註2"
                    },
                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = FDetailId,
                        Day = 2,
                        Description = "這是國外旅行行程1第二天的描述",
                        Breakfast = "這是國外旅行行程1第二天的早餐",
                        Lunch = "這是國外旅行行程1第二天的午餐",
                        Dinner = "這是國外旅行行程1第二天的晚餐",
                        Hotel = "這是國外旅行行程1第二天的飯店",
                        Attraction1 = _context.OfficialAttractions.Skip(1).First().AttractionId,
                        Attraction2 = null,
                        Attraction3 = null,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "這是國外旅行行程1第二天的備註1",
                        Note2 = "這是國外旅行行程1第二天的備註2"
                    },
                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = _context.OfficialTravelDetails.Where(f => f.OfficialTravel.Category == TravelCategory.Foreign && f.State == DetailState.Locked).Skip(1).FirstOrDefault().OfficialTravelDetailId,
                        Day = 1,
                        Description = "這是國外旅行行程2第一天的描述",
                        Breakfast = "這是國外旅行行程2第一天的早餐",
                        Lunch = "這是國外旅行行程2第一天的午餐",
                        Dinner = "這是國外旅行行程2第一天的晚餐",
                        Hotel = "這是國外旅行行程2第一天的飯店",
                        Attraction1 = _context.OfficialAttractions.First().AttractionId,
                        Attraction2 = null,
                        Attraction3 = null,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "這是國外旅行行程2第一天的備註1",
                        Note2 = "這是國外旅行行程2第一天的備註2"
                    },
                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = _context.OfficialTravelDetails.Where(f => f.OfficialTravel.Category == TravelCategory.Foreign && f.State == DetailState.Locked).Skip(1).FirstOrDefault().OfficialTravelDetailId,
                        Day = 2,
                        Description = "這是國外旅行行程2第二天的描述",
                        Breakfast = "這是國外旅行行程2第二天的早餐",
                        Lunch = "這是國外旅行行程2第二天的午餐",
                        Dinner = "這是國外旅行行程2第二天的晚餐",
                        Hotel = "這是國外旅行行程2第二天的飯店",
                        Attraction1 = _context.OfficialAttractions.Skip(1).First().AttractionId,
                        Attraction2 = null,
                        Attraction3 = null,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "這是國外旅行行程2第二天的備註1",
                        Note2 = "這是國外旅行行程2第二天的備註2"
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
                        Day = 2,
                        Time = "16:00",
                        AccommodationName = "測試2"
                    },
                    new CustomTravelContent
                    {
                        CustomTravelId = _context.CustomTravels.Skip(1).First().CustomTravelId,
                        ItemId = _context.Transports.First().TransportId,
                        Category = TravelItemCategory.Transport,
                        Day = 1,
                        Time = "10:00",
                        AccommodationName = "1測試"
                    },
                    new CustomTravelContent
                    {
                        CustomTravelId = _context.CustomTravels.Skip(1).First().CustomTravelId,
                        ItemId = _context.Accommodations.First().AccommodationId,
                        Category = TravelItemCategory.Accommodation,
                        Day = 1,
                        Time = "12:00",
                        AccommodationName = "1測試1"
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
                         OfficialTravelDetailId = _context.OfficialTravelDetails.Where(f => f.OfficialTravel.Category == TravelCategory.Foreign && f.State == DetailState.Locked).Skip(1).FirstOrDefault().OfficialTravelDetailId,
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
        //公告假資料
        private async Task SeedAnnouncementAsync()
        {
            if (!_context.Announcements.Any())
            {
                _context.Announcements.AddRange(
                    new Announcement
                    {
                        EmployeeId = 1,
                        Title = "資展國際★名言佳句",
                        Content = "{{ 今日口號 }} 一.給阿波棒。，二.先看喔先看喔,對不對,對齁。，三.A星A味,愛恩G。，四.不是尼的湊拉,是西阿歐s的問題的拉。",
                        SentAt = new DateTime(2024, 8, 11),
                        Status = AnnouncementStatus.Published
                    },
                    new Announcement
                    {
                        EmployeeId = 1,
                        Title = "【使用JC卡】",
                        Content = "要不要吃涼麵，要不要吃京多多，要不要吃小飯骨，要不要吃蛋餅。",
                        SentAt = new DateTime(2024, 8, 11),
                        Status = AnnouncementStatus.Published
                    },
                    new Announcement
                    {
                        EmployeeId = 1,
                        Title = "我是山頂洞人，我引以為傲",
                        Content = "呼叫李小姐，請您看一下LINE看一眼也好",
                        SentAt = new DateTime(2024, 8, 11),
                        Status = AnnouncementStatus.Published
                    }
                );
                await _context.SaveChangesAsync();
            }
        }
    }
}
