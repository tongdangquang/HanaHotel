using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HanaHotel.EntityLayer.Concrete;

namespace HanaHotel.DataAccessLayer.Concrete
{
    public class DataContext : IdentityDbContext<User, Role, int>
    {
        public DbSet<About> Abouts { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Guest> Guests { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<MessageCategory> MessageCategories { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<PromotionDetail> PromotionDetails { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<RoomDetail> RoomDetails { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ServiceDetail> ServiceDetails { get; set; }
        public DbSet<User> Users { get; set; } // Identity users stored here
        public DbSet<Role> Roles { get; set; } // Identity roles stored here
        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<HotelDetail> HotelDetails { get; set; }

		public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        // Nếu bạn vẫn cần OnConfiguring (ví dụ để dev), giữ nhưng khi dùng AddDbContext từ WebUI, options sẽ được cung cấp.
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = "Server=db31658.public.databaseasp.net; Database=db31658; User Id=db31658; Password=12345678; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;";
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // nếu cần cấu hình thêm cho identity hoặc các entity, bổ sung ở đây
            // ============================
            // PROMOTION
            // ============================
            modelBuilder.Entity<Promotion>()
                .HasKey(p => p.PromotionID);

            modelBuilder.Entity<Promotion>()
                .Property(p => p.PromotionName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Promotion>()
                .Property(p => p.DiscountAmount)
                .HasColumnType("decimal(18,2)");

            // ============================
            // PROMOTION DETAIL
            // ============================
            modelBuilder.Entity<PromotionDetail>()
                .HasOne(pd => pd.Promotion)
                .WithMany(p => p.PromotionDetails)
                .HasForeignKey(pd => pd.PromotionID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PromotionDetail>()
                .HasOne(pd => pd.Room)
                .WithMany(r => r.PromotionDetails) // hoặc .WithMany(r => r.PromotionDetails) nếu Room có collection
                .HasForeignKey(pd => pd.RoomId)
                .HasPrincipalKey(r => r.Id)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<ServiceDetail>()
                .HasKey(rs => new { rs.RoomId, rs.ServiceId });

            modelBuilder.Entity<ServiceDetail>()
                .HasOne(rs => rs.Room)
                .WithMany(r => r.RoomServices)
                .HasForeignKey(rs => rs.RoomId);

            modelBuilder.Entity<ServiceDetail>()
                .HasOne(rs => rs.Service)
                .WithMany(s => s.RoomServices)
                .HasForeignKey(rs => rs.ServiceId);


        }
    }
}
