using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PadelAppointments.Converters;
using PadelAppointments.Entities;
using PadelAppointments.Enums;
using PadelAppointments.Models.Authentication;

namespace PadelAppointments
{
    class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Court> Courts { get; set; } = null!;
        public DbSet<Appointment> Appointments { get; set; } = null!;
        public DbSet<Recurrence> Recurrences { get; set; } = null!;

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
            });

            modelBuilder.Entity<Appointment>(builder =>
            {
                builder.ToTable("Appointments");

                builder.HasKey(a => new { a.Id });
                builder.Property(a => a.Id).UseIdentityColumn();

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
                    .HasOne(a => a.Court)
                    .WithMany(c => c.Appointments)
                    .HasForeignKey(a => a.CourtId);

                builder
                    .HasOne(a => a.Recurrence)
                    .WithMany(r => r.Appointments)
                    .HasForeignKey(a => a.RecurrenceId);

                // Date is a DateOnly property and date on database
                builder.Property(a => a.Date)
                    .HasConversion<DateOnlyConverter, DateOnlyComparer>();

                // Time is a TimeOnly property and time on database
                builder.Property(a => a.Time)
                    .HasConversion<TimeOnlyConverter, TimeOnlyComparer>();

                builder.HasIndex(a => new { a.Date, a.Time, a.CourtId }).IsUnique();
            });

            modelBuilder.Entity<Recurrence>(builder =>
            {
                builder.ToTable("Recurrences");

                builder.HasKey(r => new { r.Id });
                builder.Property(r => r.Id).UseIdentityColumn();

                builder
                    .HasMany(r => r.Appointments)
                    .WithOne(a => a.Recurrence)
                    .HasForeignKey(a => a.RecurrenceId);

                builder
                    .Property(r => r.Type)
                    .IsRequired()
                    .HasColumnType("varchar(32)")
                    .HasConversion(
                        r => r.ToString(),
                        r => (RecurrenceType)Enum.Parse(typeof(RecurrenceType), r));
            });
        }
    }
}
