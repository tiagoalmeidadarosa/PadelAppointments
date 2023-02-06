using Microsoft.EntityFrameworkCore;
using PadelAppointments.Converters;
using PadelAppointments.Entities;

namespace PadelAppointments
{
    class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Court> Courts { get; set; } = null!;
        public DbSet<Appointment> Appointments { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Court>(builder =>
            {
                builder.ToTable("Courts");

                builder.HasKey(x => new { x.Id });
                builder.Property(p => p.Id).UseIdentityColumn();

                builder.HasData(
                    new()
                    {
                        Id = 1,
                        Name = "Quadra1",
                    },
                    new()
                    {
                        Id = 2,
                        Name = "Quadra2",
                    },
                    new()
                    {
                        Id = 3,
                        Name = "Quadra3",
                    }
                );
            });

            modelBuilder.Entity<Appointment>(builder =>
            {
                builder.ToTable("Appointments");

                builder.HasKey(x => new { x.Id });
                builder.Property(p => p.Id).UseIdentityColumn();

                builder
                    .HasOne(a => a.Court)
                    .WithMany(c => c.Appointments)
                    .HasForeignKey(a => a.CourtId);

                // Date is a DateOnly property and date on database
                builder.Property(x => x.Date)
                    .HasConversion<DateOnlyConverter, DateOnlyComparer>();

                // Time is a TimeOnly property and time on database
                builder.Property(x => x.Time)
                    .HasConversion<TimeOnlyConverter, TimeOnlyComparer>();

                builder.HasIndex(x => new { x.Date, x.Time }).IsUnique();
            });
        }
    }
}
