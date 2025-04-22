using TravelAgencyFrontendAPI.Models;

namespace TravelAgencyFrontendAPI.Data
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
        }

        private async Task SeedRolesAsync()
        {
            if (!_context.Roles.Any())
            {
                _context.Roles.Add(new Role
                {
                    RoleName = "系統管理員"
                });
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedEmployeesAsync()
        {
            if (!_context.Employees.Any())
            {
                var roleId = _context.Roles.First().RoleId;

                _context.Employees.Add(new Employee
                {
                    RoleId = roleId,
                    Name = "測試員工",
                    Password = "Test@123",
                    Email = "employee@test.com",
                    Phone = "0900000000",
                    BirthDate = new DateTime(1990, 1, 1),
                    Note = "這是測試用員工"
                });

                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedMembersAsync()
        {
            if (!_context.Members.Any())
            {
                _context.Members.Add(new Member
                {
                    Name = "測試會員",
                    Email = "member@test.com",
                    Phone = "0911111111",
                    PasswordHash = "FakeHashValue",
                    PasswordSalt = "FakeSaltValue",
                    GoogleId = null,
                    IsBlacklisted = false,
                    Note = "這是測試用會員"
                });

                await _context.SaveChangesAsync();
            }
        }
    }
}
