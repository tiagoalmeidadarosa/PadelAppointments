using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PadelAppointments.Converters;

namespace PadelAppointments.Entities.Configurations
{
    public sealed class AgendaConfiguration : IEntityTypeConfiguration<Agenda>
    {
        public void Configure(EntityTypeBuilder<Agenda> builder)
        {
            builder.ToTable("Agendas");

            builder.HasKey(a => new { a.Id });
            builder.Property(a => a.Id).UseIdentityColumn();

            builder
                .Property(a => a.Name)
                .IsRequired()
                .HasColumnType("nvarchar(128)");

            // Time is a TimeOnly property and time on database
            builder.Property(a => a.StartsAt)
                .HasConversion<TimeOnlyConverter, TimeOnlyComparer>();

            // Time is a TimeOnly property and time on database
            builder.Property(a => a.EndsAt)
                .HasConversion<TimeOnlyConverter, TimeOnlyComparer>();

            // Time is a TimeOnly property and time on database
            builder.Property(a => a.Interval)
                .HasConversion<TimeOnlyConverter, TimeOnlyComparer>();

            builder
                .HasMany(ag => ag.Appointments)
                .WithOne(ap => ap.Agenda)
                .HasForeignKey(ap => ap.AgendaId);
        }
    }
}
