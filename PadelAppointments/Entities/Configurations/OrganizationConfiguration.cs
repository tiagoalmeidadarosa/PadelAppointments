using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PadelAppointments.Entities.Configurations
{
    public sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
    {
        public void Configure(EntityTypeBuilder<Organization> builder)
        {
            builder.ToTable("Organizations");

            builder.HasKey(a => new { a.Id });

            builder
                .Property(a => a.Name)
                .IsRequired()
                .HasColumnType("nvarchar(128)");

            builder
                .HasMany(o => o.Agendas)
                .WithOne(a => a.Organization)
                .HasForeignKey(a => a.OrganizationId);
        }
    }
}
