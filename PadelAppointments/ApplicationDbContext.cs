using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PadelAppointments.Converters;
using PadelAppointments.Entities;
using PadelAppointments.Models.Authentication;

namespace PadelAppointments
{
    class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Court> Courts { get; set; } = null!;
        public DbSet<Appointment> Appointments { get; set; } = null!;
        public DbSet<Schedule> Schedules { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Court>(builder =>
            {
                builder.ToTable("Courts");

                builder.HasKey(c => new { c.Id });
                builder.Property(c => c.Id).UseIdentityColumn();

                builder
                    .Property(c => c.Name)
                    .IsRequired()
                    .HasColumnType("nvarchar(128)");

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

                builder
                    .HasMany(c => c.Schedules)
                    .WithOne(s => s.Court)
                    .HasForeignKey(s => s.CourtId);
            });

            modelBuilder.Entity<Appointment>(builder =>
            {
                builder.ToTable("Appointments");

                builder.HasKey(a => new { a.Id });
                builder.Property(a => a.Id).UseIdentityColumn();

                // Date is a DateOnly property and date on database
                builder.Property(a => a.Date)
                    .HasConversion<DateOnlyConverter, DateOnlyComparer>();

                builder
                    .Property(a => a.CustomerName)
                    .IsRequired()
                    .HasColumnType("nvarchar(128)");

                builder
                    .Property(a => a.CustomerPhoneNumber)
                    .IsRequired()
                    .HasColumnType("nvarchar(32)");

                builder
                    .Property(a => a.Price)
                    .IsRequired();

                builder
                    .HasMany(a => a.Schedules)
                    .WithOne(s => s.Appointment)
                    .HasForeignKey(s => s.AppointmentId);
            });

            modelBuilder.Entity<Schedule>(builder =>
            {
                builder.ToTable("Schedules");

                builder.HasKey(a => new { a.Id });
                builder.Property(a => a.Id).UseIdentityColumn();

                builder
                    .HasOne(s => s.Appointment)
                    .WithMany(a => a.Schedules)
                    .HasForeignKey(s => s.AppointmentId);

                builder
                    .HasOne(s => s.Court)
                    .WithMany(a => a.Schedules)
                    .HasForeignKey(s => s.CourtId);

                // Date is a DateOnly property and date on database
                builder.Property(a => a.Date)
                    .HasConversion<DateOnlyConverter, DateOnlyComparer>();

                // Time is a TimeOnly property and time on database
                builder.Property(a => a.Time)
                    .HasConversion<TimeOnlyConverter, TimeOnlyComparer>();

                builder.HasIndex(a => new { a.Date, a.Time, a.CourtId }).IsUnique();
            });
        }
    }
}
