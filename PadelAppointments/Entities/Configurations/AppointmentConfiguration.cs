using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PadelAppointments.Converters;

namespace PadelAppointments.Entities.Configurations
{
    public sealed class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
    {
        public void Configure(EntityTypeBuilder<Appointment> builder)
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

            builder
                .HasMany(a => a.Checks)
                .WithOne(c => c.Appointment)
                .HasForeignKey(c => c.AppointmentId);
        }
    }
}
