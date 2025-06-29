﻿using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Models;

namespace TravelAgency.Shared.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Member> Members { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<EmailVerificationCode> ResetPasswords { get; set; }
        public DbSet<EmailVerificationCode> EmailVerificationCodes { get; set; }

        public DbSet<MemberFavoriteTraveler> MemberFavoriteTravelers { get; set; }
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
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<Accommodation> Accommodations { get; set; }
        public DbSet<Attraction> Attractions { get; set; }
        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<Transport> Transports { get; set; }
        public DbSet<Collect> Collects { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<OrderParticipant> OrderParticipants { get; set; }
        public DbSet<Agency> Agencies { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<OrderInvoice> OrderInvoices { get; set; }
        public DbSet<CompletedOrderDetail> CompletedOrderDetails { get; set; }
        public DbSet<DocumentMenu> DocumentMenus { get; set; }
        public DbSet<OrderForm> OrderForms { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
