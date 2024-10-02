using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PadelAppointments.Converters;

namespace PadelAppointments.Entities.Configurations
{
    public sealed class CheckConfiguration : IEntityTypeConfiguration<Check>
    {
        public void Configure(EntityTypeBuilder<Check> builder)
        {
            builder.ToTable("Checks");

            builder.HasKey(c => new { c.Id });
            builder.Property(c => c.Id).UseIdentityColumn();

            // Date is a DateOnly property and date on database
            builder.Property(c => c.Date)
                .HasConversion<DateOnlyConverter, DateOnlyComparer>();

            builder
                .HasOne(c => c.Appointment)
                .WithMany(a => a.Checks)
                .HasForeignKey(i => i.AppointmentId);

            builder
                .HasMany(c => c.ItemsConsumed)
                .WithOne(i => i.Check)
                .HasForeignKey(c => c.CheckId);

            builder.HasIndex(s => new { s.Date, s.AppointmentId }).IsUnique();
        }
    }
}
