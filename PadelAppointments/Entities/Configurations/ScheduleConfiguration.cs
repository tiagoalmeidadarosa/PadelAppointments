using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PadelAppointments.Converters;

namespace PadelAppointments.Entities.Configurations
{
    public sealed class ScheduleConfiguration : IEntityTypeConfiguration<Schedule>
    {
        public void Configure(EntityTypeBuilder<Schedule> builder)
        {
            builder.ToTable("Schedules");

            builder.HasKey(s => new { s.Id });
            builder.Property(s => s.Id).UseIdentityColumn();

            builder
                .HasOne(s => s.Appointment)
                .WithMany(a => a.Schedules)
                .HasForeignKey(s => s.AppointmentId);

            // Date is a DateOnly property and date on database
            builder.Property(s => s.Date)
                .HasConversion<DateOnlyConverter, DateOnlyComparer>();

            // Time is a TimeOnly property and time on database
            builder.Property(s => s.Time)
                .HasConversion<TimeOnlyConverter, TimeOnlyComparer>();
        }
    }
}
