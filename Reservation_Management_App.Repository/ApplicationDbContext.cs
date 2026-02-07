using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Reservation_Management_App.Domain.DomainModels;
using Reservation_Management_App.Domain.Identity;

namespace Reservation_Management_App.Repository
{
    public class ApplicationDbContext : IdentityDbContext<Reservation_Management_AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public virtual DbSet<Event> Events { get; set; }
        public virtual DbSet<Location> Locations { get; set; }
        public virtual DbSet<Performer> Performers { get; set; }
        public virtual DbSet<Reservation> Reservations { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Event -> MainAct (Performer)
            builder.Entity<Event>()
                .HasOne(e => e.MainAct)
                .WithMany(p => p.EventsAsMainAct)
                .HasForeignKey(e => e.MainActId)
                .OnDelete(DeleteBehavior.Restrict);

            // Event -> Dj (Performer)
            builder.Entity<Event>()
                .HasOne(e => e.Dj)
                .WithMany(p => p.EventsAsDj)
                .HasForeignKey(e => e.DjId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Event>()
                .HasOne(e => e.Location)
                .WithMany(l => l.Events)
                .HasForeignKey(e => e.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
