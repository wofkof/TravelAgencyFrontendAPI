using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.Data.Configurations;
using TravelAgencyFrontendAPI.Models;

namespace TravelAgencyFrontendAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Member> Members { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<ResetPassword> ResetPasswords { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<ChatRoom> ChatRooms { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<MessageMedia> MessageMedias { get; set; }
        public DbSet<Sticker> Stickers { get; set; }
        public DbSet<CallLog> CallLogs { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<OfficialTravel> OfficialTravels { get; set; }
        public DbSet<OfficialTravelDetail> OfficialTravelDetails { get; set; }
        public DbSet<Region> Regions { get; set; }
        public DbSet<GroupTravel> GroupTravels { get; set; }
        public DbSet<OfficialTravelSchedule> OfficialTravelSchedules { get; set; }
        public DbSet<TravelSupplier> TravelSuppliers { get; set; }
        public DbSet<OfficialAccommodation> OfficialAccommodations { get; set; }
        public DbSet<OfficialAttraction> OfficialAttractions { get; set; }
        public DbSet<OfficialRestaurant> OfficialRestaurants { get; set; }
        public DbSet<CustomTravel> CustomTravels { get; set; }
        public DbSet<CustomTravelContent> CustomTravelContents { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<TravelRecord> TravelRecords { get; set; }
        public DbSet<Country> Countrys { get; set; }
        public DbSet<VisaType> VisaTypes { get; set; }
        public DbSet<Requirement> Requirements { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<VisaInformation> VisaInformations { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<Accommodation> Accommodations { get; set; }
        public DbSet<Attraction> Attractions { get; set; }
        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<Transport> Transports { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new MemberConfig());
            modelBuilder.ApplyConfiguration(new EmployeeConfig());
            modelBuilder.ApplyConfiguration(new RoleConfig());
            modelBuilder.ApplyConfiguration(new ResetPasswordConfig());
            modelBuilder.ApplyConfiguration(new ParticipantConfig());
            modelBuilder.ApplyConfiguration(new ChatRoomConfig());
            modelBuilder.ApplyConfiguration(new MessageConfig());
            modelBuilder.ApplyConfiguration(new MessageMediaConfig());
            modelBuilder.ApplyConfiguration(new StickerConfig());
            modelBuilder.ApplyConfiguration(new CallLogConfig());
            modelBuilder.ApplyConfiguration(new AnnouncementConfig());
            modelBuilder.ApplyConfiguration(new OfficialTravelConfig());
            modelBuilder.ApplyConfiguration(new OfficialTravelDetailConfig());
            modelBuilder.ApplyConfiguration(new RegionConfig());
            modelBuilder.ApplyConfiguration(new GroupTravelConfig());
            modelBuilder.ApplyConfiguration(new OfficialTravelScheduleConfig());
            modelBuilder.ApplyConfiguration(new TravelSupplierConfig());
            modelBuilder.ApplyConfiguration(new OfficialAccommodationConfig());
            modelBuilder.ApplyConfiguration(new OfficialAttractionConfig());
            modelBuilder.ApplyConfiguration(new OfficialRestaurantConfig());
            modelBuilder.ApplyConfiguration(new CustomTravelConfig());
            modelBuilder.ApplyConfiguration(new CustomTravelContentConfig());
            modelBuilder.ApplyConfiguration(new OrderConfig());
            modelBuilder.ApplyConfiguration(new TravelRecordConfig());
            modelBuilder.ApplyConfiguration(new CountryConfig());
            modelBuilder.ApplyConfiguration(new VisaTypeConfig());
            modelBuilder.ApplyConfiguration(new RequirementConfig());
            modelBuilder.ApplyConfiguration(new DocumentConfig());
            modelBuilder.ApplyConfiguration(new VisaInformationConfig());
            modelBuilder.ApplyConfiguration(new PermissionConfig());
            modelBuilder.ApplyConfiguration(new RolePermissionConfig());
            modelBuilder.ApplyConfiguration(new CityConfig());
            modelBuilder.ApplyConfiguration(new DistrictConfig());
            modelBuilder.ApplyConfiguration(new AccommodationConfig());
            modelBuilder.ApplyConfiguration(new AttractionConfig());
            modelBuilder.ApplyConfiguration(new RestaurantConfig());
            modelBuilder.ApplyConfiguration(new TransportConfig());

        }
    }
}
