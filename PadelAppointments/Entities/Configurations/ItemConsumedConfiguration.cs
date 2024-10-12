using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PadelAppointments.Entities.Configurations
{
    public sealed class ItemConsumedConfiguration : IEntityTypeConfiguration<ItemConsumed>
    {
        public void Configure(EntityTypeBuilder<ItemConsumed> builder)
        {
            builder.ToTable("ItemsConsumed");

            builder.HasKey(i => new { i.Id });
            builder.Property(i => i.Id).UseIdentityColumn();

            builder
                .Property(a => a.Description)
                .IsRequired()
                .HasColumnType("nvarchar(256)");

            builder
                .HasOne(i => i.Check)
                .WithMany(a => a.ItemsConsumed)
                .HasForeignKey(i => i.CheckId);
        }
    }
}
