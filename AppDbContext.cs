using System;
using System.Linq;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sa3dny.Data.Models;
using Sa3dny.Models;

namespace Sa3dny.Data
{
    // ✅ التعديل: الوراثة من IdentityDbContext<ApplicationUser> لدعم الموديل المخصص
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Provider> Providers { get; set; }
        public DbSet<Requests> Requests { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<ProviderOffer> ProviderOffers { get; set; } // ✅ إضافة ProviderOffers
        public DbSet<Admin> Admin { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Home_Service> Home_Services { get; set; }
        public DbSet<Edu_Service> Edu_Services { get; set; }
        public DbSet<Review> reviews { get; set; }
        public DbSet<Provider_Service> Provider_Services { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Governorate> Governorates { get; set; }
        public DbSet<ServiceCategory> ServiceCategories { get; set; }
        public DbSet<Notification> Notifications { get; set; } // ✅ إضافة Notifications
        public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Payment>()
            .Property(p => p.Amount)
            .HasPrecision(18, 2);

            // ✅ ضبط العلاقات لمنع الـ Cascade Cycles (مهم جداً للـ SQL Server)
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }

            // ✅ استثناء علاقات الربط مع اليوزر (حذف اليوزر يحذف ملفه الشخصي)
            modelBuilder.Entity<Customer>()
                .HasOne(c => c.User).WithOne().HasForeignKey<Customer>(c => c.UserId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Provider>()
                .HasOne(p => p.User).WithOne().HasForeignKey<Provider>(p => p.UserId).OnDelete(DeleteBehavior.Cascade);

            // ==========================================
            // ✅ الحل البرمجي لمشكلة العلاقة بين العروض والطلبات
            // ==========================================
            modelBuilder.Entity<ProviderOffer>()
                .HasOne(po => po.Request)        // العرض يرتبط بطلب واحد
                .WithMany(r => r.Offers)         // الطلب له قائمة عروض (Offers)
                .HasForeignKey(po => po.RequestId) // المفتاح الأجنبي هو RequestId
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Provider_Service>()
                .HasKey(ps => new { ps.ProviderId, ps.ServiceId }); // ✅ تحديث أسماء الحقول لتناسب الموديل الجديد

            modelBuilder.Entity<Requests>()
                .Property(r => r.Total_Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Service>()
                .Property(s => s.Min_price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ProviderOffer>() // ✅ إضافة Precision لجدول العروض
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            // ✅ بذرة البيانات (Seed Data)
            modelBuilder.Entity<ServiceCategory>().HasData(
                new ServiceCategory { Id_Category = 1, Name_Category = "Home Services" },
                new ServiceCategory { Id_Category = 2, Name_Category = "Educational Services" },
                new ServiceCategory { Id_Category = 3, Name_Category = "Healthcare Services" }
            );

            modelBuilder.Entity<Service>().HasData(
                new Service { service_id = Guid.Parse("11111111-1111-1111-1111-111111111111"), service_name = "Cleaning", Description = "Home cleaning service", Min_price = 0 },
                new Service { service_id = Guid.Parse("22222222-2222-2222-2222-222222222222"), service_name = "Plumbing", Description = "Plumbing service", Min_price = 0 },
                new Service { service_id = Guid.Parse("33333333-3333-3333-3333-333333333333"), service_name = "Electricity", Description = "Electrical service", Min_price = 0 },
                new Service { service_id = Guid.Parse("44444444-4444-4444-4444-444444444444"), service_name = "Carpentry", Description = "Carpentry service", Min_price = 0 },
                new Service { service_id = Guid.Parse("55555555-5555-5555-5555-555555555555"), service_name = "Word / Report", Description = "Word and report writing", Min_price = 0 },
                new Service { service_id = Guid.Parse("66666666-6666-6666-6666-666666666666"), service_name = "Presentation", Description = "Presentation design", Min_price = 0 },
                new Service { service_id = Guid.Parse("77777777-7777-7777-7777-777777777777"), service_name = "Excel", Description = "Excel sheets service", Min_price = 0 },
                new Service { service_id = Guid.Parse("88888888-8888-8888-8888-888888888888"), service_name = "CV Creation", Description = "CV writing service", Min_price = 0 },
                new Service { service_id = Guid.Parse("99999999-9999-9999-9999-999999999999"), service_name = "Home Nursing", Description = "Nursing at home", Min_price = 0 },
                new Service { service_id = Guid.Parse("10101010-1010-1010-1010-101010101010"), service_name = "Doctor Visit", Description = "Doctor home visit", Min_price = 0 },
                new Service { service_id = Guid.Parse("12121212-1212-1212-1212-121212121212"), service_name = "Injection Service", Description = "Injection at home", Min_price = 0 },
                new Service { service_id = Guid.Parse("13131313-1313-1313-1313-131313131313"), service_name = "Follow-up", Description = "Medical follow-up", Min_price = 0 }
            );

            modelBuilder.Entity<Governorate>().HasData(
                new Governorate { Id_Governorate = 1, Name_Governorate = "Cairo" },
                new Governorate { Id_Governorate = 2, Name_Governorate = "Giza" },
                new Governorate { Id_Governorate = 3, Name_Governorate = "Alexandria" },
                new Governorate { Id_Governorate = 4, Name_Governorate = "Assiut" },
                new Governorate { Id_Governorate = 5, Name_Governorate = "Aswan" },
                new Governorate { Id_Governorate = 6, Name_Governorate = "Luxor" },
                new Governorate { Id_Governorate = 7, Name_Governorate = "Sohag" },
                new Governorate { Id_Governorate = 8, Name_Governorate = "Qena" },
                new Governorate { Id_Governorate = 9, Name_Governorate = "Minya" },
                new Governorate { Id_Governorate = 10, Name_Governorate = "Beni Suef" },
                new Governorate { Id_Governorate = 11, Name_Governorate = "Fayoum" },
                new Governorate { Id_Governorate = 12, Name_Governorate = "Dakahlia" },
                new Governorate { Id_Governorate = 13, Name_Governorate = "Sharqia" },
                new Governorate { Id_Governorate = 14, Name_Governorate = "Gharbia" },
                new Governorate { Id_Governorate = 15, Name_Governorate = "Monufia" },
                new Governorate { Id_Governorate = 16, Name_Governorate = "Qalyubia" },
                new Governorate { Id_Governorate = 17, Name_Governorate = "Kafr El Sheikh" },
                new Governorate { Id_Governorate = 18, Name_Governorate = "Beheira" },
                new Governorate { Id_Governorate = 19, Name_Governorate = "Damietta" },
                new Governorate { Id_Governorate = 20, Name_Governorate = "Port Said" },
                new Governorate { Id_Governorate = 21, Name_Governorate = "Ismailia" },
                new Governorate { Id_Governorate = 22, Name_Governorate = "Suez" },
                new Governorate { Id_Governorate = 23, Name_Governorate = "North Sinai" },
                new Governorate { Id_Governorate = 24, Name_Governorate = "South Sinai" },
                new Governorate { Id_Governorate = 25, Name_Governorate = "Red Sea" },
                new Governorate { Id_Governorate = 26, Name_Governorate = "New Valley" },
                new Governorate { Id_Governorate = 27, Name_Governorate = "Matruh" }
            );

            modelBuilder.Entity<Location>().HasData(
                new Location { Id_Location = 1, Name_Location = "Ferial" },
                new Location { Id_Location = 2, Name_Location = "Mousna3 Sayed" },
                new Location { Id_Location = 3, Name_Location = "Governorate Street" },
                new Location { Id_Location = 4, Name_Location = "Libraries" },
                new Location { Id_Location = 5, Name_Location = "Asmaa Allah Square" },
                new Location { Id_Location = 6, Name_Location = "Station" },
                new Location { Id_Location = 7, Name_Location = "Fateh" },
                new Location { Id_Location = 8, Name_Location = "Hamraa" }
            );
        }
    }
}