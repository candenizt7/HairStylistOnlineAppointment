using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using OnlineAppointment.Models;
   
namespace OnlineAppointment.Data
{
    // IdentityDbContext<ApplicationUser> ile Identity tablolarını da ekliyoruz
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // Hair Stylist randevu sistemi tabloları
        public DbSet<Business> Businesses { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // Identity tabloları için gerekli

            // Örnek: randevu sorgularını hızlandırmak için index ekleyebiliriz
            builder.Entity<Appointment>()
                   .HasIndex(a => new { a.ServiceId, a.Start });
            builder.Entity<Service>().HasIndex(s => s.Name).IsUnique();

        }
    }

}
