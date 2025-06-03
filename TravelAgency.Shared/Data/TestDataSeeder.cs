using Microsoft.EntityFrameworkCore;
using System;
using System.Net;
using TravelAgency.Shared.Models;

namespace TravelAgency.Shared.Data
{
    public class TestDataSeeder
    {
        private readonly AppDbContext _context;
        private Random _random = new Random();

        public TestDataSeeder(AppDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            await SeedRolesAsync();
            await SeedEmployeesAsync();
            await SeedMembersAsync();
            await SeedCollectionAsync();
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
            await SeedOrdersAndRelatedDataAsync();
            await SeedPermissionsAsync();
            await SeedRolePermissionsAsync();
            await SeedMemberFavoriteTravelerAsync();
            await SeedDocumentMenuAsync();
            await SeedOrderFormAsync();
            await SeedPaymentMethodAsync();
            await SeedCompletedOrderDetailAsync();
            await SeedAnnouncementAsync();
            await SeedCommentsAsync();
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
                    Birthday = new DateTime(1990, 5, 6),
                    Email = "member1989@gmail.com",
                    Phone = "0925806525",
                    Gender = GenderType.Male,
                    IdNumber = "A120738965",
                    PassportSurname = "YEH",
                    PassportGivenName = "YEHYEH",
                    PassportExpireDate = new DateTime(2030, 12, 5),
                    Nationality = "TW",
                    DocumentType = DocumentType.ID_CARD_TW,
                    DocumentNumber = "317926777201",
                    Address = "高雄市前金區中正四路211號8樓",
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
        private async Task SeedCollectionAsync()
        {
            if (!_context.Collects.Any())
            {
                _context.Collects.AddRange(
                    new Collect
                    {
                        MemberId = 11110,
                        TravelId = 1,
                        TravelType = CollectType.Official,
                        CreatedAt = DateTime.UtcNow.AddDays(-5)
                    },
                    new Collect
                    {
                        MemberId = 11110,
                        TravelId = 2,
                        TravelType = CollectType.Official,
                        CreatedAt = DateTime.UtcNow.AddDays(-2)
                    },
                    new Collect
                    {
                        MemberId = 11110,
                        TravelId = 3,
                        TravelType = CollectType.Official,
                        CreatedAt = DateTime.UtcNow
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
                // 日本
                new Region { Country = "日本", Name = "北海道" },
                new Region { Country = "日本", Name = "東北" },
                new Region { Country = "日本", Name = "關東" },
                new Region { Country = "日本", Name = "中部" },
                new Region { Country = "日本", Name = "近畿" },
                new Region { Country = "日本", Name = "中國" },
                new Region { Country = "日本", Name = "四國" },
                new Region { Country = "日本", Name = "九州" },
                new Region { Country = "日本", Name = "沖繩" },

                // 台灣
                new Region { Country = "台灣", Name = "北部" },
                new Region { Country = "台灣", Name = "中部" },
                new Region { Country = "台灣", Name = "南部" },
                new Region { Country = "台灣", Name = "東部" },
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
                    // 日本北海道 ID = 1
                    new TravelSupplier // ID = 1
                    {
                        SupplierName = "札幌白色戀人公園",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "山田太郎",
                        ContactPhone = "+81-11-6666-1111",
                        ContactEmail = "info@shiroikoibito.jp",
                        SupplierNote = "巧克力工廠與觀光設施"
                    },
                    new TravelSupplier // ID = 2
                    {
                        SupplierName = "旭山動物園",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "佐藤惠理",
                        ContactPhone = "+81-166-36-1104",
                        ContactEmail = "contact@asahiyamazoo.jp",
                        SupplierNote = "人氣最高的日本動物園之一"
                    },
                    new TravelSupplier // ID = 3
                    {
                        SupplierName = "小樽運河歷史館",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "小川美香",
                        ContactPhone = "+81-134-23-1234",
                        ContactEmail = "info@otaru-canal.jp",
                        SupplierNote = "歷史老街搭配運河景觀"
                    },
                    new TravelSupplier // ID = 4
                    {
                        SupplierName = "富良野薰衣草田",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "長谷川大輔",
                        ContactPhone = "+81-167-22-1111",
                        ContactEmail = "lavender@furano.jp",
                        SupplierNote = "夏季熱門花田景點"
                    },
                    new TravelSupplier // ID = 5
                    {
                        SupplierName = "美瑛青池導覽",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "木村翔",
                        ContactPhone = "+81-90-8888-1111",
                        ContactEmail = "info@biei-blue.jp",
                        SupplierNote = "自然景觀拍照聖地"
                    },
                    new TravelSupplier // ID = 6
                    {
                        SupplierName = "登別地獄谷觀光協會",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "谷口浩一",
                        ContactPhone = "+81-143-84-1234",
                        ContactEmail = "guide@noboribetsu.jp",
                        SupplierNote = "火山地形與溫泉煙氣景觀"
                    },
                    new TravelSupplier // ID = 7
                    {
                        SupplierName = "函館山夜景纜車",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "田村由香",
                        ContactPhone = "+81-138-23-3105",
                        ContactEmail = "support@hakodate-ropeway.jp",
                        SupplierNote = "百萬夜景與纜車體驗"
                    },
                    new TravelSupplier // ID = 8
                    {
                        SupplierName = "知床五湖導覽中心",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "中島亮",
                        ContactPhone = "+81-152-24-3456",
                        ContactEmail = "shiretoko@guides.jp",
                        SupplierNote = "世界自然遺產景區"
                    },
                    new TravelSupplier // ID = 9
                    {
                        SupplierName = "釧路濕原自然漫步",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "藤井純",
                        ContactPhone = "+81-154-55-6789",
                        ContactEmail = "eco@kushiro-nature.jp",
                        SupplierNote = "賞鳥與濕地保護區"
                    },
                    new TravelSupplier // ID = 10
                    {
                        SupplierName = "摩周湖觀景平台",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "高橋直樹",
                        ContactPhone = "+81-90-1234-9876",
                        ContactEmail = "info@mashu-lake.jp",
                        SupplierNote = "神秘湖泊觀景區"
                    },
                    new TravelSupplier // ID = 11
                    {
                        SupplierName = "網走流冰體驗船",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "阿部哲也",
                        ContactPhone = "+81-152-43-1234",
                        ContactEmail = "driftice@abashiri-tour.jp",
                        SupplierNote = "冬季限定破冰船體驗"
                    },
                    new TravelSupplier // ID = 12
                    {
                        SupplierName = "大雪山國立公園步道",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "坂本綾",
                        ContactPhone = "+81-166-97-2153",
                        ContactEmail = "trekking@taisetsuzan.jp",
                        SupplierNote = "登山健行與自然景觀"
                    },
                    new TravelSupplier // ID = 13
                    {
                        SupplierName = "十勝牧場雪橇",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "森田優子",
                        ContactPhone = "+81-155-25-4321",
                        ContactEmail = "sledding@tokachi.jp",
                        SupplierNote = "馬拉雪橇與酪農體驗"
                    },
                    new TravelSupplier // ID = 14
                    {
                        SupplierName = "洞爺湖環湖遊覽",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "村山拓也",
                        ContactPhone = "+81-142-75-1111",
                        ContactEmail = "info@toyalake.jp",
                        SupplierNote = "火山湖觀光遊船"
                    },
                    new TravelSupplier // ID = 15
                    {
                        SupplierName = "支笏湖透明獨木舟",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "石川早苗",
                        ContactPhone = "+81-123-25-2008",
                        ContactEmail = "kayak@shikotsu.jp",
                        SupplierNote = "清澈湖泊划船體驗"
                    },
                    new TravelSupplier // ID = 16
                    {
                        SupplierName = "北國雪屋博物館",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "青木亮介",
                        ContactPhone = "+81-11-223-4567",
                        ContactEmail = "museum@snowculture.jp",
                        SupplierNote = "介紹北海道雪文化"
                    },
                    new TravelSupplier // ID = 17
                    {
                        SupplierName = "旭岳纜車",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "柴田和也",
                        ContactPhone = "+81-166-68-9111",
                        ContactEmail = "ropeway@mtasahi.jp",
                        SupplierNote = "高山植物與夏日健行"
                    },
                    new TravelSupplier // ID = 18
                    {
                        SupplierName = "二世谷滑雪場接駁",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "石田理恵",
                        ContactPhone = "+81-136-22-0109",
                        ContactEmail = "snowbus@niseko.jp",
                        SupplierNote = "冬季滑雪團與教練預約"
                    },
                    new TravelSupplier // ID = 19
                    {
                        SupplierName = "小樽玻璃工坊體驗",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "森本麻衣",
                        ContactPhone = "+81-134-34-0001",
                        ContactEmail = "glass@otarucraft.jp",
                        SupplierNote = "玻璃製品DIY工作坊"
                    },
                    new TravelSupplier // ID = 20
                    {
                        SupplierName = "三角市場早市體驗",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "堀內達也",
                        ContactPhone = "+81-136-55-6543",
                        ContactEmail = "market@otaru-fish.jp",
                        SupplierNote = "在地海鮮市場導覽"
                    },
                    new TravelSupplier // ID = 21
                    {
                        SupplierName = "札幌螃蟹本家",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "島田健",
                        ContactPhone = "+81-11-222-1111",
                        ContactEmail = "booking@kani.jp",
                        SupplierNote = "螃蟹專門料理名店"
                    },
                    new TravelSupplier // ID = 22
                    {
                        SupplierName = "成吉思汗烤肉館",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "內田香",
                        ContactPhone = "+81-11-333-2222",
                        ContactEmail = "info@jingisukan.jp",
                        SupplierNote = "羊肉烤肉特色料理"
                    },
                    new TravelSupplier // ID = 23
                    {
                        SupplierName = "湯咖哩一番亭",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "矢野真一",
                        ContactPhone = "+81-11-444-3333",
                        ContactEmail = "reserve@soupcurry.jp",
                        SupplierNote = "札幌發源地湯咖哩名店"
                    },
                    new TravelSupplier // ID = 24
                    {
                        SupplierName = "函館朝市食堂",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "佐佐木良",
                        ContactPhone = "+81-138-24-3333",
                        ContactEmail = "marketfood@hakodate.jp",
                        SupplierNote = "新鮮海鮮丼早午餐"
                    },
                    new TravelSupplier // ID = 25
                    {
                        SupplierName = "釧路爐端燒",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "大西剛",
                        ContactPhone = "+81-154-42-1111",
                        ContactEmail = "robata@kushiro.jp",
                        SupplierNote = "北海道鄉土爐端燒"
                    },
                    new TravelSupplier // ID = 26
                    {
                        SupplierName = "小樽壽司通本店",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "山口直人",
                        ContactPhone = "+81-134-22-0001",
                        ContactEmail = "sushi@otaru.jp",
                        SupplierNote = "海鮮壽司老舖"
                    },
                    new TravelSupplier // ID = 27
                    {
                        SupplierName = "旭川拉麵橫丁",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "福田真理",
                        ContactPhone = "+81-166-55-1234",
                        ContactEmail = "ramen@asahikawa.jp",
                        SupplierNote = "北海道三大拉麵之一"
                    },
                    new TravelSupplier // ID = 28
                    {
                        SupplierName = "登別溫泉懷石料理",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "藤原健",
                        ContactPhone = "+81-143-88-1111",
                        ContactEmail = "kaiseki@noboribetsu.jp",
                        SupplierNote = "配合住宿提供會席套餐"
                    },
                    new TravelSupplier // ID = 29
                    {
                        SupplierName = "富良野起司工房",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "江口舞",
                        ContactPhone = "+81-167-22-7890",
                        ContactEmail = "cheese@furano.jp",
                        SupplierNote = "起司製作與義式輕食"
                    },
                    new TravelSupplier // ID = 30
                    {
                        SupplierName = "知床鄉味居酒屋",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "片山直樹",
                        ContactPhone = "+81-152-33-3333",
                        ContactEmail = "izakaya@shiretoko.jp",
                        SupplierNote = "鄉村風格居酒屋"
                    },
                    new TravelSupplier // ID = 31
                    {
                        SupplierName = "札幌王子大飯店",
                        SupplierType = SupplierType.Accommodation,
                        ContactName = "高木光",
                        ContactPhone = "+81-11-241-1111",
                        ContactEmail = "hotel@prince-sapporo.jp",
                        SupplierNote = "市中心大型觀光飯店"
                    },
                    new TravelSupplier // ID = 32
                    {
                        SupplierName = "定山溪溫泉旅館",
                        SupplierType = SupplierType.Accommodation,
                        ContactName = "川口幸子",
                        ContactPhone = "+81-11-598-2222",
                        ContactEmail = "onsen@jozankei.jp",
                        SupplierNote = "溫泉湯屋與和式住宿"
                    },
                    new TravelSupplier // ID = 33
                    {
                        SupplierName = "旭川車站前商務旅館",
                        SupplierType = SupplierType.Accommodation,
                        ContactName = "岩田陽",
                        ContactPhone = "+81-166-23-9999",
                        ContactEmail = "info@asahikawahotel.jp",
                        SupplierNote = "交通便利，適合短期商務住宿"
                    },

                    // 東北 ID = 2
                    new TravelSupplier // ID = 34
                    {
                        SupplierName = "藏王樹冰觀光纜車",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "佐藤智子",
                        ContactPhone = "+81-23-694-9518",
                        ContactEmail = "info@zao-ropeway.jp",
                        SupplierNote = "冬季樹冰景觀體驗"
                    },
                    new TravelSupplier // ID = 35
                    {
                        SupplierName = "十和田湖遊覽船",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "工藤翔",
                        ContactPhone = "+81-176-75-2909",
                        ContactEmail = "lake@towada.jp",
                        SupplierNote = "青森與秋田交界的湖上觀光"
                    },
                    new TravelSupplier // ID = 36
                    {
                        SupplierName = "角館武家屋敷",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "遠藤綾",
                        ContactPhone = "+81-187-54-2700",
                        ContactEmail = "guide@kakunodate.jp",
                        SupplierNote = "保存完整的武士老街"
                    },
                    new TravelSupplier // ID = 37
                    {
                        SupplierName = "青森睡魔祭館",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "大島翼",
                        ContactPhone = "+81-17-752-1311",
                        ContactEmail = "info@nebuta.jp",
                        SupplierNote = "東北夏祭知名花燈展覽"
                    },
                    new TravelSupplier // ID = 38
                    {
                        SupplierName = "銀山溫泉老街導覽",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "小林結衣",
                        ContactPhone = "+81-237-28-3933",
                        ContactEmail = "onsen@ginzan.jp",
                        SupplierNote = "復古日式街景與溫泉飯店群"
                    },
                    new TravelSupplier // ID = 39
                    {
                        SupplierName = "男鹿真山傳承館",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "高橋信也",
                        ContactPhone = "+81-185-33-2558",
                        ContactEmail = "namahage@oga.jp",
                        SupplierNote = "體驗秋田生剝鬼文化"
                    },
                    new TravelSupplier // ID = 40
                    {
                        SupplierName = "立石寺千年石階",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "山岸誠",
                        ContactPhone = "+81-23-695-2843",
                        ContactEmail = "info@yamadera.jp",
                        SupplierNote = "登山健行與歷史佛寺景觀"
                    },
                    new TravelSupplier // ID = 41
                    {
                        SupplierName = "五色沼自然步道",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "三浦愛",
                        ContactPhone = "+81-241-32-2349",
                        ContactEmail = "eco@goshikinuma.jp",
                        SupplierNote = "彩色池沼健行自然保護區"
                    },
                    new TravelSupplier // ID = 42
                    {
                        SupplierName = "田澤湖畔健行體驗",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "石川舞",
                        ContactPhone = "+81-187-58-1100",
                        ContactEmail = "lake@tazawako.jp",
                        SupplierNote = "日本最深湖泊，四季皆宜"
                    },
                    new TravelSupplier // ID = 43
                    {
                        SupplierName = "弘前城公園櫻花季",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "中野紗季",
                        ContactPhone = "+81-172-33-8733",
                        ContactEmail = "sakura@hirosaki.jp",
                        SupplierNote = "春季限定夜櫻祭典"
                    },
                    new TravelSupplier // ID = 44
                    {
                        SupplierName = "猊鼻溪遊船",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "阿部美樹",
                        ContactPhone = "+81-191-47-2341",
                        ContactEmail = "cruise@geibikei.jp",
                        SupplierNote = "河川觀光與傳統船夫歌謠"
                    },
                    new TravelSupplier // ID = 45
                    {
                        SupplierName = "大內宿茅草老街",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "佐佐木正",
                        ContactPhone = "+81-241-68-3611",
                        ContactEmail = "heritage@ouchijuku.jp",
                        SupplierNote = "古風街道體驗"
                    },
                    new TravelSupplier // ID = 46
                    {
                        SupplierName = "花卷溫泉玫瑰園",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "西村千代",
                        ContactPhone = "+81-198-37-2111",
                        ContactEmail = "flowers@hanamaki.jp",
                        SupplierNote = "玫瑰與溫泉並存的療癒景點"
                    },
                    new TravelSupplier // ID = 47
                    {
                        SupplierName = "蔦沼紅葉攝影地",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "村井剛",
                        ContactPhone = "+81-176-74-2345",
                        ContactEmail = "photo@tsutanuma.jp",
                        SupplierNote = "絕美倒影與攝影熱門點"
                    },
                    new TravelSupplier // ID = 48
                    {
                        SupplierName = "會津若松鶴城",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "林幸雄",
                        ContactPhone = "+81-242-27-4005",
                        ContactEmail = "castle@aizu.jp",
                        SupplierNote = "歷史戰國古城遺址"
                    },
                    new TravelSupplier // ID = 49
                    {
                        SupplierName = "三內丸山遺跡導覽",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "伊藤舞",
                        ContactPhone = "+81-17-766-8282",
                        ContactEmail = "history@sannaimaruyama.jp",
                        SupplierNote = "日本重要繩文遺跡"
                    },
                    new TravelSupplier // ID = 50
                    {
                        SupplierName = "鳴子峽賞楓步道",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "川村直子",
                        ContactPhone = "+81-229-82-1234",
                        ContactEmail = "autumn@naruko.jp",
                        SupplierNote = "秋季紅葉絕景"
                    },
                    new TravelSupplier // ID = 51
                    {
                        SupplierName = "盛岡手工藝村",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "村山結",
                        ContactPhone = "+81-19-654-8881",
                        ContactEmail = "craft@iwate.jp",
                        SupplierNote = "體驗南部鐵器與民俗藝品"
                    },
                    new TravelSupplier // ID = 52
                    {
                        SupplierName = "秋田內陸縱貫鐵道",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "岡本壽",
                        ContactPhone = "+81-187-58-3000",
                        ContactEmail = "train@akita-rail.jp",
                        SupplierNote = "觀光列車與雪景體驗"
                    },
                    new TravelSupplier // ID = 53
                    {
                        SupplierName = "津輕三味線演奏廳",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "吉田光",
                        ContactPhone = "+81-172-38-1234",
                        ContactEmail = "shamisen@tsugaru.jp",
                        SupplierNote = "傳統音樂與現場演奏"
                    },
                    new TravelSupplier // ID = 54
                    {
                        SupplierName = "盛岡冷麵館",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "渡邊陽介",
                        ContactPhone = "+81-19-623-1122",
                        ContactEmail = "coldnoodle@morioka.jp",
                        SupplierNote = "岩手名物冷麵"
                    },
                    new TravelSupplier // ID = 55
                    {
                        SupplierName = "山形芋煮食堂",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "田中幸子",
                        ContactPhone = "+81-23-625-2222",
                        ContactEmail = "imoni@yamagata.jp",
                        SupplierNote = "東北傳統燉煮料理"
                    },
                    new TravelSupplier // ID = 56
                    {
                        SupplierName = "比內地雞本舖",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "高見澤優",
                        ContactPhone = "+81-18-865-7777",
                        ContactEmail = "chicken@akita.jp",
                        SupplierNote = "秋田地雞專門店"
                    },
                    new TravelSupplier // ID = 57
                    {
                        SupplierName = "仙台牛舌炭燒亭",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "佐佐木龍",
                        ContactPhone = "+81-22-262-3000",
                        ContactEmail = "gyutan@sendai.jp",
                        SupplierNote = "厚切牛舌名店"
                    },
                    new TravelSupplier // ID = 58
                    {
                        SupplierName = "青森蘋果甜點工房",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "菊地美香",
                        ContactPhone = "+81-17-777-8888",
                        ContactEmail = "apple@aomori.jp",
                        SupplierNote = "使用在地蘋果製作甜點"
                    },
                    new TravelSupplier // ID = 59
                    {
                        SupplierName = "角館稻庭烏龍麵店",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "內山優子",
                        ContactPhone = "+81-187-55-2222",
                        ContactEmail = "udon@kakunodate.jp",
                        SupplierNote = "秋田傳統手延烏龍麵"
                    },
                    new TravelSupplier // ID = 60
                    {
                        SupplierName = "福島喜多方拉麵村",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "齋藤光",
                        ContactPhone = "+81-242-22-1234",
                        ContactEmail = "ramen@kitakata.jp",
                        SupplierNote = "多家拉麵名店聚集地"
                    },
                    new TravelSupplier // ID = 61
                    {
                        SupplierName = "弘前城下和食亭",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "佐野拓也",
                        ContactPhone = "+81-172-38-0005",
                        ContactEmail = "washoku@hirosaki.jp",
                        SupplierNote = "在地季節性會席料理"
                    },
                    new TravelSupplier // ID = 62
                    {
                        SupplierName = "會津味噌烤肉屋",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "岡田直樹",
                        ContactPhone = "+81-242-33-7654",
                        ContactEmail = "bbq@aizumiso.jp",
                        SupplierNote = "使用自家釀味噌調味"
                    },
                    new TravelSupplier // ID = 63
                    {
                        SupplierName = "鶴岡壽司海味亭",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "堀內翔",
                        ContactPhone = "+81-235-22-8989",
                        ContactEmail = "sushi@tsuruoka.jp",
                        SupplierNote = "使用庄內沿海漁獲"
                    },
                    new TravelSupplier // ID = 64
                    {
                        SupplierName = "仙台東橫INN",
                        SupplierType = SupplierType.Accommodation,
                        ContactName = "伊藤美雪",
                        ContactPhone = "+81-22-281-1045",
                        ContactEmail = "stay@toyoko-inn.jp",
                        SupplierNote = "市中心連鎖商務旅館"
                    },
                    new TravelSupplier // ID = 65
                    {
                        SupplierName = "銀山溫泉藤屋",
                        SupplierType = SupplierType.Accommodation,
                        ContactName = "今井忍",
                        ContactPhone = "+81-237-28-3333",
                        ContactEmail = "onsen@fujiya.jp",
                        SupplierNote = "日式木造建築與露天湯屋"
                    },
                    new TravelSupplier // ID = 66
                    {
                        SupplierName = "會津若松大飯店",
                        SupplierType = SupplierType.Accommodation,
                        ContactName = "大竹優",
                        ContactPhone = "+81-242-26-5555",
                        ContactEmail = "info@aizu-hotel.jp",
                        SupplierNote = "近鶴城與市區，交通便利"
                    },

                    // 關東 ID = 3
                    new TravelSupplier // ID = 67
                    {
                        SupplierName = "東京晴空塔",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "石井陽子",
                        ContactPhone = "+81-3-6658-8888",
                        ContactEmail = "info@skytree.jp",
                        SupplierNote = "日本最高建築，觀景台視野極佳"
                    },
                    new TravelSupplier // ID = 68
                    {
                        SupplierName = "淺草雷門與仲見世通",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "佐藤和馬",
                        ContactPhone = "+81-3-3842-0181",
                        ContactEmail = "asakusa@tokyo.jp",
                        SupplierNote = "東京最具代表性的傳統商店街"
                    },
                    new TravelSupplier // ID = 69
                    {
                        SupplierName = "明治神宮森林公園",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "高橋真紀",
                        ContactPhone = "+81-3-3379-5511",
                        ContactEmail = "meiji@shrine.jp",
                        SupplierNote = "神社與森林步道共存的心靈景點"
                    },
                    new TravelSupplier // ID = 70
                    {
                        SupplierName = "上野動物園",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "前田龍之介",
                        ContactPhone = "+81-3-3828-5171",
                        ContactEmail = "zoo@ueno.jp",
                        SupplierNote = "擁有大熊貓等人氣動物的東京動物園"
                    },
                    new TravelSupplier // ID = 71
                    {
                        SupplierName = "橫濱紅磚倉庫",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "田中真央",
                        ContactPhone = "+81-45-227-2002",
                        ContactEmail = "info@akarenga.jp",
                        SupplierNote = "歷史建築改建的藝術商業區"
                    },
                    new TravelSupplier // ID = 72
                    {
                        SupplierName = "東京迪士尼樂園",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "長谷川翔",
                        ContactPhone = "+81-45-683-3777",
                        ContactEmail = "booking@tokyodisneyresort.jp",
                        SupplierNote = "亞洲人氣最高的主題樂園"
                    },
                    new TravelSupplier // ID = 73
                    {
                        SupplierName = "橫濱八景島海島樂園",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "松本美香",
                        ContactPhone = "+81-45-788-8888",
                        ContactEmail = "aquapark@hakkeijima.jp",
                        SupplierNote = "水族館與遊樂園綜合景點"
                    },
                    new TravelSupplier // ID = 74
                    {
                        SupplierName = "新宿御苑",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "村田詠子",
                        ContactPhone = "+81-3-3341-1461",
                        ContactEmail = "garden@shinjuku.jp",
                        SupplierNote = "四季變化的日式西式綜合庭園"
                    },
                    new TravelSupplier // ID = 75
                    {
                        SupplierName = "築地市場歷史導覽",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "小野勝",
                        ContactPhone = "+81-3-3541-9444",
                        ContactEmail = "tour@tsukiji.jp",
                        SupplierNote = "原海鮮批發地的文化與飲食體驗"
                    },
                    new TravelSupplier // ID = 76
                    {
                        SupplierName = "箱根蘆之湖觀光船",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "木下裕也",
                        ContactPhone = "+81-460-83-6351",
                        ContactEmail = "cruise@hakone.jp",
                        SupplierNote = "搭乘觀光船賞湖景與富士山"
                    },
                    new TravelSupplier // ID = 77
                    {
                        SupplierName = "江之島展望台與海岸線",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "青木優",
                        ContactPhone = "+81-466-23-2444",
                        ContactEmail = "info@enoshima.jp",
                        SupplierNote = "適合散步、看海與觀景台"
                    },
                    new TravelSupplier // ID = 78
                    {
                        SupplierName = "川越小江戶老街",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "藤田結衣",
                        ContactPhone = "+81-49-222-5556",
                        ContactEmail = "historic@kawagoe.jp",
                        SupplierNote = "保存江戶風貌的觀光古街"
                    },
                    new TravelSupplier // ID = 79
                    {
                        SupplierName = "日光東照宮",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "黒川英樹",
                        ContactPhone = "+81-288-54-0560",
                        ContactEmail = "shrine@toshogu.jp",
                        SupplierNote = "世界文化遺產，日本著名神社"
                    },
                    new TravelSupplier // ID = 80
                    {
                        SupplierName = "鎌倉大佛與鶴岡八幡宮",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "中山麗",
                        ContactPhone = "+81-467-22-0703",
                        ContactEmail = "temple@kamakura.jp",
                        SupplierNote = "歷史寺院與文化街區"
                    },
                    new TravelSupplier // ID = 81
                    {
                        SupplierName = "茨城國營常陸海濱公園",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "竹內豪",
                        ContactPhone = "+81-29-265-9001",
                        ContactEmail = "flowers@hitachikaihin.jp",
                        SupplierNote = "知名藍色粉蝶花與波斯菊季節花海"
                    },
                    new TravelSupplier // ID = 82
                    {
                        SupplierName = "館山野島崎燈塔",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "大森智",
                        ContactPhone = "+81-470-28-1111",
                        ContactEmail = "light@nojimasaki.jp",
                        SupplierNote = "千葉海角景觀燈塔"
                    },
                    new TravelSupplier // ID = 83
                    {
                        SupplierName = "秩父芝櫻之丘",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "石原由美",
                        ContactPhone = "+81-494-25-5209",
                        ContactEmail = "moss@chichibu.jp",
                        SupplierNote = "春季賞花名勝"
                    },
                    new TravelSupplier // ID = 84
                    {
                        SupplierName = "東京灣夜景巡航船",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "江口健太",
                        ContactPhone = "+81-3-3454-1234",
                        ContactEmail = "cruise@tokyobay.jp",
                        SupplierNote = "晚間東京灣觀景與用餐"
                    },
                    new TravelSupplier // ID = 85
                    {
                        SupplierName = "三鷹之森吉卜力美術館",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "松田未來",
                        ContactPhone = "+81-422-45-1234",
                        ContactEmail = "ghibli@mitaka.jp",
                        SupplierNote = "動畫迷必訪之地"
                    },
                    new TravelSupplier // ID = 86
                    {
                        SupplierName = "國立科學博物館",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "上村徹",
                        ContactPhone = "+81-3-3822-0111",
                        ContactEmail = "museum@kagaku.jp",
                        SupplierNote = "自然科學與恐龍展覽"
                    },
                    new TravelSupplier // ID = 87
                    {
                        SupplierName = "築地壽司大",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "橋本光",
                        ContactPhone = "+81-3-3547-6797",
                        ContactEmail = "sushi@tsukiji.jp",
                        SupplierNote = "東京名壽司排隊名店"
                    },
                    new TravelSupplier // ID = 88
                    {
                        SupplierName = "東京豚骨一蘭拉麵",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "栗原剛",
                        ContactPhone = "+81-3-6206-9111",
                        ContactEmail = "ichiran@ramen.jp",
                        SupplierNote = "連鎖拉麵品牌知名分店"
                    },
                    new TravelSupplier // ID = 89
                    {
                        SupplierName = "淺草天婦羅大黑家",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "佐佐木千代",
                        ContactPhone = "+81-3-3844-2222",
                        ContactEmail = "tempura@asakusa.jp",
                        SupplierNote = "百年老字號天婦羅店"
                    },
                    new TravelSupplier // ID = 90
                    {
                        SupplierName = "橫濱中華街老四川",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "王美玲",
                        ContactPhone = "+81-45-681-0208",
                        ContactEmail = "sichuan@yokohama.jp",
                        SupplierNote = "正宗四川菜人氣餐廳"
                    },
                    new TravelSupplier // ID = 91
                    {
                        SupplierName = "原宿竹下通可麗餅屋",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "林彩香",
                        ContactPhone = "+81-3-3405-1234",
                        ContactEmail = "crepe@harajuku.jp",
                        SupplierNote = "學生族人氣甜點小吃"
                    },
                    new TravelSupplier // ID = 92
                    {
                        SupplierName = "新宿高樓景觀餐廳",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "安田優",
                        ContactPhone = "+81-3-3345-7777",
                        ContactEmail = "viewdining@shinjuku.jp",
                        SupplierNote = "可遠眺夜景的餐廳"
                    },
                    new TravelSupplier // ID = 93
                    {
                        SupplierName = "池袋拉麵街",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "岡田正",
                        ContactPhone = "+81-3-5951-8888",
                        ContactEmail = "ramen@ikebukuro.jp",
                        SupplierNote = "集合多間拉麵名店的區域"
                    },
                    new TravelSupplier // ID = 94
                    {
                        SupplierName = "吉祥寺甜點工房",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "中島莉奈",
                        ContactPhone = "+81-422-27-1234",
                        ContactEmail = "dessert@kichijoji.jp",
                        SupplierNote = "文青風甜點與咖啡廳"
                    },
                    new TravelSupplier // ID = 95
                    {
                        SupplierName = "鎌倉海邊披薩屋",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "鈴木翔太",
                        ContactPhone = "+81-467-33-5566",
                        ContactEmail = "pizza@kamakura.jp",
                        SupplierNote = "地中海式海景餐廳"
                    },
                    new TravelSupplier // ID = 96
                    {
                        SupplierName = "日光湯葉料理坊",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "藤原香織",
                        ContactPhone = "+81-288-54-8888",
                        ContactEmail = "yuba@nikkokaiseki.jp",
                        SupplierNote = "日光特色素食湯葉"
                    },
                    new TravelSupplier // ID = 97
                    {
                        SupplierName = "新宿王子飯店",
                        SupplierType = SupplierType.Accommodation,
                        ContactName = "村上剛",
                        ContactPhone = "+81-3-3205-1111",
                        ContactEmail = "info@prince-shinjuku.jp",
                        SupplierNote = "市中心交通方便觀光飯店"
                    },
                    new TravelSupplier // ID = 98
                    {
                        SupplierName = "橫濱灣大飯店",
                        SupplierType = SupplierType.Accommodation,
                        ContactName = "川合紗希",
                        ContactPhone = "+81-45-682-2222",
                        ContactEmail = "stay@yokohamabay.jp",
                        SupplierNote = "港口旁海景飯店"
                    },
                    new TravelSupplier // ID = 99
                    {
                        SupplierName = "日光金谷飯店",
                        SupplierType = SupplierType.Accommodation,
                        ContactName = "高木光",
                        ContactPhone = "+81-288-54-0007",
                        ContactEmail = "kanayahotel@nikkostay.jp",
                        SupplierNote = "百年老牌溫泉旅館"
                    },

                    // 中部 ID = 4
                    new TravelSupplier // ID = 100
                    {
                        SupplierName = "富士山五合目登山口",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "長谷川大輔",
                        ContactPhone = "+81-555-22-1234",
                        ContactEmail = "fujisan@climb.jp",
                        SupplierNote = "知名登山起點，賞景與登頂入口"
                    },
                    new TravelSupplier // ID = 101
                    {
                        SupplierName = "白川鄉合掌村導覽中心",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "山口美和",
                        ContactPhone = "+81-5769-6-1011",
                        ContactEmail = "info@shirakawago.jp",
                        SupplierNote = "世界文化遺產、茅草屋聚落導覽"
                    },
                    new TravelSupplier // ID = 102
                    {
                        SupplierName = "立山黑部阿爾卑斯路線",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "井上翔",
                        ContactPhone = "+81-76-432-2819",
                        ContactEmail = "alps@kurobe.jp",
                        SupplierNote = "日本絕景山岳道路"
                    },
                    new TravelSupplier // ID = 103
                    {
                        SupplierName = "高山古街歷史協會",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "佐佐木舞",
                        ContactPhone = "+81-577-32-3333",
                        ContactEmail = "oldtown@takayama.jp",
                        SupplierNote = "飛驒地區古色古香的江戶街道"
                    },
                    new TravelSupplier // ID = 104
                    {
                        SupplierName = "金澤兼六園遊園券",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "木村太郎",
                        ContactPhone = "+81-76-234-3800",
                        ContactEmail = "garden@kenrokuen.jp",
                        SupplierNote = "日本三大名園之一"
                    },
                    new TravelSupplier // ID = 105
                    {
                        SupplierName = "松本城觀光案內",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "村田幸子",
                        ContactPhone = "+81-263-32-2902",
                        ContactEmail = "castle@matsumoto.jp",
                        SupplierNote = "保存完整的戰國時期黑城"
                    },
                    new TravelSupplier // ID = 106
                    {
                        SupplierName = "名古屋城天守導覽",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "小島剛",
                        ContactPhone = "+81-52-231-1700",
                        ContactEmail = "nagoyajo@aichi.jp",
                        SupplierNote = "尾張德川家歷史據點"
                    },
                    new TravelSupplier // ID = 107
                    {
                        SupplierName = "熱田神宮祈福體驗",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "大澤涼子",
                        ContactPhone = "+81-52-671-4151",
                        ContactEmail = "shrine@atsuta.jp",
                        SupplierNote = "名古屋著名神社"
                    },
                    new TravelSupplier // ID = 108
                    {
                        SupplierName = "白山比咩神社",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "高田信也",
                        ContactPhone = "+81-76-272-0680",
                        ContactEmail = "info@hakusan.jp",
                        SupplierNote = "石川知名靈場與登山入口"
                    },
                    new TravelSupplier // ID = 109
                    {
                        SupplierName = "妻籠宿與馬籠宿歷史步道",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "藤本愛",
                        ContactPhone = "+81-265-43-3121",
                        ContactEmail = "trail@kiso-valley.jp",
                        SupplierNote = "中山道保存最完整的宿場街"
                    },
                    new TravelSupplier // ID = 110
                    {
                        SupplierName = "富山玻璃美術館",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "本田綾乃",
                        ContactPhone = "+81-76-461-3100",
                        ContactEmail = "glass@toyama.jp",
                        SupplierNote = "當代藝術與建築融合空間"
                    },
                    new TravelSupplier // ID = 111
                    {
                        SupplierName = "輪島朝市文化街",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "長井翔平",
                        ContactPhone = "+81-768-22-7651",
                        ContactEmail = "market@wajima.jp",
                        SupplierNote = "能登半島百年魚市與工藝市集"
                    },
                    new TravelSupplier // ID = 112
                    {
                        SupplierName = "郡上八幡水之町景",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "吉村誠",
                        ContactPhone = "+81-575-67-1808",
                        ContactEmail = "tour@gujo.jp",
                        SupplierNote = "清流之鄉水景小鎮"
                    },
                    new TravelSupplier // ID = 113
                    {
                        SupplierName = "伊豆下田海中水族館",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "森山百合",
                        ContactPhone = "+81-558-22-3567",
                        ContactEmail = "info@shimoda-aquarium.jp",
                        SupplierNote = "海岸型水族館"
                    },
                    new TravelSupplier // ID = 114
                    {
                        SupplierName = "彥根城與玄宮園",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "大西敦",
                        ContactPhone = "+81-749-22-2742",
                        ContactEmail = "castle@hikone.jp",
                        SupplierNote = "國寶城與日式庭園結合"
                    },
                    new TravelSupplier // ID = 115
                    {
                        SupplierName = "伊勢神宮正宮參拜",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "加藤悠",
                        ContactPhone = "+81-596-24-1111",
                        ContactEmail = "grandshrine@ise.jp",
                        SupplierNote = "日本最尊崇神宮"
                    },
                    new TravelSupplier // ID = 116
                    {
                        SupplierName = "奈良井宿中山道古道",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "橘美沙",
                        ContactPhone = "+81-264-34-3051",
                        ContactEmail = "oldroad@narai.jp",
                        SupplierNote = "保存最完整宿場之一"
                    },
                    new TravelSupplier // ID = 117
                    {
                        SupplierName = "諏訪湖花火船票",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "若林真",
                        ContactPhone = "+81-266-52-2111",
                        ContactEmail = "fireworks@suwa.jp",
                        SupplierNote = "夏季水上煙火節"
                    },
                    new TravelSupplier // ID = 118
                    {
                        SupplierName = "駿府城公園遺址",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "青野雄一",
                        ContactPhone = "+81-54-221-1433",
                        ContactEmail = "ruins@sunpu.jp",
                        SupplierNote = "德川家康幼年時居所"
                    },
                    new TravelSupplier // ID = 119
                    {
                        SupplierName = "飛驒大鍾乳洞",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "柴田夏美",
                        ContactPhone = "+81-577-79-2211",
                        ContactEmail = "cave@hida.jp",
                        SupplierNote = "日本最深鍾乳石洞穴群"
                    },
                    new TravelSupplier // ID = 120
                    {
                        SupplierName = "名古屋矢場味噌豬排",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "中村勝",
                        ContactPhone = "+81-52-252-8810",
                        ContactEmail = "tonkatsu@yabaton.jp",
                        SupplierNote = "味噌炸豬排的發源名店"
                    },
                    new TravelSupplier // ID = 121
                    {
                        SupplierName = "高山飛驒牛壽喜燒",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "石井菜月",
                        ContactPhone = "+81-577-36-1234",
                        ContactEmail = "sukiyaki@hida.jp",
                        SupplierNote = "飛驒牛肉料理專門"
                    },
                    new TravelSupplier // ID = 122
                    {
                        SupplierName = "金澤近江町市場壽司亭",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "藤原剛",
                        ContactPhone = "+81-76-231-1462",
                        ContactEmail = "sushi@omicho.jp",
                        SupplierNote = "海鮮壽司市場名店"
                    },
                    new TravelSupplier // ID = 123
                    {
                        SupplierName = "松本蕎麥麵本家",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "早川俊",
                        ContactPhone = "+81-263-33-4567",
                        ContactEmail = "soba@matsumoto.jp",
                        SupplierNote = "信州蕎麥產地名店"
                    },
                    new TravelSupplier // ID = 124
                    {
                        SupplierName = "白川鄉田樂之家",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "池田彌生",
                        ContactPhone = "+81-5769-6-2000",
                        ContactEmail = "dengaku@shirakawa.jp",
                        SupplierNote = "鄉土料理與炭烤味噌豆腐"
                    },
                    new TravelSupplier // ID = 125
                    {
                        SupplierName = "富士吉田烏龍麵老店",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "岡村淳",
                        ContactPhone = "+81-555-22-7890",
                        ContactEmail = "udon@fujiyoshida.jp",
                        SupplierNote = "Q 彈烏龍麵風味獨特"
                    },
                    new TravelSupplier // ID = 126
                    {
                        SupplierName = "輪島海鮮燒店",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "森本大地",
                        ContactPhone = "+81-768-23-7654",
                        ContactEmail = "grill@wajima.jp",
                        SupplierNote = "炭烤干貝與海產燒烤"
                    },
                    new TravelSupplier // ID = 127
                    {
                        SupplierName = "名古屋雞翅山ちゃん",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "竹中隆",
                        ContactPhone = "+81-52-241-3355",
                        ContactEmail = "tebasaki@yamachan.jp",
                        SupplierNote = "經典名古屋手羽先連鎖店"
                    },
                    new TravelSupplier // ID = 128
                    {
                        SupplierName = "靜岡濱松餃子之家",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "瀧澤幸",
                        ContactPhone = "+81-53-458-1234",
                        ContactEmail = "gyoza@hamamatsu.jp",
                        SupplierNote = "圓型煎餃盛盤特色"
                    },
                    new TravelSupplier // ID = 129
                    {
                        SupplierName = "伊勢烏龍麵茶屋",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "西川舞",
                        ContactPhone = "+81-596-23-4567",
                        ContactEmail = "udon@ise.jp",
                        SupplierNote = "搭配伊勢蝦與海藻"
                    },
                    new TravelSupplier // ID = 130
                    {
                        SupplierName = "金澤日航飯店",
                        SupplierType = SupplierType.Accommodation,
                        ContactName = "大島花",
                        ContactPhone = "+81-76-234-1111",
                        ContactEmail = "stay@nikko-kanazawa.jp",
                        SupplierNote = "車站旁高樓飯店，交通方便"
                    },
                    new TravelSupplier // ID = 131
                    {
                        SupplierName = "飛驒高山溫泉旅館",
                        SupplierType = SupplierType.Accommodation,
                        ContactName = "安藤剛",
                        ContactPhone = "+81-577-32-5566",
                        ContactEmail = "onsen@hidatakayama.jp",
                        SupplierNote = "和式住宿與露天溫泉"
                    },
                    new TravelSupplier // ID = 132
                    {
                        SupplierName = "富士山本宮富士館",
                        SupplierType = SupplierType.Accommodation,
                        ContactName = "山本真理",
                        ContactPhone = "+81-555-22-3333",
                        ContactEmail = "hotel@fujistay.jp",
                        SupplierNote = "可觀富士山全景"
                    },

                    // 近畿 ID = 5
                    new TravelSupplier // ID = 133
                    {
                        SupplierName = "京都清水寺觀光處",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "中村美鈴",
                        ContactPhone = "+81-75-551-1234",
                        ContactEmail = "info@kiyomizu.jp",
                        SupplierNote = "世界文化遺產，俯瞰京都市區"
                    },
                    new TravelSupplier // ID = 134
                    {
                        SupplierName = "金閣寺參拜事務局",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "渡邊隆",
                        ContactPhone = "+81-75-461-0013",
                        ContactEmail = "temple@kinkakuji.jp",
                        SupplierNote = "金色外牆，京都最著名寺院之一"
                    },
                    new TravelSupplier // ID = 135
                    {
                        SupplierName = "伏見稻荷大社參拜服務",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "高橋咲",
                        ContactPhone = "+81-75-641-7331",
                        ContactEmail = "fushimi@inari.jp",
                        SupplierNote = "千本鳥居參道知名景點"
                    },
                    new TravelSupplier // ID = 136
                    {
                        SupplierName = "嵐山竹林步道導覽",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "吉田健",
                        ContactPhone = "+81-75-861-0012",
                        ContactEmail = "arashiyama@kyoto.jp",
                        SupplierNote = "自然步道與竹林交錯美景"
                    },
                    new TravelSupplier // ID = 137
                    {
                        SupplierName = "大阪城天守閣導覽處",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "山口淳",
                        ContactPhone = "+81-6-6941-3044",
                        ContactEmail = "castle@osaka.jp",
                        SupplierNote = "大阪歷史象徵建築"
                    },
                    new TravelSupplier // ID = 138
                    {
                        SupplierName = "大阪環球影城官方票務",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "長谷川彩",
                        ContactPhone = "+81-6-6465-1111",
                        ContactEmail = "ticket@usj.jp",
                        SupplierNote = "熱門主題樂園，親子必訪"
                    },
                    new TravelSupplier // ID = 139
                    {
                        SupplierName = "神戶港塔觀景台",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "原田裕",
                        ContactPhone = "+81-78-391-6751",
                        ContactEmail = "tower@kobeport.jp",
                        SupplierNote = "俯瞰神戶港與城市夜景"
                    },
                    new TravelSupplier // ID = 140
                    {
                        SupplierName = "奈良公園與東大寺導覽",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "藤井結",
                        ContactPhone = "+81-742-22-5511",
                        ContactEmail = "nara@deerpark.jp",
                        SupplierNote = "與鹿互動及參拜大佛"
                    },
                    new TravelSupplier // ID = 141
                    {
                        SupplierName = "通天閣觀光塔",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "坂本真一",
                        ContactPhone = "+81-6-6641-9555",
                        ContactEmail = "info@tsutenkaku.jp",
                        SupplierNote = "大阪下町象徵，夜景熱點"
                    },
                    new TravelSupplier // ID = 142
                    {
                        SupplierName = "比叡山延曆寺",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "川島隆",
                        ContactPhone = "+81-77-578-0001",
                        ContactEmail = "temple@hieizan.jp",
                        SupplierNote = "天台宗總本山，靜謐修行地"
                    },
                    new TravelSupplier // ID = 143
                    {
                        SupplierName = "宇治平等院鳳凰堂",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "上原優子",
                        ContactPhone = "+81-774-21-2861",
                        ContactEmail = "heritage@byodoin.jp",
                        SupplierNote = "10日圓硬幣圖案來源名勝"
                    },
                    new TravelSupplier // ID = 144
                    {
                        SupplierName = "三十三間堂參拜服務",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "田邊結衣",
                        ContactPhone = "+81-75-561-0467",
                        ContactEmail = "temple@sanjusangendo.jp",
                        SupplierNote = "千尊觀音與射箭聖地"
                    },
                    new TravelSupplier // ID = 145
                    {
                        SupplierName = "京都國立博物館",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "安藤翔",
                        ContactPhone = "+81-75-525-2473",
                        ContactEmail = "museum@kyohaku.jp",
                        SupplierNote = "重要文化財展出館"
                    },
                    new TravelSupplier // ID = 146
                    {
                        SupplierName = "鞍馬山登山路線",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "本田誠",
                        ContactPhone = "+81-75-741-2003",
                        ContactEmail = "trek@kurama.jp",
                        SupplierNote = "傳說與自然共存的靈山"
                    },
                    new TravelSupplier // ID = 147
                    {
                        SupplierName = "梅田空中庭園展望台",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "林佳奈",
                        ContactPhone = "+81-6-6440-3855",
                        ContactEmail = "view@umeda.jp",
                        SupplierNote = "高空360度夜景體驗"
                    },
                    new TravelSupplier // ID = 148
                    {
                        SupplierName = "甲子園歷史館",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "村井雅人",
                        ContactPhone = "+81-798-44-3310",
                        ContactEmail = "baseball@koshien.jp",
                        SupplierNote = "高校棒球與阪神虎歷史展館"
                    },
                    new TravelSupplier // ID = 149
                    {
                        SupplierName = "神戶異人館街",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "久保田翔",
                        ContactPhone = "+81-78-251-8360",
                        ContactEmail = "ijinkan@kobeheritage.jp",
                        SupplierNote = "西式古建築群與文化街"
                    },
                    new TravelSupplier // ID = 150
                    {
                        SupplierName = "道頓堀觀光船",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "石川愛",
                        ContactPhone = "+81-6-6212-5511",
                        ContactEmail = "cruise@dotonbori.jp",
                        SupplierNote = "河道遊覽與霓虹夜景"
                    },
                    new TravelSupplier // ID = 151
                    {
                        SupplierName = "大阪萬博紀念公園",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "岡崎英樹",
                        ContactPhone = "+81-6-6877-7387",
                        ContactEmail = "expo@osaka.jp",
                        SupplierNote = "太陽之塔與文化展覽園區"
                    },
                    new TravelSupplier // ID = 152
                    {
                        SupplierName = "奈良今井町歷史街區",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "吉本達也",
                        ContactPhone = "+81-744-24-8719",
                        ContactEmail = "heritage@imai-town.jp",
                        SupplierNote = "保存完好的江戶時代建築群"
                    },
                    new TravelSupplier // ID = 153
                    {
                        SupplierName = "大阪章魚燒本舖",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "青木翔",
                        ContactPhone = "+81-6-6213-1234",
                        ContactEmail = "takoyaki@dotonbori.jp",
                        SupplierNote = "關西名物小吃"
                    },
                    new TravelSupplier // ID = 154
                    {
                        SupplierName = "京都湯豆腐南禪寺順正",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "伊藤美月",
                        ContactPhone = "+81-75-771-7778",
                        ContactEmail = "tofu@nanzanji.jp",
                        SupplierNote = "南禪寺旁著名湯豆腐會席"
                    },
                    new TravelSupplier // ID = 155
                    {
                        SupplierName = "神戶牛鐵板燒老舖",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "佐佐木豪",
                        ContactPhone = "+81-78-391-2983",
                        ContactEmail = "steak@kobebeef.jp",
                        SupplierNote = "正宗神戶牛排體驗"
                    },
                    new TravelSupplier // ID = 156
                    {
                        SupplierName = "奈良柿葉壽司名店",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "安田信",
                        ContactPhone = "+81-742-22-0003",
                        ContactEmail = "sushi@nara.jp",
                        SupplierNote = "柿葉包壽司在地特色"
                    },
                    new TravelSupplier // ID = 157
                    {
                        SupplierName = "大阪壽喜燒今井亭",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "今井達也",
                        ContactPhone = "+81-6-6341-8888",
                        ContactEmail = "sukiyaki@osaka.jp",
                        SupplierNote = "使用和牛的關西壽喜燒"
                    },
                    new TravelSupplier // ID = 158
                    {
                        SupplierName = "京都抹茶甜點宇治園",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "松田愛子",
                        ContactPhone = "+81-75-221-4567",
                        ContactEmail = "matcha@uji.jp",
                        SupplierNote = "抹茶冰淇淋、和菓子專門"
                    },
                    new TravelSupplier // ID = 159
                    {
                        SupplierName = "大阪串炸一丁目",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "三浦健太",
                        ContactPhone = "+81-6-6222-3333",
                        ContactEmail = "kushikatsu@shinsekai.jp",
                        SupplierNote = "新世界地區代表性美食"
                    },
                    new TravelSupplier // ID = 160
                    {
                        SupplierName = "京都懷石料理瓢亭",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "片山昌弘",
                        ContactPhone = "+81-75-771-4116",
                        ContactEmail = "kaiseki@hyotei.jp",
                        SupplierNote = "百年歷史懷石老舖"
                    },
                    new TravelSupplier // ID = 161
                    {
                        SupplierName = "神戶中華街老店萬福樓",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "陳麗花",
                        ContactPhone = "+81-78-332-1234",
                        ContactEmail = "dim@kobe-chinatown.jp",
                        SupplierNote = "人氣點心與四川料理"
                    },
                    new TravelSupplier // ID = 162
                    {
                        SupplierName = "奈良茶粥早餐堂",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "林真琴",
                        ContactPhone = "+81-742-27-9999",
                        ContactEmail = "chagayu@nara.jp",
                        SupplierNote = "奈良特色茶粥文化早餐"
                    },
                    new TravelSupplier // ID = 163
                    {
                        SupplierName = "京都車站日航飯店",
                        SupplierType = SupplierType.Accommodation,
                        ContactName = "村田浩",
                        ContactPhone = "+81-75-342-7888",
                        ContactEmail = "hotel@nikko-kyoto.jp",
                        SupplierNote = "直通京都車站的便利觀光據點"
                    },
                    new TravelSupplier // ID = 164
                    {
                        SupplierName = "大阪心齋橋大和ROYNET飯店",
                        SupplierType = SupplierType.Accommodation,
                        ContactName = "長尾咲",
                        ContactPhone = "+81-6-6121-1234",
                        ContactEmail = "stay@roynet-osaka.jp",
                        SupplierNote = "購物商圈中心交通方便"
                    },
                    new TravelSupplier // ID = 165
                    {
                        SupplierName = "神戶北野觀光飯店",
                        SupplierType = SupplierType.Accommodation,
                        ContactName = "三谷直樹",
                        ContactPhone = "+81-78-222-2000",
                        ContactEmail = "info@kobe-hotel.jp",
                        SupplierNote = "鄰近異人館文化區"
                    },

                    // 中國 ID = 6
                    new TravelSupplier // ID = 166
                    {
                        SupplierName = "嚴島神社導覽處",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "松岡翔",
                        ContactPhone = "+81-829-44-2020",
                        ContactEmail = "shrine@itsukushima.jp",
                        SupplierNote = "世界文化遺產，海上鳥居為絕景代表"
                    },
                    new TravelSupplier // ID = 167
                    {
                        SupplierName = "廣島和平紀念公園",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "田邊沙耶",
                        ContactPhone = "+81-82-242-7831",
                        ContactEmail = "peace@hiroshima.jp",
                        SupplierNote = "原爆紀念與和平教育基地"
                    },
                    new TravelSupplier // ID = 168
                    {
                        SupplierName = "鳥取砂丘騎駱駝體驗",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "青山剛",
                        ContactPhone = "+81-857-23-7654",
                        ContactEmail = "camel@tottori.jp",
                        SupplierNote = "日本唯一沙漠型自然地貌"
                    },
                    new TravelSupplier // ID = 169
                    {
                        SupplierName = "松江城古蹟導覽",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "森本佳奈",
                        ContactPhone = "+81-852-21-4030",
                        ContactEmail = "castle@matsue.jp",
                        SupplierNote = "保存完整的國寶木造城堡"
                    },
                    new TravelSupplier // ID = 170
                    {
                        SupplierName = "出雲大社正式參拜服務",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "島田光",
                        ContactPhone = "+81-853-53-3100",
                        ContactEmail = "worship@izumo.jp",
                        SupplierNote = "日本神話起源地與結緣神社"
                    },
                    new TravelSupplier // ID = 171
                    {
                        SupplierName = "錦帶橋木橋導覽",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "大西和也",
                        ContactPhone = "+81-827-29-5116",
                        ContactEmail = "bridge@kintaikyo.jp",
                        SupplierNote = "五拱木造橋名勝"
                    },
                    new TravelSupplier // ID = 172
                    {
                        SupplierName = "岡山後樂園庭園漫遊",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "井口真理",
                        ContactPhone = "+81-86-272-1148",
                        ContactEmail = "garden@korakuen.jp",
                        SupplierNote = "日本三大名園之一"
                    },
                    new TravelSupplier // ID = 173
                    {
                        SupplierName = "倉敷美觀歷史保存區",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "田中浩一",
                        ContactPhone = "+81-86-422-0542",
                        ContactEmail = "bikan@kurashiki.jp",
                        SupplierNote = "白牆倉庫群與古街景"
                    },
                    new TravelSupplier // ID = 174
                    {
                        SupplierName = "萩城下町古屋敷",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "永井信",
                        ContactPhone = "+81-838-25-3139",
                        ContactEmail = "oldtown@hagi.jp",
                        SupplierNote = "歷史保存重建街區"
                    },
                    new TravelSupplier // ID = 175
                    {
                        SupplierName = "大山登山健行會",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "高木理惠",
                        ContactPhone = "+81-859-52-2502",
                        ContactEmail = "trek@daisen.jp",
                        SupplierNote = "中國地區第一高峰"
                    },
                    new TravelSupplier // ID = 176
                    {
                        SupplierName = "廣島城天守導覽",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "江口誠",
                        ContactPhone = "+81-82-221-7512",
                        ContactEmail = "castle@hiroshima.jp",
                        SupplierNote = "重建歷史遺構，展望台設有展示廳"
                    },
                    new TravelSupplier // ID = 177
                    {
                        SupplierName = "鳥取沙丘兒童博物館",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "中原沙織",
                        ContactPhone = "+81-857-22-1234",
                        ContactEmail = "museum@kidsandtottori.jp",
                        SupplierNote = "親子互動館"
                    },
                    new TravelSupplier // ID = 178
                    {
                        SupplierName = "湯田溫泉足湯街",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "木下優",
                        ContactPhone = "+81-83-932-1234",
                        ContactEmail = "onsen@yuda.jp",
                        SupplierNote = "山口知名溫泉街"
                    },
                    new TravelSupplier // ID = 179
                    {
                        SupplierName = "津和野武家屋敷街",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "藤田直人",
                        ContactPhone = "+81-856-72-1771",
                        ContactEmail = "heritage@tsuwano.jp",
                        SupplierNote = "城下町與日式庭園保留區"
                    },
                    new TravelSupplier // ID = 180
                    {
                        SupplierName = "廣島三景園庭園漫遊",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "早川里奈",
                        ContactPhone = "+81-82-262-7111",
                        ContactEmail = "garden@sankei.jp",
                        SupplierNote = "再現山水田園風景"
                    },
                    new TravelSupplier // ID = 181
                    {
                        SupplierName = "岩國白蛇館",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "岸本晴",
                        ContactPhone = "+81-827-35-5301",
                        ContactEmail = "whitesnake@iwakuni.jp",
                        SupplierNote = "保育稀有白蛇文化與傳說"
                    },
                    new TravelSupplier // ID = 182
                    {
                        SupplierName = "島根足立美術館",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "三島香",
                        ContactPhone = "+81-854-28-7111",
                        ContactEmail = "museum@adachi.jp",
                        SupplierNote = "名園與現代日本畫"
                    },
                    new TravelSupplier // ID = 183
                    {
                        SupplierName = "青海島自然海岸觀光",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "岡本海人",
                        ContactPhone = "+81-837-26-1234",
                        ContactEmail = "coast@omijima.jp",
                        SupplierNote = "斷崖奇岩與海蝕地形觀光"
                    },
                    new TravelSupplier // ID = 184
                    {
                        SupplierName = "山口瑠璃光寺五重塔",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "大倉史郎",
                        ContactPhone = "+81-83-922-2409",
                        ContactEmail = "temple@rurikoji.jp",
                        SupplierNote = "室町時代國寶級建築"
                    },
                    new TravelSupplier // ID = 185
                    {
                        SupplierName = "因幡萬葉歷史館",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "石井百合",
                        ContactPhone = "+81-857-51-1234",
                        ContactEmail = "history@inaba.jp",
                        SupplierNote = "以萬葉集與古典文化為主題的展示館"
                    },
                    new TravelSupplier // ID = 186
                    {
                        SupplierName = "廣島燒長田屋",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "長田雅也",
                        ContactPhone = "+81-82-247-0787",
                        ContactEmail = "okonomiyaki@hiroshima.jp",
                        SupplierNote = "廣島燒代表性老店"
                    },
                    new TravelSupplier // ID = 187
                    {
                        SupplierName = "岡山桃太郎壽司",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "赤木大樹",
                        ContactPhone = "+81-86-233-3344",
                        ContactEmail = "sushi@okayama.jp",
                        SupplierNote = "在地壽司與水產料理"
                    },
                    new TravelSupplier // ID = 188
                    {
                        SupplierName = "鳥取和牛炭燒亭",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "片山翔",
                        ContactPhone = "+81-857-27-1111",
                        ContactEmail = "wagyu@tottori.jp",
                        SupplierNote = "鳥取牛排與燒肉"
                    },
                    new TravelSupplier // ID = 189
                    {
                        SupplierName = "出雲蕎麥麵本舖",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "高見結衣",
                        ContactPhone = "+81-853-21-1234",
                        ContactEmail = "soba@izumo.jp",
                        SupplierNote = "日本三大蕎麥之一"
                    },
                    new TravelSupplier // ID = 190
                    {
                        SupplierName = "山口瓦蕎麥專門店",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "三浦光",
                        ContactPhone = "+81-83-932-5555",
                        ContactEmail = "soba@yamaguchi.jp",
                        SupplierNote = "瓦片烘烤蕎麥創意吃法"
                    },
                    new TravelSupplier // ID = 191
                    {
                        SupplierName = "倉敷白壁和食亭",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "藤本楓",
                        ContactPhone = "+81-86-421-2222",
                        ContactEmail = "washoku@kurashiki.jp",
                        SupplierNote = "古街風格日式家庭料理"
                    },
                    new TravelSupplier // ID = 192
                    {
                        SupplierName = "松江鰻魚飯老店",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "安倍優",
                        ContactPhone = "+81-852-26-1234",
                        ContactEmail = "unagi@matsue.jp",
                        SupplierNote = "傳統蒲燒鰻魚料理"
                    },
                    new TravelSupplier // ID = 193
                    {
                        SupplierName = "宮島牡蠣料理坊",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "濱田愛",
                        ContactPhone = "+81-829-44-2345",
                        ContactEmail = "kaki@itsukushima.jp",
                        SupplierNote = "廣島牡蠣套餐與定食"
                    },
                    new TravelSupplier // ID = 194
                    {
                        SupplierName = "津和野壽司與茶屋",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "宇野信",
                        ContactPhone = "+81-856-72-8888",
                        ContactEmail = "sushi@tsuwano.jp",
                        SupplierNote = "結合在地茶與輕食"
                    },
                    new TravelSupplier // ID = 195
                    {
                        SupplierName = "岩國蓮根創意料理",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "久保咲",
                        ContactPhone = "+81-827-29-3333",
                        ContactEmail = "lotus@iwakuni.jp",
                        SupplierNote = "以岩國特產蓮根為主題"
                    },
                    new TravelSupplier // ID = 196
                    {
                        SupplierName = "廣島ANA洲際飯店",
                        SupplierType = SupplierType.Accommodation,
                        ContactName = "田島圭",
                        ContactPhone = "+81-82-241-1111",
                        ContactEmail = "stay@ana-hiroshima.jp",
                        SupplierNote = "市中心高級觀光飯店"
                    },
                    new TravelSupplier // ID = 197
                    {
                        SupplierName = "松江城見城景旅館",
                        SupplierType = SupplierType.Accommodation,
                        ContactName = "江島瑞穗",
                        ContactPhone = "+81-852-25-8888",
                        ContactEmail = "hotel@matsue.jp",
                        SupplierNote = "靠近城址的日式旅館"
                    },
                    new TravelSupplier // ID = 198
                    {
                        SupplierName = "鳥取沙丘之湯溫泉旅館",
                        SupplierType = SupplierType.Accommodation,
                        ContactName = "山崎篤",
                        ContactPhone = "+81-857-29-7777",
                        ContactEmail = "onsen@tottorisand.jp",
                        SupplierNote = "遠眺沙丘，附設天然湯泉"
                    },

                    // 四國 ID = 7
                    new TravelSupplier // ID = 199
                    {
                        SupplierName = "栗林公園遊園導覽",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "中村悠真",
                        ContactPhone = "+81-87-833-7411",
                        ContactEmail = "info@ritsuringarden.jp",
                        SupplierNote = "日本最美庭園之一，位於高松"
                    },
                    new TravelSupplier // ID = 200
                    {
                        SupplierName = "金刀比羅宮參拜服務",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "長谷川由香",
                        ContactPhone = "+81-877-75-2121",
                        ContactEmail = "shrine@kotohira.jp",
                        SupplierNote = "香川知名神社，785階石梯知名"
                    },
                    new TravelSupplier // ID = 201
                    {
                        SupplierName = "直島地中美術館導覽",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "本田翔",
                        ContactPhone = "+81-87-892-3755",
                        ContactEmail = "art@naoshima.jp",
                        SupplierNote = "安藤忠雄設計的現代藝術館"
                    },
                    new TravelSupplier // ID = 202
                    {
                        SupplierName = "德島阿波舞會館",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "佐藤志織",
                        ContactPhone = "+81-88-611-1611",
                        ContactEmail = "awa@tokushima.jp",
                        SupplierNote = "每日演出阿波舞蹈與歷史展示"
                    },
                    new TravelSupplier // ID = 203
                    {
                        SupplierName = "鳴門漩渦觀潮船",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "石井海人",
                        ContactPhone = "+81-88-687-0101",
                        ContactEmail = "whirlpool@naruto.jp",
                        SupplierNote = "搭船近距離觀賞漩渦潮流奇景"
                    },
                    new TravelSupplier // ID = 204
                    {
                        SupplierName = "眉山纜車展望台",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "吉田英樹",
                        ContactPhone = "+81-88-652-3617",
                        ContactEmail = "view@bizan.jp",
                        SupplierNote = "德島市夜景觀賞首選"
                    },
                    new TravelSupplier // ID = 205
                    {
                        SupplierName = "道後溫泉本館",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "村上理恵",
                        ContactPhone = "+81-89-921-5141",
                        ContactEmail = "onsen@dogo.jp",
                        SupplierNote = "日本最古老溫泉建築之一"
                    },
                    new TravelSupplier // ID = 206
                    {
                        SupplierName = "松山城空中纜車",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "藤田拓也",
                        ContactPhone = "+81-89-921-4873",
                        ContactEmail = "castle@ehime.jp",
                        SupplierNote = "城堡與城市全景"
                    },
                    new TravelSupplier // ID = 207
                    {
                        SupplierName = "下灘車站海岸景點",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "大西舞",
                        ContactPhone = "+81-89-986-1211",
                        ContactEmail = "sunset@shimonada.jp",
                        SupplierNote = "知名無人海景車站"
                    },
                    new TravelSupplier // ID = 208
                    {
                        SupplierName = "宇和島鯛釣體驗中心",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "岸本洋介",
                        ContactPhone = "+81-895-22-3333",
                        ContactEmail = "fishing@uwajima.jp",
                        SupplierNote = "親自體驗海釣活動"
                    },
                    new TravelSupplier // ID = 209
                    {
                        SupplierName = "四萬十川屋形船觀光",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "岡本優",
                        ContactPhone = "+81-880-35-5511",
                        ContactEmail = "boat@shimanto.jp",
                        SupplierNote = "高知自然川景代表行程"
                    },
                    new TravelSupplier // ID = 210
                    {
                        SupplierName = "高知桂濱坂本龍馬像",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "濱田修",
                        ContactPhone = "+81-88-841-4140",
                        ContactEmail = "statue@kochi.jp",
                        SupplierNote = "紀念歷史偉人與太平洋海景"
                    },
                    new TravelSupplier // ID = 211
                    {
                        SupplierName = "足摺岬展望台",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "西村真",
                        ContactPhone = "+81-880-88-1111",
                        ContactEmail = "cape@ashizuri.jp",
                        SupplierNote = "四國最南端壯麗海角"
                    },
                    new TravelSupplier // ID = 212
                    {
                        SupplierName = "龍河洞鐘乳石洞",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "山口剛",
                        ContactPhone = "+81-887-53-2144",
                        ContactEmail = "cave@ryugado.jp",
                        SupplierNote = "高知縣內最大自然洞穴"
                    },
                    new TravelSupplier // ID = 213
                    {
                        SupplierName = "內子町歷史街道保存館",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "川端綾子",
                        ContactPhone = "+81-893-44-2118",
                        ContactEmail = "heritage@uchiko.jp",
                        SupplierNote = "愛媛懷舊木造建築街景"
                    },
                    new TravelSupplier // ID = 214
                    {
                        SupplierName = "高松玉藻公園",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "河合信",
                        ContactPhone = "+81-87-851-1521",
                        ContactEmail = "castlepark@takamatsu.jp",
                        SupplierNote = "靠海城跡與日式庭園"
                    },
                    new TravelSupplier // ID = 215
                    {
                        SupplierName = "豐島美術館參觀服務",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "森本春",
                        ContactPhone = "+81-879-68-3555",
                        ContactEmail = "museum@teshima.jp",
                        SupplierNote = "融合建築與自然的藝術空間"
                    },
                    new TravelSupplier // ID = 216
                    {
                        SupplierName = "瓶之森林自行車體驗",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "原田誠",
                        ContactPhone = "+81-88-882-5555",
                        ContactEmail = "cycling@kazura.jp",
                        SupplierNote = "遍布山林的綠色單車道"
                    },
                    new TravelSupplier // ID = 217
                    {
                        SupplierName = "仁淀藍水清流探訪",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "三宅玲",
                        ContactPhone = "+81-88-855-1234",
                        ContactEmail = "river@niyodo.jp",
                        SupplierNote = "以清澈度聞名的高知河川"
                    },
                    new TravelSupplier // ID = 218
                    {
                        SupplierName = "祖谷藤蔓橋景區",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "岸田卓",
                        ContactPhone = "+81-883-72-7620",
                        ContactEmail = "bridge@iya.jp",
                        SupplierNote = "德島秘境吊橋體驗"
                    },
                    new TravelSupplier // ID = 219
                    {
                        SupplierName = "讚岐烏龍麵本舖",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "長井翔",
                        ContactPhone = "+81-87-833-4000",
                        ContactEmail = "udon@sanuki.jp",
                        SupplierNote = "香川代表性烏龍麵名店"
                    },
                    new TravelSupplier // ID = 220
                    {
                        SupplierName = "高知鰹節藁燒屋",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "岡田健",
                        ContactPhone = "+81-88-888-3333",
                        ContactEmail = "katsuo@kochi.jp",
                        SupplierNote = "藁燒鰹魚刺身料理"
                    },
                    new TravelSupplier // ID = 221
                    {
                        SupplierName = "德島鳴門鯛料理坊",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "福島結",
                        ContactPhone = "+81-88-684-1234",
                        ContactEmail = "tai@naruto.jp",
                        SupplierNote = "以鯛魚為主的日式會席"
                    },
                    new TravelSupplier // ID = 222
                    {
                        SupplierName = "松山炸雞南蠻本舖",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "山田翔",
                        ContactPhone = "+81-89-931-4444",
                        ContactEmail = "chicken@matsuyama.jp",
                        SupplierNote = "愛媛鄉土特色炸雞"
                    },
                    new TravelSupplier // ID = 223
                    {
                        SupplierName = "香川和菓子茶屋",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "吉村舞",
                        ContactPhone = "+81-87-812-9999",
                        ContactEmail = "wagashi@kagawa.jp",
                        SupplierNote = "結合抹茶與傳統點心"
                    },
                    new TravelSupplier // ID = 224
                    {
                        SupplierName = "內子町鄉土料理亭",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "高原浩",
                        ContactPhone = "+81-893-44-2110",
                        ContactEmail = "local@uchiko.jp",
                        SupplierNote = "鄉村風味家常日式料理"
                    },
                    new TravelSupplier // ID = 225
                    {
                        SupplierName = "直島海鮮丼小館",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "木下麗",
                        ContactPhone = "+81-87-892-3333",
                        ContactEmail = "kaisendon@naoshima.jp",
                        SupplierNote = "搭配地中海風格餐點"
                    },
                    new TravelSupplier // ID = 226
                    {
                        SupplierName = "德島蕎麥專門店",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "桐山剛",
                        ContactPhone = "+81-88-655-5678",
                        ContactEmail = "soba@tokushima.jp",
                        SupplierNote = "當地手打蕎麥麵與天婦羅"
                    },
                    new TravelSupplier // ID = 227
                    {
                        SupplierName = "四萬十川川魚料理店",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "宇野貴子",
                        ContactPhone = "+81-880-35-9999",
                        ContactEmail = "sweetfish@shimanto.jp",
                        SupplierNote = "炭烤鮎魚與溪流便當"
                    },
                    new TravelSupplier // ID = 228
                    {
                        SupplierName = "道後溫泉甜點小舖",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "松井美香",
                        ContactPhone = "+81-89-925-8888",
                        ContactEmail = "dessert@dogo.jp",
                        SupplierNote = "結合溫泉蛋與布丁創意甜品"
                    },
                    new TravelSupplier // ID = 229
                    {
                        SupplierName = "高松車站前商務飯店",
                        SupplierType = SupplierType.Accommodation,
                        ContactName = "佐佐木陽",
                        ContactPhone = "+81-87-823-1111",
                        ContactEmail = "stay@takamatsu.jp",
                        SupplierNote = "交通便利的現代飯店"
                    },
                    new TravelSupplier // ID = 230
                    {
                        SupplierName = "道後溫泉大和旅館",
                        SupplierType = SupplierType.Accommodation,
                        ContactName = "野口宏",
                        ContactPhone = "+81-89-941-1234",
                        ContactEmail = "onsen@yamatoryokan.jp",
                        SupplierNote = "溫泉街和式住宿體驗"
                    },
                    new TravelSupplier // ID = 231
                    {
                        SupplierName = "高知城下溫泉飯店",
                        SupplierType = SupplierType.Accommodation,
                        ContactName = "平井誠",
                        ContactPhone = "+81-88-823-4567",
                        ContactEmail = "hotel@kochicastle.jp",
                        SupplierNote = "鄰近高知城的傳統旅宿"
                    },

                    // 九州 ID = 8
                    new TravelSupplier // ID = 232
                    {
                        SupplierName = "太宰府天滿宮",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "田中翔",
                        ContactPhone = "+81-92-922-8225",
                        ContactEmail = "shrine@dazaifu.jp",
                        SupplierNote = "學問之神，春季梅花著名"
                    },
                    new TravelSupplier // ID = 233
                    {
                        SupplierName = "博多運河城水舞秀",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "伊藤沙織",
                        ContactPhone = "+81-92-282-2525",
                        ContactEmail = "canal@hakata.jp",
                        SupplierNote = "購物與聲光水舞表演"
                    },
                    new TravelSupplier // ID = 234
                    {
                        SupplierName = "柳川川下り船屋",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "大西健",
                        ContactPhone = "+81-944-73-4343",
                        ContactEmail = "boat@yanagawa.jp",
                        SupplierNote = "乘船遊歷古運河之城"
                    },
                    new TravelSupplier // ID = 235
                    {
                        SupplierName = "長崎哥拉巴園",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "吉田亮",
                        ContactPhone = "+81-95-822-8223",
                        ContactEmail = "glover@nagaski.jp",
                        SupplierNote = "洋館建築與南山景色"
                    },
                    new TravelSupplier // ID = 236
                    {
                        SupplierName = "長崎原爆資料館",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "宮本理惠",
                        ContactPhone = "+81-95-844-1231",
                        ContactEmail = "peace@museum.jp",
                        SupplierNote = "戰爭和平教育地標"
                    },
                    new TravelSupplier // ID = 237
                    {
                        SupplierName = "豪斯登堡主題樂園",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "松下剛",
                        ContactPhone = "+81-570-064-110",
                        ContactEmail = "info@huistenbosch.jp",
                        SupplierNote = "荷蘭風格建築與燈光展演"
                    },
                    new TravelSupplier // ID = 238
                    {
                        SupplierName = "熊本城復原天守閣",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "村田真",
                        ContactPhone = "+81-96-352-5900",
                        ContactEmail = "castle@kumamoto.jp",
                        SupplierNote = "震災後重建的歷史名城"
                    },
                    new TravelSupplier // ID = 239
                    {
                        SupplierName = "阿蘇火山火口",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "岩田浩",
                        ContactPhone = "+81-967-34-1600",
                        ContactEmail = "crater@aso.jp",
                        SupplierNote = "世界最大級活火山口"
                    },
                    new TravelSupplier // ID = 240
                    {
                        SupplierName = "黑川溫泉散策步道",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "福田結衣",
                        ContactPhone = "+81-967-44-0076",
                        ContactEmail = "onsen@kurokawa.jp",
                        SupplierNote = "溫泉街與自然溪谷"
                    },
                    new TravelSupplier // ID = 241
                    {
                        SupplierName = "由布院金鱗湖",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "橋本咲",
                        ContactPhone = "+81-977-84-3111",
                        ContactEmail = "lake@yufuin.jp",
                        SupplierNote = "晨霧夢幻湖泊"
                    },
                    new TravelSupplier // ID = 242
                    {
                        SupplierName = "別府地獄溫泉巡禮",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "木村智也",
                        ContactPhone = "+81-977-66-1577",
                        ContactEmail = "hells@beppu.jp",
                        SupplierNote = "七大地獄溫泉觀光路線"
                    },
                    new TravelSupplier // ID = 243
                    {
                        SupplierName = "高千穗峽划船體驗",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "高橋真琴",
                        ContactPhone = "+81-982-73-1213",
                        ContactEmail = "gorge@takachiho.jp",
                        SupplierNote = "宮崎絕景峽谷與神話之地"
                    },
                    new TravelSupplier // ID = 244
                    {
                        SupplierName = "鵜戶神宮洞窟參拜",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "渡邊剛",
                        ContactPhone = "+81-987-29-1001",
                        ContactEmail = "shrine@udo.jp",
                        SupplierNote = "懸崖上的洞窟神社"
                    },
                    new TravelSupplier // ID = 245
                    {
                        SupplierName = "霧島神宮古道",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "遠藤健",
                        ContactPhone = "+81-995-57-0001",
                        ContactEmail = "shrine@kirishima.jp",
                        SupplierNote = "自然與信仰結合的靈地"
                    },
                    new TravelSupplier // ID = 246
                    {
                        SupplierName = "櫻島火山觀景台",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "永井誠",
                        ContactPhone = "+81-99-245-0100",
                        ContactEmail = "volcano@sakurajima.jp",
                        SupplierNote = "鹿兒島最具代表火山"
                    },
                    new TravelSupplier // ID = 247
                    {
                        SupplierName = "指宿砂浴體驗場",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "秋山真央",
                        ContactPhone = "+81-993-23-3900",
                        ContactEmail = "sand@ibusuki.jp",
                        SupplierNote = "埋身溫熱砂的特殊溫泉"
                    },
                    new TravelSupplier // ID = 248
                    {
                        SupplierName = "志布志鐵道遺跡巡禮",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "今井信",
                        ContactPhone = "+81-994-72-1234",
                        ContactEmail = "railway@shibushi.jp",
                        SupplierNote = "歷史蒸汽鐵道路線遺跡"
                    },
                    new TravelSupplier // ID = 249
                    {
                        SupplierName = "青島神社與海灘",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "杉本舞",
                        ContactPhone = "+81-985-65-1262",
                        ContactEmail = "beach@aoshima.jp",
                        SupplierNote = "位於小島上的神社與鬼之洗衣板岩層"
                    },
                    new TravelSupplier // ID = 250
                    {
                        SupplierName = "嬉野溫泉足湯街",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "田島純",
                        ContactPhone = "+81-954-43-0137",
                        ContactEmail = "onsen@ureshino.jp",
                        SupplierNote = "佐賀知名美肌溫泉"
                    },
                    new TravelSupplier // ID = 251
                    {
                        SupplierName = "平戶城遺跡與博物館",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "小倉亮",
                        ContactPhone = "+81-950-22-4111",
                        ContactEmail = "castle@hirado.jp",
                        SupplierNote = "長崎歷史與歐風建築混合城郭"
                    },
                    new TravelSupplier // ID = 252
                    {
                        SupplierName = "博多一風堂本店",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "吉川翔",
                        ContactPhone = "+81-92-771-1234",
                        ContactEmail = "ramen@ippudo.jp",
                        SupplierNote = "豚骨拉麵知名品牌發源地"
                    },
                    new TravelSupplier // ID = 253
                    {
                        SupplierName = "熊本馬肉料理屋",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "石田真",
                        ContactPhone = "+81-96-352-2222",
                        ContactEmail = "horse@kumamoto.jp",
                        SupplierNote = "生馬肉刺身與燒肉"
                    },
                    new TravelSupplier // ID = 254
                    {
                        SupplierName = "長崎皿烏龍麵堂",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "野村舞",
                        ContactPhone = "+81-95-824-4567",
                        ContactEmail = "champon@nagasaki.jp",
                        SupplierNote = "中華風融合長崎名物"
                    },
                    new TravelSupplier // ID = 255
                    {
                        SupplierName = "由布院田舍料理",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "池田洋",
                        ContactPhone = "+81-977-84-5678",
                        ContactEmail = "countryside@yufuin.jp",
                        SupplierNote = "鄉村野菜與釜飯料理"
                    },
                    new TravelSupplier // ID = 256
                    {
                        SupplierName = "別府地獄蒸工房",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "浜口結",
                        ContactPhone = "+81-977-66-3775",
                        ContactEmail = "steam@beppu.jp",
                        SupplierNote = "使用地熱蒸氣現做食材"
                    },
                    new TravelSupplier // ID = 257
                    {
                        SupplierName = "鹿兒島黑豚燒肉店",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "中嶋健",
                        ContactPhone = "+81-99-250-5566",
                        ContactEmail = "kurobuta@kagoshima.jp",
                        SupplierNote = "黑毛豬肉為主食材"
                    },
                    new TravelSupplier // ID = 258
                    {
                        SupplierName = "高千穗鄉土料理館",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "河合千夏",
                        ContactPhone = "+81-982-73-1022",
                        ContactEmail = "local@takachiho.jp",
                        SupplierNote = "山野炊與鄉村料理為特色"
                    },
                    new TravelSupplier // ID = 259
                    {
                        SupplierName = "嬉野溫泉豆腐料理坊",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "大谷理恵",
                        ContactPhone = "+81-954-43-3333",
                        ContactEmail = "tofu@ureshino.jp",
                        SupplierNote = "滑嫩湯豆腐主題套餐"
                    },
                    new TravelSupplier // ID = 260
                    {
                        SupplierName = "柳川鰻魚飯屋",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "藤本卓也",
                        ContactPhone = "+81-944-72-9876",
                        ContactEmail = "unagi@yanagawa.jp",
                        SupplierNote = "蒸籠鰻魚飯搭配甜醬"
                    },
                    new TravelSupplier // ID = 261
                    {
                        SupplierName = "宮崎芒果甜點舖",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "三宅花",
                        ContactPhone = "+81-985-26-1234",
                        ContactEmail = "mango@dessert.jp",
                        SupplierNote = "使用當地芒果製成多種甜點"
                    },
                    new TravelSupplier // ID = 262
                    {
                        SupplierName = "福岡天神東橫INN",
                        SupplierType = SupplierType.Accommodation,
                        ContactName = "小林龍",
                        ContactPhone = "+81-92-725-1045",
                        ContactEmail = "stay@toyoko-inn.jp",
                        SupplierNote = "市區連鎖商務型飯店"
                    },
                    new TravelSupplier // ID = 263
                    {
                        SupplierName = "由布院溫泉和風旅館",
                        SupplierType = SupplierType.Accommodation,
                        ContactName = "森田香",
                        ContactPhone = "+81-977-84-5656",
                        ContactEmail = "onsen@yufuin.jp",
                        SupplierNote = "附設露天風呂與私人湯屋"
                    },
                    new TravelSupplier // ID = 264
                    {
                        SupplierName = "鹿兒島城山觀光飯店",
                        SupplierType = SupplierType.Accommodation,
                        ContactName = "東條誠",
                        ContactPhone = "+81-99-224-2211",
                        ContactEmail = "hotel@shiroyama.jp",
                        SupplierNote = "可眺望櫻島的高級飯店"
                    },

                    // 沖繩 ID = 9
                    new TravelSupplier // ID = 265
                    {
                        SupplierName = "沖繩美麗海水族館",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "新垣舞",
                        ContactPhone = "+81-980-48-3748",
                        ContactEmail = "info@churaumi.jp",
                        SupplierNote = "世界最大級水族館之一，黑潮大水槽著名"
                    },
                    new TravelSupplier // ID = 266
                    {
                        SupplierName = "首里城公園歷史導覽",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "比嘉翔",
                        ContactPhone = "+81-98-886-2020",
                        ContactEmail = "shurijo@naha.jp",
                        SupplierNote = "琉球王國歷史重建宮殿"
                    },
                    new TravelSupplier // ID = 267
                    {
                        SupplierName = "玉泉洞鐘乳石洞",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "宮城綾",
                        ContactPhone = "+81-98-949-7421",
                        ContactEmail = "cave@gyokusendo.jp",
                        SupplierNote = "沖繩最大天然鐘乳石洞"
                    },
                    new TravelSupplier // ID = 268
                    {
                        SupplierName = "琉球村文化主題園區",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "仲宗根大樹",
                        ContactPhone = "+81-98-965-1234",
                        ContactEmail = "culture@ryukyumura.jp",
                        SupplierNote = "重現古琉球生活風貌的主題村"
                    },
                    new TravelSupplier // ID = 269
                    {
                        SupplierName = "萬座毛絕壁海岸",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "城間真央",
                        ContactPhone = "+81-98-966-8080",
                        ContactEmail = "cliff@manzamo.jp",
                        SupplierNote = "象鼻岩自然奇觀與夕陽景點"
                    },
                    new TravelSupplier // ID = 270
                    {
                        SupplierName = "國際通購物街導覽",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "照屋智也",
                        ContactPhone = "+81-98-863-2755",
                        ContactEmail = "street@kokusaidori.jp",
                        SupplierNote = "那霸最熱鬧的觀光大街"
                    },
                    new TravelSupplier // ID = 271
                    {
                        SupplierName = "波上宮與海灘",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "平良恵",
                        ContactPhone = "+81-98-868-3697",
                        ContactEmail = "shrine@waves.jp",
                        SupplierNote = "神社與海灘共存的市中心景點"
                    },
                    new TravelSupplier // ID = 272
                    {
                        SupplierName = "殘波岬燈塔公園",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "金城龍",
                        ContactPhone = "+81-98-958-5588",
                        ContactEmail = "cape@zanpa.jp",
                        SupplierNote = "壯觀岩岸與觀景燈塔"
                    },
                    new TravelSupplier // ID = 273
                    {
                        SupplierName = "今歸仁城跡櫻花祭",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "知念花",
                        ContactPhone = "+81-980-56-4400",
                        ContactEmail = "castle@nakijin.jp",
                        SupplierNote = "北部古城與冬櫻花祭"
                    },
                    new TravelSupplier // ID = 274
                    {
                        SupplierName = "沖繩世界蛇酒展示館",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "大城剛",
                        ContactPhone = "+81-98-949-7421",
                        ContactEmail = "habu@okinawaworld.jp",
                        SupplierNote = "蛇酒製法與琉球文化展示"
                    },
                    new TravelSupplier // ID = 275
                    {
                        SupplierName = "古宇利大橋海景觀光",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "伊佐海",
                        ContactPhone = "+81-980-56-2256",
                        ContactEmail = "bridge@kouri.jp",
                        SupplierNote = "連接本島與離島的絕景大橋"
                    },
                    new TravelSupplier // ID = 276
                    {
                        SupplierName = "瀨底島浮潛俱樂部",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "島袋瑞希",
                        ContactPhone = "+81-980-47-1234",
                        ContactEmail = "snorkel@sesoko.jp",
                        SupplierNote = "珊瑚礁與熱帶魚天堂"
                    },
                    new TravelSupplier // ID = 277
                    {
                        SupplierName = "水納島一日快艇遊",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "照屋光",
                        ContactPhone = "+81-980-48-2222",
                        ContactEmail = "tour@minna.jp",
                        SupplierNote = "人氣離島透明海水之旅"
                    },
                    new TravelSupplier // ID = 278
                    {
                        SupplierName = "青之洞窟潛水中心",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "仲間剛志",
                        ContactPhone = "+81-98-982-1234",
                        ContactEmail = "diving@bluecave.jp",
                        SupplierNote = "恩納村著名潛水景點"
                    },
                    new TravelSupplier // ID = 279
                    {
                        SupplierName = "海中道路自行車遊",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "宮平翔",
                        ContactPhone = "+81-98-978-1234",
                        ContactEmail = "cycling@kaichu.jp",
                        SupplierNote = "橫跨海上的騎行體驗"
                    },
                    new TravelSupplier // ID = 280
                    {
                        SupplierName = "宜野灣熱帶海灘",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "喜納舞",
                        ContactPhone = "+81-98-897-2751",
                        ContactEmail = "beach@ginowan.jp",
                        SupplierNote = "適合海灘活動與露營的海岸"
                    },
                    new TravelSupplier // ID = 281
                    {
                        SupplierName = "泡瀨漁港夕陽觀景",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "上原裕",
                        ContactPhone = "+81-98-937-1234",
                        ContactEmail = "sunset@awase.jp",
                        SupplierNote = "本島東岸最美夕陽之一"
                    },
                    new TravelSupplier // ID = 282
                    {
                        SupplierName = "石垣島川平灣玻璃船",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "仲井真里",
                        ContactPhone = "+81-980-83-1234",
                        ContactEmail = "glassboat@kabira.jp",
                        SupplierNote = "無需潛水也能欣賞珊瑚"
                    },
                    new TravelSupplier // ID = 283
                    {
                        SupplierName = "西表島森林探險導覽",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "新城光",
                        ContactPhone = "+81-980-85-2222",
                        ContactEmail = "jungle@iriomote.jp",
                        SupplierNote = "熱帶雨林與瀑布健行路線"
                    },
                    new TravelSupplier // ID = 284
                    {
                        SupplierName = "宮古島來間大橋觀景平台",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "比嘉由香",
                        ContactPhone = "+81-980-72-3333",
                        ContactEmail = "kurima@miyako.jp",
                        SupplierNote = "絕美橋樑與浮潛勝地"
                    },
                    new TravelSupplier // ID = 285
                    {
                        SupplierName = "國際通沖繩料理花笠食堂",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "嘉數仁",
                        ContactPhone = "+81-98-863-5556",
                        ContactEmail = "okinawa@hanagasa.jp",
                        SupplierNote = "提供苦瓜炒蛋、滷豬腳等琉球特色料理"

                    },
                    new TravelSupplier // ID = 286
                    {
                        SupplierName = "首里城御膳懷石屋",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "喜屋武美雪",
                        ContactPhone = "+81-98-886-1414",
                        ContactEmail = "kaiseki@shuriryori.jp",
                        SupplierNote = "結合日式與琉球王朝御膳"
                    },
                    new TravelSupplier // ID = 287
                    {
                        SupplierName = "那霸市場海鮮丼亭",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "平良龍",
                        ContactPhone = "+81-98-867-4444",
                        ContactEmail = "don@nahaichiba.jp",
                        SupplierNote = "使用當日漁港直送鮮魚"
                    },
                    new TravelSupplier // ID = 288
                    {
                        SupplierName = "北谷美國村牛排館",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "志村翔",
                        ContactPhone = "+81-98-936-7890",
                        ContactEmail = "steak@americanvillage.jp",
                        SupplierNote = "美式風格，深夜營業"
                    },
                    new TravelSupplier // ID = 289
                    {
                        SupplierName = "古宇利島海景咖啡",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "山城詩織",
                        ContactPhone = "+81-980-56-8888",
                        ContactEmail = "cafe@kouri.jp",
                        SupplierNote = "遠眺大橋的高人氣咖啡店"
                    },
                    new TravelSupplier // ID = 290
                    {
                        SupplierName = "恩納村海葡萄海鮮店",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "宮里一郎",
                        ContactPhone = "+81-98-966-5555",
                        ContactEmail = "umi@sea-grape.jp",
                        SupplierNote = "主打沖繩海藻與貝類生食"
                    },
                    new TravelSupplier // ID = 291
                    {
                        SupplierName = "石垣牛壽喜燒本舖",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "與那嶺翔",
                        ContactPhone = "+81-980-82-7777",
                        ContactEmail = "sukiyaki@ishigaki.jp",
                        SupplierNote = "石垣島名產牛肉料理"
                    },
                    new TravelSupplier // ID = 292
                    {
                        SupplierName = "宮古島芒果甜點專賣店",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "城間愛",
                        ContactPhone = "+81-980-72-4567",
                        ContactEmail = "mango@miyako.jp",
                        SupplierNote = "各式芒果布丁、刨冰、果汁"
                    },
                    new TravelSupplier // ID = 293
                    {
                        SupplierName = "西表島山野料理小屋",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "大嶺健",
                        ContactPhone = "+81-980-85-4321",
                        ContactEmail = "local@iriomote.jp",
                        SupplierNote = "提供山野菜、野豬燉物與藥膳風味"
                    },
                    new TravelSupplier // ID = 294
                    {
                        SupplierName = "瀨長島泡盛居酒屋",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "上間智子",
                        ContactPhone = "+81-98-857-9999",
                        ContactEmail = "awamori@senagajima.jp",
                        SupplierNote = "夜景搭配沖繩燒酒與串燒"
                    },
                    new TravelSupplier // ID = 295
                    {
                        SupplierName = "那霸國際通飯店",
                        SupplierType = SupplierType.Accommodation,
                        ContactName = "仲村誠",
                        ContactPhone = "+81-98-861-1234",
                        ContactEmail = "hotel@kokusaidori.jp",
                        SupplierNote = "市中心商圈便利地點"
                    },
                    new TravelSupplier // ID = 296
                    {
                        SupplierName = "沖繩北部海濱度假村",
                        SupplierType = SupplierType.Accommodation,
                        ContactName = "伊地知幸",
                        ContactPhone = "+81-980-51-6789",
                        ContactEmail = "resort@okinawanorth.jp",
                        SupplierNote = "私人沙灘與親子設施齊全"
                    },
                    new TravelSupplier // ID = 297
                    {
                        SupplierName = "石垣島海景飯店",
                        SupplierType = SupplierType.Accommodation,
                        ContactName = "新垣優",
                        ContactPhone = "+81-980-88-2222",
                        ContactEmail = "stay@ishigaki.jp",
                        SupplierNote = "可直接眺望川平灣與珊瑚礁海域"
                    }
                );
                await _context.SaveChangesAsync();
                _context.TravelSuppliers.AddRange(
                    // 台灣北部 ID = 10
                    new TravelSupplier // ID = 298
                    {
                        SupplierName = "台北101觀景台",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "林怡君",
                        ContactPhone = "02-8101-8899",
                        ContactEmail = "info@taipei101.com.tw",
                        SupplierNote = "台北市地標，高樓景觀"
                    },
                    new TravelSupplier // ID = 299
                    {
                        SupplierName = "國立故宮博物院",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "張育誠",
                        ContactPhone = "02-2881-2021",
                        ContactEmail = "service@npm.gov.tw",
                        SupplierNote = "亞洲重要文物典藏博物館"
                    },
                    new TravelSupplier // ID = 300
                    {
                        SupplierName = "士林官邸花園",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "陳淑芬",
                        ContactPhone = "02-2883-6340",
                        ContactEmail = "garden@shilin.gov.tw",
                        SupplierNote = "蔣中正官邸，玫瑰花季聞名"
                    },
                    new TravelSupplier // ID = 301
                    {
                        SupplierName = "陽明山國家公園",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "黃志賢",
                        ContactPhone = "02-2861-3601",
                        ContactEmail = "info@ymsnp.gov.tw",
                        SupplierNote = "火山地形與溫泉自然景觀"
                    },
                    new TravelSupplier // ID = 302
                    {
                        SupplierName = "北投溫泉博物館",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "李思涵",
                        ContactPhone = "02-2893-9981",
                        ContactEmail = "museum@beitou.org.tw",
                        SupplierNote = "日治時期溫泉文化建築"
                    },
                    new TravelSupplier // ID = 303
                    {
                        SupplierName = "淡水漁人碼頭",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "洪子庭",
                        ContactPhone = "02-2805-9998",
                        ContactEmail = "harbor@tamsui.gov.tw",
                        SupplierNote = "情人橋與河海交匯美景"
                    },
                    new TravelSupplier // ID = 304
                    {
                        SupplierName = "九份老街與昇平戲院",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "蔡家維",
                        ContactPhone = "02-2496-2800",
                        ContactEmail = "tour@jiufen.org",
                        SupplierNote = "山城風情與復古戲院"
                    },
                    new TravelSupplier // ID = 305
                    {
                        SupplierName = "金瓜石黃金博物館",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "曾芷涵",
                        ContactPhone = "02-2496-2800",
                        ContactEmail = "gold@museum.org",
                        SupplierNote = "舊礦山歷史與黃金體驗"
                    },
                    new TravelSupplier // ID = 306
                    {
                        SupplierName = "野柳地質公園",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "邱承恩",
                        ContactPhone = "02-2492-2016",
                        ContactEmail = "info@yehliu.org.tw",
                        SupplierNote = "女王頭與海蝕奇岩"
                    },
                    new TravelSupplier // ID = 307
                    {
                        SupplierName = "基隆和平島公園",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "李庭旭",
                        ContactPhone = "02-2463-5452",
                        ContactEmail = "park@hepingdao.tw",
                        SupplierNote = "濱海步道與海水泳池"
                    },
                    new TravelSupplier // ID = 308
                    {
                        SupplierName = "桃園大溪老街",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "郭婉婷",
                        ContactPhone = "03-388-2201",
                        ContactEmail = "tour@daxi.gov.tw",
                        SupplierNote = "古建築與豆乾老店"
                    },
                    new TravelSupplier // ID = 309
                    {
                        SupplierName = "小烏來天空步道",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "江柏文",
                        ContactPhone = "03-382-1835",
                        ContactEmail = "skywalk@xiaowulai.gov.tw",
                        SupplierNote = "透明步道與瀑布美景"
                    },
                    new TravelSupplier // ID = 310
                    {
                        SupplierName = "新竹市城隍廟",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "周孟真",
                        ContactPhone = "03-522-7016",
                        ContactEmail = "temple@hsinchu.tw",
                        SupplierNote = "歷史悠久的宗教信仰中心"
                    },
                    new TravelSupplier // ID = 311
                    {
                        SupplierName = "十八尖山健行步道",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "莊子賢",
                        ContactPhone = "03-528-3223",
                        ContactEmail = "hiking@shibashan.tw",
                        SupplierNote = "親民城市郊山步道"
                    },
                    new TravelSupplier // ID = 312
                    {
                        SupplierName = "內灣老街與火車站",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "簡思妤",
                        ContactPhone = "03-584-1234",
                        ContactEmail = "tour@neiwang.tw",
                        SupplierNote = "山區鐵道與客家文化"
                    },
                    new TravelSupplier // ID = 313
                    {
                        SupplierName = "竹東五峰冷泉",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "蕭振宇",
                        ContactPhone = "03-584-6523",
                        ContactEmail = "spring@wufeng.tw",
                        SupplierNote = "自然冷泉泡腳步道"
                    },
                    new TravelSupplier // ID = 314
                    {
                        SupplierName = "樂善堂古蹟巡禮",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "沈韻慈",
                        ContactPhone = "03-526-8901",
                        ContactEmail = "tour@leshantang.tw",
                        SupplierNote = "百年古蹟與慈善歷史"
                    },
                    new TravelSupplier // ID = 315
                    {
                        SupplierName = "貓空纜車茶園之旅",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "魏紹宗",
                        ContactPhone = "02-218-1234",
                        ContactEmail = "gondola@maokong.org",
                        SupplierNote = "空中茶園觀景體驗"
                    },
                    new TravelSupplier // ID = 316
                    {
                        SupplierName = "象山觀景步道",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "陳雯雯",
                        ContactPhone = "02-234-2345",
                        ContactEmail = "trail@xiangshan.tw",
                        SupplierNote = "最佳101觀景點之一"
                    },
                    new TravelSupplier // ID = 317
                    {
                        SupplierName = "淡水紅毛城古蹟區",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "周翊豪",
                        ContactPhone = "02-2623-1001",
                        ContactEmail = "history@fortsan.org",
                        SupplierNote = "荷蘭時代留下的城堡"
                    },
                    new TravelSupplier // ID = 318
                    {
                        SupplierName = "欣葉台菜餐廳信義店",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "黃郁婷",
                        ContactPhone = "02-8786-0123",
                        ContactEmail = "xinye@taiwanfood.com",
                        SupplierNote = "道地台菜料理，適合團體用餐"
                    },
                    new TravelSupplier // ID = 319
                    {
                        SupplierName = "阿宗麵線西門店",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "林志遠",
                        ContactPhone = "02-2388-8808",
                        ContactEmail = "ahchung@noodles.tw",
                        SupplierNote = "人氣排隊美食，小吃代表"
                    },
                    new TravelSupplier // ID = 320
                    {
                        SupplierName = "鼎泰豐信義本店",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "陳怡廷",
                        ContactPhone = "02-2321-8928",
                        ContactEmail = "reserv@diners.com.tw",
                        SupplierNote = "國際級小籠包專賣"
                    },
                    new TravelSupplier // ID = 321
                    {
                        SupplierName = "米香餐廳 - 大倉久和飯店",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "簡宏毅",
                        ContactPhone = "02-2523-1111",
                        ContactEmail = "mikang@okura.tw",
                        SupplierNote = "精緻台式套餐與宴會"
                    },
                    new TravelSupplier // ID = 322
                    {
                        SupplierName = "基隆廟口夜市美食導覽",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "吳佳穎",
                        ContactPhone = "02-2428-1111",
                        ContactEmail = "foodtour@keelungnight.tw",
                        SupplierNote = "夜市小吃巡禮活動"
                    },
                    new TravelSupplier // ID = 323
                    {
                        SupplierName = "新竹城隍廟貢丸米粉專門店",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "徐偉傑",
                        ContactPhone = "03-522-8888",
                        ContactEmail = "hsinchu@noodles.tw",
                        SupplierNote = "客家風味代表"
                    },
                    new TravelSupplier // ID = 324
                    {
                        SupplierName = "桃園南門市場牛肉麵",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "羅曉雯",
                        ContactPhone = "03-334-2233",
                        ContactEmail = "beefnoodle@taoyuan.tw",
                        SupplierNote = "市場裡的在地美味"
                    },
                    new TravelSupplier // ID = 325
                    {
                        SupplierName = "淡水老街阿給名店",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "江書婷",
                        ContactPhone = "02-2621-5555",
                        ContactEmail = "agei@tamsui.org",
                        SupplierNote = "老字號阿給與魚酥湯"
                    },
                    new TravelSupplier // ID = 326
                    {
                        SupplierName = "饒河夜市胡椒餅店",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "高志凱",
                        ContactPhone = "02-2768-8888",
                        ContactEmail = "pepperpie@nightmarket.tw",
                        SupplierNote = "炭烤胡椒餅人氣王"
                    },
                    new TravelSupplier // ID = 327
                    {
                        SupplierName = "北投溫泉路懷石料理",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "田中貞夫",
                        ContactPhone = "02-2896-7777",
                        ContactEmail = "kaiseki@beitou.tw",
                        SupplierNote = "結合日式與本地食材的精緻料理"
                    },
                    new TravelSupplier // ID = 328
                    {
                        SupplierName = "台北君悅酒店",
                        SupplierType = SupplierType.Accommodation,
                        ContactName = "鄭雅文",
                        ContactPhone = "02-2720-1234",
                        ContactEmail = "grandhyatt@taipei.com",
                        SupplierNote = "五星級飯店，信義區核心地段"
                    },
                    new TravelSupplier // ID = 329
                    {
                        SupplierName = "北投日勝生加賀屋",
                        SupplierType = SupplierType.Accommodation,
                        ContactName = "林書宏",
                        ContactPhone = "02-2891-1234",
                        ContactEmail = "onsen@kagaya.com.tw",
                        SupplierNote = "溫泉與和風服務聞名"
                    },
                    new TravelSupplier // ID = 330
                    {
                        SupplierName = "新竹福泰商務飯店",
                        SupplierType = SupplierType.Accommodation,
                        ContactName = "賴佳豪",
                        ContactPhone = "03-528-2222",
                        ContactEmail = "hhotel@forte.tw",
                        SupplierNote = "交通便利，適合自由行與商務"
                    },

                    // 中部 ID = 11
                    new TravelSupplier // ID = 331
                    {
                        SupplierName = "日月潭纜車景觀站",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "林志偉",
                        ContactPhone = "049-285-0666",
                        ContactEmail = "cablecar@sunmoonlake.tw",
                        SupplierNote = "連接九族文化村與日月潭的空中纜車"
                    },
                    new TravelSupplier // ID = 332
                    {
                        SupplierName = "九族文化村主題樂園",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "張佳宜",
                        ContactPhone = "049-289-5361",
                        ContactEmail = "info@formosan.com.tw",
                        SupplierNote = "結合原住民文化與遊樂設施"
                    },
                    new TravelSupplier // ID = 333
                    {
                        SupplierName = "溪頭自然教育園區",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "何正宇",
                        ContactPhone = "049-261-2111",
                        ContactEmail = "forest@xitou.gov.tw",
                        SupplierNote = "森林浴與大學池生態步道"
                    },
                    new TravelSupplier // ID = 334
                    {
                        SupplierName = "台中歌劇院導覽行程",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "陳思瑤",
                        ContactPhone = "04-2251-1777",
                        ContactEmail = "tour@npac-ntt.org",
                        SupplierNote = "建築奇觀與表演空間結合"
                    },
                    new TravelSupplier // ID = 335
                    {
                        SupplierName = "彩虹眷村藝術導覽",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "蔡榮義",
                        ContactPhone = "04-2380-2351",
                        ContactEmail = "rainbow@taichungtour.org",
                        SupplierNote = "退伍軍人打造的彩繪村落"
                    },
                    new TravelSupplier // ID = 336
                    {
                        SupplierName = "鹿港老街歷史巡禮",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "洪鈺婷",
                        ContactPhone = "04-777-1221",
                        ContactEmail = "tour@lugang.org",
                        SupplierNote = "古蹟、廟宇與台灣傳統糕餅名地"
                    },
                    new TravelSupplier // ID = 337
                    {
                        SupplierName = "南投猴探井天空之橋",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "許俊逸",
                        ContactPhone = "049-223-1111",
                        ContactEmail = "bridge@skywalk.org",
                        SupplierNote = "山谷間的懸空吊橋觀景點"
                    },
                    new TravelSupplier // ID = 338
                    {
                        SupplierName = "高美濕地生態導覽",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "戴怡君",
                        ContactPhone = "04-2656-1234",
                        ContactEmail = "wetland@gaomei.gov.tw",
                        SupplierNote = "風車夕陽與泥灘生態區"
                    },
                    new TravelSupplier // ID = 339
                    {
                        SupplierName = "草屯毓繡美術館",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "王美華",
                        ContactPhone = "049-256-5868",
                        ContactEmail = "museum@yuxiu.org.tw",
                        SupplierNote = "結合自然山林與藝術展覽空間"
                    },
                    new TravelSupplier // ID = 340
                    {
                        SupplierName = "苗栗大湖草莓文化館",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "劉哲安",
                        ContactPhone = "037-996-888",
                        ContactEmail = "berry@dahu.tw",
                        SupplierNote = "草莓季觀光與採果體驗"
                    },
                    new TravelSupplier // ID = 341
                    {
                        SupplierName = "泰安溫泉風景區",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "簡忠勝",
                        ContactPhone = "037-941-111",
                        ContactEmail = "hot@taian.gov.tw",
                        SupplierNote = "苗栗山區天然溫泉秘境"
                    },
                    new TravelSupplier // ID = 342
                    {
                        SupplierName = "集集綠色隧道腳踏車道",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "廖育誠",
                        ContactPhone = "049-276-5432",
                        ContactEmail = "bike@jiji.org",
                        SupplierNote = "綠蔭包圍的鐵道小鎮單車路線"
                    },
                    new TravelSupplier // ID = 343
                    {
                        SupplierName = "通霄海水浴場",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "蕭筱芸",
                        ContactPhone = "037-752-456",
                        ContactEmail = "beach@tongxiao.gov.tw",
                        SupplierNote = "中部少見的海灘戲水點"
                    },
                    new TravelSupplier // ID = 344
                    {
                        SupplierName = "彰化扇形車庫參訪行程",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "陳信宏",
                        ContactPhone = "04-762-4438",
                        ContactEmail = "train@fanroundhouse.org",
                        SupplierNote = "台灣唯一仍運作之蒸汽火車旋轉車庫"
                    },
                    new TravelSupplier // ID = 345
                    {
                        SupplierName = "八卦山大佛景區",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "黃芳儀",
                        ContactPhone = "04-728-7121",
                        ContactEmail = "temple@bagua.org",
                        SupplierNote = "結合佛教文化與觀景平台"
                    },
                    new TravelSupplier // ID = 346
                    {
                        SupplierName = "鯉魚潭風景區",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "廖文龍",
                        ContactPhone = "04-2223-9987",
                        ContactEmail = "lake@liyu.gov.tw",
                        SupplierNote = "賞湖、露營與划船活動"
                    },
                    new TravelSupplier // ID = 347
                    {
                        SupplierName = "新社花海季節限定行程",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "楊珮甄",
                        ContactPhone = "04-2582-9666",
                        ContactEmail = "flower@xinshe.org",
                        SupplierNote = "秋冬限定大面積花田活動"
                    },
                    new TravelSupplier // ID = 348
                    {
                        SupplierName = "霧峰林家花園導覽",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "林志彥",
                        ContactPhone = "04-2331-3456",
                        ContactEmail = "garden@wufeng.org",
                        SupplierNote = "閩式古宅與官宦世家的文化園區"
                    },
                    new TravelSupplier // ID = 349
                    {
                        SupplierName = "清境農場青青草原",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "張志勳",
                        ContactPhone = "049-280-2222",
                        ContactEmail = "farm@chingjing.gov.tw",
                        SupplierNote = "綿羊秀與高山牧場景觀"
                    },
                    new TravelSupplier // ID = 350
                    {
                        SupplierName = "紙教堂藝術展演中心",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "邱慧珍",
                        ContactPhone = "049-291-4922",
                        ContactEmail = "paper@pu-li.org",
                        SupplierNote = "災後重建象徵與國際合作設計"
                    },
                    new TravelSupplier // ID = 351
                    {
                        SupplierName = "屋馬燒肉園邸店",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "許婉婷",
                        ContactPhone = "04-2251-5066",
                        ContactEmail = "service@wumay.com",
                        SupplierNote = "中部知名連鎖燒肉店，適合團體用餐"
                    },
                    new TravelSupplier // ID = 352
                    {
                        SupplierName = "台中第二市場王記菜頭粿",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "劉榮德",
                        ContactPhone = "04-2225-3234",
                        ContactEmail = "wang@marketfood.tw",
                        SupplierNote = "台中老字號小吃名店"
                    },
                    new TravelSupplier // ID = 353
                    {
                        SupplierName = "清水休息站牛肉麵",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "張士翔",
                        ContactPhone = "04-2622-1147",
                        ContactEmail = "beef@restarea.com",
                        SupplierNote = "高速公路人氣料理代表"
                    },
                    new TravelSupplier // ID = 354
                    {
                        SupplierName = "南投魚池紅茶香餐坊",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "黃心瑜",
                        ContactPhone = "049-289-7755",
                        ContactEmail = "blacktea@fishpond.tw",
                        SupplierNote = "主打紅茶風味創意料理"
                    },
                    new TravelSupplier // ID = 355
                    {
                        SupplierName = "草屯蚵仔煎文化館",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "林昭慶",
                        ContactPhone = "049-230-4400",
                        ContactEmail = "oyster@caotun.org",
                        SupplierNote = "在地文化結合美食體驗"
                    },
                    new TravelSupplier // ID = 356
                    {
                        SupplierName = "彰化阿璋肉圓",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "洪錦雯",
                        ContactPhone = "04-722-9517",
                        ContactEmail = "meatball@changhua.tw",
                        SupplierNote = "排隊名店，百年老店風味"
                    },
                    new TravelSupplier // ID = 357
                    {
                        SupplierName = "集集老街杏仁茶專賣",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "周俊賢",
                        ContactPhone = "049-276-9000",
                        ContactEmail = "almond@jijistreet.org",
                        SupplierNote = "傳統熱飲與點心組合"
                    },
                    new TravelSupplier // ID = 358
                    {
                        SupplierName = "苗栗客家文化餐館",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "謝孟璇",
                        ContactPhone = "037-666-777",
                        ContactEmail = "hakkafood@miaoli.gov.tw",
                        SupplierNote = "擂茶、粄條等道地客家餐點"
                    },
                    new TravelSupplier // ID = 359
                    {
                        SupplierName = "台中逢甲夜市美食導覽",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "朱怡安",
                        ContactPhone = "04-2451-1234",
                        ContactEmail = "tour@fengjia.tw",
                        SupplierNote = "導遊式夜市美食探索行程"
                    },
                    new TravelSupplier // ID = 360
                    {
                        SupplierName = "新社香菇風味館",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "簡子齊",
                        ContactPhone = "04-2581-3456",
                        ContactEmail = "mushroom@xinshe.org",
                        SupplierNote = "香菇特色料理體驗餐廳"
                    },
                    new TravelSupplier // ID = 361
                    {
                        SupplierName = "日月潭涵碧樓酒店",
                        SupplierType = SupplierType.Accommodation,
                        ContactName = "詹文凱",
                        ContactPhone = "049-285-5311",
                        ContactEmail = "lalu@sunmoonlake.com",
                        SupplierNote = "湖景無邊泳池與精品設計住宿"
                    },
                    new TravelSupplier // ID = 362
                    {
                        SupplierName = "台中金典酒店",
                        SupplierType = SupplierType.Accommodation,
                        ContactName = "沈雅筑",
                        ContactPhone = "04-2324-6000",
                        ContactEmail = "splendor@taichunghotel.com",
                        SupplierNote = "中部指標性五星級飯店"
                    },
                    new TravelSupplier // ID = 363
                    {
                        SupplierName = "溪頭立德飯店",
                        SupplierType = SupplierType.Accommodation,
                        ContactName = "何健安",
                        ContactPhone = "049-261-2121",
                        ContactEmail = "hotel@xitou.org",
                        SupplierNote = "森林中寧靜住宿，鄰近溪頭教育園區"
                    },

                    // 南部
                    new TravelSupplier
                    {
                        SupplierName = "蓮池潭龍虎塔",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "林佑軒",
                        ContactPhone = "07-588-3241",
                        ContactEmail = "tower@lotuslake.org",
                        SupplierNote = "高雄地標建築，傳統宗教融合景觀"
                    }, // ID = 364
                    new TravelSupplier
                    {
                        SupplierName = "駁二藝術特區",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "周思瑤",
                        ContactPhone = "07-521-4899",
                        ContactEmail = "pier2@artpark.gov.tw",
                        SupplierNote = "文創市集與展覽空間"
                    }, // ID = 365
                    new TravelSupplier
                    {
                        SupplierName = "旗津燈塔與貝殼館",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "王柏翰",
                        ContactPhone = "07-571-2181",
                        ContactEmail = "light@qijin.org",
                        SupplierNote = "海岸生態與航海歷史展示"
                    }, // ID = 366
                    new TravelSupplier
                    {
                        SupplierName = "台南安平古堡",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "謝佩蓉",
                        ContactPhone = "06-226-7348",
                        ContactEmail = "fort@anping.org",
                        SupplierNote = "荷蘭時代歷史古蹟，鄰近安平老街"
                    }, // ID = 367
                    new TravelSupplier
                    {
                        SupplierName = "赤崁樓文化導覽",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "劉子晴",
                        ContactPhone = "06-228-9015",
                        ContactEmail = "tour@chihkan.org",
                        SupplierNote = "台南古城的精神地標"
                    }, // ID = 368
                    new TravelSupplier
                    {
                        SupplierName = "關子嶺溫泉區",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "宋信良",
                        ContactPhone = "06-682-3481",
                        ContactEmail = "hotspring@guanziling.org",
                        SupplierNote = "泥漿溫泉特色體驗"
                    }, // ID = 369
                    new TravelSupplier
                    {
                        SupplierName = "台南奇美博物館",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "黃鈺涵",
                        ContactPhone = "06-266-0808",
                        ContactEmail = "museum@chimei.org.tw",
                        SupplierNote = "融合古典藝術與自然展示"
                    }, // ID = 370
                    new TravelSupplier
                    {
                        SupplierName = "高雄壽山猴子山步道",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "邱宏文",
                        ContactPhone = "07-313-6781",
                        ContactEmail = "hiking@shoushan.tw",
                        SupplierNote = "市區內的登山與野生獼猴觀察點"
                    }, // ID = 371
                    new TravelSupplier
                    {
                        SupplierName = "六堆客家文化園區",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "賴冠霖",
                        ContactPhone = "08-789-8800",
                        ContactEmail = "hakka@liudui.org",
                        SupplierNote = "推廣客家文化的教育空間"
                    }, // ID = 372
                    new TravelSupplier
                    {
                        SupplierName = "國立海洋生物博物館",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "簡嘉慧",
                        ContactPhone = "08-882-5001",
                        ContactEmail = "nmmba@ocean.org.tw",
                        SupplierNote = "亞洲知名大型水族館"
                    }, // ID = 373
                    new TravelSupplier
                    {
                        SupplierName = "墾丁國家公園",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "張立文",
                        ContactPhone = "08-886-1321",
                        ContactEmail = "info@ktnp.gov.tw",
                        SupplierNote = "台灣最南端的自然保育景區"
                    }, // ID = 374
                    new TravelSupplier
                    {
                        SupplierName = "鵝鑾鼻燈塔",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "謝彥廷",
                        ContactPhone = "08-885-1234",
                        ContactEmail = "lighthouse@eluanbi.org",
                        SupplierNote = "太平洋與台灣海峽交會地標"
                    }, // ID = 375
                    new TravelSupplier
                    {
                        SupplierName = "阿里山森林鐵路體驗",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "陳玟妤",
                        ContactPhone = "05-267-9911",
                        ContactEmail = "rail@alishan.gov.tw",
                        SupplierNote = "山中鐵道與日出雲海奇觀"
                    }, // ID = 376
                    new TravelSupplier
                    {
                        SupplierName = "奮起湖老街便當體驗",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "鄭育嘉",
                        ContactPhone = "05-256-8211",
                        ContactEmail = "bento@fenqihu.org",
                        SupplierNote = "百年山城鐵道與便當文化"
                    }, // ID = 377
                    new TravelSupplier
                    {
                        SupplierName = "梅山太平雲梯",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "李政霖",
                        ContactPhone = "05-262-3721",
                        ContactEmail = "bridge@meishan.org",
                        SupplierNote = "高山吊橋與雲霧森林步道"
                    }, // ID = 378
                    new TravelSupplier
                    {
                        SupplierName = "嘉義文化路夜市",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "潘怡君",
                        ContactPhone = "05-227-5678",
                        ContactEmail = "market@chiayi.gov.tw",
                        SupplierNote = "南部經典夜市場景"
                    }, // ID = 379
                    new TravelSupplier
                    {
                        SupplierName = "高雄愛河遊船",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "林毓珊",
                        ContactPhone = "07-251-2900",
                        ContactEmail = "boat@loveriver.org",
                        SupplierNote = "夜景觀光遊船體驗"
                    }, // ID = 380
                    new TravelSupplier
                    {
                        SupplierName = "屏東大鵬灣風景區",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "杜信豪",
                        ContactPhone = "08-832-1818",
                        ContactEmail = "lagoon@dapeng.org",
                        SupplierNote = "濱海潟湖與自行車道"
                    }, // ID = 381
                    new TravelSupplier
                    {
                        SupplierName = "左營孔廟文化園區",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "曾宥廷",
                        ContactPhone = "07-583-7689",
                        ContactEmail = "confucius@zuoying.org",
                        SupplierNote = "傳統儒家建築與祭孔活動"
                    }, // ID = 382
                    new TravelSupplier
                    {
                        SupplierName = "大東文化藝術中心",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "王雅婷",
                        ContactPhone = "07-743-0011",
                        ContactEmail = "art@dadong.org",
                        SupplierNote = "表演、展覽、藝文空間綜合場域"
                    }, // ID = 383
                    new TravelSupplier
                    {
                        SupplierName = "台南度小月擔仔麵",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "黃聖凱",
                        ContactPhone = "06-223-1744",
                        ContactEmail = "danzi@tainanfood.com",
                        SupplierNote = "台南經典百年小吃"
                    }, // ID = 384
                    new TravelSupplier
                    {
                        SupplierName = "高雄六合夜市美食導覽",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "林佩芸",
                        ContactPhone = "07-285-1234",
                        ContactEmail = "tour@liuhex.org",
                        SupplierNote = "人氣夜市美食探索路線"
                    }, // ID = 385
                    new TravelSupplier
                    {
                        SupplierName = "屏東萬巒豬腳街",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "陳品宏",
                        ContactPhone = "08-781-0022",
                        ContactEmail = "pork@wanluan.org",
                        SupplierNote = "南部經典肉食饗宴"
                    }, // ID = 386
                    new TravelSupplier
                    {
                        SupplierName = "嘉義火雞肉飯老店",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "張家綺",
                        ContactPhone = "05-228-9876",
                        ContactEmail = "turkey@chiayifood.org",
                        SupplierNote = "當地代表性便餐"
                    }, // ID = 387
                    new TravelSupplier
                    {
                        SupplierName = "高雄鹽埕區鴨肉珍",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "朱怡璇",
                        ContactPhone = "07-531-3456",
                        ContactEmail = "duck@kaohsiungfood.com",
                        SupplierNote = "傳統鴨肉飯美食代表"
                    }, // ID = 388
                    new TravelSupplier
                    {
                        SupplierName = "阿霞飯店",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "李文政",
                        ContactPhone = "06-225-6789",
                        ContactEmail = "banquet@axafood.tw",
                        SupplierNote = "台南辦桌老字號名店"
                    }, // ID = 389
                    new TravelSupplier
                    {
                        SupplierName = "東港黑鮪魚料理店",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "楊宗憲",
                        ContactPhone = "08-832-2233",
                        ContactEmail = "tuna@donggang.org",
                        SupplierNote = "黑鮪魚季限定高級食材"
                    }, // ID = 390
                    new TravelSupplier
                    {
                        SupplierName = "旗山老街香蕉冰店",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "蔡宜芳",
                        ContactPhone = "07-661-8899",
                        ContactEmail = "banana@qishan.org",
                        SupplierNote = "以香蕉冰與甜點著稱的復古風味"
                    }, // ID = 391
                    new TravelSupplier
                    {
                        SupplierName = "麻豆碗粿王",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "周宏志",
                        ContactPhone = "06-571-1144",
                        ContactEmail = "wanguo@madou.org",
                        SupplierNote = "米食文化特色小吃"
                    }, // ID = 392
                    new TravelSupplier
                    {
                        SupplierName = "北門鹽鄉海產店",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "溫士傑",
                        ContactPhone = "06-786-4412",
                        ContactEmail = "seafood@beimen.org",
                        SupplierNote = "靠近鹽田與濕地的海味料理"
                    }, // ID = 393
                    new TravelSupplier
                    {
                        SupplierName = "高雄漢來大飯店",
                        SupplierType = SupplierType.Accommodation,
                        ContactName = "陳怡君",
                        ContactPhone = "07-216-1766",
                        ContactEmail = "grand@hanlaihotel.com",
                        SupplierNote = "市中心五星級飯店，鄰近港口"
                    }, // ID = 394
                    new TravelSupplier
                    {
                        SupplierName = "台南晶英酒店",
                        SupplierType = SupplierType.Accommodation,
                        ContactName = "李宥儀",
                        ContactPhone = "06-213-6290",
                        ContactEmail = "silks@tainanhotel.tw",
                        SupplierNote = "新穎時尚設計結合府城文化"
                    }, // ID = 395
                    new TravelSupplier
                    {
                        SupplierName = "墾丁悠活渡假村",
                        SupplierType = SupplierType.Accommodation,
                        ContactName = "吳宗勳",
                        ContactPhone = "08-886-2345",
                        ContactEmail = "resort@yoho.com.tw",
                        SupplierNote = "家庭式南國風渡假村，擁海灘景觀"
                    }, // ID = 396

                    // 東部
                    new TravelSupplier
                    {
                        SupplierName = "太魯閣國家公園",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "黃于潔",
                        ContactPhone = "03-862-1100",
                        ContactEmail = "info@taroko.gov.tw",
                        SupplierNote = "花蓮最著名峽谷景點，長春祠與燕子口必訪"
                    }, // ID = 397
                    new TravelSupplier
                    {
                        SupplierName = "清水斷崖觀景平台",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "陳信宏",
                        ContactPhone = "03-823-8860",
                        ContactEmail = "cliff@chingshui.org",
                        SupplierNote = "東海岸最壯觀的斷崖美景"
                    }, // ID = 398
                    new TravelSupplier
                    {
                        SupplierName = "七星潭自行車道",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "林子涵",
                        ContactPhone = "03-822-7777",
                        ContactEmail = "bike@qixingtan.org",
                        SupplierNote = "海濱沿線的休閒腳踏車路線"
                    }, // ID = 399
                    new TravelSupplier
                    {
                        SupplierName = "花蓮松園別館",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "周家豪",
                        ContactPhone = "03-835-6510",
                        ContactEmail = "pine@hualien.org",
                        SupplierNote = "日治時代歷史建築與藝文空間"
                    }, // ID = 400
                    new TravelSupplier
                    {
                        SupplierName = "慕谷慕魚生態園區",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "賴柏霖",
                        ContactPhone = "03-875-1123",
                        ContactEmail = "eco@muge.org",
                        SupplierNote = "預約制的秘境溪谷探險"
                    }, // ID = 401
                    new TravelSupplier
                    {
                        SupplierName = "瑞穗牧場",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "鍾鈺珊",
                        ContactPhone = "03-887-6611",
                        ContactEmail = "farm@ruisui.org",
                        SupplierNote = "牧場導覽與現擠牛奶冰淇淋體驗"
                    }, // ID = 402
                    new TravelSupplier
                    {
                        SupplierName = "赤柯山金針花海",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "謝宏軒",
                        ContactPhone = "03-888-2211",
                        ContactEmail = "flower@chike.org",
                        SupplierNote = "季節限定賞花景點"
                    }, // ID = 403
                    new TravelSupplier
                    {
                        SupplierName = "池上大坡池環湖步道",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "劉彥廷",
                        ContactPhone = "089-861-211",
                        ContactEmail = "lake@chishang.org",
                        SupplierNote = "結合自然與文學氣息的靜謐湖畔"
                    }, // ID = 404
                    new TravelSupplier
                    {
                        SupplierName = "關山親水公園",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "郭品妤",
                        ContactPhone = "089-810-800",
                        ContactEmail = "park@guanshan.org",
                        SupplierNote = "適合親子與露營的河畔園區"
                    }, // ID = 405
                    new TravelSupplier
                    {
                        SupplierName = "鹿野高台飛行傘基地",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "宋明哲",
                        ContactPhone = "089-552-233",
                        ContactEmail = "fly@luye.org",
                        SupplierNote = "知名熱氣球嘉年華舉辦地"
                    }, // ID = 406
                    new TravelSupplier
                    {
                        SupplierName = "初鹿牧場",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "賴芸萱",
                        ContactPhone = "089-571-002",
                        ContactEmail = "farm@chulu.org",
                        SupplierNote = "自然體驗與乳品DIY活動"
                    }, // ID = 407
                    new TravelSupplier
                    {
                        SupplierName = "三仙台海岸地形",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "吳俊霖",
                        ContactPhone = "089-853-678",
                        ContactEmail = "bridge@sansiantai.org",
                        SupplierNote = "八拱跨海橋與潮間帶生態"
                    }, // ID = 408
                    new TravelSupplier
                    {
                        SupplierName = "都歷遊客中心與天空之鏡",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "徐珮甄",
                        ContactPhone = "089-841-221",
                        ContactEmail = "mirror@dulih.org",
                        SupplierNote = "反射景象吸引攝影愛好者"
                    }, // ID = 409
                    new TravelSupplier
                    {
                        SupplierName = "東河包子文化館",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "陳俊豪",
                        ContactPhone = "089-896-123",
                        ContactEmail = "baozi@donghe.org",
                        SupplierNote = "結合美食與歷史的體驗館"
                    }, // ID = 410
                    new TravelSupplier
                    {
                        SupplierName = "花東縱谷鐵道小旅行",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "黃雅婷",
                        ContactPhone = "03-865-7000",
                        ContactEmail = "rail@hualien-taitung.org",
                        SupplierNote = "搭火車穿越花東田野風光"
                    }, // ID = 411
                    new TravelSupplier
                    {
                        SupplierName = "石梯坪地質景觀",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "江宥瑄",
                        ContactPhone = "03-878-1234",
                        ContactEmail = "rock@shiti.org",
                        SupplierNote = "海蝕平台與奇岩地形"
                    }, // ID = 412
                    new TravelSupplier
                    {
                        SupplierName = "北迴歸線標誌公園",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "洪聖偉",
                        ContactPhone = "089-821-303",
                        ContactEmail = "tropic@beihui.org",
                        SupplierNote = "台灣唯一熱帶與亞熱帶交界觀測點"
                    }, // ID = 413
                    new TravelSupplier
                    {
                        SupplierName = "豐濱天空步道",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "劉思婕",
                        ContactPhone = "03-878-9987",
                        ContactEmail = "skywalk@fengbin.org",
                        SupplierNote = "玻璃步道懸於海岸岩壁上"
                    }, // ID = 414
                    new TravelSupplier
                    {
                        SupplierName = "新社梯田文化園區",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "張宥齊",
                        ContactPhone = "089-551-678",
                        ContactEmail = "terrace@xinshih.org",
                        SupplierNote = "在地部落導覽與梯田景觀"
                    }, // ID = 415
                    new TravelSupplier
                    {
                        SupplierName = "知本溫泉風景區",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "鄭宥婷",
                        ContactPhone = "089-515-888",
                        ContactEmail = "hotspring@zhiben.org",
                        SupplierNote = "以溫泉與療癒旅遊聞名"
                    }, // ID = 416
                    new TravelSupplier
                    {
                        SupplierName = "花蓮炸彈蔥油餅總店",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "張文彬",
                        ContactPhone = "03-834-3436",
                        ContactEmail = "onion@hualienfood.org",
                        SupplierNote = "排隊名店，限量供應酥脆蔥餅"
                    }, // ID = 417
                    new TravelSupplier
                    {
                        SupplierName = "台東卑南包子店",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "蘇佳玲",
                        ContactPhone = "089-322-178",
                        ContactEmail = "baozi@beinan.org",
                        SupplierNote = "傳承三代的台東特色點心"
                    }, // ID = 418
                    new TravelSupplier
                    {
                        SupplierName = "公正街包子與蒸餃",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "黃振宏",
                        ContactPhone = "03-835-0343",
                        ContactEmail = "dumpling@gongzheng.org",
                        SupplierNote = "老字號早點名店"
                    }, // ID = 419
                    new TravelSupplier
                    {
                        SupplierName = "池上便當文化館食堂",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "謝孟庭",
                        ContactPhone = "089-862-588",
                        ContactEmail = "bento@chishang.org",
                        SupplierNote = "知名便當品牌現做體驗"
                    }, // ID = 420
                    new TravelSupplier
                    {
                        SupplierName = "知本溫泉甕缸雞料理",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "陳沛文",
                        ContactPhone = "089-515-202",
                        ContactEmail = "chicken@zhiben.org",
                        SupplierNote = "在地山產風味結合特色料理"
                    }, // ID = 421
                    new TravelSupplier
                    {
                        SupplierName = "富里米食風味館",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "張家誠",
                        ContactPhone = "03-882-5566",
                        ContactEmail = "rice@fuli.org",
                        SupplierNote = "強調台灣米的飲食文化"
                    }, // ID = 422
                    new TravelSupplier
                    {
                        SupplierName = "原住民風味餐體驗館",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "賴宜庭",
                        ContactPhone = "089-332-009",
                        ContactEmail = "tribe@hualien-taitung.org",
                        SupplierNote = "提供阿美族與卑南族傳統佳餚"
                    }, // ID = 423
                    new TravelSupplier
                    {
                        SupplierName = "光復糖廠冰品部",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "王承鈞",
                        ContactPhone = "03-870-4125",
                        ContactEmail = "icecream@guangfu.org",
                        SupplierNote = "日式風格糖廠與冰淇淋販售"
                    }, // ID = 424
                    new TravelSupplier
                    {
                        SupplierName = "台東大武海鮮料理店",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "簡筱芸",
                        ContactPhone = "089-741-202",
                        ContactEmail = "seafood@dawu.org",
                        SupplierNote = "當地漁港海鮮即時料理"
                    }, // ID = 425
                    new TravelSupplier
                    {
                        SupplierName = "玉里麵老店",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "楊婉婷",
                        ContactPhone = "03-888-6688",
                        ContactEmail = "noodle@yuli.org",
                        SupplierNote = "以手工製麵與豬油拌麵聞名"
                    }, // ID = 426
                    new TravelSupplier
                    {
                        SupplierName = "花蓮遠雄悅來大飯店",
                        SupplierType = SupplierType.Accommodation,
                        ContactName = "梁雅筑",
                        ContactPhone = "03-812-3999",
                        ContactEmail = "resort@farglory.com.tw",
                        SupplierNote = "鄰近海洋公園的山海景觀渡假飯店"
                    }, // ID = 427
                    new TravelSupplier
                    {
                        SupplierName = "台東娜路彎大酒店",
                        SupplierType = SupplierType.Accommodation,
                        ContactName = "游宗霖",
                        ContactPhone = "089-239-666",
                        ContactEmail = "hotel@naruwan.com.tw",
                        SupplierNote = "結合原民風與現代設計的高級飯店"
                    }, // ID = 428
                    new TravelSupplier
                    {
                        SupplierName = "鹿鳴溫泉酒店",
                        SupplierType = SupplierType.Accommodation,
                        ContactName = "林宥廷",
                        ContactPhone = "089-550-888",
                        ContactEmail = "hotspa@luminghotel.tw",
                        SupplierNote = "提供全套溫泉設施與山景客房"
                    }, // ID = 429

                    // 離島
                    new TravelSupplier
                    {
                        SupplierName = "澎湖雙心石滬",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "蔡育賢",
                        ContactPhone = "06-926-3322",
                        ContactEmail = "stone@penghu.org",
                        SupplierNote = "澎湖代表性潮間帶石滬景觀"
                    }, // ID = 430
                    new TravelSupplier
                    {
                        SupplierName = "金門莒光樓",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "黃柏瑞",
                        ContactPhone = "082-325-115",
                        ContactEmail = "tower@kinmen.gov.tw",
                        SupplierNote = "金門歷史象徵建築"
                    }, // ID = 431
                    new TravelSupplier
                    {
                        SupplierName = "馬祖北竿芹壁聚落",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "陳宥彤",
                        ContactPhone = "0836-56541",
                        ContactEmail = "village@beigan.org",
                        SupplierNote = "保存良好的閩東式石頭厝村落"
                    }, // ID = 432
                    new TravelSupplier
                    {
                        SupplierName = "小琉球美人洞風景區",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "李信賢",
                        ContactPhone = "08-861-2141",
                        ContactEmail = "beauty@liuqiu.org",
                        SupplierNote = "珊瑚礁岩洞與觀海步道"
                    }, // ID = 433
                    new TravelSupplier
                    {
                        SupplierName = "澎湖跨海大橋",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "鄭安婷",
                        ContactPhone = "06-927-1122",
                        ContactEmail = "bridge@penghu.org",
                        SupplierNote = "台灣第一條跨海長橋"
                    }, // ID = 434
                    new TravelSupplier
                    {
                        SupplierName = "金門翟山坑道",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "林柏翰",
                        ContactPhone = "082-313-241",
                        ContactEmail = "tunnel@kinmen.gov.tw",
                        SupplierNote = "冷戰時期戰備工事"
                    }, // ID = 435
                    new TravelSupplier
                    {
                        SupplierName = "馬祖大坵島梅花鹿保育區",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "張郁婷",
                        ContactPhone = "0836-75341",
                        ContactEmail = "deer@daciou.org",
                        SupplierNote = "可近距離觀察野生梅花鹿"
                    }, // ID = 436
                    new TravelSupplier
                    {
                        SupplierName = "小琉球潮間帶導覽",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "曾彥豪",
                        ContactPhone = "08-861-0033",
                        ContactEmail = "ecotour@liuqiu.org",
                        SupplierNote = "結合保育與親子教育的潮間帶探索"
                    }, // ID = 437
                    new TravelSupplier
                    {
                        SupplierName = "澎湖風櫃洞",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "黃怡婷",
                        ContactPhone = "06-927-3311",
                        ContactEmail = "cave@fenggui.org",
                        SupplierNote = "奇岩海蝕洞與噴氣聲奇觀"
                    }, // ID = 438
                    new TravelSupplier
                    {
                        SupplierName = "金門古崗樓文化園區",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "陳振宇",
                        ContactPhone = "082-321-551",
                        ContactEmail = "watchtower@gugang.org",
                        SupplierNote = "中西融合式防衛建築"
                    }, // ID = 439
                    new TravelSupplier
                    {
                        SupplierName = "馬祖東莒燈塔",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "劉佩璇",
                        ContactPhone = "0836-11445",
                        ContactEmail = "lighthouse@dongju.org",
                        SupplierNote = "英式建築風格的古燈塔"
                    }, // ID = 440
                    new TravelSupplier
                    {
                        SupplierName = "澎湖南寮古厝文化村",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "邱若涵",
                        ContactPhone = "06-921-4432",
                        ContactEmail = "culture@naliao.org",
                        SupplierNote = "傳統聚落與文創結合"
                    }, // ID = 441
                    new TravelSupplier
                    {
                        SupplierName = "金門民俗文化村",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "高冠宇",
                        ContactPhone = "082-334-721",
                        ContactEmail = "folk@kinmen.org",
                        SupplierNote = "閩南聚落完整保存地"
                    }, // ID = 442
                    new TravelSupplier
                    {
                        SupplierName = "馬祖媽祖巨神像",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "吳俊賢",
                        ContactPhone = "0836-88675",
                        ContactEmail = "matsu@beigan.org",
                        SupplierNote = "世界最高媽祖石像"
                    }, // ID = 443
                    new TravelSupplier
                    {
                        SupplierName = "小琉球花瓶岩",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "曾品瑄",
                        ContactPhone = "08-861-1123",
                        ContactEmail = "rock@liuqiu.org",
                        SupplierNote = "地標性珊瑚礁地形"
                    }, // ID = 444
                    new TravelSupplier
                    {
                        SupplierName = "澎湖雞善嶼潮間帶探訪",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "詹佑昇",
                        ContactPhone = "06-928-5533",
                        ContactEmail = "eco@penghutour.org",
                        SupplierNote = "保育與教育兼具的無人島導覽"
                    }, // ID = 445
                    new TravelSupplier
                    {
                        SupplierName = "金門模範街歷史街區",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "李柏瑞",
                        ContactPhone = "082-316-122",
                        ContactEmail = "street@kinmen.gov.tw",
                        SupplierNote = "近代閩南建築群"
                    }, // ID = 446
                    new TravelSupplier
                    {
                        SupplierName = "馬祖津沙聚落星空導覽",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "范子寧",
                        ContactPhone = "0836-77741",
                        ContactEmail = "stars@jinsha.org",
                        SupplierNote = "結合在地人文與天文教育"
                    }, // ID = 447
                    new TravelSupplier
                    {
                        SupplierName = "小琉球玻璃船觀賞海龜",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "林宏哲",
                        ContactPhone = "08-861-7788",
                        ContactEmail = "glass@liuqiu.org",
                        SupplierNote = "搭船近距離觀察珊瑚與海龜"
                    }, // ID = 448
                    new TravelSupplier
                    {
                        SupplierName = "澎湖鯨魚洞自然景觀區",
                        SupplierType = SupplierType.Attraction,
                        ContactName = "陳珮甄",
                        ContactPhone = "06-927-4567",
                        ContactEmail = "whale@penghu.org",
                        SupplierNote = "天然石洞形似鯨魚而得名"
                    }, // ID = 449
                    new TravelSupplier
                    {
                        SupplierName = "澎湖阿華海產",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "黃郁婷",
                        ContactPhone = "06-926-1122",
                        ContactEmail = "seafood@penghu.org",
                        SupplierNote = "當地現撈海鮮名店"
                    }, // ID = 450
                    new TravelSupplier
                    {
                        SupplierName = "金門金城牛肉麵",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "李政勳",
                        ContactPhone = "082-325-678",
                        ContactEmail = "beefnoodle@kinmen.org",
                        SupplierNote = "在地人氣中式麵店"
                    }, // ID = 451
                    new TravelSupplier
                    {
                        SupplierName = "馬祖老酒麵線之家",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "張筱芸",
                        ContactPhone = "0836-882-223",
                        ContactEmail = "noodle@matsu.org",
                        SupplierNote = "馬祖特色料理：紅糟老酒料理"
                    }, // ID = 452
                    new TravelSupplier
                    {
                        SupplierName = "小琉球海龜燒烤BBQ",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "陳冠宇",
                        ContactPhone = "08-861-4455",
                        ContactEmail = "bbq@liuqiu.org",
                        SupplierNote = "沙灘烤肉與海景第一排"
                    }, // ID = 453
                    new TravelSupplier
                    {
                        SupplierName = "金門高粱醉雞料理坊",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "吳秉翔",
                        ContactPhone = "082-322-019",
                        ContactEmail = "gaoliang@kinmen.org",
                        SupplierNote = "特色高粱入菜料理體驗"
                    }, // ID = 454
                    new TravelSupplier
                    {
                        SupplierName = "馬祖紅糟燒肉館",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "謝宜蓁",
                        ContactPhone = "0836-741-741",
                        ContactEmail = "hongzao@beigan.org",
                        SupplierNote = "傳統紅糟醃漬與烤肉料理"
                    }, // ID = 455
                    new TravelSupplier
                    {
                        SupplierName = "澎湖仙人掌冰店",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "郭芷妤",
                        ContactPhone = "06-926-9876",
                        ContactEmail = "ice@penghu.org",
                        SupplierNote = "澎湖特產冰品"
                    }, // ID = 456
                    new TravelSupplier
                    {
                        SupplierName = "小琉球港口海鮮粥",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "賴昀潔",
                        ContactPhone = "08-861-2341",
                        ContactEmail = "porridge@liuqiu.org",
                        SupplierNote = "當地新鮮漁獲熬煮海味粥品"
                    }, // ID = 457
                    new TravelSupplier
                    {
                        SupplierName = "金門蚵仔煎老街店",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "林宗賢",
                        ContactPhone = "082-311-449",
                        ContactEmail = "oyster@kinmen.org",
                        SupplierNote = "鄰近水頭老街的地道小吃"
                    }, // ID = 458
                    new TravelSupplier
                    {
                        SupplierName = "馬祖淡菜風味餐廳",
                        SupplierType = SupplierType.Restaurant,
                        ContactName = "王婉蓉",
                        ContactPhone = "0836-886-541",
                        ContactEmail = "mussels@dongyin.org",
                        SupplierNote = "馬祖淡菜、貽貝特色餐廳"
                    }, // ID = 459
                    new TravelSupplier
                    {
                        SupplierName = "澎湖福朋喜來登飯店",
                        SupplierType = SupplierType.Accommodation,
                        ContactName = "林昱廷",
                        ContactPhone = "06-926-6288",
                        ContactEmail = "hotel@penghusheraton.com",
                        SupplierNote = "國際連鎖品牌，鄰近馬公市區"
                    }, // ID = 460
                    new TravelSupplier
                    {
                        SupplierName = "金門昇恆昌金湖飯店",
                        SupplierType = SupplierType.Accommodation,
                        ContactName = "黃詩涵",
                        ContactPhone = "082-333-999",
                        ContactEmail = "grand@everrich.com.tw",
                        SupplierNote = "金門地區高端住宿首選"
                    }, // ID = 461
                    new TravelSupplier
                    {
                        SupplierName = "小琉球海景民宿聯盟",
                        SupplierType = SupplierType.Accommodation,
                        ContactName = "詹怡婷",
                        ContactPhone = "08-861-5566",
                        ContactEmail = "bnb@liuqiu.org",
                        SupplierNote = "整合多家合法民宿的服務平台"
                    } // ID = 462
                );
            }

        }

        private async Task SeedOfficialAccommodationAsync()
        {
            if (!_context.OfficialAccommodations.Any())
            {
                _context.OfficialAccommodations.AddRange(
                    new OfficialAccommodation
                    {
                        TravelSupplierId = 31,
                        RegionId = 1,
                        Name = "札幌王子大飯店",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAccommodation
                    {
                        TravelSupplierId = 32,
                        RegionId = 1,
                        Name = "定山溪溫泉旅館",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAccommodation
                    {
                        TravelSupplierId = 33,
                        RegionId = 1,
                        Name = "旭川車站前商務旅館",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAccommodation
                    {
                        TravelSupplierId = 64,
                        RegionId = 2,
                        Name = "仙台東橫INN",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAccommodation
                    {
                        TravelSupplierId = 65,
                        RegionId = 2,
                        Name = "銀山溫泉藤屋",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAccommodation
                    {
                        TravelSupplierId = 66,
                        RegionId = 2,
                        Name = "會津若松大飯店",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAccommodation
                    {
                        TravelSupplierId = 97,
                        RegionId = 3,
                        Name = "新宿王子飯店",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAccommodation
                    {
                        TravelSupplierId = 98,
                        RegionId = 3,
                        Name = "橫濱灣大飯店",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAccommodation
                    {
                        TravelSupplierId = 99,
                        RegionId = 3,
                        Name = "日光金谷飯店",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAccommodation
                    {
                        TravelSupplierId = 130,
                        RegionId = 4,
                        Name = "金澤日航飯店",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAccommodation
                    {
                        TravelSupplierId = 131,
                        RegionId = 4,
                        Name = "飛驒高山溫泉旅館",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAccommodation
                    {
                        TravelSupplierId = 132,
                        RegionId = 4,
                        Name = "富士山本宮富士館",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAccommodation
                    {
                        TravelSupplierId = 163,
                        RegionId = 5,
                        Name = "京都車站日航飯店",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAccommodation
                    {
                        TravelSupplierId = 164,
                        RegionId = 5,
                        Name = "大阪心齋橋大和ROYNET飯店",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAccommodation
                    {
                        TravelSupplierId = 165,
                        RegionId = 5,
                        Name = "神戶北野觀光飯店",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAccommodation
                    {
                        TravelSupplierId = 196,
                        RegionId = 6,
                        Name = "廣島ANA洲際飯店",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAccommodation
                    {
                        TravelSupplierId = 197,
                        RegionId = 6,
                        Name = "松江城見城景旅館",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAccommodation
                    {
                        TravelSupplierId = 198,
                        RegionId = 6,
                        Name = "鳥取沙丘之湯溫泉旅館",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAccommodation
                    {
                        TravelSupplierId = 229,
                        RegionId = 7,
                        Name = "高松車站前商務飯店",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAccommodation
                    {
                        TravelSupplierId = 230,
                        RegionId = 7,
                        Name = "道後溫泉大和旅館",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAccommodation
                    {
                        TravelSupplierId = 231,
                        RegionId = 7,
                        Name = "高知城下溫泉飯店",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAccommodation
                    {
                        TravelSupplierId = 262,
                        RegionId = 8,
                        Name = "福岡天神東橫INN",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAccommodation
                    {
                        TravelSupplierId = 263,
                        RegionId = 8,
                        Name = "由布院溫泉和風旅館",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAccommodation
                    {
                        TravelSupplierId = 264,
                        RegionId = 8,
                        Name = "鹿兒島城山觀光飯店",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAccommodation
                    {
                        TravelSupplierId = 295,
                        RegionId = 9,
                        Name = "那霸國際通飯店",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAccommodation
                    {
                        TravelSupplierId = 296,
                        RegionId = 9,
                        Name = "沖繩北部海濱度假村",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAccommodation
                    {
                        TravelSupplierId = 297,
                        RegionId = 9,
                        Name = "石垣島海景飯店",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    }
                );
                await _context.SaveChangesAsync();
                _context.OfficialAccommodations.AddRange(
                    // 北部 ID = 10
                    new OfficialAccommodation
                    {
                        TravelSupplierId = 328,
                        RegionId = 10,
                        Name = "台北君悅酒店",
                        Description = "五星級飯店，位於信義區核心地段，交通便利、設施齊全，適合商務與觀光。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAccommodation
                    {
                        TravelSupplierId = 329,
                        RegionId = 10,
                        Name = "北投日勝生加賀屋",
                        Description = "融合日式溫泉文化與頂級服務，提供獨特的和風住宿體驗。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAccommodation
                    {
                        TravelSupplierId = 330,
                        RegionId = 10,
                        Name = "新竹福泰商務飯店",
                        Description = "位於市中心，交通便利，提供舒適商務住宿空間與便捷服務。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },

                    // 中部 ID = 11
                    new OfficialAccommodation
                    {
                        TravelSupplierId = 361,
                        RegionId = 11,
                        Name = "日月潭涵碧樓酒店",
                        Description = "擁有無邊泳池與湖景房，結合精品設計與高端度假體驗。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAccommodation
                    {
                        TravelSupplierId = 362,
                        RegionId = 11,
                        Name = "台中金典酒店",
                        Description = "中部指標性五星級飯店，結合舒適與專業的住宿環境。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAccommodation
                    {
                        TravelSupplierId = 363,
                        RegionId = 11,
                        Name = "溪頭立德飯店",
                        Description = "座落於森林之中，鄰近溪頭自然教育園區，環境靜謐舒適。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },

                    // 南部 ID = 12
                    new OfficialAccommodation
                    {
                        TravelSupplierId = 394,
                        RegionId = 12,
                        Name = "高雄漢來大飯店",
                        Description = "位於高雄市中心，鄰近港口與百貨，為五星級指標性飯店。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAccommodation
                    {
                        TravelSupplierId = 395,
                        RegionId = 12,
                        Name = "台南晶英酒店",
                        Description = "融合現代時尚與台南歷史文化，提供高品質住宿體驗。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAccommodation
                    {
                        TravelSupplierId = 396,
                        RegionId = 12,
                        Name = "墾丁悠活渡假村",
                        Description = "南國風情渡假村，擁有沙灘美景，適合親子與家庭旅遊。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },

                    // 東部 ID = 13
                    new OfficialAccommodation
                    {
                        TravelSupplierId = 427,
                        RegionId = 13,
                        Name = "花蓮遠雄悅來大飯店",
                        Description = "坐擁山海景觀，鄰近海洋公園，是渡假與親子旅遊首選。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAccommodation
                    {
                        TravelSupplierId = 428,
                        RegionId = 13,
                        Name = "台東娜路彎大酒店",
                        Description = "結合原住民元素與現代設計，提供舒適且具特色的住宿。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAccommodation
                    {
                        TravelSupplierId = 429,
                        RegionId = 13,
                        Name = "鹿鳴溫泉酒店",
                        Description = "提供豐富溫泉設施與山景客房，適合放鬆與養生。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },

                    // 離島 ID = 14
                    new OfficialAccommodation
                    {
                        TravelSupplierId = 460,
                        RegionId = 14,
                        Name = "澎湖福朋喜來登飯店",
                        Description = "國際連鎖品牌，位於馬公市中心，適合觀光與商務旅客。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAccommodation
                    {
                        TravelSupplierId = 461,
                        RegionId = 14,
                        Name = "金門昇恆昌金湖飯店",
                        Description = "金門首屈一指的高級飯店，提供豪華住宿與完善設施。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAccommodation
                    {
                        TravelSupplierId = 462,
                        RegionId = 14,
                        Name = "小琉球海景民宿聯盟",
                        Description = "整合小琉球民宿資源，提供便捷預訂與海景住宿體驗。",
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
                _context.OfficialRestaurants.AddRange(
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 21,
                        RegionId = 1,
                        Name = "札幌螃蟹本家",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 22,
                        RegionId = 1,
                        Name = "成吉思汗烤肉館",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 23,
                        RegionId = 1,
                        Name = "湯咖哩一番亭",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 24,
                        RegionId = 1,
                        Name = "函館朝市食堂",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 25,
                        RegionId = 1,
                        Name = "釧路爐端燒",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 26,
                        RegionId = 1,
                        Name = "小樽壽司通本店",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 27,
                        RegionId = 1,
                        Name = "旭川拉麵橫丁",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 28,
                        RegionId = 1,
                        Name = "登別溫泉懷石料理",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 29,
                        RegionId = 1,
                        Name = "富良野起司工房",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 30,
                        RegionId = 1,
                        Name = "知床鄉味居酒屋",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 54,
                        RegionId = 2,
                        Name = "盛岡冷麵館",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 55,
                        RegionId = 2,
                        Name = "山形芋煮食堂",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 56,
                        RegionId = 2,
                        Name = "比內地雞本舖",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 57,
                        RegionId = 2,
                        Name = "仙台牛舌炭燒亭",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 58,
                        RegionId = 2,
                        Name = "青森蘋果甜點工房",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 59,
                        RegionId = 2,
                        Name = "角館稻庭烏龍麵店",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 60,
                        RegionId = 2,
                        Name = "福島喜多方拉麵村",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 61,
                        RegionId = 2,
                        Name = "弘前城下和食亭",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 62,
                        RegionId = 2,
                        Name = "會津味噌烤肉屋",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 63,
                        RegionId = 2,
                        Name = "鶴岡壽司海味亭",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 87,
                        RegionId = 3,
                        Name = "築地壽司大",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 88,
                        RegionId = 3,
                        Name = "東京豚骨一蘭拉麵",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 89,
                        RegionId = 3,
                        Name = "淺草天婦羅大黑家",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 90,
                        RegionId = 3,
                        Name = "橫濱中華街老四川",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 91,
                        RegionId = 3,
                        Name = "原宿竹下通可麗餅屋",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 92,
                        RegionId = 3,
                        Name = "新宿高樓景觀餐廳",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 93,
                        RegionId = 3,
                        Name = "池袋拉麵街",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 94,
                        RegionId = 3,
                        Name = "吉祥寺甜點工房",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 95,
                        RegionId = 3,
                        Name = "鎌倉海邊披薩屋",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 96,
                        RegionId = 3,
                        Name = "日光湯葉料理坊",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 120,
                        RegionId = 4,
                        Name = "名古屋矢場味噌豬排",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 121,
                        RegionId = 4,
                        Name = "高山飛驒牛壽喜燒",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 122,
                        RegionId = 4,
                        Name = "金澤近江町市場壽司亭",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 123,
                        RegionId = 4,
                        Name = "松本蕎麥麵本家",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 124,
                        RegionId = 4,
                        Name = "白川鄉田樂之家",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 125,
                        RegionId = 4,
                        Name = "富士吉田烏龍麵老店",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 126,
                        RegionId = 4,
                        Name = "輪島海鮮燒店",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 127,
                        RegionId = 4,
                        Name = "名古屋雞翅山ちゃん",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 128,
                        RegionId = 4,
                        Name = "靜岡濱松餃子之家",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 129,
                        RegionId = 4,
                        Name = "伊勢烏龍麵茶屋",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 153,
                        RegionId = 5,
                        Name = "大阪章魚燒本舖",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 154,
                        RegionId = 5,
                        Name = "京都湯豆腐南禪寺順正",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 155,
                        RegionId = 5,
                        Name = "神戶牛鐵板燒老舖",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 156,
                        RegionId = 5,
                        Name = "奈良柿葉壽司名店",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 157,
                        RegionId = 5,
                        Name = "大阪壽喜燒今井亭",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 158,
                        RegionId = 5,
                        Name = "京都抹茶甜點宇治園",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 159,
                        RegionId = 5,
                        Name = "大阪串炸一丁目",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 160,
                        RegionId = 5,
                        Name = "京都懷石料理瓢亭",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 161,
                        RegionId = 5,
                        Name = "神戶中華街老店萬福樓",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 162,
                        RegionId = 5,
                        Name = "奈良茶粥早餐堂",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 186,
                        RegionId = 6,
                        Name = "廣島燒長田屋",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 187,
                        RegionId = 6,
                        Name = "岡山桃太郎壽司",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 188,
                        RegionId = 6,
                        Name = "鳥取和牛炭燒亭",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 189,
                        RegionId = 6,
                        Name = "出雲蕎麥麵本舖",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 190,
                        RegionId = 6,
                        Name = "山口瓦蕎麥專門店",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 191,
                        RegionId = 6,
                        Name = "倉敷白壁和食亭",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 192,
                        RegionId = 6,
                        Name = "松江鰻魚飯老店",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 193,
                        RegionId = 6,
                        Name = "宮島牡蠣料理坊",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 194,
                        RegionId = 6,
                        Name = "津和野壽司與茶屋",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 195,
                        RegionId = 6,
                        Name = "岩國蓮根創意料理",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 219,
                        RegionId = 7,
                        Name = "讚岐烏龍麵本舖",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 220,
                        RegionId = 7,
                        Name = "高知鰹節藁燒屋",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 221,
                        RegionId = 7,
                        Name = "德島鳴門鯛料理坊",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 222,
                        RegionId = 7,
                        Name = "松山炸雞南蠻本舖",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 223,
                        RegionId = 7,
                        Name = "香川和菓子茶屋",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 224,
                        RegionId = 7,
                        Name = "內子町鄉土料理亭",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 225,
                        RegionId = 7,
                        Name = "直島海鮮丼小館",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 226,
                        RegionId = 7,
                        Name = "德島蕎麥專門店",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 227,
                        RegionId = 7,
                        Name = "四萬十川川魚料理店",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 228,
                        RegionId = 7,
                        Name = "道後溫泉甜點小舖",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 252,
                        RegionId = 8,
                        Name = "博多一風堂本店",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 253,
                        RegionId = 8,
                        Name = "熊本馬肉料理屋",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 254,
                        RegionId = 8,
                        Name = "長崎皿烏龍麵堂",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 255,
                        RegionId = 8,
                        Name = "由布院田舍料理",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 256,
                        RegionId = 8,
                        Name = "別府地獄蒸工房",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 257,
                        RegionId = 8,
                        Name = "鹿兒島黑豚燒肉店",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 258,
                        RegionId = 8,
                        Name = "高千穗鄉土料理館",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 259,
                        RegionId = 8,
                        Name = "嬉野溫泉豆腐料理坊",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 260,
                        RegionId = 8,
                        Name = "柳川鰻魚飯屋",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 261,
                        RegionId = 8,
                        Name = "宮崎芒果甜點舖",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 285,
                        RegionId = 9,
                        Name = "國際通沖繩料理花笠食堂",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 286,
                        RegionId = 9,
                        Name = "首里城御膳懷石屋",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 287,
                        RegionId = 9,
                        Name = "那霸市場海鮮丼亭",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 288,
                        RegionId = 9,
                        Name = "北谷美國村牛排館",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 289,
                        RegionId = 9,
                        Name = "古宇利島海景咖啡",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 290,
                        RegionId = 9,
                        Name = "恩納村海葡萄海鮮店",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 291,
                        RegionId = 9,
                        Name = "石垣牛壽喜燒本舖",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 292,
                        RegionId = 9,
                        Name = "宮古島芒果甜點專賣店",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 293,
                        RegionId = 9,
                        Name = "西表島山野料理小屋",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 294,
                        RegionId = 9,
                        Name = "瀨長島泡盛居酒屋",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    }
                );
                await _context.SaveChangesAsync();
                _context.OfficialRestaurants.AddRange(
                    // 北部 ID = 10
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 318,
                        RegionId = 10,
                        Name = "欣葉台菜餐廳信義店",
                        Description = "道地台灣風味的經典台菜，適合家庭與團體聚餐的熱門選擇。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 319,
                        RegionId = 10,
                        Name = "阿宗麵線西門店",
                        Description = "以濃郁大腸麵線聞名，是台北知名小吃代表。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 320,
                        RegionId = 10,
                        Name = "鼎泰豐信義本店",
                        Description = "全球知名的小籠包品牌，提供經典中式點心。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 321,
                        RegionId = 10,
                        Name = "米香餐廳 - 大倉久和飯店",
                        Description = "融合現代與傳統風味的台菜套餐與高級宴會服務。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 322,
                        RegionId = 10,
                        Name = "基隆廟口夜市美食導覽",
                        Description = "帶領遊客探索基隆夜市的在地美食與小吃文化。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },

                    // 中部 ID = 11
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 351,
                        RegionId = 11,
                        Name = "屋馬燒肉園邸店",
                        Description = "台中人氣燒肉品牌，提供優質肉品與貼心服務。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 352,
                        RegionId = 11,
                        Name = "台中第二市場王記菜頭粿",
                        Description = "擁有多年歷史的傳統小吃攤，以菜頭粿聞名。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 353,
                        RegionId = 11,
                        Name = "清水休息站牛肉麵",
                        Description = "高速公路休息站的知名牛肉麵店，湯頭濃郁。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 354,
                        RegionId = 11,
                        Name = "南投魚池紅茶香餐坊",
                        Description = "結合當地紅茶特色的創意料理餐廳。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 355,
                        RegionId = 11,
                        Name = "草屯蚵仔煎文化館",
                        Description = "推廣蚵仔煎文化與台灣傳統小吃的體驗餐廳。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },

                    // 南部 ID = 12
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 384,
                        RegionId = 12,
                        Name = "台南度小月擔仔麵",
                        Description = "百年傳承的經典小吃，以擔仔麵聞名台灣。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 385,
                        RegionId = 12,
                        Name = "高雄六合夜市美食導覽",
                        Description = "人氣夜市美食導覽行程，體驗高雄經典小吃。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 386,
                        RegionId = 12,
                        Name = "屏東萬巒豬腳街",
                        Description = "專賣豬腳料理的街道，是屏東知名美食地標。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 387,
                        RegionId = 12,
                        Name = "嘉義火雞肉飯老店",
                        Description = "嘉義地區著名的火雞肉飯老字號店家。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 388,
                        RegionId = 12,
                        Name = "高雄鹽埕區鴨肉珍",
                        Description = "傳承多年的鴨肉飯名店，深受當地人喜愛。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },

                    // 東部 ID = 13
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 417,
                        RegionId = 13,
                        Name = "花蓮炸彈蔥油餅總店",
                        Description = "花蓮排隊小吃，現炸蔥油餅酥脆美味限量供應。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 418,
                        RegionId = 13,
                        Name = "台東卑南包子店",
                        Description = "傳承三代的台東點心代表，以大顆多汁著稱。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 419,
                        RegionId = 13,
                        Name = "公正街包子與蒸餃",
                        Description = "當地知名的早點與包子店，歷史悠久。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 420,
                        RegionId = 13,
                        Name = "池上便當文化館食堂",
                        Description = "展示池上便當文化並供應現做便當料理。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 421,
                        RegionId = 13,
                        Name = "知本溫泉甕缸雞料理",
                        Description = "以特色甕缸雞聞名，融合知本溫泉風味。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },

                    // 離島 ID = 14
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 450,
                        RegionId = 14,
                        Name = "澎湖阿華海產",
                        Description = "提供現撈海鮮的老字號海產料理店。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 451,
                        RegionId = 14,
                        Name = "金門金城牛肉麵",
                        Description = "深受在地人喜愛的中式牛肉麵餐廳。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 452,
                        RegionId = 14,
                        Name = "馬祖老酒麵線之家",
                        Description = "馬祖特色紅糟與老酒料理的代表性餐館。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 453,
                        RegionId = 14,
                        Name = "小琉球海龜燒烤BBQ",
                        Description = "面海而食，提供燒烤與海景休閒體驗。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialRestaurant
                    {
                        TravelSupplierId = 454,
                        RegionId = 14,
                        Name = "金門高粱醉雞料理坊",
                        Description = "以金門高粱結合料理創意，推出醉雞名菜。",
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
                _context.OfficialAttractions.AddRange(
                    new OfficialAttraction
                    {
                        TravelSupplierId = 1,
                        RegionId = 1,
                        Name = "札幌白色戀人公園",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 2,
                        RegionId = 1,
                        Name = "旭山動物園",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 3,
                        RegionId = 1,
                        Name = "小樽運河歷史館",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 4,
                        RegionId = 1,
                        Name = "富良野薰衣草田",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 5,
                        RegionId = 1,
                        Name = "美瑛青池導覽",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 6,
                        RegionId = 1,
                        Name = "登別地獄谷觀光協會",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 7,
                        RegionId = 1,
                        Name = "函館山夜景纜車",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 8,
                        RegionId = 1,
                        Name = "知床五湖導覽中心",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 9,
                        RegionId = 1,
                        Name = "釧路濕原自然漫步",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 10,
                        RegionId = 1,
                        Name = "摩周湖觀景平台",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 11,
                        RegionId = 1,
                        Name = "網走流冰體驗船",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 12,
                        RegionId = 1,
                        Name = "大雪山國立公園步道",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 13,
                        RegionId = 1,
                        Name = "十勝牧場雪橇",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 14,
                        RegionId = 1,
                        Name = "洞爺湖環湖遊覽",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 15,
                        RegionId = 1,
                        Name = "支笏湖透明獨木舟",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 16,
                        RegionId = 1,
                        Name = "北國雪屋博物館",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 17,
                        RegionId = 1,
                        Name = "旭岳纜車",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 18,
                        RegionId = 1,
                        Name = "二世谷滑雪場接駁",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 19,
                        RegionId = 1,
                        Name = "小樽玻璃工坊體驗",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 20,
                        RegionId = 1,
                        Name = "三角市場早市體驗",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 34,
                        RegionId = 2,
                        Name = "藏王樹冰觀光纜車",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 35,
                        RegionId = 2,
                        Name = "十和田湖遊覽船",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 36,
                        RegionId = 2,
                        Name = "角館武家屋敷",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 37,
                        RegionId = 2,
                        Name = "青森睡魔祭館",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 38,
                        RegionId = 2,
                        Name = "銀山溫泉老街導覽",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 39,
                        RegionId = 2,
                        Name = "男鹿真山傳承館",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 40,
                        RegionId = 2,
                        Name = "立石寺千年石階",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 41,
                        RegionId = 2,
                        Name = "五色沼自然步道",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 42,
                        RegionId = 2,
                        Name = "田澤湖畔健行體驗",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 43,
                        RegionId = 2,
                        Name = "弘前城公園櫻花季",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 44,
                        RegionId = 2,
                        Name = "猊鼻溪遊船",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 45,
                        RegionId = 2,
                        Name = "大內宿茅草老街",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 46,
                        RegionId = 2,
                        Name = "花卷溫泉玫瑰園",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 47,
                        RegionId = 2,
                        Name = "蔦沼紅葉攝影地",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 48,
                        RegionId = 2,
                        Name = "會津若松鶴城",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 49,
                        RegionId = 2,
                        Name = "三內丸山遺跡導覽",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 50,
                        RegionId = 2,
                        Name = "鳴子峽賞楓步道",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 51,
                        RegionId = 2,
                        Name = "盛岡手工藝村",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 52,
                        RegionId = 2,
                        Name = "秋田內陸縱貫鐵道",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 53,
                        RegionId = 2,
                        Name = "津輕三味線演奏廳",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 67,
                        RegionId = 3,
                        Name = "東京晴空塔",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 68,
                        RegionId = 3,
                        Name = "淺草雷門與仲見世通",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 69,
                        RegionId = 3,
                        Name = "明治神宮森林公園",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 70,
                        RegionId = 3,
                        Name = "上野動物園",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 71,
                        RegionId = 3,
                        Name = "橫濱紅磚倉庫",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 72,
                        RegionId = 3,
                        Name = "東京迪士尼樂園",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 73,
                        RegionId = 3,
                        Name = "橫濱八景島海島樂園",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 74,
                        RegionId = 3,
                        Name = "新宿御苑",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 75,
                        RegionId = 3,
                        Name = "築地市場歷史導覽",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 76,
                        RegionId = 3,
                        Name = "箱根蘆之湖觀光船",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 77,
                        RegionId = 3,
                        Name = "江之島展望台與海岸線",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 78,
                        RegionId = 3,
                        Name = "川越小江戶老街",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 79,
                        RegionId = 3,
                        Name = "日光東照宮",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 80,
                        RegionId = 3,
                        Name = "鎌倉大佛與鶴岡八幡宮",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 81,
                        RegionId = 3,
                        Name = "茨城國營常陸海濱公園",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 82,
                        RegionId = 3,
                        Name = "館山野島崎燈塔",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 83,
                        RegionId = 3,
                        Name = "秩父芝櫻之丘",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 84,
                        RegionId = 3,
                        Name = "東京灣夜景巡航船",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 85,
                        RegionId = 3,
                        Name = "三鷹之森吉卜力美術館",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 86,
                        RegionId = 3,
                        Name = "國立科學博物館",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 100,
                        RegionId = 4,
                        Name = "富士山五合目登山口",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 101,
                        RegionId = 4,
                        Name = "白川鄉合掌村導覽中心",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 102,
                        RegionId = 4,
                        Name = "立山黑部阿爾卑斯路線",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 103,
                        RegionId = 4,
                        Name = "高山古街歷史協會",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 104,
                        RegionId = 4,
                        Name = "金澤兼六園遊園券",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 105,
                        RegionId = 4,
                        Name = "松本城觀光案內",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 106,
                        RegionId = 4,
                        Name = "名古屋城天守導覽",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 107,
                        RegionId = 4,
                        Name = "熱田神宮祈福體驗",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 108,
                        RegionId = 4,
                        Name = "白山比咩神社",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 109,
                        RegionId = 4,
                        Name = "妻籠宿與馬籠宿歷史步道",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 110,
                        RegionId = 4,
                        Name = "富山玻璃美術館",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 111,
                        RegionId = 4,
                        Name = "輪島朝市文化街",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 112,
                        RegionId = 4,
                        Name = "郡上八幡水之町景",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 113,
                        RegionId = 4,
                        Name = "伊豆下田海中水族館",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 114,
                        RegionId = 4,
                        Name = "彥根城與玄宮園",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 115,
                        RegionId = 4,
                        Name = "伊勢神宮正宮參拜",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 116,
                        RegionId = 4,
                        Name = "奈良井宿中山道古道",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 117,
                        RegionId = 4,
                        Name = "諏訪湖花火船票",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 118,
                        RegionId = 4,
                        Name = "駿府城公園遺址",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 119,
                        RegionId = 4,
                        Name = "飛驒大鍾乳洞",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 133,
                        RegionId = 5,
                        Name = "京都清水寺觀光處",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 134,
                        RegionId = 5,
                        Name = "金閣寺參拜事務局",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 135,
                        RegionId = 5,
                        Name = "伏見稻荷大社參拜服務",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 136,
                        RegionId = 5,
                        Name = "嵐山竹林步道導覽",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 137,
                        RegionId = 5,
                        Name = "大阪城天守閣導覽處",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 138,
                        RegionId = 5,
                        Name = "大阪環球影城官方票務",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 139,
                        RegionId = 5,
                        Name = "神戶港塔觀景台",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 140,
                        RegionId = 5,
                        Name = "奈良公園與東大寺導覽",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 141,
                        RegionId = 5,
                        Name = "通天閣觀光塔",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 142,
                        RegionId = 5,
                        Name = "比叡山延曆寺",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 143,
                        RegionId = 5,
                        Name = "宇治平等院鳳凰堂",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 144,
                        RegionId = 5,
                        Name = "三十三間堂參拜服務",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 145,
                        RegionId = 5,
                        Name = "京都國立博物館",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 146,
                        RegionId = 5,
                        Name = "鞍馬山登山路線",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 147,
                        RegionId = 5,
                        Name = "梅田空中庭園展望台",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 148,
                        RegionId = 5,
                        Name = "甲子園歷史館",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 149,
                        RegionId = 5,
                        Name = "神戶異人館街",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 150,
                        RegionId = 5,
                        Name = "道頓堀觀光船",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 151,
                        RegionId = 5,
                        Name = "大阪萬博紀念公園",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 152,
                        RegionId = 5,
                        Name = "奈良今井町歷史街區",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 166,
                        RegionId = 6,
                        Name = "嚴島神社導覽處",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 167,
                        RegionId = 6,
                        Name = "廣島和平紀念公園",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 168,
                        RegionId = 6,
                        Name = "鳥取砂丘騎駱駝體驗",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 169,
                        RegionId = 6,
                        Name = "松江城古蹟導覽",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 170,
                        RegionId = 6,
                        Name = "出雲大社正式參拜服務",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 171,
                        RegionId = 6,
                        Name = "錦帶橋木橋導覽",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 172,
                        RegionId = 6,
                        Name = "岡山後樂園庭園漫遊",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 173,
                        RegionId = 6,
                        Name = "倉敷美觀歷史保存區",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 174,
                        RegionId = 6,
                        Name = "萩城下町古屋敷",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 175,
                        RegionId = 6,
                        Name = "大山登山健行會",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 176,
                        RegionId = 6,
                        Name = "廣島城天守導覽",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 177,
                        RegionId = 6,
                        Name = "鳥取沙丘兒童博物館",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 178,
                        RegionId = 6,
                        Name = "湯田溫泉足湯街",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 179,
                        RegionId = 6,
                        Name = "津和野武家屋敷街",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 180,
                        RegionId = 6,
                        Name = "廣島三景園庭園漫遊",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 181,
                        RegionId = 6,
                        Name = "岩國白蛇館",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 182,
                        RegionId = 6,
                        Name = "島根足立美術館",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 183,
                        RegionId = 6,
                        Name = "青海島自然海岸觀光",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 184,
                        RegionId = 6,
                        Name = "山口瑠璃光寺五重塔",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 185,
                        RegionId = 6,
                        Name = "因幡萬葉歷史館",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 199,
                        RegionId = 7,
                        Name = "栗林公園遊園導覽",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 200,
                        RegionId = 7,
                        Name = "金刀比羅宮參拜服務",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 201,
                        RegionId = 7,
                        Name = "直島地中美術館導覽",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 202,
                        RegionId = 7,
                        Name = "德島阿波舞會館",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 203,
                        RegionId = 7,
                        Name = "鳴門漩渦觀潮船",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 204,
                        RegionId = 7,
                        Name = "眉山纜車展望台",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 205,
                        RegionId = 7,
                        Name = "道後溫泉本館",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 206,
                        RegionId = 7,
                        Name = "松山城空中纜車",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 207,
                        RegionId = 7,
                        Name = "下灘車站海岸景點",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 208,
                        RegionId = 7,
                        Name = "宇和島鯛釣體驗中心",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 209,
                        RegionId = 7,
                        Name = "四萬十川屋形船觀光",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 210,
                        RegionId = 7,
                        Name = "高知桂濱坂本龍馬像",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 211,
                        RegionId = 7,
                        Name = "足摺岬展望台",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 212,
                        RegionId = 7,
                        Name = "龍河洞鐘乳石洞",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 213,
                        RegionId = 7,
                        Name = "內子町歷史街道保存館",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 214,
                        RegionId = 7,
                        Name = "高松玉藻公園",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 215,
                        RegionId = 7,
                        Name = "豐島美術館參觀服務",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 216,
                        RegionId = 7,
                        Name = "瓶之森林自行車體驗",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 217,
                        RegionId = 7,
                        Name = "仁淀藍水清流探訪",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 218,
                        RegionId = 7,
                        Name = "祖谷藤蔓橋景區",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 232,
                        RegionId = 8,
                        Name = "太宰府天滿宮",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 233,
                        RegionId = 8,
                        Name = "博多運河城水舞秀",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 234,
                        RegionId = 8,
                        Name = "柳川川下り船屋",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 235,
                        RegionId = 8,
                        Name = "長崎哥拉巴園",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 236,
                        RegionId = 8,
                        Name = "長崎原爆資料館",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 237,
                        RegionId = 8,
                        Name = "豪斯登堡主題樂園",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 238,
                        RegionId = 8,
                        Name = "熊本城復原天守閣",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 239,
                        RegionId = 8,
                        Name = "阿蘇火山火口",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 240,
                        RegionId = 8,
                        Name = "黑川溫泉散策步道",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 241,
                        RegionId = 8,
                        Name = "由布院金鱗湖",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 242,
                        RegionId = 8,
                        Name = "別府地獄溫泉巡禮",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 243,
                        RegionId = 8,
                        Name = "高千穗峽划船體驗",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 244,
                        RegionId = 8,
                        Name = "鵜戶神宮洞窟參拜",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 245,
                        RegionId = 8,
                        Name = "霧島神宮古道",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 246,
                        RegionId = 8,
                        Name = "櫻島火山觀景台",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 247,
                        RegionId = 8,
                        Name = "指宿砂浴體驗場",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 248,
                        RegionId = 8,
                        Name = "志布志鐵道遺跡巡禮",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 249,
                        RegionId = 8,
                        Name = "青島神社與海灘",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 250,
                        RegionId = 8,
                        Name = "嬉野溫泉足湯街",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 251,
                        RegionId = 8,
                        Name = "平戶城遺跡與博物館",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 265,
                        RegionId = 9,
                        Name = "沖繩美麗海水族館",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 266,
                        RegionId = 9,
                        Name = "首里城公園歷史導覽",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 267,
                        RegionId = 9,
                        Name = "玉泉洞鐘乳石洞",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 268,
                        RegionId = 9,
                        Name = "琉球村文化主題園區",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 269,
                        RegionId = 9,
                        Name = "萬座毛絕壁海岸",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 270,
                        RegionId = 9,
                        Name = "國際通購物街導覽",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 271,
                        RegionId = 9,
                        Name = "波上宮與海灘",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 272,
                        RegionId = 9,
                        Name = "殘波岬燈塔公園",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 273,
                        RegionId = 9,
                        Name = "今歸仁城跡櫻花祭",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 274,
                        RegionId = 9,
                        Name = "沖繩世界蛇酒展示館",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 275,
                        RegionId = 9,
                        Name = "古宇利大橋海景觀光",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 276,
                        RegionId = 9,
                        Name = "瀨底島浮潛俱樂部",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 277,
                        RegionId = 9,
                        Name = "水納島一日快艇遊",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 278,
                        RegionId = 9,
                        Name = "青之洞窟潛水中心",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 279,
                        RegionId = 9,
                        Name = "海中道路自行車遊",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 280,
                        RegionId = 9,
                        Name = "宜野灣熱帶海灘",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 281,
                        RegionId = 9,
                        Name = "泡瀨漁港夕陽觀景",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 282,
                        RegionId = 9,
                        Name = "石垣島川平灣玻璃船",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 283,
                        RegionId = 9,
                        Name = "西表島森林探險導覽",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 284,
                        RegionId = 9,
                        Name = "宮古島來間大橋觀景平台",
                        Description = "這是官方資料",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    }
                );
                await _context.SaveChangesAsync();
                _context.OfficialAttractions.AddRange(
                    // 北部 ID = 10
                    new OfficialAttraction
                    {
                        TravelSupplierId = 298,
                        RegionId = 10,
                        Name = "台北101觀景台",
                        Description = "台北市地標高樓，可俯瞰全市美景，是熱門觀光景點。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 299,
                        RegionId = 10,
                        Name = "國立故宮博物院",
                        Description = "典藏中國歷代文物的國際級博物館，文化愛好者必訪。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 300,
                        RegionId = 10,
                        Name = "士林官邸花園",
                        Description = "總統官邸園區，四季花卉美景與歷史建築共融。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 301,
                        RegionId = 10,
                        Name = "陽明山國家公園",
                        Description = "擁有火山地形與溫泉景觀，是台北近郊的自然樂園。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 302,
                        RegionId = 10,
                        Name = "北投溫泉博物館",
                        Description = "日治時期的溫泉浴場建築，見證北投溫泉發展史。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },

                    // 中部 ID = 11
                    new OfficialAttraction
                    {
                        TravelSupplierId = 331,
                        RegionId = 11,
                        Name = "日月潭纜車景觀站",
                        Description = "連接九族文化村與日月潭的空中纜車，風景壯麗。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 332,
                        RegionId = 11,
                        Name = "九族文化村主題樂園",
                        Description = "結合原住民文化與刺激遊樂設施的綜合樂園。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 333,
                        RegionId = 11,
                        Name = "溪頭自然教育園區",
                        Description = "森林步道與生態導覽，是學習與放鬆兼具的去處。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 334,
                        RegionId = 11,
                        Name = "台中歌劇院導覽行程",
                        Description = "建築奇觀，融合聲學與劇場美學，提供導覽體驗。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 335,
                        RegionId = 11,
                        Name = "彩虹眷村藝術導覽",
                        Description = "由退伍軍人彩繪而成的眷村，色彩繽紛具藝術感。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },

                    // 南部 ID = 12
                    new OfficialAttraction
                    {
                        TravelSupplierId = 364,
                        RegionId = 12,
                        Name = "蓮池潭龍虎塔",
                        Description = "走入龍口出虎口，傳統宗教與美麗湖景融合。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 365,
                        RegionId = 12,
                        Name = "駁二藝術特區",
                        Description = "倉庫改建的藝文空間，文創展演與市集活動聚點。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 366,
                        RegionId = 12,
                        Name = "旗津燈塔與貝殼館",
                        Description = "欣賞海景與了解貝類生態的絕佳親子景點。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 367,
                        RegionId = 12,
                        Name = "台南安平古堡",
                        Description = "荷蘭時期建造的城堡，見證台灣歷史的足跡。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 368,
                        RegionId = 12,
                        Name = "赤崁樓文化導覽",
                        Description = "古蹟建築，結合歷史與文學，為台南代表性地標。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },

                    // 東部 ID = 13
                    new OfficialAttraction
                    {
                        TravelSupplierId = 397,
                        RegionId = 13,
                        Name = "太魯閣國家公園",
                        Description = "峽谷壯麗的國家級景點，包含長春祠與燕子口。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 398,
                        RegionId = 13,
                        Name = "清水斷崖觀景平台",
                        Description = "位於蘇花公路的斷崖美景，是東海岸經典畫面。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 399,
                        RegionId = 13,
                        Name = "七星潭自行車道",
                        Description = "沿海騎行路線，風光明媚，適合親子休閒活動。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 400,
                        RegionId = 13,
                        Name = "花蓮松園別館",
                        Description = "日治時期遺構與藝文空間，坐擁太平洋美景。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 401,
                        RegionId = 13,
                        Name = "慕谷慕魚生態園區",
                        Description = "花蓮秘境溪谷景觀，適合溯溪與自然觀察活動。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },

                    // 離島 ID = 14
                    new OfficialAttraction
                    {
                        TravelSupplierId = 430,
                        RegionId = 14,
                        Name = "澎湖雙心石滬",
                        Description = "浪漫雙心漁網遺跡，是澎湖最具代表性的景觀。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 431,
                        RegionId = 14,
                        Name = "金門莒光樓",
                        Description = "金門的精神象徵，見證戰地歷史與和平願景。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 432,
                        RegionId = 14,
                        Name = "馬祖北竿芹壁聚落",
                        Description = "閩東式石屋聚落，完整保留古早建築風貌。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 433,
                        RegionId = 14,
                        Name = "小琉球美人洞風景區",
                        Description = "由珊瑚礁岩形成的天然岩洞，擁海岸絕美景色。",
                        Longitude = 121.5654m,
                        Latitude = 25.0330m
                    },
                    new OfficialAttraction
                    {
                        TravelSupplierId = 434,
                        RegionId = 14,
                        Name = "澎湖跨海大橋",
                        Description = "連接白沙與西嶼的跨海通道，為交通與觀光地標。",
                        Longitude = 121.5654m,
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
                        RegionId = 1,
                        ItemId = 1,
                        Category = TravelCategory.Foreign,
                        Title = "北海道雪景7日遊",
                        AvailableFrom = new DateTime(2025, 4, 10),
                        AvailableUntil = new DateTime(2026, 1, 10),
                        Description = "這是北海道雪景7日遊的國外旅行專案，適合喜歡探索的旅客。",
                        TotalTravelCount = 1,
                        TotalDepartureCount = 7,
                        Days = 7,
                        CoverPath = "https://images.unsplash.com/photo-1739106766539-ea50f6def1df?w=500&auto=format&fit=crop&q=60&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxzZWFyY2h8MTd8fGhva2FpZG98ZW58MHx8MHx8fDA%3D",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        Status = TravelStatus.Active
                    },

                    new OfficialTravel
                    {
                        CreatedByEmployeeId = _context.Employees.First().EmployeeId,
                        RegionId = 1,
                        ItemId = 1,
                        Category = TravelCategory.Foreign,
                        Title = "北海道文化深度10日遊",
                        AvailableFrom = new DateTime(2025, 4, 10),
                        AvailableUntil = new DateTime(2026, 1, 10),
                        Description = "這是北海道文化深度10日遊的國外旅行專案，深入探索在地文化與歷史。",
                        TotalTravelCount = 2,
                        TotalDepartureCount = 8,
                        Days = 10,
                        CoverPath = "https://images.unsplash.com/photo-1739106766539-ea50f6def1df?w=500&auto=format&fit=crop&q=60&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxzZWFyY2h8MTd8fGhva2FpZG98ZW58MHx8MHx8fDA%3D",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        Status = TravelStatus.Active
                    },

                    new OfficialTravel
                    {
                        CreatedByEmployeeId = _context.Employees.First().EmployeeId,
                        RegionId = 2,
                        ItemId = 1,
                        Category = TravelCategory.Foreign,
                        Title = "東北雪景7日遊",
                        AvailableFrom = new DateTime(2025, 4, 10),
                        AvailableUntil = new DateTime(2026, 1, 10),
                        Description = "這是東北雪景7日遊的國外旅行專案，適合喜歡探索的旅客。",
                        TotalTravelCount = 1,
                        TotalDepartureCount = 7,
                        Days = 7,
                        CoverPath = "https://images.unsplash.com/photo-1686397141115-320ba1631f62?w=500&auto=format&fit=crop&q=60&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxzZWFyY2h8MTF8fGhva2FpZG98ZW58MHx8MHx8fDA%3D",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        Status = TravelStatus.Active
                    },

                    new OfficialTravel
                    {
                        CreatedByEmployeeId = _context.Employees.First().EmployeeId,
                        RegionId = 2,
                        ItemId = 1,
                        Category = TravelCategory.Foreign,
                        Title = "東北文化深度10日遊",
                        AvailableFrom = new DateTime(2025, 4, 10),
                        AvailableUntil = new DateTime(2026, 1, 10),
                        Description = "這是東北文化深度10日遊的國外旅行專案，深入探索在地文化與歷史。",
                        TotalTravelCount = 2,
                        TotalDepartureCount = 8,
                        Days = 10,
                        CoverPath = "https://images.unsplash.com/photo-1686397141115-320ba1631f62?w=500&auto=format&fit=crop&q=60&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxzZWFyY2h8MTF8fGhva2FpZG98ZW58MHx8MHx8fDA%3D",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        Status = TravelStatus.Active
                    },

                    new OfficialTravel
                    {
                        CreatedByEmployeeId = _context.Employees.First().EmployeeId,
                        RegionId = 3,
                        ItemId = 1,
                        Category = TravelCategory.Foreign,
                        Title = "關東雪景7日遊",
                        AvailableFrom = new DateTime(2025, 4, 10),
                        AvailableUntil = new DateTime(2026, 1, 10),
                        Description = "這是關東雪景7日遊的國外旅行專案，適合喜歡探索的旅客。",
                        TotalTravelCount = 1,
                        TotalDepartureCount = 7,
                        Days = 7,
                        CoverPath = "https://images.unsplash.com/photo-1542931287-023b922fa89b?w=500&auto=format&fit=crop&q=60&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxzZWFyY2h8OXx8SmFwYW58ZW58MHx8MHx8fDA%3D",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        Status = TravelStatus.Active
                    },

                    new OfficialTravel
                    {
                        CreatedByEmployeeId = _context.Employees.First().EmployeeId,
                        RegionId = 3,
                        ItemId = 1,
                        Category = TravelCategory.Foreign,
                        Title = "關東文化深度10日遊",
                        AvailableFrom = new DateTime(2025, 4, 10),
                        AvailableUntil = new DateTime(2026, 1, 10),
                        Description = "這是關東文化深度10日遊的國外旅行專案，深入探索在地文化與歷史。",
                        TotalTravelCount = 2,
                        TotalDepartureCount = 8,
                        Days = 10,
                        CoverPath = "https://images.unsplash.com/photo-1542931287-023b922fa89b?w=500&auto=format&fit=crop&q=60&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxzZWFyY2h8OXx8SmFwYW58ZW58MHx8MHx8fDA%3D",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        Status = TravelStatus.Active
                    },

                    new OfficialTravel
                    {
                        CreatedByEmployeeId = _context.Employees.First().EmployeeId,
                        RegionId = 4,
                        ItemId = 1,
                        Category = TravelCategory.Foreign,
                        Title = "中部雪景7日遊",
                        AvailableFrom = new DateTime(2025, 4, 10),
                        AvailableUntil = new DateTime(2026, 1, 10),
                        Description = "這是中部雪景7日遊的國外旅行專案，適合喜歡探索的旅客。",
                        TotalTravelCount = 1,
                        TotalDepartureCount = 7,
                        Days = 7,
                        CoverPath = "https://images.unsplash.com/photo-1612741389124-b659f38b528e?w=500&auto=format&fit=crop&q=60&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxzZWFyY2h8NTN8fEphcGFufGVufDB8fDB8fHww",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        Status = TravelStatus.Active
                    },

                    new OfficialTravel
                    {
                        CreatedByEmployeeId = _context.Employees.First().EmployeeId,
                        RegionId = 4,
                        ItemId = 1,
                        Category = TravelCategory.Foreign,
                        Title = "中部文化深度10日遊",
                        AvailableFrom = new DateTime(2025, 4, 10),
                        AvailableUntil = new DateTime(2026, 1, 10),
                        Description = "這是中部文化深度10日遊的國外旅行專案，深入探索在地文化與歷史。",
                        TotalTravelCount = 2,
                        TotalDepartureCount = 8,
                        Days = 10,
                        CoverPath = "https://images.unsplash.com/photo-1612741389124-b659f38b528e?w=500&auto=format&fit=crop&q=60&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxzZWFyY2h8NTN8fEphcGFufGVufDB8fDB8fHww",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        Status = TravelStatus.Active
                    },

                    new OfficialTravel
                    {
                        CreatedByEmployeeId = _context.Employees.First().EmployeeId,
                        RegionId = 5,
                        ItemId = 1,
                        Category = TravelCategory.Foreign,
                        Title = "近畿雪景7日遊",
                        AvailableFrom = new DateTime(2025, 4, 10),
                        AvailableUntil = new DateTime(2026, 1, 10),
                        Description = "這是近畿雪景7日遊的國外旅行專案，適合喜歡探索的旅客。",
                        TotalTravelCount = 1,
                        TotalDepartureCount = 7,
                        Days = 7,
                        CoverPath = "https://images.unsplash.com/photo-1505069446780-4ef442b5207f?w=500&auto=format&fit=crop&q=60&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxzZWFyY2h8NjN8fEphcGFufGVufDB8fDB8fHww",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        Status = TravelStatus.Active
                    },

                    new OfficialTravel
                    {
                        CreatedByEmployeeId = _context.Employees.First().EmployeeId,
                        RegionId = 5,
                        ItemId = 1,
                        Category = TravelCategory.Foreign,
                        Title = "近畿文化深度10日遊",
                        AvailableFrom = new DateTime(2025, 4, 10),
                        AvailableUntil = new DateTime(2026, 1, 10),
                        Description = "這是近畿文化深度10日遊的國外旅行專案，深入探索在地文化與歷史。",
                        TotalTravelCount = 2,
                        TotalDepartureCount = 8,
                        Days = 10,
                        CoverPath = "https://images.unsplash.com/photo-1505069446780-4ef442b5207f?w=500&auto=format&fit=crop&q=60&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxzZWFyY2h8NjN8fEphcGFufGVufDB8fDB8fHww",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        Status = TravelStatus.Active
                    },

                    new OfficialTravel
                    {
                        CreatedByEmployeeId = _context.Employees.First().EmployeeId,
                        RegionId = 6,
                        ItemId = 1,
                        Category = TravelCategory.Foreign,
                        Title = "中國雪景7日遊",
                        AvailableFrom = new DateTime(2025, 4, 10),
                        AvailableUntil = new DateTime(2026, 1, 10),
                        Description = "這是中國雪景7日遊的國外旅行專案，適合喜歡探索的旅客。",
                        TotalTravelCount = 1,
                        TotalDepartureCount = 7,
                        Days = 7,
                        CoverPath = "https://images.unsplash.com/photo-1508384429315-7cf06f2792bb?w=500&auto=format&fit=crop&q=60&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxzZWFyY2h8OTN8fEphcGFufGVufDB8fDB8fHww",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        Status = TravelStatus.Active
                    },

                    new OfficialTravel
                    {
                        CreatedByEmployeeId = _context.Employees.First().EmployeeId,
                        RegionId = 6,
                        ItemId = 1,
                        Category = TravelCategory.Foreign,
                        Title = "中國文化深度10日遊",
                        AvailableFrom = new DateTime(2025, 4, 10),
                        AvailableUntil = new DateTime(2026, 1, 10),
                        Description = "這是中國文化深度10日遊的國外旅行專案，深入探索在地文化與歷史。",
                        TotalTravelCount = 2,
                        TotalDepartureCount = 8,
                        Days = 10,
                        CoverPath = "https://images.unsplash.com/photo-1508384429315-7cf06f2792bb?w=500&auto=format&fit=crop&q=60&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxzZWFyY2h8OTN8fEphcGFufGVufDB8fDB8fHww",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        Status = TravelStatus.Active
                    },

                    new OfficialTravel
                    {
                        CreatedByEmployeeId = _context.Employees.First().EmployeeId,
                        RegionId = 7,
                        ItemId = 1,
                        Category = TravelCategory.Foreign,
                        Title = "四國雪景7日遊",
                        AvailableFrom = new DateTime(2025, 4, 10),
                        AvailableUntil = new DateTime(2026, 1, 10),
                        Description = "這是四國雪景7日遊的國外旅行專案，適合喜歡探索的旅客。",
                        TotalTravelCount = 1,
                        TotalDepartureCount = 7,
                        Days = 7,
                        CoverPath = "https://images.unsplash.com/photo-1563897189520-001001c39f3d?w=500&auto=format&fit=crop&q=60&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxzZWFyY2h8MTEyfHxKYXBhbnxlbnwwfHwwfHx8MA%3D%3D",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        Status = TravelStatus.Active
                    },

                    new OfficialTravel
                    {
                        CreatedByEmployeeId = _context.Employees.First().EmployeeId,
                        RegionId = 7,
                        ItemId = 1,
                        Category = TravelCategory.Foreign,
                        Title = "四國文化深度10日遊",
                        AvailableFrom = new DateTime(2025, 4, 10),
                        AvailableUntil = new DateTime(2026, 1, 10),
                        Description = "這是四國文化深度10日遊的國外旅行專案，深入探索在地文化與歷史。",
                        TotalTravelCount = 2,
                        TotalDepartureCount = 8,
                        Days = 10,
                        CoverPath = "https://images.unsplash.com/photo-1563897189520-001001c39f3d?w=500&auto=format&fit=crop&q=60&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxzZWFyY2h8MTEyfHxKYXBhbnxlbnwwfHwwfHx8MA%3D%3D",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        Status = TravelStatus.Active
                    },

                    new OfficialTravel
                    {
                        CreatedByEmployeeId = _context.Employees.First().EmployeeId,
                        RegionId = 8,
                        ItemId = 1,
                        Category = TravelCategory.Foreign,
                        Title = "九州雪景7日遊",
                        AvailableFrom = new DateTime(2025, 4, 10),
                        AvailableUntil = new DateTime(2026, 1, 10),
                        Description = "這是九州雪景7日遊的國外旅行專案，適合喜歡探索的旅客。",
                        TotalTravelCount = 1,
                        TotalDepartureCount = 7,
                        Days = 7,
                        CoverPath = "https://images.unsplash.com/photo-1580639613257-5b1a20fe3760?w=500&auto=format&fit=crop&q=60&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxzZWFyY2h8MTM3fHxKYXBhbnxlbnwwfHwwfHx8MA%3D%3D",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        Status = TravelStatus.Active
                    },

                    new OfficialTravel
                    {
                        CreatedByEmployeeId = _context.Employees.First().EmployeeId,
                        RegionId = 8,
                        ItemId = 1,
                        Category = TravelCategory.Foreign,
                        Title = "九州文化深度10日遊",
                        AvailableFrom = new DateTime(2025, 4, 10),
                        AvailableUntil = new DateTime(2026, 1, 10),
                        Description = "這是九州文化深度10日遊的國外旅行專案，深入探索在地文化與歷史。",
                        TotalTravelCount = 2,
                        TotalDepartureCount = 8,
                        Days = 10,
                        CoverPath = "https://images.unsplash.com/photo-1580639613257-5b1a20fe3760?w=500&auto=format&fit=crop&q=60&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxzZWFyY2h8MTM3fHxKYXBhbnxlbnwwfHwwfHx8MA%3D%3D",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        Status = TravelStatus.Active
                    },

                    new OfficialTravel
                    {
                        CreatedByEmployeeId = _context.Employees.First().EmployeeId,
                        RegionId = 9,
                        ItemId = 1,
                        Category = TravelCategory.Foreign,
                        Title = "沖繩7日遊",
                        AvailableFrom = new DateTime(2025, 4, 10),
                        AvailableUntil = new DateTime(2026, 1, 10),
                        Description = "這是沖繩7日遊的國外旅行專案，適合喜歡探索的旅客。",
                        TotalTravelCount = 1,
                        TotalDepartureCount = 7,
                        Days = 7,
                        CoverPath = "https://images.unsplash.com/photo-1501560379-05951a742668?w=500&auto=format&fit=crop&q=60&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxzZWFyY2h8NDF8fEphcGFufGVufDB8fDB8fHww",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        Status = TravelStatus.Active
                    },

                    new OfficialTravel
                    {
                        CreatedByEmployeeId = _context.Employees.First().EmployeeId,
                        RegionId = 9,
                        ItemId = 1,
                        Category = TravelCategory.Foreign,
                        Title = "沖繩文化深度10日遊",
                        AvailableFrom = new DateTime(2025, 4, 10),
                        AvailableUntil = new DateTime(2026, 1, 10),
                        Description = "這是沖繩文化深度10日遊的國外旅行專案，深入探索在地文化與歷史。",
                        TotalTravelCount = 2,
                        TotalDepartureCount = 8,
                        Days = 10,
                        CoverPath = "https://images.unsplash.com/photo-1501560379-05951a742668?w=500&auto=format&fit=crop&q=60&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxzZWFyY2h8NDF8fEphcGFufGVufDB8fDB8fHww",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        Status = TravelStatus.Active
                    },

                    new OfficialTravel
                    {
                        CreatedByEmployeeId = _context.Employees.First().EmployeeId,
                        RegionId = 10,
                        ItemId = 1,
                        Category = TravelCategory.Domestic,
                        Title = "北部古蹟3日遊",
                        AvailableFrom = new DateTime(2025, 4, 10),
                        AvailableUntil = new DateTime(2026, 1, 10),
                        Description = "這是北部古蹟3日遊的國內旅行專案，適合文化探索的旅客。",
                        TotalTravelCount = 1,
                        TotalDepartureCount = 7,
                        Days = 3,
                        CoverPath = "https://images.unsplash.com/photo-1552993873-0dd1110e025f?w=500&auto=format&fit=crop&q=60&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxzZWFyY2h8M3x8dGFpcGVpfGVufDB8fDB8fHww",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        Status = TravelStatus.Active
                    },

                    new OfficialTravel
                    {
                        CreatedByEmployeeId = _context.Employees.First().EmployeeId,
                        RegionId = 10,
                        ItemId = 1,
                        Category = TravelCategory.Domestic,
                        Title = "北部風情2日遊",
                        AvailableFrom = new DateTime(2025, 4, 10),
                        AvailableUntil = new DateTime(2026, 1, 10),
                        Description = "這是北部風情2日遊的國內旅行專案，享受自然與美食風光。",
                        TotalTravelCount = 2,
                        TotalDepartureCount = 8,
                        Days = 2,
                        CoverPath = "https://images.unsplash.com/photo-1552993873-0dd1110e025f?w=500&auto=format&fit=crop&q=60&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxzZWFyY2h8M3x8dGFpcGVpfGVufDB8fDB8fHww",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        Status = TravelStatus.Active
                    },

                    new OfficialTravel
                    {
                        CreatedByEmployeeId = _context.Employees.First().EmployeeId,
                        RegionId = 11,
                        ItemId = 1,
                        Category = TravelCategory.Domestic,
                        Title = "中部古蹟3日遊",
                        AvailableFrom = new DateTime(2025, 4, 10),
                        AvailableUntil = new DateTime(2026, 1, 10),
                        Description = "這是中部古蹟3日遊的國內旅行專案，適合文化探索的旅客。",
                        TotalTravelCount = 1,
                        TotalDepartureCount = 7,
                        Days = 3,
                        CoverPath = "https://images.unsplash.com/photo-1533574254680-ff0c0d9e1f3e?w=500&auto=format&fit=crop&q=60&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxzZWFyY2h8MjF8fHRhaXdhbnxlbnwwfHwwfHx8MA%3D%3D",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        Status = TravelStatus.Active
                    },

                    new OfficialTravel
                    {
                        CreatedByEmployeeId = _context.Employees.First().EmployeeId,
                        RegionId = 11,
                        ItemId = 1,
                        Category = TravelCategory.Domestic,
                        Title = "中部風情2日遊",
                        AvailableFrom = new DateTime(2025, 4, 10),
                        AvailableUntil = new DateTime(2026, 1, 10),
                        Description = "這是中部風情2日遊的國內旅行專案，享受自然與美食風光。",
                        TotalTravelCount = 2,
                        TotalDepartureCount = 8,
                        Days = 2,
                        CoverPath = "https://images.unsplash.com/photo-1533574254680-ff0c0d9e1f3e?w=500&auto=format&fit=crop&q=60&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxzZWFyY2h8MjF8fHRhaXdhbnxlbnwwfHwwfHx8MA%3D%3D",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        Status = TravelStatus.Active
                    },

                    new OfficialTravel
                    {
                        CreatedByEmployeeId = _context.Employees.First().EmployeeId,
                        RegionId = 12,
                        ItemId = 1,
                        Category = TravelCategory.Domestic,
                        Title = "南部古蹟3日遊",
                        AvailableFrom = new DateTime(2025, 4, 10),
                        AvailableUntil = new DateTime(2026, 1, 10),
                        Description = "這是南部古蹟3日遊的國內旅行專案，適合文化探索的旅客。",
                        TotalTravelCount = 1,
                        TotalDepartureCount = 7,
                        Days = 3,
                        CoverPath = "https://images.unsplash.com/photo-1530014708989-55a898ad9552?w=500&auto=format&fit=crop&q=60&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxzZWFyY2h8Mjl8fHRhaXdhbnxlbnwwfHwwfHx8MA%3D%3D",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        Status = TravelStatus.Active
                    },

                    new OfficialTravel
                    {
                        CreatedByEmployeeId = _context.Employees.First().EmployeeId,
                        RegionId = 12,
                        ItemId = 1,
                        Category = TravelCategory.Domestic,
                        Title = "南部風情2日遊",
                        AvailableFrom = new DateTime(2025, 4, 10),
                        AvailableUntil = new DateTime(2026, 1, 10),
                        Description = "這是南部風情2日遊的國內旅行專案，享受自然與美食風光。",
                        TotalTravelCount = 2,
                        TotalDepartureCount = 8,
                        Days = 2,
                        CoverPath = "https://images.unsplash.com/photo-1530014708989-55a898ad9552?w=500&auto=format&fit=crop&q=60&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxzZWFyY2h8Mjl8fHRhaXdhbnxlbnwwfHwwfHx8MA%3D%3D",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        Status = TravelStatus.Active
                    },

                    new OfficialTravel
                    {
                        CreatedByEmployeeId = _context.Employees.First().EmployeeId,
                        RegionId = 13,
                        ItemId = 1,
                        Category = TravelCategory.Domestic,
                        Title = "東部古蹟3日遊",
                        AvailableFrom = new DateTime(2025, 4, 10),
                        AvailableUntil = new DateTime(2026, 1, 10),
                        Description = "這是東部古蹟3日遊的國內旅行專案，適合文化探索的旅客。",
                        TotalTravelCount = 1,
                        TotalDepartureCount = 7,
                        Days = 3,
                        CoverPath = "https://images.unsplash.com/photo-1624650727156-dad96f051375?w=500&auto=format&fit=crop&q=60&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxzZWFyY2h8NDN8fHRhaXdhbnxlbnwwfHwwfHx8MA%3D%3D",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        Status = TravelStatus.Active
                    },

                    new OfficialTravel
                    {
                        CreatedByEmployeeId = _context.Employees.First().EmployeeId,
                        RegionId = 13,
                        ItemId = 1,
                        Category = TravelCategory.Domestic,
                        Title = "東部風情2日遊",
                        AvailableFrom = new DateTime(2025, 4, 10),
                        AvailableUntil = new DateTime(2026, 1, 10),
                        Description = "這是東部風情2日遊的國內旅行專案，享受自然與美食風光。",
                        TotalTravelCount = 2,
                        TotalDepartureCount = 8,
                        Days = 2,
                        CoverPath = "https://images.unsplash.com/photo-1624650727156-dad96f051375?w=500&auto=format&fit=crop&q=60&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxzZWFyY2h8NDN8fHRhaXdhbnxlbnwwfHwwfHx8MA%3D%3D",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        Status = TravelStatus.Active
                    },

                    new OfficialTravel
                    {
                        CreatedByEmployeeId = _context.Employees.First().EmployeeId,
                        RegionId = 14,
                        ItemId = 1,
                        Category = TravelCategory.Domestic,
                        Title = "離島古蹟3日遊",
                        AvailableFrom = new DateTime(2025, 4, 10),
                        AvailableUntil = new DateTime(2026, 1, 10),
                        Description = "這是離島古蹟3日遊的國內旅行專案，適合文化探索的旅客。",
                        TotalTravelCount = 1,
                        TotalDepartureCount = 7,
                        Days = 3,
                        CoverPath = "https://images.unsplash.com/photo-1620831098784-2eec2323cc5b?w=500&auto=format&fit=crop&q=60&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxzZWFyY2h8NjB8fHRhaXdhbnxlbnwwfHwwfHx8MA%3D%3D",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        Status = TravelStatus.Active
                    },

                    new OfficialTravel
                    {
                        CreatedByEmployeeId = _context.Employees.First().EmployeeId,
                        RegionId = 14,
                        ItemId = 1,
                        Category = TravelCategory.Domestic,
                        Title = "離島風情2日遊",
                        AvailableFrom = new DateTime(2025, 4, 10),
                        AvailableUntil = new DateTime(2026, 1, 10),
                        Description = "這是離島風情2日遊的國內旅行專案，享受自然與美食風光。",
                        TotalTravelCount = 2,
                        TotalDepartureCount = 8,
                        Days = 2,
                        CoverPath = "https://images.unsplash.com/photo-1620831098784-2eec2323cc5b?w=500&auto=format&fit=crop&q=60&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxzZWFyY2h8NjB8fHRhaXdhbnxlbnwwfHwwfHx8MA%3D%3D",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        Status = TravelStatus.Active
                    }
                                        );
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedOfficialTravelDetailAsync()
        {
            if (!_context.OfficialTravelDetails.Any())
            {
                _context.OfficialTravelDetails.AddRange(
                    new OfficialTravelDetail
                    {
                        OfficialTravelId = 1,
                        TravelNumber = 1,
                        AdultPrice = 60000,
                        ChildPrice = 36000,
                        BabyPrice = 12000,
                        UpdatedAt = new DateTime(2025, 4, 1),
                        State = DetailState.Locked
                    },

                    new OfficialTravelDetail
                    {
                        OfficialTravelId = 2,
                        TravelNumber = 1,
                        AdultPrice = 60000,
                        ChildPrice = 36000,
                        BabyPrice = 12000,
                        UpdatedAt = new DateTime(2025, 4, 1),
                        State = DetailState.Locked
                    },

                    new OfficialTravelDetail
                    {
                        OfficialTravelId = 2,
                        TravelNumber = 2,
                        AdultPrice = 62000,
                        ChildPrice = 37200,
                        BabyPrice = 12400,
                        UpdatedAt = new DateTime(2025, 4, 1),
                        State = DetailState.Locked
                    },

                    new OfficialTravelDetail
                    {
                        OfficialTravelId = 3,
                        TravelNumber = 1,
                        AdultPrice = 60000,
                        ChildPrice = 36000,
                        BabyPrice = 12000,
                        UpdatedAt = new DateTime(2025, 4, 1),
                        State = DetailState.Locked
                    },

                    new OfficialTravelDetail
                    {
                        OfficialTravelId = 4,
                        TravelNumber = 1,
                        AdultPrice = 60000,
                        ChildPrice = 36000,
                        BabyPrice = 12000,
                        UpdatedAt = new DateTime(2025, 4, 1),
                        State = DetailState.Locked
                    },

                    new OfficialTravelDetail
                    {
                        OfficialTravelId = 4,
                        TravelNumber = 2,
                        AdultPrice = 62000,
                        ChildPrice = 37200,
                        BabyPrice = 12400,
                        UpdatedAt = new DateTime(2025, 4, 1),
                        State = DetailState.Locked
                    },

                    new OfficialTravelDetail
                    {
                        OfficialTravelId = 5,
                        TravelNumber = 1,
                        AdultPrice = 60000,
                        ChildPrice = 36000,
                        BabyPrice = 12000,
                        UpdatedAt = new DateTime(2025, 4, 1),
                        State = DetailState.Locked
                    },

                    new OfficialTravelDetail
                    {
                        OfficialTravelId = 6,
                        TravelNumber = 1,
                        AdultPrice = 60000,
                        ChildPrice = 36000,
                        BabyPrice = 12000,
                        UpdatedAt = new DateTime(2025, 4, 1),
                        State = DetailState.Locked
                    },

                    new OfficialTravelDetail
                    {
                        OfficialTravelId = 6,
                        TravelNumber = 2,
                        AdultPrice = 62000,
                        ChildPrice = 37200,
                        BabyPrice = 12400,
                        UpdatedAt = new DateTime(2025, 4, 1),
                        State = DetailState.Locked
                    },

                    new OfficialTravelDetail
                    {
                        OfficialTravelId = 7,
                        TravelNumber = 1,
                        AdultPrice = 60000,
                        ChildPrice = 36000,
                        BabyPrice = 12000,
                        UpdatedAt = new DateTime(2025, 4, 1),
                        State = DetailState.Locked
                    },

                    new OfficialTravelDetail
                    {
                        OfficialTravelId = 8,
                        TravelNumber = 1,
                        AdultPrice = 60000,
                        ChildPrice = 36000,
                        BabyPrice = 12000,
                        UpdatedAt = new DateTime(2025, 4, 1),
                        State = DetailState.Locked
                    },

                    new OfficialTravelDetail
                    {
                        OfficialTravelId = 8,
                        TravelNumber = 2,
                        AdultPrice = 62000,
                        ChildPrice = 37200,
                        BabyPrice = 12400,
                        UpdatedAt = new DateTime(2025, 4, 1),
                        State = DetailState.Locked
                    },

                    new OfficialTravelDetail
                    {
                        OfficialTravelId = 9,
                        TravelNumber = 1,
                        AdultPrice = 60000,
                        ChildPrice = 36000,
                        BabyPrice = 12000,
                        UpdatedAt = new DateTime(2025, 4, 1),
                        State = DetailState.Locked
                    },

                    new OfficialTravelDetail
                    {
                        OfficialTravelId = 10,
                        TravelNumber = 1,
                        AdultPrice = 60000,
                        ChildPrice = 36000,
                        BabyPrice = 12000,
                        UpdatedAt = new DateTime(2025, 4, 1),
                        State = DetailState.Locked
                    },

                    new OfficialTravelDetail
                    {
                        OfficialTravelId = 10,
                        TravelNumber = 2,
                        AdultPrice = 62000,
                        ChildPrice = 37200,
                        BabyPrice = 12400,
                        UpdatedAt = new DateTime(2025, 4, 1),
                        State = DetailState.Locked
                    },

                    new OfficialTravelDetail
                    {
                        OfficialTravelId = 11,
                        TravelNumber = 1,
                        AdultPrice = 60000,
                        ChildPrice = 36000,
                        BabyPrice = 12000,
                        UpdatedAt = new DateTime(2025, 4, 1),
                        State = DetailState.Locked
                    },

                    new OfficialTravelDetail
                    {
                        OfficialTravelId = 12,
                        TravelNumber = 1,
                        AdultPrice = 60000,
                        ChildPrice = 36000,
                        BabyPrice = 12000,
                        UpdatedAt = new DateTime(2025, 4, 1),
                        State = DetailState.Locked
                    },

                    new OfficialTravelDetail
                    {
                        OfficialTravelId = 12,
                        TravelNumber = 2,
                        AdultPrice = 62000,
                        ChildPrice = 37200,
                        BabyPrice = 12400,
                        UpdatedAt = new DateTime(2025, 4, 1),
                        State = DetailState.Locked
                    },

                    new OfficialTravelDetail
                    {
                        OfficialTravelId = 13,
                        TravelNumber = 1,
                        AdultPrice = 60000,
                        ChildPrice = 36000,
                        BabyPrice = 12000,
                        UpdatedAt = new DateTime(2025, 4, 1),
                        State = DetailState.Locked
                    },

                    new OfficialTravelDetail
                    {
                        OfficialTravelId = 14,
                        TravelNumber = 1,
                        AdultPrice = 60000,
                        ChildPrice = 36000,
                        BabyPrice = 12000,
                        UpdatedAt = new DateTime(2025, 4, 1),
                        State = DetailState.Locked
                    },

                    new OfficialTravelDetail
                    {
                        OfficialTravelId = 14,
                        TravelNumber = 2,
                        AdultPrice = 62000,
                        ChildPrice = 37200,
                        BabyPrice = 12400,
                        UpdatedAt = new DateTime(2025, 4, 1),
                        State = DetailState.Locked
                    },

                    new OfficialTravelDetail
                    {
                        OfficialTravelId = 15,
                        TravelNumber = 1,
                        AdultPrice = 60000,
                        ChildPrice = 36000,
                        BabyPrice = 12000,
                        UpdatedAt = new DateTime(2025, 4, 1),
                        State = DetailState.Locked
                    },

                    new OfficialTravelDetail
                    {
                        OfficialTravelId = 16,
                        TravelNumber = 1,
                        AdultPrice = 60000,
                        ChildPrice = 36000,
                        BabyPrice = 12000,
                        UpdatedAt = new DateTime(2025, 4, 1),
                        State = DetailState.Locked
                    },

                    new OfficialTravelDetail
                    {
                        OfficialTravelId = 16,
                        TravelNumber = 2,
                        AdultPrice = 62000,
                        ChildPrice = 37200,
                        BabyPrice = 12400,
                        UpdatedAt = new DateTime(2025, 4, 1),
                        State = DetailState.Locked
                    },

                    new OfficialTravelDetail
                    {
                        OfficialTravelId = 17,
                        TravelNumber = 1,
                        AdultPrice = 60000,
                        ChildPrice = 36000,
                        BabyPrice = 12000,
                        UpdatedAt = new DateTime(2025, 4, 1),
                        State = DetailState.Locked
                    },

                    new OfficialTravelDetail
                    {
                        OfficialTravelId = 18,
                        TravelNumber = 1,
                        AdultPrice = 60000,
                        ChildPrice = 36000,
                        BabyPrice = 12000,
                        UpdatedAt = new DateTime(2025, 4, 1),
                        State = DetailState.Locked
                    },

                    new OfficialTravelDetail
                    {
                        OfficialTravelId = 18,
                        TravelNumber = 2,
                        AdultPrice = 62000,
                        ChildPrice = 37200,
                        BabyPrice = 12400,
                        UpdatedAt = new DateTime(2025, 4, 1),
                        State = DetailState.Locked
                    },

                    new OfficialTravelDetail
                    {
                        OfficialTravelId = 19,
                        TravelNumber = 1,
                        AdultPrice = 14000,
                        ChildPrice = 8400,
                        BabyPrice = 2800,
                        UpdatedAt = new DateTime(2025, 4, 1),
                        State = DetailState.Locked
                    },

                    new OfficialTravelDetail
                    {
                        OfficialTravelId = 20,
                        TravelNumber = 1,
                        AdultPrice = 14000,
                        ChildPrice = 8400,
                        BabyPrice = 2800,
                        UpdatedAt = new DateTime(2025, 4, 1),
                        State = DetailState.Locked
                    },

                    new OfficialTravelDetail
                    {
                        OfficialTravelId = 20,
                        TravelNumber = 2,
                        AdultPrice = 16000,
                        ChildPrice = 9600,
                        BabyPrice = 3200,
                        UpdatedAt = new DateTime(2025, 4, 1),
                        State = DetailState.Locked
                    },

                    new OfficialTravelDetail
                    {
                        OfficialTravelId = 21,
                        TravelNumber = 1,
                        AdultPrice = 14000,
                        ChildPrice = 8400,
                        BabyPrice = 2800,
                        UpdatedAt = new DateTime(2025, 4, 1),
                        State = DetailState.Locked
                    },

                    new OfficialTravelDetail
                    {
                        OfficialTravelId = 22,
                        TravelNumber = 1,
                        AdultPrice = 14000,
                        ChildPrice = 8400,
                        BabyPrice = 2800,
                        UpdatedAt = new DateTime(2025, 4, 1),
                        State = DetailState.Locked
                    },

                    new OfficialTravelDetail
                    {
                        OfficialTravelId = 22,
                        TravelNumber = 2,
                        AdultPrice = 16000,
                        ChildPrice = 9600,
                        BabyPrice = 3200,
                        UpdatedAt = new DateTime(2025, 4, 1),
                        State = DetailState.Locked
                    },

                    new OfficialTravelDetail
                    {
                        OfficialTravelId = 23,
                        TravelNumber = 1,
                        AdultPrice = 14000,
                        ChildPrice = 8400,
                        BabyPrice = 2800,
                        UpdatedAt = new DateTime(2025, 4, 1),
                        State = DetailState.Locked
                    },

                    new OfficialTravelDetail
                    {
                        OfficialTravelId = 24,
                        TravelNumber = 1,
                        AdultPrice = 14000,
                        ChildPrice = 8400,
                        BabyPrice = 2800,
                        UpdatedAt = new DateTime(2025, 4, 1),
                        State = DetailState.Locked
                    },

                    new OfficialTravelDetail
                    {
                        OfficialTravelId = 24,
                        TravelNumber = 2,
                        AdultPrice = 16000,
                        ChildPrice = 9600,
                        BabyPrice = 3200,
                        UpdatedAt = new DateTime(2025, 4, 1),
                        State = DetailState.Locked
                    },

                    new OfficialTravelDetail
                    {
                        OfficialTravelId = 25,
                        TravelNumber = 1,
                        AdultPrice = 14000,
                        ChildPrice = 8400,
                        BabyPrice = 2800,
                        UpdatedAt = new DateTime(2025, 4, 1),
                        State = DetailState.Locked
                    },

                    new OfficialTravelDetail
                    {
                        OfficialTravelId = 26,
                        TravelNumber = 1,
                        AdultPrice = 14000,
                        ChildPrice = 8400,
                        BabyPrice = 2800,
                        UpdatedAt = new DateTime(2025, 4, 1),
                        State = DetailState.Locked
                    },

                    new OfficialTravelDetail
                    {
                        OfficialTravelId = 26,
                        TravelNumber = 2,
                        AdultPrice = 16000,
                        ChildPrice = 9600,
                        BabyPrice = 3200,
                        UpdatedAt = new DateTime(2025, 4, 1),
                        State = DetailState.Locked
                    },

                    new OfficialTravelDetail
                    {
                        OfficialTravelId = 27,
                        TravelNumber = 1,
                        AdultPrice = 14000,
                        ChildPrice = 8400,
                        BabyPrice = 2800,
                        UpdatedAt = new DateTime(2025, 4, 1),
                        State = DetailState.Locked
                    },

                    new OfficialTravelDetail
                    {
                        OfficialTravelId = 28,
                        TravelNumber = 1,
                        AdultPrice = 14000,
                        ChildPrice = 8400,
                        BabyPrice = 2800,
                        UpdatedAt = new DateTime(2025, 4, 1),
                        State = DetailState.Locked
                    },

                    new OfficialTravelDetail
                    {
                        OfficialTravelId = 28,
                        TravelNumber = 2,
                        AdultPrice = 16000,
                        ChildPrice = 9600,
                        BabyPrice = 3200,
                        UpdatedAt = new DateTime(2025, 4, 1),
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
                _context.OfficialTravelSchedules.AddRange(
                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 1,
                        Day = 1,
                        Description = "第 1 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "札幌王子大飯店",
                        Attraction1 = 15,
                        Attraction2 = 17,
                        Attraction3 = 10,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 1,
                        Day = 2,
                        Description = "第 2 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "札幌王子大飯店",
                        Attraction1 = 4,
                        Attraction2 = 18,
                        Attraction3 = 5,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 1,
                        Day = 3,
                        Description = "第 3 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "札幌王子大飯店",
                        Attraction1 = 4,
                        Attraction2 = 6,
                        Attraction3 = 11,
                        Attraction4 = 20,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 1,
                        Day = 4,
                        Description = "第 4 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "札幌王子大飯店",
                        Attraction1 = 10,
                        Attraction2 = 16,
                        Attraction3 = 11,
                        Attraction4 = 19,
                        Attraction5 = 8,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 1,
                        Day = 5,
                        Description = "第 5 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "札幌王子大飯店",
                        Attraction1 = 19,
                        Attraction2 = 20,
                        Attraction3 = 14,
                        Attraction4 = 13,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 1,
                        Day = 6,
                        Description = "第 6 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "札幌王子大飯店",
                        Attraction1 = 4,
                        Attraction2 = 14,
                        Attraction3 = 15,
                        Attraction4 = 1,
                        Attraction5 = 12,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 1,
                        Day = 7,
                        Description = "第 7 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "返回溫暖的家",
                        Attraction1 = 17,
                        Attraction2 = 15,
                        Attraction3 = 14,
                        Attraction4 = 10,
                        Attraction5 = 2,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 2,
                        Day = 1,
                        Description = "第 1 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "札幌王子大飯店",
                        Attraction1 = 14,
                        Attraction2 = 17,
                        Attraction3 = 7,
                        Attraction4 = 1,
                        Attraction5 = 2,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 2,
                        Day = 2,
                        Description = "第 2 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "札幌王子大飯店",
                        Attraction1 = 10,
                        Attraction2 = 8,
                        Attraction3 = 1,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 2,
                        Day = 3,
                        Description = "第 3 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "札幌王子大飯店",
                        Attraction1 = 8,
                        Attraction2 = 19,
                        Attraction3 = 4,
                        Attraction4 = 10,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 2,
                        Day = 4,
                        Description = "第 4 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "札幌王子大飯店",
                        Attraction1 = 1,
                        Attraction2 = 12,
                        Attraction3 = 13,
                        Attraction4 = 11,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 2,
                        Day = 5,
                        Description = "第 5 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "札幌王子大飯店",
                        Attraction1 = 19,
                        Attraction2 = 2,
                        Attraction3 = 10,
                        Attraction4 = 1,
                        Attraction5 = 15,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 2,
                        Day = 6,
                        Description = "第 6 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "札幌王子大飯店",
                        Attraction1 = 4,
                        Attraction2 = 9,
                        Attraction3 = 3,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 2,
                        Day = 7,
                        Description = "第 7 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "札幌王子大飯店",
                        Attraction1 = 11,
                        Attraction2 = 9,
                        Attraction3 = 3,
                        Attraction4 = 10,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 2,
                        Day = 8,
                        Description = "第 8 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "札幌王子大飯店",
                        Attraction1 = 7,
                        Attraction2 = 13,
                        Attraction3 = 18,
                        Attraction4 = 17,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 2,
                        Day = 9,
                        Description = "第 9 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "札幌王子大飯店",
                        Attraction1 = 16,
                        Attraction2 = 19,
                        Attraction3 = 9,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 2,
                        Day = 10,
                        Description = "第 10 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "返回溫暖的家",
                        Attraction1 = 9,
                        Attraction2 = 13,
                        Attraction3 = 11,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 3,
                        Day = 1,
                        Description = "第 1 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "札幌王子大飯店",
                        Attraction1 = 18,
                        Attraction2 = 4,
                        Attraction3 = 6,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 3,
                        Day = 2,
                        Description = "第 2 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "札幌王子大飯店",
                        Attraction1 = 7,
                        Attraction2 = 13,
                        Attraction3 = 20,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 3,
                        Day = 3,
                        Description = "第 3 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "札幌王子大飯店",
                        Attraction1 = 7,
                        Attraction2 = 19,
                        Attraction3 = 20,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 3,
                        Day = 4,
                        Description = "第 4 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "札幌王子大飯店",
                        Attraction1 = 7,
                        Attraction2 = 10,
                        Attraction3 = 15,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 3,
                        Day = 5,
                        Description = "第 5 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "札幌王子大飯店",
                        Attraction1 = 11,
                        Attraction2 = 13,
                        Attraction3 = 4,
                        Attraction4 = 5,
                        Attraction5 = 20,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 3,
                        Day = 6,
                        Description = "第 6 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "札幌王子大飯店",
                        Attraction1 = 3,
                        Attraction2 = 11,
                        Attraction3 = 17,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 3,
                        Day = 7,
                        Description = "第 7 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "札幌王子大飯店",
                        Attraction1 = 5,
                        Attraction2 = 19,
                        Attraction3 = 13,
                        Attraction4 = 16,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 3,
                        Day = 8,
                        Description = "第 8 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "札幌王子大飯店",
                        Attraction1 = 20,
                        Attraction2 = 11,
                        Attraction3 = 8,
                        Attraction4 = 2,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 3,
                        Day = 9,
                        Description = "第 9 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "札幌王子大飯店",
                        Attraction1 = 3,
                        Attraction2 = 7,
                        Attraction3 = 5,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 3,
                        Day = 10,
                        Description = "第 10 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "返回溫暖的家",
                        Attraction1 = 2,
                        Attraction2 = 11,
                        Attraction3 = 13,
                        Attraction4 = 9,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 4,
                        Day = 1,
                        Description = "第 1 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "仙台東橫INN",
                        Attraction1 = 22,
                        Attraction2 = 33,
                        Attraction3 = 40,
                        Attraction4 = 28,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 4,
                        Day = 2,
                        Description = "第 2 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "仙台東橫INN",
                        Attraction1 = 27,
                        Attraction2 = 34,
                        Attraction3 = 32,
                        Attraction4 = 37,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 4,
                        Day = 3,
                        Description = "第 3 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "仙台東橫INN",
                        Attraction1 = 22,
                        Attraction2 = 21,
                        Attraction3 = 39,
                        Attraction4 = 28,
                        Attraction5 = 31,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 4,
                        Day = 4,
                        Description = "第 4 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "仙台東橫INN",
                        Attraction1 = 28,
                        Attraction2 = 24,
                        Attraction3 = 33,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 4,
                        Day = 5,
                        Description = "第 5 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "仙台東橫INN",
                        Attraction1 = 26,
                        Attraction2 = 22,
                        Attraction3 = 29,
                        Attraction4 = 28,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 4,
                        Day = 6,
                        Description = "第 6 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "仙台東橫INN",
                        Attraction1 = 29,
                        Attraction2 = 32,
                        Attraction3 = 27,
                        Attraction4 = 35,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 4,
                        Day = 7,
                        Description = "第 7 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "返回溫暖的家",
                        Attraction1 = 21,
                        Attraction2 = 24,
                        Attraction3 = 38,
                        Attraction4 = 36,
                        Attraction5 = 22,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 5,
                        Day = 1,
                        Description = "第 1 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "仙台東橫INN",
                        Attraction1 = 34,
                        Attraction2 = 31,
                        Attraction3 = 25,
                        Attraction4 = 23,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 5,
                        Day = 2,
                        Description = "第 2 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "仙台東橫INN",
                        Attraction1 = 34,
                        Attraction2 = 38,
                        Attraction3 = 24,
                        Attraction4 = 26,
                        Attraction5 = 31,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 5,
                        Day = 3,
                        Description = "第 3 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "仙台東橫INN",
                        Attraction1 = 23,
                        Attraction2 = 27,
                        Attraction3 = 32,
                        Attraction4 = 34,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 5,
                        Day = 4,
                        Description = "第 4 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "仙台東橫INN",
                        Attraction1 = 30,
                        Attraction2 = 27,
                        Attraction3 = 31,
                        Attraction4 = 36,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 5,
                        Day = 5,
                        Description = "第 5 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "仙台東橫INN",
                        Attraction1 = 31,
                        Attraction2 = 29,
                        Attraction3 = 34,
                        Attraction4 = 23,
                        Attraction5 = 37,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 5,
                        Day = 6,
                        Description = "第 6 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "仙台東橫INN",
                        Attraction1 = 29,
                        Attraction2 = 35,
                        Attraction3 = 27,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 5,
                        Day = 7,
                        Description = "第 7 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "仙台東橫INN",
                        Attraction1 = 24,
                        Attraction2 = 37,
                        Attraction3 = 34,
                        Attraction4 = 36,
                        Attraction5 = 40,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 5,
                        Day = 8,
                        Description = "第 8 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "仙台東橫INN",
                        Attraction1 = 31,
                        Attraction2 = 34,
                        Attraction3 = 39,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 5,
                        Day = 9,
                        Description = "第 9 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "仙台東橫INN",
                        Attraction1 = 21,
                        Attraction2 = 37,
                        Attraction3 = 39,
                        Attraction4 = 22,
                        Attraction5 = 34,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 5,
                        Day = 10,
                        Description = "第 10 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "返回溫暖的家",
                        Attraction1 = 40,
                        Attraction2 = 32,
                        Attraction3 = 38,
                        Attraction4 = 31,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 6,
                        Day = 1,
                        Description = "第 1 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "仙台東橫INN",
                        Attraction1 = 29,
                        Attraction2 = 25,
                        Attraction3 = 27,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 6,
                        Day = 2,
                        Description = "第 2 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "仙台東橫INN",
                        Attraction1 = 31,
                        Attraction2 = 40,
                        Attraction3 = 37,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 6,
                        Day = 3,
                        Description = "第 3 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "仙台東橫INN",
                        Attraction1 = 40,
                        Attraction2 = 25,
                        Attraction3 = 26,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 6,
                        Day = 4,
                        Description = "第 4 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "仙台東橫INN",
                        Attraction1 = 27,
                        Attraction2 = 33,
                        Attraction3 = 21,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 6,
                        Day = 5,
                        Description = "第 5 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "仙台東橫INN",
                        Attraction1 = 23,
                        Attraction2 = 38,
                        Attraction3 = 26,
                        Attraction4 = 24,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 6,
                        Day = 6,
                        Description = "第 6 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "仙台東橫INN",
                        Attraction1 = 30,
                        Attraction2 = 21,
                        Attraction3 = 29,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 6,
                        Day = 7,
                        Description = "第 7 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "仙台東橫INN",
                        Attraction1 = 34,
                        Attraction2 = 25,
                        Attraction3 = 30,
                        Attraction4 = 35,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 6,
                        Day = 8,
                        Description = "第 8 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "仙台東橫INN",
                        Attraction1 = 24,
                        Attraction2 = 36,
                        Attraction3 = 22,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 6,
                        Day = 9,
                        Description = "第 9 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "仙台東橫INN",
                        Attraction1 = 23,
                        Attraction2 = 32,
                        Attraction3 = 38,
                        Attraction4 = 30,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 6,
                        Day = 10,
                        Description = "第 10 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "返回溫暖的家",
                        Attraction1 = 36,
                        Attraction2 = 37,
                        Attraction3 = 34,
                        Attraction4 = 32,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 7,
                        Day = 1,
                        Description = "第 1 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "新宿王子飯店",
                        Attraction1 = 45,
                        Attraction2 = 56,
                        Attraction3 = 54,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 7,
                        Day = 2,
                        Description = "第 2 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "新宿王子飯店",
                        Attraction1 = 46,
                        Attraction2 = 41,
                        Attraction3 = 52,
                        Attraction4 = 45,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 7,
                        Day = 3,
                        Description = "第 3 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "新宿王子飯店",
                        Attraction1 = 57,
                        Attraction2 = 55,
                        Attraction3 = 41,
                        Attraction4 = 54,
                        Attraction5 = 58,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 7,
                        Day = 4,
                        Description = "第 4 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "新宿王子飯店",
                        Attraction1 = 58,
                        Attraction2 = 42,
                        Attraction3 = 47,
                        Attraction4 = 55,
                        Attraction5 = 44,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 7,
                        Day = 5,
                        Description = "第 5 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "新宿王子飯店",
                        Attraction1 = 47,
                        Attraction2 = 58,
                        Attraction3 = 59,
                        Attraction4 = 60,
                        Attraction5 = 50,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 7,
                        Day = 6,
                        Description = "第 6 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "新宿王子飯店",
                        Attraction1 = 49,
                        Attraction2 = 56,
                        Attraction3 = 43,
                        Attraction4 = 58,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 7,
                        Day = 7,
                        Description = "第 7 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "返回溫暖的家",
                        Attraction1 = 46,
                        Attraction2 = 49,
                        Attraction3 = 58,
                        Attraction4 = 54,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 8,
                        Day = 1,
                        Description = "第 1 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "新宿王子飯店",
                        Attraction1 = 57,
                        Attraction2 = 51,
                        Attraction3 = 41,
                        Attraction4 = 47,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 8,
                        Day = 2,
                        Description = "第 2 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "新宿王子飯店",
                        Attraction1 = 60,
                        Attraction2 = 48,
                        Attraction3 = 45,
                        Attraction4 = 54,
                        Attraction5 = 58,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 8,
                        Day = 3,
                        Description = "第 3 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "新宿王子飯店",
                        Attraction1 = 58,
                        Attraction2 = 52,
                        Attraction3 = 55,
                        Attraction4 = 46,
                        Attraction5 = 51,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 8,
                        Day = 4,
                        Description = "第 4 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "新宿王子飯店",
                        Attraction1 = 45,
                        Attraction2 = 48,
                        Attraction3 = 43,
                        Attraction4 = 54,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 8,
                        Day = 5,
                        Description = "第 5 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "新宿王子飯店",
                        Attraction1 = 45,
                        Attraction2 = 58,
                        Attraction3 = 43,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 8,
                        Day = 6,
                        Description = "第 6 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "新宿王子飯店",
                        Attraction1 = 48,
                        Attraction2 = 51,
                        Attraction3 = 46,
                        Attraction4 = 59,
                        Attraction5 = 41,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 8,
                        Day = 7,
                        Description = "第 7 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "新宿王子飯店",
                        Attraction1 = 59,
                        Attraction2 = 52,
                        Attraction3 = 50,
                        Attraction4 = 41,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 8,
                        Day = 8,
                        Description = "第 8 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "新宿王子飯店",
                        Attraction1 = 60,
                        Attraction2 = 43,
                        Attraction3 = 50,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 8,
                        Day = 9,
                        Description = "第 9 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "新宿王子飯店",
                        Attraction1 = 46,
                        Attraction2 = 59,
                        Attraction3 = 45,
                        Attraction4 = 58,
                        Attraction5 = 41,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 8,
                        Day = 10,
                        Description = "第 10 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "返回溫暖的家",
                        Attraction1 = 47,
                        Attraction2 = 58,
                        Attraction3 = 46,
                        Attraction4 = 60,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 9,
                        Day = 1,
                        Description = "第 1 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "新宿王子飯店",
                        Attraction1 = 44,
                        Attraction2 = 56,
                        Attraction3 = 50,
                        Attraction4 = 46,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 9,
                        Day = 2,
                        Description = "第 2 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "新宿王子飯店",
                        Attraction1 = 41,
                        Attraction2 = 44,
                        Attraction3 = 46,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 9,
                        Day = 3,
                        Description = "第 3 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "新宿王子飯店",
                        Attraction1 = 60,
                        Attraction2 = 46,
                        Attraction3 = 47,
                        Attraction4 = 45,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 9,
                        Day = 4,
                        Description = "第 4 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "新宿王子飯店",
                        Attraction1 = 43,
                        Attraction2 = 45,
                        Attraction3 = 53,
                        Attraction4 = 41,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 9,
                        Day = 5,
                        Description = "第 5 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "新宿王子飯店",
                        Attraction1 = 50,
                        Attraction2 = 55,
                        Attraction3 = 46,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 9,
                        Day = 6,
                        Description = "第 6 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "新宿王子飯店",
                        Attraction1 = 58,
                        Attraction2 = 55,
                        Attraction3 = 51,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 9,
                        Day = 7,
                        Description = "第 7 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "新宿王子飯店",
                        Attraction1 = 56,
                        Attraction2 = 57,
                        Attraction3 = 45,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 9,
                        Day = 8,
                        Description = "第 8 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "新宿王子飯店",
                        Attraction1 = 42,
                        Attraction2 = 51,
                        Attraction3 = 50,
                        Attraction4 = 47,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 9,
                        Day = 9,
                        Description = "第 9 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "新宿王子飯店",
                        Attraction1 = 46,
                        Attraction2 = 51,
                        Attraction3 = 44,
                        Attraction4 = 47,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 9,
                        Day = 10,
                        Description = "第 10 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "返回溫暖的家",
                        Attraction1 = 48,
                        Attraction2 = 52,
                        Attraction3 = 45,
                        Attraction4 = 55,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 10,
                        Day = 1,
                        Description = "第 1 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "金澤日航飯店",
                        Attraction1 = 72,
                        Attraction2 = 64,
                        Attraction3 = 79,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 10,
                        Day = 2,
                        Description = "第 2 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "金澤日航飯店",
                        Attraction1 = 79,
                        Attraction2 = 67,
                        Attraction3 = 71,
                        Attraction4 = 62,
                        Attraction5 = 70,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 10,
                        Day = 3,
                        Description = "第 3 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "金澤日航飯店",
                        Attraction1 = 69,
                        Attraction2 = 64,
                        Attraction3 = 72,
                        Attraction4 = 80,
                        Attraction5 = 78,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 10,
                        Day = 4,
                        Description = "第 4 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "金澤日航飯店",
                        Attraction1 = 71,
                        Attraction2 = 73,
                        Attraction3 = 79,
                        Attraction4 = 72,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 10,
                        Day = 5,
                        Description = "第 5 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "金澤日航飯店",
                        Attraction1 = 80,
                        Attraction2 = 65,
                        Attraction3 = 68,
                        Attraction4 = 77,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 10,
                        Day = 6,
                        Description = "第 6 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "金澤日航飯店",
                        Attraction1 = 78,
                        Attraction2 = 71,
                        Attraction3 = 75,
                        Attraction4 = 79,
                        Attraction5 = 67,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 10,
                        Day = 7,
                        Description = "第 7 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "返回溫暖的家",
                        Attraction1 = 80,
                        Attraction2 = 64,
                        Attraction3 = 72,
                        Attraction4 = 66,
                        Attraction5 = 68,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 11,
                        Day = 1,
                        Description = "第 1 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "金澤日航飯店",
                        Attraction1 = 61,
                        Attraction2 = 63,
                        Attraction3 = 77,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 11,
                        Day = 2,
                        Description = "第 2 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "金澤日航飯店",
                        Attraction1 = 79,
                        Attraction2 = 62,
                        Attraction3 = 63,
                        Attraction4 = 74,
                        Attraction5 = 80,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 11,
                        Day = 3,
                        Description = "第 3 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "金澤日航飯店",
                        Attraction1 = 70,
                        Attraction2 = 74,
                        Attraction3 = 77,
                        Attraction4 = 63,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 11,
                        Day = 4,
                        Description = "第 4 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "金澤日航飯店",
                        Attraction1 = 62,
                        Attraction2 = 67,
                        Attraction3 = 63,
                        Attraction4 = 69,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 11,
                        Day = 5,
                        Description = "第 5 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "金澤日航飯店",
                        Attraction1 = 63,
                        Attraction2 = 72,
                        Attraction3 = 80,
                        Attraction4 = 79,
                        Attraction5 = 67,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 11,
                        Day = 6,
                        Description = "第 6 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "金澤日航飯店",
                        Attraction1 = 78,
                        Attraction2 = 71,
                        Attraction3 = 66,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 11,
                        Day = 7,
                        Description = "第 7 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "金澤日航飯店",
                        Attraction1 = 65,
                        Attraction2 = 64,
                        Attraction3 = 74,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 11,
                        Day = 8,
                        Description = "第 8 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "金澤日航飯店",
                        Attraction1 = 74,
                        Attraction2 = 77,
                        Attraction3 = 78,
                        Attraction4 = 66,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 11,
                        Day = 9,
                        Description = "第 9 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "金澤日航飯店",
                        Attraction1 = 63,
                        Attraction2 = 67,
                        Attraction3 = 78,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 11,
                        Day = 10,
                        Description = "第 10 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "返回溫暖的家",
                        Attraction1 = 67,
                        Attraction2 = 76,
                        Attraction3 = 64,
                        Attraction4 = 70,
                        Attraction5 = 78,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 12,
                        Day = 1,
                        Description = "第 1 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "金澤日航飯店",
                        Attraction1 = 67,
                        Attraction2 = 66,
                        Attraction3 = 80,
                        Attraction4 = 73,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 12,
                        Day = 2,
                        Description = "第 2 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "金澤日航飯店",
                        Attraction1 = 71,
                        Attraction2 = 68,
                        Attraction3 = 63,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 12,
                        Day = 3,
                        Description = "第 3 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "金澤日航飯店",
                        Attraction1 = 62,
                        Attraction2 = 80,
                        Attraction3 = 75,
                        Attraction4 = 72,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 12,
                        Day = 4,
                        Description = "第 4 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "金澤日航飯店",
                        Attraction1 = 76,
                        Attraction2 = 73,
                        Attraction3 = 69,
                        Attraction4 = 79,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 12,
                        Day = 5,
                        Description = "第 5 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "金澤日航飯店",
                        Attraction1 = 67,
                        Attraction2 = 70,
                        Attraction3 = 64,
                        Attraction4 = 69,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 12,
                        Day = 6,
                        Description = "第 6 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "金澤日航飯店",
                        Attraction1 = 73,
                        Attraction2 = 61,
                        Attraction3 = 77,
                        Attraction4 = 67,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 12,
                        Day = 7,
                        Description = "第 7 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "金澤日航飯店",
                        Attraction1 = 75,
                        Attraction2 = 67,
                        Attraction3 = 70,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 12,
                        Day = 8,
                        Description = "第 8 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "金澤日航飯店",
                        Attraction1 = 77,
                        Attraction2 = 61,
                        Attraction3 = 70,
                        Attraction4 = 71,
                        Attraction5 = 78,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 12,
                        Day = 9,
                        Description = "第 9 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "金澤日航飯店",
                        Attraction1 = 80,
                        Attraction2 = 74,
                        Attraction3 = 68,
                        Attraction4 = 73,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 12,
                        Day = 10,
                        Description = "第 10 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "返回溫暖的家",
                        Attraction1 = 72,
                        Attraction2 = 63,
                        Attraction3 = 73,
                        Attraction4 = 67,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 13,
                        Day = 1,
                        Description = "第 1 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "京都車站日航飯店",
                        Attraction1 = 97,
                        Attraction2 = 93,
                        Attraction3 = 84,
                        Attraction4 = 91,
                        Attraction5 = 90,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 13,
                        Day = 2,
                        Description = "第 2 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "京都車站日航飯店",
                        Attraction1 = 89,
                        Attraction2 = 85,
                        Attraction3 = 99,
                        Attraction4 = 87,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 13,
                        Day = 3,
                        Description = "第 3 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "京都車站日航飯店",
                        Attraction1 = 92,
                        Attraction2 = 82,
                        Attraction3 = 98,
                        Attraction4 = 94,
                        Attraction5 = 97,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 13,
                        Day = 4,
                        Description = "第 4 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "京都車站日航飯店",
                        Attraction1 = 82,
                        Attraction2 = 93,
                        Attraction3 = 96,
                        Attraction4 = 88,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 13,
                        Day = 5,
                        Description = "第 5 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "京都車站日航飯店",
                        Attraction1 = 81,
                        Attraction2 = 100,
                        Attraction3 = 82,
                        Attraction4 = 96,
                        Attraction5 = 84,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 13,
                        Day = 6,
                        Description = "第 6 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "京都車站日航飯店",
                        Attraction1 = 89,
                        Attraction2 = 98,
                        Attraction3 = 94,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 13,
                        Day = 7,
                        Description = "第 7 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "返回溫暖的家",
                        Attraction1 = 91,
                        Attraction2 = 95,
                        Attraction3 = 99,
                        Attraction4 = 82,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 14,
                        Day = 1,
                        Description = "第 1 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "京都車站日航飯店",
                        Attraction1 = 81,
                        Attraction2 = 87,
                        Attraction3 = 97,
                        Attraction4 = 96,
                        Attraction5 = 86,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 14,
                        Day = 2,
                        Description = "第 2 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "京都車站日航飯店",
                        Attraction1 = 95,
                        Attraction2 = 82,
                        Attraction3 = 81,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 14,
                        Day = 3,
                        Description = "第 3 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "京都車站日航飯店",
                        Attraction1 = 93,
                        Attraction2 = 83,
                        Attraction3 = 98,
                        Attraction4 = 82,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 14,
                        Day = 4,
                        Description = "第 4 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "京都車站日航飯店",
                        Attraction1 = 88,
                        Attraction2 = 100,
                        Attraction3 = 90,
                        Attraction4 = 97,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 14,
                        Day = 5,
                        Description = "第 5 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "京都車站日航飯店",
                        Attraction1 = 88,
                        Attraction2 = 96,
                        Attraction3 = 98,
                        Attraction4 = 100,
                        Attraction5 = 90,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 14,
                        Day = 6,
                        Description = "第 6 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "京都車站日航飯店",
                        Attraction1 = 97,
                        Attraction2 = 83,
                        Attraction3 = 86,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 14,
                        Day = 7,
                        Description = "第 7 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "京都車站日航飯店",
                        Attraction1 = 93,
                        Attraction2 = 87,
                        Attraction3 = 86,
                        Attraction4 = 95,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 14,
                        Day = 8,
                        Description = "第 8 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "京都車站日航飯店",
                        Attraction1 = 89,
                        Attraction2 = 98,
                        Attraction3 = 84,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 14,
                        Day = 9,
                        Description = "第 9 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "京都車站日航飯店",
                        Attraction1 = 85,
                        Attraction2 = 96,
                        Attraction3 = 86,
                        Attraction4 = 97,
                        Attraction5 = 98,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 14,
                        Day = 10,
                        Description = "第 10 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "返回溫暖的家",
                        Attraction1 = 95,
                        Attraction2 = 83,
                        Attraction3 = 97,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 15,
                        Day = 1,
                        Description = "第 1 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "京都車站日航飯店",
                        Attraction1 = 82,
                        Attraction2 = 81,
                        Attraction3 = 94,
                        Attraction4 = 92,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 15,
                        Day = 2,
                        Description = "第 2 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "京都車站日航飯店",
                        Attraction1 = 95,
                        Attraction2 = 84,
                        Attraction3 = 98,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 15,
                        Day = 3,
                        Description = "第 3 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "京都車站日航飯店",
                        Attraction1 = 94,
                        Attraction2 = 90,
                        Attraction3 = 88,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 15,
                        Day = 4,
                        Description = "第 4 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "京都車站日航飯店",
                        Attraction1 = 81,
                        Attraction2 = 90,
                        Attraction3 = 87,
                        Attraction4 = 83,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 15,
                        Day = 5,
                        Description = "第 5 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "京都車站日航飯店",
                        Attraction1 = 99,
                        Attraction2 = 100,
                        Attraction3 = 83,
                        Attraction4 = 81,
                        Attraction5 = 84,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 15,
                        Day = 6,
                        Description = "第 6 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "京都車站日航飯店",
                        Attraction1 = 82,
                        Attraction2 = 96,
                        Attraction3 = 93,
                        Attraction4 = 81,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 15,
                        Day = 7,
                        Description = "第 7 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "京都車站日航飯店",
                        Attraction1 = 94,
                        Attraction2 = 96,
                        Attraction3 = 84,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 15,
                        Day = 8,
                        Description = "第 8 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "京都車站日航飯店",
                        Attraction1 = 88,
                        Attraction2 = 87,
                        Attraction3 = 83,
                        Attraction4 = 82,
                        Attraction5 = 95,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 15,
                        Day = 9,
                        Description = "第 9 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "京都車站日航飯店",
                        Attraction1 = 81,
                        Attraction2 = 86,
                        Attraction3 = 83,
                        Attraction4 = 92,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 15,
                        Day = 10,
                        Description = "第 10 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "返回溫暖的家",
                        Attraction1 = 94,
                        Attraction2 = 89,
                        Attraction3 = 95,
                        Attraction4 = 82,
                        Attraction5 = 99,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 16,
                        Day = 1,
                        Description = "第 1 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "廣島ANA洲際飯店",
                        Attraction1 = 106,
                        Attraction2 = 112,
                        Attraction3 = 118,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 16,
                        Day = 2,
                        Description = "第 2 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "廣島ANA洲際飯店",
                        Attraction1 = 120,
                        Attraction2 = 104,
                        Attraction3 = 115,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 16,
                        Day = 3,
                        Description = "第 3 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "廣島ANA洲際飯店",
                        Attraction1 = 104,
                        Attraction2 = 110,
                        Attraction3 = 102,
                        Attraction4 = 112,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 16,
                        Day = 4,
                        Description = "第 4 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "廣島ANA洲際飯店",
                        Attraction1 = 116,
                        Attraction2 = 109,
                        Attraction3 = 111,
                        Attraction4 = 120,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 16,
                        Day = 5,
                        Description = "第 5 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "廣島ANA洲際飯店",
                        Attraction1 = 103,
                        Attraction2 = 118,
                        Attraction3 = 115,
                        Attraction4 = 113,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 16,
                        Day = 6,
                        Description = "第 6 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "廣島ANA洲際飯店",
                        Attraction1 = 120,
                        Attraction2 = 112,
                        Attraction3 = 111,
                        Attraction4 = 103,
                        Attraction5 = 110,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 16,
                        Day = 7,
                        Description = "第 7 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "返回溫暖的家",
                        Attraction1 = 118,
                        Attraction2 = 115,
                        Attraction3 = 110,
                        Attraction4 = 113,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 17,
                        Day = 1,
                        Description = "第 1 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "廣島ANA洲際飯店",
                        Attraction1 = 116,
                        Attraction2 = 109,
                        Attraction3 = 102,
                        Attraction4 = 114,
                        Attraction5 = 110,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 17,
                        Day = 2,
                        Description = "第 2 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "廣島ANA洲際飯店",
                        Attraction1 = 107,
                        Attraction2 = 110,
                        Attraction3 = 102,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 17,
                        Day = 3,
                        Description = "第 3 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "廣島ANA洲際飯店",
                        Attraction1 = 115,
                        Attraction2 = 120,
                        Attraction3 = 113,
                        Attraction4 = 101,
                        Attraction5 = 119,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 17,
                        Day = 4,
                        Description = "第 4 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "廣島ANA洲際飯店",
                        Attraction1 = 102,
                        Attraction2 = 114,
                        Attraction3 = 101,
                        Attraction4 = 113,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 17,
                        Day = 5,
                        Description = "第 5 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "廣島ANA洲際飯店",
                        Attraction1 = 117,
                        Attraction2 = 109,
                        Attraction3 = 120,
                        Attraction4 = 113,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 17,
                        Day = 6,
                        Description = "第 6 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "廣島ANA洲際飯店",
                        Attraction1 = 114,
                        Attraction2 = 108,
                        Attraction3 = 106,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 17,
                        Day = 7,
                        Description = "第 7 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "廣島ANA洲際飯店",
                        Attraction1 = 120,
                        Attraction2 = 119,
                        Attraction3 = 115,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 17,
                        Day = 8,
                        Description = "第 8 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "廣島ANA洲際飯店",
                        Attraction1 = 120,
                        Attraction2 = 111,
                        Attraction3 = 113,
                        Attraction4 = 118,
                        Attraction5 = 109,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 17,
                        Day = 9,
                        Description = "第 9 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "廣島ANA洲際飯店",
                        Attraction1 = 115,
                        Attraction2 = 105,
                        Attraction3 = 111,
                        Attraction4 = 110,
                        Attraction5 = 119,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 17,
                        Day = 10,
                        Description = "第 10 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "返回溫暖的家",
                        Attraction1 = 103,
                        Attraction2 = 119,
                        Attraction3 = 105,
                        Attraction4 = 111,
                        Attraction5 = 116,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 18,
                        Day = 1,
                        Description = "第 1 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "廣島ANA洲際飯店",
                        Attraction1 = 116,
                        Attraction2 = 109,
                        Attraction3 = 106,
                        Attraction4 = 111,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 18,
                        Day = 2,
                        Description = "第 2 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "廣島ANA洲際飯店",
                        Attraction1 = 118,
                        Attraction2 = 113,
                        Attraction3 = 105,
                        Attraction4 = 106,
                        Attraction5 = 110,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 18,
                        Day = 3,
                        Description = "第 3 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "廣島ANA洲際飯店",
                        Attraction1 = 118,
                        Attraction2 = 104,
                        Attraction3 = 105,
                        Attraction4 = 116,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 18,
                        Day = 4,
                        Description = "第 4 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "廣島ANA洲際飯店",
                        Attraction1 = 103,
                        Attraction2 = 108,
                        Attraction3 = 110,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 18,
                        Day = 5,
                        Description = "第 5 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "廣島ANA洲際飯店",
                        Attraction1 = 116,
                        Attraction2 = 112,
                        Attraction3 = 107,
                        Attraction4 = 118,
                        Attraction5 = 119,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 18,
                        Day = 6,
                        Description = "第 6 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "廣島ANA洲際飯店",
                        Attraction1 = 120,
                        Attraction2 = 116,
                        Attraction3 = 107,
                        Attraction4 = 104,
                        Attraction5 = 109,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 18,
                        Day = 7,
                        Description = "第 7 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "廣島ANA洲際飯店",
                        Attraction1 = 112,
                        Attraction2 = 104,
                        Attraction3 = 109,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 18,
                        Day = 8,
                        Description = "第 8 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "廣島ANA洲際飯店",
                        Attraction1 = 102,
                        Attraction2 = 117,
                        Attraction3 = 115,
                        Attraction4 = 113,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 18,
                        Day = 9,
                        Description = "第 9 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "廣島ANA洲際飯店",
                        Attraction1 = 107,
                        Attraction2 = 113,
                        Attraction3 = 108,
                        Attraction4 = 120,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 18,
                        Day = 10,
                        Description = "第 10 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "返回溫暖的家",
                        Attraction1 = 101,
                        Attraction2 = 117,
                        Attraction3 = 107,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 19,
                        Day = 1,
                        Description = "第 1 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "高松車站前商務飯店",
                        Attraction1 = 122,
                        Attraction2 = 130,
                        Attraction3 = 140,
                        Attraction4 = 125,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 19,
                        Day = 2,
                        Description = "第 2 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "高松車站前商務飯店",
                        Attraction1 = 140,
                        Attraction2 = 137,
                        Attraction3 = 133,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 19,
                        Day = 3,
                        Description = "第 3 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "高松車站前商務飯店",
                        Attraction1 = 123,
                        Attraction2 = 140,
                        Attraction3 = 121,
                        Attraction4 = 139,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 19,
                        Day = 4,
                        Description = "第 4 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "高松車站前商務飯店",
                        Attraction1 = 123,
                        Attraction2 = 133,
                        Attraction3 = 121,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 19,
                        Day = 5,
                        Description = "第 5 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "高松車站前商務飯店",
                        Attraction1 = 133,
                        Attraction2 = 135,
                        Attraction3 = 121,
                        Attraction4 = 140,
                        Attraction5 = 122,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 19,
                        Day = 6,
                        Description = "第 6 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "高松車站前商務飯店",
                        Attraction1 = 132,
                        Attraction2 = 125,
                        Attraction3 = 130,
                        Attraction4 = 124,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 19,
                        Day = 7,
                        Description = "第 7 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "返回溫暖的家",
                        Attraction1 = 132,
                        Attraction2 = 121,
                        Attraction3 = 134,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 20,
                        Day = 1,
                        Description = "第 1 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "高松車站前商務飯店",
                        Attraction1 = 133,
                        Attraction2 = 130,
                        Attraction3 = 124,
                        Attraction4 = 129,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 20,
                        Day = 2,
                        Description = "第 2 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "高松車站前商務飯店",
                        Attraction1 = 135,
                        Attraction2 = 123,
                        Attraction3 = 130,
                        Attraction4 = 139,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 20,
                        Day = 3,
                        Description = "第 3 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "高松車站前商務飯店",
                        Attraction1 = 132,
                        Attraction2 = 135,
                        Attraction3 = 137,
                        Attraction4 = 126,
                        Attraction5 = 134,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 20,
                        Day = 4,
                        Description = "第 4 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "高松車站前商務飯店",
                        Attraction1 = 130,
                        Attraction2 = 139,
                        Attraction3 = 124,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 20,
                        Day = 5,
                        Description = "第 5 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "高松車站前商務飯店",
                        Attraction1 = 137,
                        Attraction2 = 126,
                        Attraction3 = 125,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 20,
                        Day = 6,
                        Description = "第 6 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "高松車站前商務飯店",
                        Attraction1 = 130,
                        Attraction2 = 124,
                        Attraction3 = 129,
                        Attraction4 = 136,
                        Attraction5 = 140,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 20,
                        Day = 7,
                        Description = "第 7 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "高松車站前商務飯店",
                        Attraction1 = 130,
                        Attraction2 = 127,
                        Attraction3 = 121,
                        Attraction4 = 136,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 20,
                        Day = 8,
                        Description = "第 8 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "高松車站前商務飯店",
                        Attraction1 = 136,
                        Attraction2 = 122,
                        Attraction3 = 121,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 20,
                        Day = 9,
                        Description = "第 9 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "高松車站前商務飯店",
                        Attraction1 = 139,
                        Attraction2 = 130,
                        Attraction3 = 129,
                        Attraction4 = 125,
                        Attraction5 = 140,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 20,
                        Day = 10,
                        Description = "第 10 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "返回溫暖的家",
                        Attraction1 = 134,
                        Attraction2 = 122,
                        Attraction3 = 139,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 21,
                        Day = 1,
                        Description = "第 1 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "高松車站前商務飯店",
                        Attraction1 = 126,
                        Attraction2 = 121,
                        Attraction3 = 130,
                        Attraction4 = 125,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 21,
                        Day = 2,
                        Description = "第 2 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "高松車站前商務飯店",
                        Attraction1 = 125,
                        Attraction2 = 128,
                        Attraction3 = 138,
                        Attraction4 = 130,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 21,
                        Day = 3,
                        Description = "第 3 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "高松車站前商務飯店",
                        Attraction1 = 122,
                        Attraction2 = 140,
                        Attraction3 = 133,
                        Attraction4 = 125,
                        Attraction5 = 131,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 21,
                        Day = 4,
                        Description = "第 4 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "高松車站前商務飯店",
                        Attraction1 = 121,
                        Attraction2 = 130,
                        Attraction3 = 125,
                        Attraction4 = 132,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 21,
                        Day = 5,
                        Description = "第 5 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "高松車站前商務飯店",
                        Attraction1 = 124,
                        Attraction2 = 133,
                        Attraction3 = 140,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 21,
                        Day = 6,
                        Description = "第 6 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "高松車站前商務飯店",
                        Attraction1 = 130,
                        Attraction2 = 128,
                        Attraction3 = 137,
                        Attraction4 = 139,
                        Attraction5 = 136,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 21,
                        Day = 7,
                        Description = "第 7 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "高松車站前商務飯店",
                        Attraction1 = 134,
                        Attraction2 = 121,
                        Attraction3 = 139,
                        Attraction4 = 124,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 21,
                        Day = 8,
                        Description = "第 8 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "高松車站前商務飯店",
                        Attraction1 = 136,
                        Attraction2 = 122,
                        Attraction3 = 129,
                        Attraction4 = 139,
                        Attraction5 = 128,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 21,
                        Day = 9,
                        Description = "第 9 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "高松車站前商務飯店",
                        Attraction1 = 130,
                        Attraction2 = 126,
                        Attraction3 = 134,
                        Attraction4 = 128,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 21,
                        Day = 10,
                        Description = "第 10 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "返回溫暖的家",
                        Attraction1 = 130,
                        Attraction2 = 140,
                        Attraction3 = 128,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 22,
                        Day = 1,
                        Description = "第 1 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "福岡天神東橫INN",
                        Attraction1 = 146,
                        Attraction2 = 153,
                        Attraction3 = 160,
                        Attraction4 = 145,
                        Attraction5 = 154,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 22,
                        Day = 2,
                        Description = "第 2 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "福岡天神東橫INN",
                        Attraction1 = 157,
                        Attraction2 = 147,
                        Attraction3 = 152,
                        Attraction4 = 151,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 22,
                        Day = 3,
                        Description = "第 3 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "福岡天神東橫INN",
                        Attraction1 = 141,
                        Attraction2 = 142,
                        Attraction3 = 152,
                        Attraction4 = 150,
                        Attraction5 = 148,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 22,
                        Day = 4,
                        Description = "第 4 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "福岡天神東橫INN",
                        Attraction1 = 159,
                        Attraction2 = 150,
                        Attraction3 = 160,
                        Attraction4 = 158,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 22,
                        Day = 5,
                        Description = "第 5 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "福岡天神東橫INN",
                        Attraction1 = 151,
                        Attraction2 = 145,
                        Attraction3 = 152,
                        Attraction4 = 158,
                        Attraction5 = 147,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 22,
                        Day = 6,
                        Description = "第 6 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "福岡天神東橫INN",
                        Attraction1 = 156,
                        Attraction2 = 160,
                        Attraction3 = 145,
                        Attraction4 = 151,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 22,
                        Day = 7,
                        Description = "第 7 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "返回溫暖的家",
                        Attraction1 = 157,
                        Attraction2 = 141,
                        Attraction3 = 151,
                        Attraction4 = 152,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 23,
                        Day = 1,
                        Description = "第 1 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "福岡天神東橫INN",
                        Attraction1 = 154,
                        Attraction2 = 157,
                        Attraction3 = 143,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 23,
                        Day = 2,
                        Description = "第 2 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "福岡天神東橫INN",
                        Attraction1 = 157,
                        Attraction2 = 155,
                        Attraction3 = 156,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 23,
                        Day = 3,
                        Description = "第 3 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "福岡天神東橫INN",
                        Attraction1 = 153,
                        Attraction2 = 158,
                        Attraction3 = 146,
                        Attraction4 = 148,
                        Attraction5 = 143,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 23,
                        Day = 4,
                        Description = "第 4 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "福岡天神東橫INN",
                        Attraction1 = 153,
                        Attraction2 = 149,
                        Attraction3 = 143,
                        Attraction4 = 148,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 23,
                        Day = 5,
                        Description = "第 5 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "福岡天神東橫INN",
                        Attraction1 = 160,
                        Attraction2 = 146,
                        Attraction3 = 158,
                        Attraction4 = 151,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 23,
                        Day = 6,
                        Description = "第 6 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "福岡天神東橫INN",
                        Attraction1 = 157,
                        Attraction2 = 148,
                        Attraction3 = 141,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 23,
                        Day = 7,
                        Description = "第 7 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "福岡天神東橫INN",
                        Attraction1 = 158,
                        Attraction2 = 152,
                        Attraction3 = 160,
                        Attraction4 = 141,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 23,
                        Day = 8,
                        Description = "第 8 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "福岡天神東橫INN",
                        Attraction1 = 148,
                        Attraction2 = 152,
                        Attraction3 = 158,
                        Attraction4 = 160,
                        Attraction5 = 151,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 23,
                        Day = 9,
                        Description = "第 9 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "福岡天神東橫INN",
                        Attraction1 = 159,
                        Attraction2 = 145,
                        Attraction3 = 146,
                        Attraction4 = 141,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 23,
                        Day = 10,
                        Description = "第 10 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "返回溫暖的家",
                        Attraction1 = 160,
                        Attraction2 = 142,
                        Attraction3 = 154,
                        Attraction4 = 144,
                        Attraction5 = 158,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 24,
                        Day = 1,
                        Description = "第 1 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "福岡天神東橫INN",
                        Attraction1 = 144,
                        Attraction2 = 159,
                        Attraction3 = 147,
                        Attraction4 = 158,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 24,
                        Day = 2,
                        Description = "第 2 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "福岡天神東橫INN",
                        Attraction1 = 146,
                        Attraction2 = 148,
                        Attraction3 = 152,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 24,
                        Day = 3,
                        Description = "第 3 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "福岡天神東橫INN",
                        Attraction1 = 149,
                        Attraction2 = 151,
                        Attraction3 = 147,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 24,
                        Day = 4,
                        Description = "第 4 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "福岡天神東橫INN",
                        Attraction1 = 144,
                        Attraction2 = 152,
                        Attraction3 = 145,
                        Attraction4 = 148,
                        Attraction5 = 156,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 24,
                        Day = 5,
                        Description = "第 5 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "福岡天神東橫INN",
                        Attraction1 = 150,
                        Attraction2 = 146,
                        Attraction3 = 141,
                        Attraction4 = 155,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 24,
                        Day = 6,
                        Description = "第 6 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "福岡天神東橫INN",
                        Attraction1 = 156,
                        Attraction2 = 151,
                        Attraction3 = 144,
                        Attraction4 = 143,
                        Attraction5 = 158,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 24,
                        Day = 7,
                        Description = "第 7 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "福岡天神東橫INN",
                        Attraction1 = 144,
                        Attraction2 = 150,
                        Attraction3 = 158,
                        Attraction4 = 141,
                        Attraction5 = 145,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 24,
                        Day = 8,
                        Description = "第 8 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "福岡天神東橫INN",
                        Attraction1 = 150,
                        Attraction2 = 147,
                        Attraction3 = 148,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 24,
                        Day = 9,
                        Description = "第 9 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "福岡天神東橫INN",
                        Attraction1 = 150,
                        Attraction2 = 155,
                        Attraction3 = 160,
                        Attraction4 = 157,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 24,
                        Day = 10,
                        Description = "第 10 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "返回溫暖的家",
                        Attraction1 = 144,
                        Attraction2 = 143,
                        Attraction3 = 159,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 25,
                        Day = 1,
                        Description = "第 1 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "那霸國際通飯店",
                        Attraction1 = 170,
                        Attraction2 = 162,
                        Attraction3 = 177,
                        Attraction4 = 169,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 25,
                        Day = 2,
                        Description = "第 2 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "那霸國際通飯店",
                        Attraction1 = 162,
                        Attraction2 = 167,
                        Attraction3 = 170,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 25,
                        Day = 3,
                        Description = "第 3 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "那霸國際通飯店",
                        Attraction1 = 170,
                        Attraction2 = 172,
                        Attraction3 = 173,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 25,
                        Day = 4,
                        Description = "第 4 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "那霸國際通飯店",
                        Attraction1 = 162,
                        Attraction2 = 173,
                        Attraction3 = 180,
                        Attraction4 = 167,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 25,
                        Day = 5,
                        Description = "第 5 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "那霸國際通飯店",
                        Attraction1 = 172,
                        Attraction2 = 169,
                        Attraction3 = 164,
                        Attraction4 = 175,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 25,
                        Day = 6,
                        Description = "第 6 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "那霸國際通飯店",
                        Attraction1 = 170,
                        Attraction2 = 162,
                        Attraction3 = 177,
                        Attraction4 = 164,
                        Attraction5 = 168,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 25,
                        Day = 7,
                        Description = "第 7 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "返回溫暖的家",
                        Attraction1 = 180,
                        Attraction2 = 179,
                        Attraction3 = 165,
                        Attraction4 = 169,
                        Attraction5 = 178,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 26,
                        Day = 1,
                        Description = "第 1 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "那霸國際通飯店",
                        Attraction1 = 162,
                        Attraction2 = 172,
                        Attraction3 = 176,
                        Attraction4 = 175,
                        Attraction5 = 161,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 26,
                        Day = 2,
                        Description = "第 2 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "那霸國際通飯店",
                        Attraction1 = 166,
                        Attraction2 = 175,
                        Attraction3 = 164,
                        Attraction4 = 163,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 26,
                        Day = 3,
                        Description = "第 3 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "那霸國際通飯店",
                        Attraction1 = 178,
                        Attraction2 = 173,
                        Attraction3 = 161,
                        Attraction4 = 164,
                        Attraction5 = 162,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 26,
                        Day = 4,
                        Description = "第 4 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "那霸國際通飯店",
                        Attraction1 = 180,
                        Attraction2 = 169,
                        Attraction3 = 170,
                        Attraction4 = 167,
                        Attraction5 = 173,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 26,
                        Day = 5,
                        Description = "第 5 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "那霸國際通飯店",
                        Attraction1 = 163,
                        Attraction2 = 175,
                        Attraction3 = 176,
                        Attraction4 = 177,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 26,
                        Day = 6,
                        Description = "第 6 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "那霸國際通飯店",
                        Attraction1 = 167,
                        Attraction2 = 165,
                        Attraction3 = 162,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 26,
                        Day = 7,
                        Description = "第 7 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "那霸國際通飯店",
                        Attraction1 = 177,
                        Attraction2 = 175,
                        Attraction3 = 162,
                        Attraction4 = 164,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 26,
                        Day = 8,
                        Description = "第 8 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "那霸國際通飯店",
                        Attraction1 = 170,
                        Attraction2 = 164,
                        Attraction3 = 176,
                        Attraction4 = 172,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 26,
                        Day = 9,
                        Description = "第 9 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "那霸國際通飯店",
                        Attraction1 = 180,
                        Attraction2 = 166,
                        Attraction3 = 174,
                        Attraction4 = 169,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 26,
                        Day = 10,
                        Description = "第 10 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "返回溫暖的家",
                        Attraction1 = 169,
                        Attraction2 = 167,
                        Attraction3 = 173,
                        Attraction4 = 170,
                        Attraction5 = 168,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 27,
                        Day = 1,
                        Description = "第 1 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "那霸國際通飯店",
                        Attraction1 = 162,
                        Attraction2 = 179,
                        Attraction3 = 172,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 27,
                        Day = 2,
                        Description = "第 2 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "那霸國際通飯店",
                        Attraction1 = 171,
                        Attraction2 = 172,
                        Attraction3 = 175,
                        Attraction4 = 164,
                        Attraction5 = 180,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 27,
                        Day = 3,
                        Description = "第 3 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "那霸國際通飯店",
                        Attraction1 = 163,
                        Attraction2 = 174,
                        Attraction3 = 164,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 27,
                        Day = 4,
                        Description = "第 4 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "那霸國際通飯店",
                        Attraction1 = 176,
                        Attraction2 = 163,
                        Attraction3 = 175,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 27,
                        Day = 5,
                        Description = "第 5 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "那霸國際通飯店",
                        Attraction1 = 166,
                        Attraction2 = 180,
                        Attraction3 = 175,
                        Attraction4 = 164,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 27,
                        Day = 6,
                        Description = "第 6 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "那霸國際通飯店",
                        Attraction1 = 175,
                        Attraction2 = 168,
                        Attraction3 = 163,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 27,
                        Day = 7,
                        Description = "第 7 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "那霸國際通飯店",
                        Attraction1 = 164,
                        Attraction2 = 170,
                        Attraction3 = 179,
                        Attraction4 = 176,
                        Attraction5 = 163,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 27,
                        Day = 8,
                        Description = "第 8 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "那霸國際通飯店",
                        Attraction1 = 171,
                        Attraction2 = 170,
                        Attraction3 = 178,
                        Attraction4 = 165,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 27,
                        Day = 9,
                        Description = "第 9 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "那霸國際通飯店",
                        Attraction1 = 163,
                        Attraction2 = 170,
                        Attraction3 = 168,
                        Attraction4 = 171,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 27,
                        Day = 10,
                        Description = "第 10 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "返回溫暖的家",
                        Attraction1 = 173,
                        Attraction2 = 167,
                        Attraction3 = 161,
                        Attraction4 = 178,
                        Attraction5 = 177,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 28,
                        Day = 1,
                        Description = "第 1 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "台北君悅酒店",
                        Attraction1 = 181,
                        Attraction2 = 191,
                        Attraction3 = 184,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 28,
                        Day = 2,
                        Description = "第 2 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "台北君悅酒店",
                        Attraction1 = 190,
                        Attraction2 = 193,
                        Attraction3 = 192,
                        Attraction4 = 189,
                        Attraction5 = 186,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 28,
                        Day = 3,
                        Description = "第 3 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "返回溫暖的家",
                        Attraction1 = 188,
                        Attraction2 = 197,
                        Attraction3 = 195,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 29,
                        Day = 1,
                        Description = "第 1 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "台北君悅酒店",
                        Attraction1 = 195,
                        Attraction2 = 192,
                        Attraction3 = 181,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 29,
                        Day = 2,
                        Description = "第 2 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "返回溫暖的家",
                        Attraction1 = 185,
                        Attraction2 = 199,
                        Attraction3 = 188,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 30,
                        Day = 1,
                        Description = "第 1 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "台北君悅酒店",
                        Attraction1 = 198,
                        Attraction2 = 189,
                        Attraction3 = 183,
                        Attraction4 = 195,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 30,
                        Day = 2,
                        Description = "第 2 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "返回溫暖的家",
                        Attraction1 = 187,
                        Attraction2 = 189,
                        Attraction3 = 196,
                        Attraction4 = 195,
                        Attraction5 = 191,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 31,
                        Day = 1,
                        Description = "第 1 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "日月潭涵碧樓酒店",
                        Attraction1 = 202,
                        Attraction2 = 217,
                        Attraction3 = 216,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 31,
                        Day = 2,
                        Description = "第 2 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "日月潭涵碧樓酒店",
                        Attraction1 = 212,
                        Attraction2 = 201,
                        Attraction3 = 211,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 31,
                        Day = 3,
                        Description = "第 3 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "返回溫暖的家",
                        Attraction1 = 207,
                        Attraction2 = 214,
                        Attraction3 = 217,
                        Attraction4 = 220,
                        Attraction5 = 205,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 32,
                        Day = 1,
                        Description = "第 1 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "日月潭涵碧樓酒店",
                        Attraction1 = 220,
                        Attraction2 = 216,
                        Attraction3 = 205,
                        Attraction4 = 201,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 32,
                        Day = 2,
                        Description = "第 2 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "返回溫暖的家",
                        Attraction1 = 218,
                        Attraction2 = 220,
                        Attraction3 = 210,
                        Attraction4 = 207,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 33,
                        Day = 1,
                        Description = "第 1 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "日月潭涵碧樓酒店",
                        Attraction1 = 201,
                        Attraction2 = 215,
                        Attraction3 = 216,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 33,
                        Day = 2,
                        Description = "第 2 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "返回溫暖的家",
                        Attraction1 = 211,
                        Attraction2 = 209,
                        Attraction3 = 220,
                        Attraction4 = 218,
                        Attraction5 = 217,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 34,
                        Day = 1,
                        Description = "第 1 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "高雄漢來大飯店",
                        Attraction1 = 225,
                        Attraction2 = 227,
                        Attraction3 = 229,
                        Attraction4 = 222,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 34,
                        Day = 2,
                        Description = "第 2 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "高雄漢來大飯店",
                        Attraction1 = 233,
                        Attraction2 = 229,
                        Attraction3 = 227,
                        Attraction4 = 230,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 34,
                        Day = 3,
                        Description = "第 3 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "返回溫暖的家",
                        Attraction1 = 230,
                        Attraction2 = 239,
                        Attraction3 = 237,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 35,
                        Day = 1,
                        Description = "第 1 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "高雄漢來大飯店",
                        Attraction1 = 228,
                        Attraction2 = 232,
                        Attraction3 = 240,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 35,
                        Day = 2,
                        Description = "第 2 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "返回溫暖的家",
                        Attraction1 = 232,
                        Attraction2 = 233,
                        Attraction3 = 236,
                        Attraction4 = 229,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 36,
                        Day = 1,
                        Description = "第 1 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "高雄漢來大飯店",
                        Attraction1 = 234,
                        Attraction2 = 223,
                        Attraction3 = 232,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 36,
                        Day = 2,
                        Description = "第 2 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "返回溫暖的家",
                        Attraction1 = 232,
                        Attraction2 = 225,
                        Attraction3 = 223,
                        Attraction4 = 231,
                        Attraction5 = 221,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 37,
                        Day = 1,
                        Description = "第 1 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "花蓮遠雄悅來大飯店",
                        Attraction1 = 245,
                        Attraction2 = 257,
                        Attraction3 = 252,
                        Attraction4 = 255,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 37,
                        Day = 2,
                        Description = "第 2 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "花蓮遠雄悅來大飯店",
                        Attraction1 = 253,
                        Attraction2 = 245,
                        Attraction3 = 248,
                        Attraction4 = 256,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 37,
                        Day = 3,
                        Description = "第 3 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "返回溫暖的家",
                        Attraction1 = 256,
                        Attraction2 = 254,
                        Attraction3 = 246,
                        Attraction4 = 260,
                        Attraction5 = 259,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 38,
                        Day = 1,
                        Description = "第 1 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "花蓮遠雄悅來大飯店",
                        Attraction1 = 245,
                        Attraction2 = 241,
                        Attraction3 = 246,
                        Attraction4 = 256,
                        Attraction5 = 250,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 38,
                        Day = 2,
                        Description = "第 2 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "返回溫暖的家",
                        Attraction1 = 259,
                        Attraction2 = 258,
                        Attraction3 = 255,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 39,
                        Day = 1,
                        Description = "第 1 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "花蓮遠雄悅來大飯店",
                        Attraction1 = 244,
                        Attraction2 = 257,
                        Attraction3 = 260,
                        Attraction4 = 255,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 39,
                        Day = 2,
                        Description = "第 2 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "返回溫暖的家",
                        Attraction1 = 252,
                        Attraction2 = 249,
                        Attraction3 = 255,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 40,
                        Day = 1,
                        Description = "第 1 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "澎湖福朋喜來登飯店",
                        Attraction1 = 272,
                        Attraction2 = 268,
                        Attraction3 = 277,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 40,
                        Day = 2,
                        Description = "第 2 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "澎湖福朋喜來登飯店",
                        Attraction1 = 275,
                        Attraction2 = 261,
                        Attraction3 = 272,
                        Attraction4 = 270,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 40,
                        Day = 3,
                        Description = "第 3 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "返回溫暖的家",
                        Attraction1 = 266,
                        Attraction2 = 278,
                        Attraction3 = 265,
                        Attraction4 = 271,
                        Attraction5 = 272,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 41,
                        Day = 1,
                        Description = "第 1 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "澎湖福朋喜來登飯店",
                        Attraction1 = 262,
                        Attraction2 = 267,
                        Attraction3 = 270,
                        Attraction4 = 280,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 41,
                        Day = 2,
                        Description = "第 2 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "返回溫暖的家",
                        Attraction1 = 278,
                        Attraction2 = 276,
                        Attraction3 = 265,
                        Attraction4 = 280,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 42,
                        Day = 1,
                        Description = "第 1 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "澎湖福朋喜來登飯店",
                        Attraction1 = 266,
                        Attraction2 = 270,
                        Attraction3 = 279,
                        Attraction4 = null,
                        Attraction5 = null,
                        Note1 = "",
                        Note2 = ""
                    },

                    new OfficialTravelSchedule
                    {
                        OfficialTravelDetailId = 42,
                        Day = 2,
                        Description = "第 2 天行程包含文化與自然景點參訪，享受當地風情。",
                        Breakfast = "飯店內自助早餐",
                        Lunch = "在當地享用特色餐",
                        Dinner = "在當地享用特色餐",
                        Hotel = "返回溫暖的家",
                        Attraction1 = 274,
                        Attraction2 = 265,
                        Attraction3 = 262,
                        Attraction4 = 263,
                        Attraction5 = 277,
                        Note1 = "",
                        Note2 = ""
                    }
                                        );

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
                _context.GroupTravels.AddRange(
                    new GroupTravel
                    {
                        OfficialTravelDetailId = 1,
                        DepartureDate = new DateTime(2025, 5, 25),
                        ReturnDate = new DateTime(2025, 5, 31),
                        TotalSeats = 31,
                        SoldSeats = 8,
                        OrderDeadline = new DateTime(2025, 5, 15),
                        MinimumParticipants = 16,
                        GroupStatus = "不可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 1,
                        DepartureDate = new DateTime(2025, 5, 31),
                        ReturnDate = new DateTime(2025, 6, 6),
                        TotalSeats = 18,
                        SoldSeats = 18,
                        OrderDeadline = new DateTime(2025, 5, 21),
                        MinimumParticipants = 16,
                        GroupStatus = "不可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 1,
                        DepartureDate = new DateTime(2025, 6, 6),
                        ReturnDate = new DateTime(2025, 6, 12),
                        TotalSeats = 32,
                        SoldSeats = 18,
                        OrderDeadline = new DateTime(2025, 5, 27),
                        MinimumParticipants = 16,
                        GroupStatus = "聯絡專人",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 1,
                        DepartureDate = new DateTime(2025, 6, 12),
                        ReturnDate = new DateTime(2025, 6, 18),
                        TotalSeats = 21,
                        SoldSeats = 17,
                        OrderDeadline = new DateTime(2025, 6, 2),
                        MinimumParticipants = 16,
                        GroupStatus = "聯絡專人",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 1,
                        DepartureDate = new DateTime(2025, 6, 18),
                        ReturnDate = new DateTime(2025, 6, 24),
                        TotalSeats = 26,
                        SoldSeats = 10,
                        OrderDeadline = new DateTime(2025, 6, 8),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 1,
                        DepartureDate = new DateTime(2025, 6, 24),
                        ReturnDate = new DateTime(2025, 6, 30),
                        TotalSeats = 20,
                        SoldSeats = 10,
                        OrderDeadline = new DateTime(2025, 6, 14),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 1,
                        DepartureDate = new DateTime(2025, 6, 30),
                        ReturnDate = new DateTime(2025, 7, 6),
                        TotalSeats = 16,
                        SoldSeats = 0,
                        OrderDeadline = new DateTime(2025, 6, 20),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 2,
                        DepartureDate = new DateTime(2025, 5, 28),
                        ReturnDate = new DateTime(2025, 6, 6),
                        TotalSeats = 18,
                        SoldSeats = 5,
                        OrderDeadline = new DateTime(2025, 5, 18),
                        MinimumParticipants = 16,
                        GroupStatus = "不可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 2,
                        DepartureDate = new DateTime(2025, 6, 3),
                        ReturnDate = new DateTime(2025, 6, 12),
                        TotalSeats = 17,
                        SoldSeats = 12,
                        OrderDeadline = new DateTime(2025, 5, 24),
                        MinimumParticipants = 16,
                        GroupStatus = "不可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 2,
                        DepartureDate = new DateTime(2025, 6, 9),
                        ReturnDate = new DateTime(2025, 6, 18),
                        TotalSeats = 18,
                        SoldSeats = 1,
                        OrderDeadline = new DateTime(2025, 5, 30),
                        MinimumParticipants = 16,
                        GroupStatus = "聯絡專人",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 2,
                        DepartureDate = new DateTime(2025, 6, 15),
                        ReturnDate = new DateTime(2025, 6, 24),
                        TotalSeats = 25,
                        SoldSeats = 18,
                        OrderDeadline = new DateTime(2025, 6, 5),
                        MinimumParticipants = 16,
                        GroupStatus = "聯絡專人",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 3,
                        DepartureDate = new DateTime(2025, 5, 28),
                        ReturnDate = new DateTime(2025, 6, 6),
                        TotalSeats = 29,
                        SoldSeats = 25,
                        OrderDeadline = new DateTime(2025, 5, 18),
                        MinimumParticipants = 16,
                        GroupStatus = "不可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 3,
                        DepartureDate = new DateTime(2025, 6, 3),
                        ReturnDate = new DateTime(2025, 6, 12),
                        TotalSeats = 21,
                        SoldSeats = 14,
                        OrderDeadline = new DateTime(2025, 5, 24),
                        MinimumParticipants = 16,
                        GroupStatus = "不可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 3,
                        DepartureDate = new DateTime(2025, 6, 9),
                        ReturnDate = new DateTime(2025, 6, 18),
                        TotalSeats = 27,
                        SoldSeats = 27,
                        OrderDeadline = new DateTime(2025, 5, 30),
                        MinimumParticipants = 16,
                        GroupStatus = "聯絡專人",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 3,
                        DepartureDate = new DateTime(2025, 6, 15),
                        ReturnDate = new DateTime(2025, 6, 24),
                        TotalSeats = 16,
                        SoldSeats = 10,
                        OrderDeadline = new DateTime(2025, 6, 5),
                        MinimumParticipants = 16,
                        GroupStatus = "聯絡專人",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 4,
                        DepartureDate = new DateTime(2025, 5, 31),
                        ReturnDate = new DateTime(2025, 6, 6),
                        TotalSeats = 28,
                        SoldSeats = 4,
                        OrderDeadline = new DateTime(2025, 5, 21),
                        MinimumParticipants = 16,
                        GroupStatus = "不可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 4,
                        DepartureDate = new DateTime(2025, 6, 6),
                        ReturnDate = new DateTime(2025, 6, 12),
                        TotalSeats = 16,
                        SoldSeats = 8,
                        OrderDeadline = new DateTime(2025, 5, 27),
                        MinimumParticipants = 16,
                        GroupStatus = "聯絡專人",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 4,
                        DepartureDate = new DateTime(2025, 6, 12),
                        ReturnDate = new DateTime(2025, 6, 18),
                        TotalSeats = 28,
                        SoldSeats = 19,
                        OrderDeadline = new DateTime(2025, 6, 2),
                        MinimumParticipants = 16,
                        GroupStatus = "聯絡專人",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 4,
                        DepartureDate = new DateTime(2025, 6, 18),
                        ReturnDate = new DateTime(2025, 6, 24),
                        TotalSeats = 25,
                        SoldSeats = 13,
                        OrderDeadline = new DateTime(2025, 6, 8),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 4,
                        DepartureDate = new DateTime(2025, 6, 24),
                        ReturnDate = new DateTime(2025, 6, 30),
                        TotalSeats = 22,
                        SoldSeats = 16,
                        OrderDeadline = new DateTime(2025, 6, 14),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 4,
                        DepartureDate = new DateTime(2025, 6, 30),
                        ReturnDate = new DateTime(2025, 7, 6),
                        TotalSeats = 26,
                        SoldSeats = 14,
                        OrderDeadline = new DateTime(2025, 6, 20),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 4,
                        DepartureDate = new DateTime(2025, 7, 6),
                        ReturnDate = new DateTime(2025, 7, 12),
                        TotalSeats = 17,
                        SoldSeats = 13,
                        OrderDeadline = new DateTime(2025, 6, 26),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 5,
                        DepartureDate = new DateTime(2025, 6, 3),
                        ReturnDate = new DateTime(2025, 6, 12),
                        TotalSeats = 21,
                        SoldSeats = 3,
                        OrderDeadline = new DateTime(2025, 5, 24),
                        MinimumParticipants = 16,
                        GroupStatus = "不可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 5,
                        DepartureDate = new DateTime(2025, 6, 9),
                        ReturnDate = new DateTime(2025, 6, 18),
                        TotalSeats = 19,
                        SoldSeats = 9,
                        OrderDeadline = new DateTime(2025, 5, 30),
                        MinimumParticipants = 16,
                        GroupStatus = "聯絡專人",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 5,
                        DepartureDate = new DateTime(2025, 6, 15),
                        ReturnDate = new DateTime(2025, 6, 24),
                        TotalSeats = 20,
                        SoldSeats = 13,
                        OrderDeadline = new DateTime(2025, 6, 5),
                        MinimumParticipants = 16,
                        GroupStatus = "聯絡專人",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 5,
                        DepartureDate = new DateTime(2025, 6, 21),
                        ReturnDate = new DateTime(2025, 6, 30),
                        TotalSeats = 30,
                        SoldSeats = 6,
                        OrderDeadline = new DateTime(2025, 6, 11),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 6,
                        DepartureDate = new DateTime(2025, 6, 3),
                        ReturnDate = new DateTime(2025, 6, 12),
                        TotalSeats = 29,
                        SoldSeats = 20,
                        OrderDeadline = new DateTime(2025, 5, 24),
                        MinimumParticipants = 16,
                        GroupStatus = "不可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 6,
                        DepartureDate = new DateTime(2025, 6, 9),
                        ReturnDate = new DateTime(2025, 6, 18),
                        TotalSeats = 26,
                        SoldSeats = 0,
                        OrderDeadline = new DateTime(2025, 5, 30),
                        MinimumParticipants = 16,
                        GroupStatus = "聯絡專人",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 6,
                        DepartureDate = new DateTime(2025, 6, 15),
                        ReturnDate = new DateTime(2025, 6, 24),
                        TotalSeats = 25,
                        SoldSeats = 2,
                        OrderDeadline = new DateTime(2025, 6, 5),
                        MinimumParticipants = 16,
                        GroupStatus = "聯絡專人",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 6,
                        DepartureDate = new DateTime(2025, 6, 21),
                        ReturnDate = new DateTime(2025, 6, 30),
                        TotalSeats = 25,
                        SoldSeats = 6,
                        OrderDeadline = new DateTime(2025, 6, 11),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 7,
                        DepartureDate = new DateTime(2025, 6, 6),
                        ReturnDate = new DateTime(2025, 6, 12),
                        TotalSeats = 22,
                        SoldSeats = 5,
                        OrderDeadline = new DateTime(2025, 5, 27),
                        MinimumParticipants = 16,
                        GroupStatus = "聯絡專人",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 7,
                        DepartureDate = new DateTime(2025, 6, 12),
                        ReturnDate = new DateTime(2025, 6, 18),
                        TotalSeats = 20,
                        SoldSeats = 10,
                        OrderDeadline = new DateTime(2025, 6, 2),
                        MinimumParticipants = 16,
                        GroupStatus = "聯絡專人",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 7,
                        DepartureDate = new DateTime(2025, 6, 18),
                        ReturnDate = new DateTime(2025, 6, 24),
                        TotalSeats = 25,
                        SoldSeats = 23,
                        OrderDeadline = new DateTime(2025, 6, 8),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 7,
                        DepartureDate = new DateTime(2025, 6, 24),
                        ReturnDate = new DateTime(2025, 6, 30),
                        TotalSeats = 32,
                        SoldSeats = 24,
                        OrderDeadline = new DateTime(2025, 6, 14),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 7,
                        DepartureDate = new DateTime(2025, 6, 30),
                        ReturnDate = new DateTime(2025, 7, 6),
                        TotalSeats = 21,
                        SoldSeats = 15,
                        OrderDeadline = new DateTime(2025, 6, 20),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 7,
                        DepartureDate = new DateTime(2025, 7, 6),
                        ReturnDate = new DateTime(2025, 7, 12),
                        TotalSeats = 22,
                        SoldSeats = 13,
                        OrderDeadline = new DateTime(2025, 6, 26),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 7,
                        DepartureDate = new DateTime(2025, 7, 12),
                        ReturnDate = new DateTime(2025, 7, 18),
                        TotalSeats = 21,
                        SoldSeats = 11,
                        OrderDeadline = new DateTime(2025, 7, 2),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 8,
                        DepartureDate = new DateTime(2025, 6, 9),
                        ReturnDate = new DateTime(2025, 6, 18),
                        TotalSeats = 28,
                        SoldSeats = 9,
                        OrderDeadline = new DateTime(2025, 5, 30),
                        MinimumParticipants = 16,
                        GroupStatus = "聯絡專人",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 8,
                        DepartureDate = new DateTime(2025, 6, 15),
                        ReturnDate = new DateTime(2025, 6, 24),
                        TotalSeats = 28,
                        SoldSeats = 26,
                        OrderDeadline = new DateTime(2025, 6, 5),
                        MinimumParticipants = 16,
                        GroupStatus = "聯絡專人",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 8,
                        DepartureDate = new DateTime(2025, 6, 21),
                        ReturnDate = new DateTime(2025, 6, 30),
                        TotalSeats = 18,
                        SoldSeats = 2,
                        OrderDeadline = new DateTime(2025, 6, 11),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 8,
                        DepartureDate = new DateTime(2025, 6, 27),
                        ReturnDate = new DateTime(2025, 7, 6),
                        TotalSeats = 28,
                        SoldSeats = 8,
                        OrderDeadline = new DateTime(2025, 6, 17),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 9,
                        DepartureDate = new DateTime(2025, 6, 9),
                        ReturnDate = new DateTime(2025, 6, 18),
                        TotalSeats = 20,
                        SoldSeats = 18,
                        OrderDeadline = new DateTime(2025, 5, 30),
                        MinimumParticipants = 16,
                        GroupStatus = "聯絡專人",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 9,
                        DepartureDate = new DateTime(2025, 6, 15),
                        ReturnDate = new DateTime(2025, 6, 24),
                        TotalSeats = 16,
                        SoldSeats = 2,
                        OrderDeadline = new DateTime(2025, 6, 5),
                        MinimumParticipants = 16,
                        GroupStatus = "聯絡專人",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 9,
                        DepartureDate = new DateTime(2025, 6, 21),
                        ReturnDate = new DateTime(2025, 6, 30),
                        TotalSeats = 29,
                        SoldSeats = 27,
                        OrderDeadline = new DateTime(2025, 6, 11),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 9,
                        DepartureDate = new DateTime(2025, 6, 27),
                        ReturnDate = new DateTime(2025, 7, 6),
                        TotalSeats = 19,
                        SoldSeats = 10,
                        OrderDeadline = new DateTime(2025, 6, 17),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 10,
                        DepartureDate = new DateTime(2025, 6, 12),
                        ReturnDate = new DateTime(2025, 6, 18),
                        TotalSeats = 25,
                        SoldSeats = 9,
                        OrderDeadline = new DateTime(2025, 6, 2),
                        MinimumParticipants = 16,
                        GroupStatus = "聯絡專人",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 10,
                        DepartureDate = new DateTime(2025, 6, 18),
                        ReturnDate = new DateTime(2025, 6, 24),
                        TotalSeats = 25,
                        SoldSeats = 10,
                        OrderDeadline = new DateTime(2025, 6, 8),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 10,
                        DepartureDate = new DateTime(2025, 6, 24),
                        ReturnDate = new DateTime(2025, 6, 30),
                        TotalSeats = 19,
                        SoldSeats = 13,
                        OrderDeadline = new DateTime(2025, 6, 14),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 10,
                        DepartureDate = new DateTime(2025, 6, 30),
                        ReturnDate = new DateTime(2025, 7, 6),
                        TotalSeats = 22,
                        SoldSeats = 20,
                        OrderDeadline = new DateTime(2025, 6, 20),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 10,
                        DepartureDate = new DateTime(2025, 7, 6),
                        ReturnDate = new DateTime(2025, 7, 12),
                        TotalSeats = 17,
                        SoldSeats = 13,
                        OrderDeadline = new DateTime(2025, 6, 26),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 10,
                        DepartureDate = new DateTime(2025, 7, 12),
                        ReturnDate = new DateTime(2025, 7, 18),
                        TotalSeats = 22,
                        SoldSeats = 19,
                        OrderDeadline = new DateTime(2025, 7, 2),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 10,
                        DepartureDate = new DateTime(2025, 7, 18),
                        ReturnDate = new DateTime(2025, 7, 24),
                        TotalSeats = 18,
                        SoldSeats = 0,
                        OrderDeadline = new DateTime(2025, 7, 8),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 11,
                        DepartureDate = new DateTime(2025, 6, 15),
                        ReturnDate = new DateTime(2025, 6, 24),
                        TotalSeats = 21,
                        SoldSeats = 12,
                        OrderDeadline = new DateTime(2025, 6, 5),
                        MinimumParticipants = 16,
                        GroupStatus = "聯絡專人",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 11,
                        DepartureDate = new DateTime(2025, 6, 21),
                        ReturnDate = new DateTime(2025, 6, 30),
                        TotalSeats = 32,
                        SoldSeats = 16,
                        OrderDeadline = new DateTime(2025, 6, 11),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 11,
                        DepartureDate = new DateTime(2025, 6, 27),
                        ReturnDate = new DateTime(2025, 7, 6),
                        TotalSeats = 16,
                        SoldSeats = 1,
                        OrderDeadline = new DateTime(2025, 6, 17),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 11,
                        DepartureDate = new DateTime(2025, 7, 3),
                        ReturnDate = new DateTime(2025, 7, 12),
                        TotalSeats = 32,
                        SoldSeats = 13,
                        OrderDeadline = new DateTime(2025, 6, 23),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 12,
                        DepartureDate = new DateTime(2025, 6, 15),
                        ReturnDate = new DateTime(2025, 6, 24),
                        TotalSeats = 25,
                        SoldSeats = 11,
                        OrderDeadline = new DateTime(2025, 6, 5),
                        MinimumParticipants = 16,
                        GroupStatus = "聯絡專人",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 12,
                        DepartureDate = new DateTime(2025, 6, 21),
                        ReturnDate = new DateTime(2025, 6, 30),
                        TotalSeats = 27,
                        SoldSeats = 17,
                        OrderDeadline = new DateTime(2025, 6, 11),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 12,
                        DepartureDate = new DateTime(2025, 6, 27),
                        ReturnDate = new DateTime(2025, 7, 6),
                        TotalSeats = 24,
                        SoldSeats = 17,
                        OrderDeadline = new DateTime(2025, 6, 17),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 12,
                        DepartureDate = new DateTime(2025, 7, 3),
                        ReturnDate = new DateTime(2025, 7, 12),
                        TotalSeats = 19,
                        SoldSeats = 16,
                        OrderDeadline = new DateTime(2025, 6, 23),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 13,
                        DepartureDate = new DateTime(2025, 6, 18),
                        ReturnDate = new DateTime(2025, 6, 24),
                        TotalSeats = 24,
                        SoldSeats = 3,
                        OrderDeadline = new DateTime(2025, 6, 8),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 13,
                        DepartureDate = new DateTime(2025, 6, 24),
                        ReturnDate = new DateTime(2025, 6, 30),
                        TotalSeats = 21,
                        SoldSeats = 11,
                        OrderDeadline = new DateTime(2025, 6, 14),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 13,
                        DepartureDate = new DateTime(2025, 6, 30),
                        ReturnDate = new DateTime(2025, 7, 6),
                        TotalSeats = 17,
                        SoldSeats = 13,
                        OrderDeadline = new DateTime(2025, 6, 20),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 13,
                        DepartureDate = new DateTime(2025, 7, 6),
                        ReturnDate = new DateTime(2025, 7, 12),
                        TotalSeats = 16,
                        SoldSeats = 16,
                        OrderDeadline = new DateTime(2025, 6, 26),
                        MinimumParticipants = 16,
                        GroupStatus = "可候補",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 13,
                        DepartureDate = new DateTime(2025, 7, 12),
                        ReturnDate = new DateTime(2025, 7, 18),
                        TotalSeats = 18,
                        SoldSeats = 11,
                        OrderDeadline = new DateTime(2025, 7, 2),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 13,
                        DepartureDate = new DateTime(2025, 7, 18),
                        ReturnDate = new DateTime(2025, 7, 24),
                        TotalSeats = 32,
                        SoldSeats = 1,
                        OrderDeadline = new DateTime(2025, 7, 8),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 13,
                        DepartureDate = new DateTime(2025, 7, 24),
                        ReturnDate = new DateTime(2025, 7, 30),
                        TotalSeats = 25,
                        SoldSeats = 17,
                        OrderDeadline = new DateTime(2025, 7, 14),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 14,
                        DepartureDate = new DateTime(2025, 6, 21),
                        ReturnDate = new DateTime(2025, 6, 30),
                        TotalSeats = 31,
                        SoldSeats = 4,
                        OrderDeadline = new DateTime(2025, 6, 11),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 14,
                        DepartureDate = new DateTime(2025, 6, 27),
                        ReturnDate = new DateTime(2025, 7, 6),
                        TotalSeats = 22,
                        SoldSeats = 0,
                        OrderDeadline = new DateTime(2025, 6, 17),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 14,
                        DepartureDate = new DateTime(2025, 7, 3),
                        ReturnDate = new DateTime(2025, 7, 12),
                        TotalSeats = 21,
                        SoldSeats = 13,
                        OrderDeadline = new DateTime(2025, 6, 23),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 14,
                        DepartureDate = new DateTime(2025, 7, 9),
                        ReturnDate = new DateTime(2025, 7, 18),
                        TotalSeats = 16,
                        SoldSeats = 10,
                        OrderDeadline = new DateTime(2025, 6, 29),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 15,
                        DepartureDate = new DateTime(2025, 6, 21),
                        ReturnDate = new DateTime(2025, 6, 30),
                        TotalSeats = 31,
                        SoldSeats = 15,
                        OrderDeadline = new DateTime(2025, 6, 11),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 15,
                        DepartureDate = new DateTime(2025, 6, 27),
                        ReturnDate = new DateTime(2025, 7, 6),
                        TotalSeats = 17,
                        SoldSeats = 14,
                        OrderDeadline = new DateTime(2025, 6, 17),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 15,
                        DepartureDate = new DateTime(2025, 7, 3),
                        ReturnDate = new DateTime(2025, 7, 12),
                        TotalSeats = 16,
                        SoldSeats = 6,
                        OrderDeadline = new DateTime(2025, 6, 23),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 15,
                        DepartureDate = new DateTime(2025, 7, 9),
                        ReturnDate = new DateTime(2025, 7, 18),
                        TotalSeats = 20,
                        SoldSeats = 13,
                        OrderDeadline = new DateTime(2025, 6, 29),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 16,
                        DepartureDate = new DateTime(2025, 6, 24),
                        ReturnDate = new DateTime(2025, 6, 30),
                        TotalSeats = 16,
                        SoldSeats = 13,
                        OrderDeadline = new DateTime(2025, 6, 14),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 16,
                        DepartureDate = new DateTime(2025, 6, 30),
                        ReturnDate = new DateTime(2025, 7, 6),
                        TotalSeats = 31,
                        SoldSeats = 15,
                        OrderDeadline = new DateTime(2025, 6, 20),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 16,
                        DepartureDate = new DateTime(2025, 7, 6),
                        ReturnDate = new DateTime(2025, 7, 12),
                        TotalSeats = 30,
                        SoldSeats = 27,
                        OrderDeadline = new DateTime(2025, 6, 26),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 16,
                        DepartureDate = new DateTime(2025, 7, 12),
                        ReturnDate = new DateTime(2025, 7, 18),
                        TotalSeats = 24,
                        SoldSeats = 5,
                        OrderDeadline = new DateTime(2025, 7, 2),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 16,
                        DepartureDate = new DateTime(2025, 7, 18),
                        ReturnDate = new DateTime(2025, 7, 24),
                        TotalSeats = 17,
                        SoldSeats = 7,
                        OrderDeadline = new DateTime(2025, 7, 8),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 16,
                        DepartureDate = new DateTime(2025, 7, 24),
                        ReturnDate = new DateTime(2025, 7, 30),
                        TotalSeats = 23,
                        SoldSeats = 6,
                        OrderDeadline = new DateTime(2025, 7, 14),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 16,
                        DepartureDate = new DateTime(2025, 7, 30),
                        ReturnDate = new DateTime(2025, 8, 5),
                        TotalSeats = 23,
                        SoldSeats = 6,
                        OrderDeadline = new DateTime(2025, 7, 20),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 17,
                        DepartureDate = new DateTime(2025, 6, 27),
                        ReturnDate = new DateTime(2025, 7, 6),
                        TotalSeats = 19,
                        SoldSeats = 0,
                        OrderDeadline = new DateTime(2025, 6, 17),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 17,
                        DepartureDate = new DateTime(2025, 7, 3),
                        ReturnDate = new DateTime(2025, 7, 12),
                        TotalSeats = 29,
                        SoldSeats = 24,
                        OrderDeadline = new DateTime(2025, 6, 23),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 17,
                        DepartureDate = new DateTime(2025, 7, 9),
                        ReturnDate = new DateTime(2025, 7, 18),
                        TotalSeats = 22,
                        SoldSeats = 1,
                        OrderDeadline = new DateTime(2025, 6, 29),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 17,
                        DepartureDate = new DateTime(2025, 7, 15),
                        ReturnDate = new DateTime(2025, 7, 24),
                        TotalSeats = 28,
                        SoldSeats = 14,
                        OrderDeadline = new DateTime(2025, 7, 5),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 18,
                        DepartureDate = new DateTime(2025, 6, 27),
                        ReturnDate = new DateTime(2025, 7, 6),
                        TotalSeats = 16,
                        SoldSeats = 0,
                        OrderDeadline = new DateTime(2025, 6, 17),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 18,
                        DepartureDate = new DateTime(2025, 7, 3),
                        ReturnDate = new DateTime(2025, 7, 12),
                        TotalSeats = 19,
                        SoldSeats = 16,
                        OrderDeadline = new DateTime(2025, 6, 23),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 18,
                        DepartureDate = new DateTime(2025, 7, 9),
                        ReturnDate = new DateTime(2025, 7, 18),
                        TotalSeats = 29,
                        SoldSeats = 14,
                        OrderDeadline = new DateTime(2025, 6, 29),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 18,
                        DepartureDate = new DateTime(2025, 7, 15),
                        ReturnDate = new DateTime(2025, 7, 24),
                        TotalSeats = 16,
                        SoldSeats = 4,
                        OrderDeadline = new DateTime(2025, 7, 5),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 19,
                        DepartureDate = new DateTime(2025, 6, 30),
                        ReturnDate = new DateTime(2025, 7, 6),
                        TotalSeats = 16,
                        SoldSeats = 10,
                        OrderDeadline = new DateTime(2025, 6, 20),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 19,
                        DepartureDate = new DateTime(2025, 7, 6),
                        ReturnDate = new DateTime(2025, 7, 12),
                        TotalSeats = 19,
                        SoldSeats = 3,
                        OrderDeadline = new DateTime(2025, 6, 26),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 19,
                        DepartureDate = new DateTime(2025, 7, 12),
                        ReturnDate = new DateTime(2025, 7, 18),
                        TotalSeats = 18,
                        SoldSeats = 0,
                        OrderDeadline = new DateTime(2025, 7, 2),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 19,
                        DepartureDate = new DateTime(2025, 7, 18),
                        ReturnDate = new DateTime(2025, 7, 24),
                        TotalSeats = 20,
                        SoldSeats = 20,
                        OrderDeadline = new DateTime(2025, 7, 8),
                        MinimumParticipants = 16,
                        GroupStatus = "可候補",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 19,
                        DepartureDate = new DateTime(2025, 7, 24),
                        ReturnDate = new DateTime(2025, 7, 30),
                        TotalSeats = 31,
                        SoldSeats = 11,
                        OrderDeadline = new DateTime(2025, 7, 14),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 19,
                        DepartureDate = new DateTime(2025, 7, 30),
                        ReturnDate = new DateTime(2025, 8, 5),
                        TotalSeats = 25,
                        SoldSeats = 12,
                        OrderDeadline = new DateTime(2025, 7, 20),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 19,
                        DepartureDate = new DateTime(2025, 8, 5),
                        ReturnDate = new DateTime(2025, 8, 11),
                        TotalSeats = 32,
                        SoldSeats = 3,
                        OrderDeadline = new DateTime(2025, 7, 26),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 20,
                        DepartureDate = new DateTime(2025, 7, 3),
                        ReturnDate = new DateTime(2025, 7, 12),
                        TotalSeats = 22,
                        SoldSeats = 6,
                        OrderDeadline = new DateTime(2025, 6, 23),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 20,
                        DepartureDate = new DateTime(2025, 7, 9),
                        ReturnDate = new DateTime(2025, 7, 18),
                        TotalSeats = 24,
                        SoldSeats = 19,
                        OrderDeadline = new DateTime(2025, 6, 29),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 20,
                        DepartureDate = new DateTime(2025, 7, 15),
                        ReturnDate = new DateTime(2025, 7, 24),
                        TotalSeats = 28,
                        SoldSeats = 0,
                        OrderDeadline = new DateTime(2025, 7, 5),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 20,
                        DepartureDate = new DateTime(2025, 7, 21),
                        ReturnDate = new DateTime(2025, 7, 30),
                        TotalSeats = 32,
                        SoldSeats = 9,
                        OrderDeadline = new DateTime(2025, 7, 11),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 21,
                        DepartureDate = new DateTime(2025, 7, 3),
                        ReturnDate = new DateTime(2025, 7, 12),
                        TotalSeats = 19,
                        SoldSeats = 9,
                        OrderDeadline = new DateTime(2025, 6, 23),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 21,
                        DepartureDate = new DateTime(2025, 7, 9),
                        ReturnDate = new DateTime(2025, 7, 18),
                        TotalSeats = 17,
                        SoldSeats = 6,
                        OrderDeadline = new DateTime(2025, 6, 29),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 21,
                        DepartureDate = new DateTime(2025, 7, 15),
                        ReturnDate = new DateTime(2025, 7, 24),
                        TotalSeats = 20,
                        SoldSeats = 18,
                        OrderDeadline = new DateTime(2025, 7, 5),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 21,
                        DepartureDate = new DateTime(2025, 7, 21),
                        ReturnDate = new DateTime(2025, 7, 30),
                        TotalSeats = 26,
                        SoldSeats = 22,
                        OrderDeadline = new DateTime(2025, 7, 11),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 22,
                        DepartureDate = new DateTime(2025, 7, 6),
                        ReturnDate = new DateTime(2025, 7, 12),
                        TotalSeats = 31,
                        SoldSeats = 20,
                        OrderDeadline = new DateTime(2025, 6, 26),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 22,
                        DepartureDate = new DateTime(2025, 7, 12),
                        ReturnDate = new DateTime(2025, 7, 18),
                        TotalSeats = 16,
                        SoldSeats = 6,
                        OrderDeadline = new DateTime(2025, 7, 2),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 22,
                        DepartureDate = new DateTime(2025, 7, 18),
                        ReturnDate = new DateTime(2025, 7, 24),
                        TotalSeats = 27,
                        SoldSeats = 2,
                        OrderDeadline = new DateTime(2025, 7, 8),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 22,
                        DepartureDate = new DateTime(2025, 7, 24),
                        ReturnDate = new DateTime(2025, 7, 30),
                        TotalSeats = 17,
                        SoldSeats = 6,
                        OrderDeadline = new DateTime(2025, 7, 14),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 22,
                        DepartureDate = new DateTime(2025, 7, 30),
                        ReturnDate = new DateTime(2025, 8, 5),
                        TotalSeats = 21,
                        SoldSeats = 19,
                        OrderDeadline = new DateTime(2025, 7, 20),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 22,
                        DepartureDate = new DateTime(2025, 8, 5),
                        ReturnDate = new DateTime(2025, 8, 11),
                        TotalSeats = 32,
                        SoldSeats = 19,
                        OrderDeadline = new DateTime(2025, 7, 26),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 22,
                        DepartureDate = new DateTime(2025, 8, 11),
                        ReturnDate = new DateTime(2025, 8, 17),
                        TotalSeats = 26,
                        SoldSeats = 26,
                        OrderDeadline = new DateTime(2025, 8, 1),
                        MinimumParticipants = 16,
                        GroupStatus = "可候補",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 23,
                        DepartureDate = new DateTime(2025, 7, 9),
                        ReturnDate = new DateTime(2025, 7, 18),
                        TotalSeats = 24,
                        SoldSeats = 18,
                        OrderDeadline = new DateTime(2025, 6, 29),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 23,
                        DepartureDate = new DateTime(2025, 7, 15),
                        ReturnDate = new DateTime(2025, 7, 24),
                        TotalSeats = 31,
                        SoldSeats = 17,
                        OrderDeadline = new DateTime(2025, 7, 5),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 23,
                        DepartureDate = new DateTime(2025, 7, 21),
                        ReturnDate = new DateTime(2025, 7, 30),
                        TotalSeats = 26,
                        SoldSeats = 4,
                        OrderDeadline = new DateTime(2025, 7, 11),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 23,
                        DepartureDate = new DateTime(2025, 7, 27),
                        ReturnDate = new DateTime(2025, 8, 5),
                        TotalSeats = 31,
                        SoldSeats = 26,
                        OrderDeadline = new DateTime(2025, 7, 17),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 24,
                        DepartureDate = new DateTime(2025, 7, 9),
                        ReturnDate = new DateTime(2025, 7, 18),
                        TotalSeats = 23,
                        SoldSeats = 17,
                        OrderDeadline = new DateTime(2025, 6, 29),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 24,
                        DepartureDate = new DateTime(2025, 7, 15),
                        ReturnDate = new DateTime(2025, 7, 24),
                        TotalSeats = 32,
                        SoldSeats = 18,
                        OrderDeadline = new DateTime(2025, 7, 5),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 24,
                        DepartureDate = new DateTime(2025, 7, 21),
                        ReturnDate = new DateTime(2025, 7, 30),
                        TotalSeats = 29,
                        SoldSeats = 1,
                        OrderDeadline = new DateTime(2025, 7, 11),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 24,
                        DepartureDate = new DateTime(2025, 7, 27),
                        ReturnDate = new DateTime(2025, 8, 5),
                        TotalSeats = 29,
                        SoldSeats = 14,
                        OrderDeadline = new DateTime(2025, 7, 17),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 25,
                        DepartureDate = new DateTime(2025, 7, 12),
                        ReturnDate = new DateTime(2025, 7, 18),
                        TotalSeats = 21,
                        SoldSeats = 3,
                        OrderDeadline = new DateTime(2025, 7, 2),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 25,
                        DepartureDate = new DateTime(2025, 7, 18),
                        ReturnDate = new DateTime(2025, 7, 24),
                        TotalSeats = 30,
                        SoldSeats = 4,
                        OrderDeadline = new DateTime(2025, 7, 8),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 25,
                        DepartureDate = new DateTime(2025, 7, 24),
                        ReturnDate = new DateTime(2025, 7, 30),
                        TotalSeats = 22,
                        SoldSeats = 9,
                        OrderDeadline = new DateTime(2025, 7, 14),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 25,
                        DepartureDate = new DateTime(2025, 7, 30),
                        ReturnDate = new DateTime(2025, 8, 5),
                        TotalSeats = 21,
                        SoldSeats = 18,
                        OrderDeadline = new DateTime(2025, 7, 20),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 25,
                        DepartureDate = new DateTime(2025, 8, 5),
                        ReturnDate = new DateTime(2025, 8, 11),
                        TotalSeats = 20,
                        SoldSeats = 7,
                        OrderDeadline = new DateTime(2025, 7, 26),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 25,
                        DepartureDate = new DateTime(2025, 8, 11),
                        ReturnDate = new DateTime(2025, 8, 17),
                        TotalSeats = 22,
                        SoldSeats = 14,
                        OrderDeadline = new DateTime(2025, 8, 1),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 25,
                        DepartureDate = new DateTime(2025, 8, 17),
                        ReturnDate = new DateTime(2025, 8, 23),
                        TotalSeats = 30,
                        SoldSeats = 9,
                        OrderDeadline = new DateTime(2025, 8, 7),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 26,
                        DepartureDate = new DateTime(2025, 7, 15),
                        ReturnDate = new DateTime(2025, 7, 24),
                        TotalSeats = 25,
                        SoldSeats = 6,
                        OrderDeadline = new DateTime(2025, 7, 5),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 26,
                        DepartureDate = new DateTime(2025, 7, 21),
                        ReturnDate = new DateTime(2025, 7, 30),
                        TotalSeats = 19,
                        SoldSeats = 17,
                        OrderDeadline = new DateTime(2025, 7, 11),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 26,
                        DepartureDate = new DateTime(2025, 7, 27),
                        ReturnDate = new DateTime(2025, 8, 5),
                        TotalSeats = 27,
                        SoldSeats = 15,
                        OrderDeadline = new DateTime(2025, 7, 17),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 26,
                        DepartureDate = new DateTime(2025, 8, 2),
                        ReturnDate = new DateTime(2025, 8, 11),
                        TotalSeats = 26,
                        SoldSeats = 25,
                        OrderDeadline = new DateTime(2025, 7, 23),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 27,
                        DepartureDate = new DateTime(2025, 7, 15),
                        ReturnDate = new DateTime(2025, 7, 24),
                        TotalSeats = 22,
                        SoldSeats = 1,
                        OrderDeadline = new DateTime(2025, 7, 5),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 27,
                        DepartureDate = new DateTime(2025, 7, 21),
                        ReturnDate = new DateTime(2025, 7, 30),
                        TotalSeats = 20,
                        SoldSeats = 0,
                        OrderDeadline = new DateTime(2025, 7, 11),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 27,
                        DepartureDate = new DateTime(2025, 7, 27),
                        ReturnDate = new DateTime(2025, 8, 5),
                        TotalSeats = 23,
                        SoldSeats = 0,
                        OrderDeadline = new DateTime(2025, 7, 17),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 27,
                        DepartureDate = new DateTime(2025, 8, 2),
                        ReturnDate = new DateTime(2025, 8, 11),
                        TotalSeats = 29,
                        SoldSeats = 0,
                        OrderDeadline = new DateTime(2025, 7, 23),
                        MinimumParticipants = 16,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 28,
                        DepartureDate = new DateTime(2025, 7, 18),
                        ReturnDate = new DateTime(2025, 7, 20),
                        TotalSeats = 29,
                        SoldSeats = 25,
                        OrderDeadline = new DateTime(2025, 7, 8),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 28,
                        DepartureDate = new DateTime(2025, 7, 22),
                        ReturnDate = new DateTime(2025, 7, 24),
                        TotalSeats = 23,
                        SoldSeats = 22,
                        OrderDeadline = new DateTime(2025, 7, 12),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 28,
                        DepartureDate = new DateTime(2025, 7, 26),
                        ReturnDate = new DateTime(2025, 7, 28),
                        TotalSeats = 21,
                        SoldSeats = 1,
                        OrderDeadline = new DateTime(2025, 7, 16),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 28,
                        DepartureDate = new DateTime(2025, 7, 30),
                        ReturnDate = new DateTime(2025, 8, 1),
                        TotalSeats = 23,
                        SoldSeats = 20,
                        OrderDeadline = new DateTime(2025, 7, 20),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 28,
                        DepartureDate = new DateTime(2025, 8, 3),
                        ReturnDate = new DateTime(2025, 8, 5),
                        TotalSeats = 28,
                        SoldSeats = 0,
                        OrderDeadline = new DateTime(2025, 7, 24),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 28,
                        DepartureDate = new DateTime(2025, 8, 7),
                        ReturnDate = new DateTime(2025, 8, 9),
                        TotalSeats = 25,
                        SoldSeats = 4,
                        OrderDeadline = new DateTime(2025, 7, 28),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 28,
                        DepartureDate = new DateTime(2025, 8, 11),
                        ReturnDate = new DateTime(2025, 8, 13),
                        TotalSeats = 16,
                        SoldSeats = 14,
                        OrderDeadline = new DateTime(2025, 8, 1),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 29,
                        DepartureDate = new DateTime(2025, 7, 21),
                        ReturnDate = new DateTime(2025, 7, 22),
                        TotalSeats = 30,
                        SoldSeats = 5,
                        OrderDeadline = new DateTime(2025, 7, 11),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 29,
                        DepartureDate = new DateTime(2025, 7, 25),
                        ReturnDate = new DateTime(2025, 7, 26),
                        TotalSeats = 16,
                        SoldSeats = 13,
                        OrderDeadline = new DateTime(2025, 7, 15),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 29,
                        DepartureDate = new DateTime(2025, 7, 29),
                        ReturnDate = new DateTime(2025, 7, 30),
                        TotalSeats = 17,
                        SoldSeats = 15,
                        OrderDeadline = new DateTime(2025, 7, 19),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 29,
                        DepartureDate = new DateTime(2025, 8, 2),
                        ReturnDate = new DateTime(2025, 8, 3),
                        TotalSeats = 23,
                        SoldSeats = 14,
                        OrderDeadline = new DateTime(2025, 7, 23),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 30,
                        DepartureDate = new DateTime(2025, 7, 21),
                        ReturnDate = new DateTime(2025, 7, 22),
                        TotalSeats = 21,
                        SoldSeats = 19,
                        OrderDeadline = new DateTime(2025, 7, 11),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 30,
                        DepartureDate = new DateTime(2025, 7, 25),
                        ReturnDate = new DateTime(2025, 7, 26),
                        TotalSeats = 27,
                        SoldSeats = 27,
                        OrderDeadline = new DateTime(2025, 7, 15),
                        MinimumParticipants = 5,
                        GroupStatus = "可候補",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 30,
                        DepartureDate = new DateTime(2025, 7, 29),
                        ReturnDate = new DateTime(2025, 7, 30),
                        TotalSeats = 25,
                        SoldSeats = 21,
                        OrderDeadline = new DateTime(2025, 7, 19),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 30,
                        DepartureDate = new DateTime(2025, 8, 2),
                        ReturnDate = new DateTime(2025, 8, 3),
                        TotalSeats = 16,
                        SoldSeats = 16,
                        OrderDeadline = new DateTime(2025, 7, 23),
                        MinimumParticipants = 5,
                        GroupStatus = "可候補",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 31,
                        DepartureDate = new DateTime(2025, 7, 24),
                        ReturnDate = new DateTime(2025, 7, 26),
                        TotalSeats = 22,
                        SoldSeats = 21,
                        OrderDeadline = new DateTime(2025, 7, 14),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 31,
                        DepartureDate = new DateTime(2025, 7, 28),
                        ReturnDate = new DateTime(2025, 7, 30),
                        TotalSeats = 23,
                        SoldSeats = 23,
                        OrderDeadline = new DateTime(2025, 7, 18),
                        MinimumParticipants = 5,
                        GroupStatus = "可候補",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 31,
                        DepartureDate = new DateTime(2025, 8, 1),
                        ReturnDate = new DateTime(2025, 8, 3),
                        TotalSeats = 16,
                        SoldSeats = 16,
                        OrderDeadline = new DateTime(2025, 7, 22),
                        MinimumParticipants = 5,
                        GroupStatus = "可候補",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 31,
                        DepartureDate = new DateTime(2025, 8, 5),
                        ReturnDate = new DateTime(2025, 8, 7),
                        TotalSeats = 20,
                        SoldSeats = 18,
                        OrderDeadline = new DateTime(2025, 7, 26),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 31,
                        DepartureDate = new DateTime(2025, 8, 9),
                        ReturnDate = new DateTime(2025, 8, 11),
                        TotalSeats = 19,
                        SoldSeats = 5,
                        OrderDeadline = new DateTime(2025, 7, 30),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 31,
                        DepartureDate = new DateTime(2025, 8, 13),
                        ReturnDate = new DateTime(2025, 8, 15),
                        TotalSeats = 23,
                        SoldSeats = 6,
                        OrderDeadline = new DateTime(2025, 8, 3),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 31,
                        DepartureDate = new DateTime(2025, 8, 17),
                        ReturnDate = new DateTime(2025, 8, 19),
                        TotalSeats = 23,
                        SoldSeats = 6,
                        OrderDeadline = new DateTime(2025, 8, 7),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 32,
                        DepartureDate = new DateTime(2025, 7, 27),
                        ReturnDate = new DateTime(2025, 7, 28),
                        TotalSeats = 18,
                        SoldSeats = 0,
                        OrderDeadline = new DateTime(2025, 7, 17),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 32,
                        DepartureDate = new DateTime(2025, 7, 31),
                        ReturnDate = new DateTime(2025, 8, 1),
                        TotalSeats = 24,
                        SoldSeats = 16,
                        OrderDeadline = new DateTime(2025, 7, 21),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 32,
                        DepartureDate = new DateTime(2025, 8, 4),
                        ReturnDate = new DateTime(2025, 8, 5),
                        TotalSeats = 20,
                        SoldSeats = 12,
                        OrderDeadline = new DateTime(2025, 7, 25),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 32,
                        DepartureDate = new DateTime(2025, 8, 8),
                        ReturnDate = new DateTime(2025, 8, 9),
                        TotalSeats = 30,
                        SoldSeats = 21,
                        OrderDeadline = new DateTime(2025, 7, 29),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 33,
                        DepartureDate = new DateTime(2025, 7, 27),
                        ReturnDate = new DateTime(2025, 7, 28),
                        TotalSeats = 30,
                        SoldSeats = 9,
                        OrderDeadline = new DateTime(2025, 7, 17),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 33,
                        DepartureDate = new DateTime(2025, 7, 31),
                        ReturnDate = new DateTime(2025, 8, 1),
                        TotalSeats = 18,
                        SoldSeats = 17,
                        OrderDeadline = new DateTime(2025, 7, 21),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 33,
                        DepartureDate = new DateTime(2025, 8, 4),
                        ReturnDate = new DateTime(2025, 8, 5),
                        TotalSeats = 31,
                        SoldSeats = 27,
                        OrderDeadline = new DateTime(2025, 7, 25),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 33,
                        DepartureDate = new DateTime(2025, 8, 8),
                        ReturnDate = new DateTime(2025, 8, 9),
                        TotalSeats = 24,
                        SoldSeats = 4,
                        OrderDeadline = new DateTime(2025, 7, 29),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 34,
                        DepartureDate = new DateTime(2025, 7, 30),
                        ReturnDate = new DateTime(2025, 8, 1),
                        TotalSeats = 21,
                        SoldSeats = 2,
                        OrderDeadline = new DateTime(2025, 7, 20),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 34,
                        DepartureDate = new DateTime(2025, 8, 3),
                        ReturnDate = new DateTime(2025, 8, 5),
                        TotalSeats = 28,
                        SoldSeats = 7,
                        OrderDeadline = new DateTime(2025, 7, 24),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 34,
                        DepartureDate = new DateTime(2025, 8, 7),
                        ReturnDate = new DateTime(2025, 8, 9),
                        TotalSeats = 16,
                        SoldSeats = 2,
                        OrderDeadline = new DateTime(2025, 7, 28),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 34,
                        DepartureDate = new DateTime(2025, 8, 11),
                        ReturnDate = new DateTime(2025, 8, 13),
                        TotalSeats = 30,
                        SoldSeats = 9,
                        OrderDeadline = new DateTime(2025, 8, 1),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 34,
                        DepartureDate = new DateTime(2025, 8, 15),
                        ReturnDate = new DateTime(2025, 8, 17),
                        TotalSeats = 23,
                        SoldSeats = 5,
                        OrderDeadline = new DateTime(2025, 8, 5),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 34,
                        DepartureDate = new DateTime(2025, 8, 19),
                        ReturnDate = new DateTime(2025, 8, 21),
                        TotalSeats = 20,
                        SoldSeats = 11,
                        OrderDeadline = new DateTime(2025, 8, 9),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 34,
                        DepartureDate = new DateTime(2025, 8, 23),
                        ReturnDate = new DateTime(2025, 8, 25),
                        TotalSeats = 23,
                        SoldSeats = 1,
                        OrderDeadline = new DateTime(2025, 8, 13),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 35,
                        DepartureDate = new DateTime(2025, 8, 2),
                        ReturnDate = new DateTime(2025, 8, 3),
                        TotalSeats = 27,
                        SoldSeats = 16,
                        OrderDeadline = new DateTime(2025, 7, 23),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 35,
                        DepartureDate = new DateTime(2025, 8, 6),
                        ReturnDate = new DateTime(2025, 8, 7),
                        TotalSeats = 24,
                        SoldSeats = 4,
                        OrderDeadline = new DateTime(2025, 7, 27),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 35,
                        DepartureDate = new DateTime(2025, 8, 10),
                        ReturnDate = new DateTime(2025, 8, 11),
                        TotalSeats = 20,
                        SoldSeats = 4,
                        OrderDeadline = new DateTime(2025, 7, 31),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 35,
                        DepartureDate = new DateTime(2025, 8, 14),
                        ReturnDate = new DateTime(2025, 8, 15),
                        TotalSeats = 18,
                        SoldSeats = 12,
                        OrderDeadline = new DateTime(2025, 8, 4),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 36,
                        DepartureDate = new DateTime(2025, 8, 2),
                        ReturnDate = new DateTime(2025, 8, 3),
                        TotalSeats = 22,
                        SoldSeats = 1,
                        OrderDeadline = new DateTime(2025, 7, 23),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 36,
                        DepartureDate = new DateTime(2025, 8, 6),
                        ReturnDate = new DateTime(2025, 8, 7),
                        TotalSeats = 28,
                        SoldSeats = 11,
                        OrderDeadline = new DateTime(2025, 7, 27),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 36,
                        DepartureDate = new DateTime(2025, 8, 10),
                        ReturnDate = new DateTime(2025, 8, 11),
                        TotalSeats = 18,
                        SoldSeats = 10,
                        OrderDeadline = new DateTime(2025, 7, 31),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 36,
                        DepartureDate = new DateTime(2025, 8, 14),
                        ReturnDate = new DateTime(2025, 8, 15),
                        TotalSeats = 28,
                        SoldSeats = 11,
                        OrderDeadline = new DateTime(2025, 8, 4),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 37,
                        DepartureDate = new DateTime(2025, 8, 5),
                        ReturnDate = new DateTime(2025, 8, 7),
                        TotalSeats = 20,
                        SoldSeats = 8,
                        OrderDeadline = new DateTime(2025, 7, 26),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 37,
                        DepartureDate = new DateTime(2025, 8, 9),
                        ReturnDate = new DateTime(2025, 8, 11),
                        TotalSeats = 28,
                        SoldSeats = 8,
                        OrderDeadline = new DateTime(2025, 7, 30),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 37,
                        DepartureDate = new DateTime(2025, 8, 13),
                        ReturnDate = new DateTime(2025, 8, 15),
                        TotalSeats = 17,
                        SoldSeats = 9,
                        OrderDeadline = new DateTime(2025, 8, 3),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 37,
                        DepartureDate = new DateTime(2025, 8, 17),
                        ReturnDate = new DateTime(2025, 8, 19),
                        TotalSeats = 16,
                        SoldSeats = 13,
                        OrderDeadline = new DateTime(2025, 8, 7),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 37,
                        DepartureDate = new DateTime(2025, 8, 21),
                        ReturnDate = new DateTime(2025, 8, 23),
                        TotalSeats = 26,
                        SoldSeats = 15,
                        OrderDeadline = new DateTime(2025, 8, 11),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 37,
                        DepartureDate = new DateTime(2025, 8, 25),
                        ReturnDate = new DateTime(2025, 8, 27),
                        TotalSeats = 19,
                        SoldSeats = 5,
                        OrderDeadline = new DateTime(2025, 8, 15),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 37,
                        DepartureDate = new DateTime(2025, 8, 29),
                        ReturnDate = new DateTime(2025, 8, 31),
                        TotalSeats = 25,
                        SoldSeats = 4,
                        OrderDeadline = new DateTime(2025, 8, 19),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 38,
                        DepartureDate = new DateTime(2025, 8, 8),
                        ReturnDate = new DateTime(2025, 8, 9),
                        TotalSeats = 27,
                        SoldSeats = 26,
                        OrderDeadline = new DateTime(2025, 7, 29),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 38,
                        DepartureDate = new DateTime(2025, 8, 12),
                        ReturnDate = new DateTime(2025, 8, 13),
                        TotalSeats = 23,
                        SoldSeats = 13,
                        OrderDeadline = new DateTime(2025, 8, 2),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 38,
                        DepartureDate = new DateTime(2025, 8, 16),
                        ReturnDate = new DateTime(2025, 8, 17),
                        TotalSeats = 22,
                        SoldSeats = 21,
                        OrderDeadline = new DateTime(2025, 8, 6),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 38,
                        DepartureDate = new DateTime(2025, 8, 20),
                        ReturnDate = new DateTime(2025, 8, 21),
                        TotalSeats = 26,
                        SoldSeats = 19,
                        OrderDeadline = new DateTime(2025, 8, 10),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 39,
                        DepartureDate = new DateTime(2025, 8, 8),
                        ReturnDate = new DateTime(2025, 8, 9),
                        TotalSeats = 24,
                        SoldSeats = 10,
                        OrderDeadline = new DateTime(2025, 7, 29),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 39,
                        DepartureDate = new DateTime(2025, 8, 12),
                        ReturnDate = new DateTime(2025, 8, 13),
                        TotalSeats = 24,
                        SoldSeats = 24,
                        OrderDeadline = new DateTime(2025, 8, 2),
                        MinimumParticipants = 5,
                        GroupStatus = "可候補",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 39,
                        DepartureDate = new DateTime(2025, 8, 16),
                        ReturnDate = new DateTime(2025, 8, 17),
                        TotalSeats = 28,
                        SoldSeats = 4,
                        OrderDeadline = new DateTime(2025, 8, 6),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 39,
                        DepartureDate = new DateTime(2025, 8, 20),
                        ReturnDate = new DateTime(2025, 8, 21),
                        TotalSeats = 28,
                        SoldSeats = 23,
                        OrderDeadline = new DateTime(2025, 8, 10),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 40,
                        DepartureDate = new DateTime(2025, 8, 11),
                        ReturnDate = new DateTime(2025, 8, 13),
                        TotalSeats = 24,
                        SoldSeats = 14,
                        OrderDeadline = new DateTime(2025, 8, 1),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 40,
                        DepartureDate = new DateTime(2025, 8, 15),
                        ReturnDate = new DateTime(2025, 8, 17),
                        TotalSeats = 23,
                        SoldSeats = 21,
                        OrderDeadline = new DateTime(2025, 8, 5),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 40,
                        DepartureDate = new DateTime(2025, 8, 19),
                        ReturnDate = new DateTime(2025, 8, 21),
                        TotalSeats = 17,
                        SoldSeats = 14,
                        OrderDeadline = new DateTime(2025, 8, 9),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 40,
                        DepartureDate = new DateTime(2025, 8, 23),
                        ReturnDate = new DateTime(2025, 8, 25),
                        TotalSeats = 17,
                        SoldSeats = 8,
                        OrderDeadline = new DateTime(2025, 8, 13),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 40,
                        DepartureDate = new DateTime(2025, 8, 27),
                        ReturnDate = new DateTime(2025, 8, 29),
                        TotalSeats = 20,
                        SoldSeats = 8,
                        OrderDeadline = new DateTime(2025, 8, 17),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 40,
                        DepartureDate = new DateTime(2025, 8, 31),
                        ReturnDate = new DateTime(2025, 9, 2),
                        TotalSeats = 31,
                        SoldSeats = 20,
                        OrderDeadline = new DateTime(2025, 8, 21),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 40,
                        DepartureDate = new DateTime(2025, 9, 4),
                        ReturnDate = new DateTime(2025, 9, 6),
                        TotalSeats = 20,
                        SoldSeats = 7,
                        OrderDeadline = new DateTime(2025, 8, 25),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 41,
                        DepartureDate = new DateTime(2025, 8, 14),
                        ReturnDate = new DateTime(2025, 8, 15),
                        TotalSeats = 21,
                        SoldSeats = 21,
                        OrderDeadline = new DateTime(2025, 8, 4),
                        MinimumParticipants = 5,
                        GroupStatus = "可候補",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 41,
                        DepartureDate = new DateTime(2025, 8, 18),
                        ReturnDate = new DateTime(2025, 8, 19),
                        TotalSeats = 18,
                        SoldSeats = 10,
                        OrderDeadline = new DateTime(2025, 8, 8),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 41,
                        DepartureDate = new DateTime(2025, 8, 22),
                        ReturnDate = new DateTime(2025, 8, 23),
                        TotalSeats = 24,
                        SoldSeats = 6,
                        OrderDeadline = new DateTime(2025, 8, 12),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 41,
                        DepartureDate = new DateTime(2025, 8, 26),
                        ReturnDate = new DateTime(2025, 8, 27),
                        TotalSeats = 25,
                        SoldSeats = 7,
                        OrderDeadline = new DateTime(2025, 8, 16),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 42,
                        DepartureDate = new DateTime(2025, 8, 14),
                        ReturnDate = new DateTime(2025, 8, 15),
                        TotalSeats = 19,
                        SoldSeats = 0,
                        OrderDeadline = new DateTime(2025, 8, 4),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 42,
                        DepartureDate = new DateTime(2025, 8, 18),
                        ReturnDate = new DateTime(2025, 8, 19),
                        TotalSeats = 31,
                        SoldSeats = 27,
                        OrderDeadline = new DateTime(2025, 8, 8),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 42,
                        DepartureDate = new DateTime(2025, 8, 22),
                        ReturnDate = new DateTime(2025, 8, 23),
                        TotalSeats = 18,
                        SoldSeats = 18,
                        OrderDeadline = new DateTime(2025, 8, 12),
                        MinimumParticipants = 5,
                        GroupStatus = "可候補",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    },

                    new GroupTravel
                    {
                        OfficialTravelDetailId = 42,
                        DepartureDate = new DateTime(2025, 8, 26),
                        ReturnDate = new DateTime(2025, 8, 27),
                        TotalSeats = 31,
                        SoldSeats = 19,
                        OrderDeadline = new DateTime(2025, 8, 16),
                        MinimumParticipants = 5,
                        GroupStatus = "可報名",
                        CreatedAt = new DateTime(2025, 3, 10),
                        UpdatedAt = new DateTime(2025, 4, 1),
                        RecordStatus = "正常"
                    }
                                        );

                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedOrdersAndRelatedDataAsync()
        {
            Console.WriteLine("SeedOrdersAndRelatedDataAsync: 方法開始執行...");

            const string firstOrderMerchantTradeNoSeedKey = "SEED_MNO_ORDER_1";
            if (await _context.Orders.AnyAsync(o => o.MerchantTradeNo == firstOrderMerchantTradeNoSeedKey))
            {
                Console.WriteLine($"SeedOrdersAndRelatedDataAsync: 標記訂單 (MerchantTradeNo: {firstOrderMerchantTradeNoSeedKey}) 已存在於資料庫中，跳過新增。");
                return;
            }
            Console.WriteLine($"SeedOrdersAndRelatedDataAsync: 標記訂單 (MerchantTradeNo: {firstOrderMerchantTradeNoSeedKey}) 不存在，繼續執行新增邏輯。");

            var member1 = await _context.Members.OrderBy(m => m.MemberId).FirstOrDefaultAsync();
            if (member1 == null)
            {
                Console.WriteLine("SeedOrdersAndRelatedDataAsync: 錯誤 - 必要的會員資料 (member1) 未找到。無法建立訂單。請確保 SeedMembersAsync 已成功執行且資料庫中有會員。");
                return;
            }
            Console.WriteLine($"SeedOrdersAndRelatedDataAsync: 成功查找到會員: ID={member1.MemberId}, Name='{member1.Name}'");

            var ordersToAdd = new List<Order>();
            var now = DateTime.Now;
            int invoiceCounter = 1;
            int generatedItemIdCounter = 1000;

            var travelStartDate = new DateTime(2025, 5, 31);
            var travelEndDate = new DateTime(2025, 6, 1);

            // --- 訂單 1: 已完成 - 官方團體旅遊 ---
            Console.WriteLine($"SeedOrdersAndRelatedDataAsync: 準備建立訂單1 (已完成 - 官方團體旅遊)。");
            var order1TotalAmount = (decimal)_random.Next(3000, 8001); // 整數金額
            var order1 = new Order
            {
                MemberId = member1.MemberId,
                TotalAmount = order1TotalAmount,
                PaymentMethod = PaymentMethod.ECPay_CreditCard,
                Status = OrderStatus.Completed,
                CreatedAt = now.AddDays(-10),
                PaymentDate = now.AddDays(-10).AddHours(1),
                InvoiceDeliveryEmail = member1.Email,
                InvoiceOption = InvoiceOption.Personal,
                Note = "訂單1備註：希望高樓層房間 - 已完成官方團體",
                OrdererName = member1.Name,
                OrdererPhone = member1.Phone ?? "0912345601",
                OrdererEmail = member1.Email,
                OrdererNationality = member1.Nationality ?? "TW",
                OrdererDocumentType = member1.DocumentType.ToString(),
                OrdererDocumentNumber = member1.DocumentNumber ?? "A123456701",
                MerchantTradeNo = firstOrderMerchantTradeNoSeedKey,
                ECPayTradeNo = "SEED_ECP_ORDER_4" // 模擬種子ECPay交易號
            };

            var orderDetail1 = new OrderDetail
            {
                Order = order1,
                Category = ProductCategory.GroupTravel,
                ItemId = generatedItemIdCounter++,
                Description = "精彩沖繩3日遊 (官方團體)",
                Quantity = 1,
                Price = order1.TotalAmount, // 假設只有一個項目，單價即總價
                TotalAmount = order1.TotalAmount,
                CreatedAt = order1.CreatedAt,
                UpdatedAt = order1.CreatedAt,
                StartDate = travelStartDate,
                EndDate = travelEndDate,
                Note = "成人票"
            };
            orderDetail1.OrderParticipants.Add(new OrderParticipant
            {
                Order = order1,
                OrderDetail = orderDetail1,
                Name = member1.Name,
                BirthDate = member1.Birthday ?? new DateTime(1990, 1, 1),
                IdNumber = member1.DocumentNumber ?? "A123456701",
                Gender = member1.Gender ?? GenderType.Female,
                Phone = member1.Phone ?? "0912345601",
                Email = member1.Email,
                DocumentType = member1.DocumentType ?? DocumentType.ID_CARD_TW,
                DocumentNumber = member1.DocumentNumber ?? "A123456701",
                Nationality = member1.Nationality ?? "TW",
                Note = "主訂購人"
            });
            order1.OrderDetails.Add(orderDetail1);
            // 將參與者同時加入 Order 的參與者列表 (如果您的業務邏輯需要這樣，否則僅加入OrderDetail的參與者列表已足夠關聯)
            // order1.OrderParticipants.Add(orderDetail1.OrderParticipants.First());


            order1.OrderInvoices.Add(new OrderInvoice
            {
                Order = order1,
                InvoiceNumber = $"SDINV{invoiceCounter++:D4}",
                BuyerName = member1.Name,
                InvoiceItemDesc = orderDetail1.Description,
                TotalAmount = order1.TotalAmount,
                CreatedAt = order1.PaymentDate ?? order1.CreatedAt,
                UpdatedAt = order1.PaymentDate ?? order1.CreatedAt,
                InvoiceType = InvoiceType.ElectronicInvoice,
                InvoiceStatus = InvoiceStatus.Opened,
                RandomCode = _random.Next(1000, 9999).ToString()
            });
            ordersToAdd.Add(order1);
            Console.WriteLine($"SeedOrdersAndRelatedDataAsync: 訂單1 (已完成 - 官方團體旅遊) 已成功加入待新增列表。");

            // --- 訂單 2: 已完成 - 客製化旅遊 ---
            Console.WriteLine($"SeedOrdersAndRelatedDataAsync: 準備建立訂單2 (已完成 - 客製化旅遊)。");
            var order2TotalAmount = (decimal)_random.Next(10000, 25001); // 整數金額
            var order2 = new Order
            {
                MemberId = member1.MemberId,
                TotalAmount = order2TotalAmount,
                PaymentMethod = PaymentMethod.ECPay_CreditCard,
                Status = OrderStatus.Completed,
                CreatedAt = now.AddDays(-8),
                PaymentDate = now.AddDays(-8).AddHours(2),
                InvoiceDeliveryEmail = "finance@example-company.test",
                InvoiceOption = InvoiceOption.Company,
                InvoiceUniformNumber = "87654321",
                InvoiceTitle = "範例測試股份有限公司",
                InvoiceAddBillingAddr = true,
                InvoiceBillingAddress = "測試市測試路123號",
                Note = "訂單2備註：需要無麩質餐點 - 已完成客製化",
                OrdererName = member1.Name,
                OrdererPhone = member1.Phone ?? "0912345602",
                OrdererEmail = member1.Email,
                OrdererNationality = member1.Nationality ?? "TW",
                OrdererDocumentType = member1.DocumentType.ToString(),
                OrdererDocumentNumber = member1.DocumentNumber ?? "A123456702",
                MerchantTradeNo = firstOrderMerchantTradeNoSeedKey,
                ECPayTradeNo = "SEED_ECP_ORDER_3",
            };

            var orderDetail2Quantity = 2;
            var orderDetail2Price = order2.TotalAmount / orderDetail2Quantity; // 確保價格是整數或合理分配
            if (order2.TotalAmount % orderDetail2Quantity != 0)
            {
                // 處理無法均分的情況，例如將餘額加到第一個項目或調整總價
                // 為了種子資料簡單，這裡假設可以均分，或者接受小数價格
                orderDetail2Price = Math.Round(order2.TotalAmount / orderDetail2Quantity, 2);
            }


            var orderDetail2 = new OrderDetail
            {
                Order = order2,
                Category = ProductCategory.CustomTravel,
                ItemId = generatedItemIdCounter++,
                Description = "北海道5日精緻遊 (客製化)",
                Quantity = orderDetail2Quantity,
                Price = orderDetail2Price, // 可能需要處理小數問題，若 Price 也需整數
                TotalAmount = order2.TotalAmount, // 確保總和正確
                CreatedAt = order2.CreatedAt,
                UpdatedAt = order2.CreatedAt,
                StartDate = travelStartDate,
                EndDate = travelEndDate,
                Note = "兩人成行，含導遊"
            };
            orderDetail2.OrderParticipants.Add(new OrderParticipant
            {
                Order = order2,
                Name = "林小花",
                BirthDate = new DateTime(1992, 5, 15),
                Gender = GenderType.Female,
                Phone = "0922334455",
                Email = "lin.flower@example.com",
                DocumentType = DocumentType.ID_CARD_TW,
                DocumentNumber = "F234567890",
                Nationality = "TW",
                Note = "參與人1"
            });
            orderDetail2.OrderParticipants.Add(new OrderParticipant
            {
                Order = order2,
                Name = "陳大明",
                BirthDate = new DateTime(1988, 10, 20),
                Gender = GenderType.Male,
                Phone = "0933445566",
                Email = "chen.bright@example.com",
                DocumentType = DocumentType.ID_CARD_TW,
                DocumentNumber = "A134567891",
                Nationality = "TW",
                Note = "參與人2"
            });
            order2.OrderDetails.Add(orderDetail2);
            // foreach(var p in orderDetail2.OrderParticipants) { order2.OrderParticipants.Add(p); }


            order2.OrderInvoices.Add(new OrderInvoice
            {
                Order = order2,
                InvoiceNumber = $"SDINV{invoiceCounter++:D4}",
                BuyerName = order2.InvoiceTitle,
                InvoiceItemDesc = orderDetail2.Description,
                TotalAmount = order2.TotalAmount,
                CreatedAt = order2.PaymentDate ?? order2.CreatedAt,
                UpdatedAt = order2.PaymentDate ?? order2.CreatedAt,
                InvoiceType = InvoiceType.Triplet,
                InvoiceStatus = InvoiceStatus.Opened,
                BuyerUniformNumber = order2.InvoiceUniformNumber
            });
            ordersToAdd.Add(order2);
            Console.WriteLine($"SeedOrdersAndRelatedDataAsync: 訂單2 (已完成 - 客製化旅遊) 已成功加入待新增列表。");


            // --- 訂單 3: 已取消 - 官方團體旅遊 ---
            Console.WriteLine($"SeedOrdersAndRelatedDataAsync: 準備建立訂單3 (已取消 - 官方團體旅遊)。");
            var order3TotalAmount = (decimal)_random.Next(4000, 9001); // 整數金額
            var order3 = new Order
            {
                MemberId = member1.MemberId,
                TotalAmount = order3TotalAmount,
                PaymentMethod = PaymentMethod.ECPay_CreditCard,
                Status = OrderStatus.Cancelled,
                CreatedAt = now.AddDays(-5),
                PaymentDate = null,
                InvoiceDeliveryEmail = member1.Email,
                InvoiceOption = InvoiceOption.Personal,
                Note = "訂單3備註：行程衝突取消 - 已取消官方團體",
                OrdererName = member1.Name,
                OrdererPhone = member1.Phone ?? "0912345603",
                OrdererEmail = member1.Email,
                OrdererNationality = member1.Nationality ?? "TW",
                OrdererDocumentType = member1.DocumentType.ToString(),
                OrdererDocumentNumber = member1.DocumentNumber ?? "A123456703",
                MerchantTradeNo = firstOrderMerchantTradeNoSeedKey,
                ECPayTradeNo = $"SEED_ECP_ORDER_1" // 即使取消也可能有嘗試付款的交易號
            };

            var orderDetail3 = new OrderDetail
            {
                Order = order3,
                Category = ProductCategory.GroupTravel,
                ItemId = generatedItemIdCounter++,
                Description = "東京動漫探索5日遊 (官方團體) - 已取消",
                Quantity = 1,
                Price = order3.TotalAmount,
                TotalAmount = order3.TotalAmount,
                CreatedAt = order3.CreatedAt,
                UpdatedAt = order3.CreatedAt,
                StartDate = travelStartDate,
                EndDate = travelEndDate,
                Note = "成人票 - 已取消"
            };
            orderDetail3.OrderParticipants.Add(new OrderParticipant
            {
                Order = order3,
                Name = "王小明",
                BirthDate = new DateTime(1995, 3, 22),
                Gender = GenderType.Male,
                Phone = "0988776655",
                Email = "wang.siao.ming@example.com",
                DocumentType = DocumentType.ID_CARD_TW,
                DocumentNumber = "L123456789",
                Nationality = "TW",
                Note = "參與人 (訂單已取消)"
            });
            order3.OrderDetails.Add(orderDetail3);
            // order3.OrderParticipants.Add(orderDetail3.OrderParticipants.First());


            order3.OrderInvoices.Add(new OrderInvoice
            {
                Order = order3,
                InvoiceNumber = null,
                BuyerName = member1.Name,
                InvoiceItemDesc = orderDetail3.Description,
                TotalAmount = order3.TotalAmount,
                CreatedAt = order3.CreatedAt,
                UpdatedAt = order3.CreatedAt,
                InvoiceType = InvoiceType.ElectronicInvoice,
                InvoiceStatus = InvoiceStatus.Voided,
                Note = "訂單取消，發票作廢"
            });
            ordersToAdd.Add(order3);
            Console.WriteLine($"SeedOrdersAndRelatedDataAsync: 訂單3 (已取消 - 官方團體旅遊) 已成功加入待新增列表。");

            // --- 訂單 4: 已取消 - 客製化旅遊 ---
            Console.WriteLine($"SeedOrdersAndRelatedDataAsync: 準備建立訂單4 (已取消 - 客製化旅遊)。");
            var order4TotalAmount = (decimal)_random.Next(12000, 30001); // 整數金額
            var order4 = new Order
            {
                MemberId = member1.MemberId,
                TotalAmount = order4TotalAmount,
                PaymentMethod = PaymentMethod.Other,
                Status = OrderStatus.Cancelled,
                CreatedAt = now.AddDays(-2),
                PaymentDate = null,
                InvoiceDeliveryEmail = "cancel@example-company.test",
                InvoiceOption = InvoiceOption.Company,
                InvoiceUniformNumber = "12348765",
                InvoiceTitle = "取消測試有限公司",
                Note = "訂單4備註：【種子】預算不足取消 - 已取消客製化",
                OrdererName = "李大文",
                OrdererPhone = "0955667704",
                OrdererEmail = "lee.david@example.com",
                OrdererNationality = "US",
                OrdererDocumentType = DocumentType.PASSPORT.ToString(),
                OrdererDocumentNumber = "USA1234504",
                MerchantTradeNo = "SEED_ECP_ORDER_2",
                ECPayTradeNo = null // 其他付款方式，可能沒有ECPay交易號
            };

            var orderDetail4 = new OrderDetail
            {
                Order = order4,
                Category = ProductCategory.CustomTravel,
                ItemId = generatedItemIdCounter++,
                Description = "歐洲古堡秘境探索 (客製化) - 已取消",
                Quantity = 1,
                Price = order4.TotalAmount,
                TotalAmount = order4.TotalAmount,
                CreatedAt = order4.CreatedAt,
                UpdatedAt = order4.CreatedAt,
                StartDate = travelStartDate,
                EndDate = travelEndDate,
                Note = "單人豪華客製 - 已取消"
            };
            orderDetail4.OrderParticipants.Add(new OrderParticipant
            {
                Order = order4,
                Name = "李大文",
                BirthDate = new DateTime(1985, 12, 1),
                Gender = GenderType.Male,
                Phone = "0955667704",
                Email = "lee.david@example.com",
                DocumentType = DocumentType.PASSPORT,
                DocumentNumber = "USA1234504",
                PassportSurname = "LEE",
                PassportGivenName = "DAVID",
                PassportExpireDate = new DateTime(2028, 1, 1),
                Nationality = "US",
                Note = "參與人 (訂單已取消)"
            });
            order4.OrderDetails.Add(orderDetail4);
            // order4.OrderParticipants.Add(orderDetail4.OrderParticipants.First());


            order4.OrderInvoices.Add(new OrderInvoice
            {
                Order = order4,
                InvoiceNumber = null,
                BuyerName = order4.InvoiceTitle,
                InvoiceItemDesc = orderDetail4.Description,
                TotalAmount = order4.TotalAmount,
                CreatedAt = order4.CreatedAt,
                UpdatedAt = order4.CreatedAt,
                InvoiceType = InvoiceType.Triplet,
                InvoiceStatus = InvoiceStatus.Voided,
                BuyerUniformNumber = order4.InvoiceUniformNumber,
                Note = "訂單取消，公司發票作廢"
            });
            ordersToAdd.Add(order4);
            Console.WriteLine($"SeedOrdersAndRelatedDataAsync: 訂單4 (已取消 - 客製化旅遊) 已成功加入待新增列表。");

            if (ordersToAdd.Any())
            {
                Console.WriteLine($"SeedOrdersAndRelatedDataAsync: 即將新增 {ordersToAdd.Count} 筆訂單到資料庫...");
                try
                {
                    _context.Orders.AddRange(ordersToAdd);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"SeedOrdersAndRelatedDataAsync: 已成功新增 {ordersToAdd.Count} 筆訂單種子資料。");
                }
                catch (DbUpdateException ex)
                {
                    Console.WriteLine($"SeedOrdersAndRelatedDataAsync: 新增訂單到資料庫時發生 DbUpdateException 錯誤: {ex.ToString()}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"SeedOrdersAndRelatedDataAsync: 內部錯誤詳情: {ex.InnerException.ToString()}");
                    }
                    foreach (var entry in ex.Entries)
                    {
                        Console.WriteLine($"DbUpdateException Entry: Entity {entry.Entity.GetType().Name} in state {entry.State} could not be saved.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"SeedOrdersAndRelatedDataAsync: 新增訂單到資料庫時發生一般錯誤: {ex.ToString()}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"SeedOrdersAndRelatedDataAsync: 內部錯誤詳情: {ex.InnerException.ToString()}");
                    }
                }
            }
            else
            {
                Console.WriteLine("SeedOrdersAndRelatedDataAsync: ordersToAdd 列表為空，沒有新的訂單種子資料被新增。請檢查上述日誌確認前置資料是否正確查詢到，以及其他依賴的種子方法是否成功執行。");
            }
            Console.WriteLine("SeedOrdersAndRelatedDataAsync: 方法執行完畢。");
        }


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



        //證件選單假資料 富成
        //固態假資料,表示非同步操作的關鍵
        public async Task SeedDocumentMenuAsync()
        //定義了一個公開的**、非同步的方法。這個方法的主要作用是初始化(設定好起點，確保一個系統、元件或資料從一開始就處於正確、可用、或符合預期行為的狀態。)（或稱為「種子」）資料庫中的 DocumentMenu 表格資料。
        {
            if (!_context.DocumentMenus.Any())
            //意思是「如果 DocumentMenu 表格中沒有任何資料」，那麼就執行後續的程式碼塊。這樣做的目的是為了防止重複添加資料，確保只有在表格為空時才進行初始化。
            {
                _context.DocumentMenus.AddRange(
                    //.AddRange(...): 這是 Entity Framework Core 提供的一個方法，用於向資料庫上下文的集合中添加一個或多個實體。這裡它會準備將一個新的 DocumentMenu 物件添加到表格中。
                    new DocumentMenu
                    //是在創建一個新的 DocumentMenu 物件，並為其屬性賦值
                    {//這裡的東西是要從資料庫抓到頁面顯示給使用者看的
                        RocPassportOption = "中華民國護照",
                        ForeignVisaOption = "",
                        ApplicationType = ApplicationTypeEnum.passport,
                        ProcessingItem = "新辦/更換(14歲以上)",
                        CaseType = CaseTypeEnum.general,
                        ProcessingDays = "16個工作天",
                        DocumentValidityPeriod = "效期10年",
                        StayDuration = "",
                        Fee = "TWD 1,700",

                    }
                );

                await _context.SaveChangesAsync();
                //await: 這是 async/await 模式的一部分。它會等待 _context.SaveChangesAsync() 方法完成。_context.SaveChangesAsync(): 這個方法會將資料庫上下文中所做的所有更改（例如這裡的添加操作）異步地保存到實際的資料庫中。
            }
        }

        //填寫訂單假資料 富成
        public async Task SeedOrderFormAsync()

        {
            if (!_context.OrderForms.Any())

            {
                _context.OrderForms.AddRange(

                    new OrderForm

                    {
                        MemberId = 11110,
                        DocumentMenuId = 1,
                        DepartureDate = new DateTime(2025, 6, 12),
                        ProcessingQuantity = 1,
                        ChineseSurname = "金",
                        ChineseName = "城武",
                        EnglishSurname = "JIN",
                        EnglishName = "CHENG-WU",
                        Gender = GenderEnum.Male,
                        BirthDate = new DateTime(1990, 1, 1),
                        ContactPersonName = "梁朝偉",
                        ContactPersonEmail = "abc123@gmail.com",
                        ContactPersonPhoneNumber = "0912345678",
                        PickupMethodOption = PickupMethodEnum.門市,
                        MailingCity = "",
                        MailingDetailAddress = "",
                        StoreDetailAddress = "高雄市前金區中正四路211號8樓之1",
                        TaxIdOption = TaxIdOptionEnum.不需要,
                        CompanyName = "",
                        TaxIdNumber = "",
                        OrderCreationTime = DateTime.Now,

                    }
                );

                await _context.SaveChangesAsync();

            }
        }

        //付款假資料 富成
        public async Task SeedPaymentMethodAsync()

        {
            if (!_context.Payments.Any())

            {
                _context.Payments.AddRange(

                    new Payment

                    {
                        OrderFormId = 1,
                        DocumentMenuId = 1,
                        PaymentMethod = PaymentMethodEnum.信用卡,
                    }
                );

                await _context.SaveChangesAsync();

            }
        }

        //完成訂單明細假資料 富成
        public async Task SeedCompletedOrderDetailAsync()
        {
            if (!_context.CompletedOrderDetails.Any())

            {
                _context.CompletedOrderDetails.AddRange(

                    new CompletedOrderDetail

                    {
                        DocumentMenuId = 1,
                        OrderFormId = 1,
                    }
                );

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

        public async Task SeedCommentsAsync()
        {
            var completedOrderDetail = await _context.OrderDetails
                .Include(od => od.Order)
                .FirstOrDefaultAsync(od => od.Order.Status == OrderStatus.Completed);

            if (completedOrderDetail == null)
                return;

            var alreadyExists = await _context.Comments.AnyAsync(c =>
                c.OrderDetailId == completedOrderDetail.OrderDetailId &&
                c.MemberId == completedOrderDetail.Order.MemberId);

            if (alreadyExists)
                return;

            var comment = new Comment
            {
                MemberId = completedOrderDetail.Order.MemberId,
                OrderDetailId = completedOrderDetail.OrderDetailId,
                Category = completedOrderDetail.Category,
                Rating = 4,
                Content = "這次行程真的很棒，導遊很專業！",
                Status = CommentStatus.Visible,
                CreatedAt = DateTime.Now
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
        }

    }
}
