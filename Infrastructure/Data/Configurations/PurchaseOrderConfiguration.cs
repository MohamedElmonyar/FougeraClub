using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;

namespace Infrastructure.Data.Configurations
{
    public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
    {
        public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
        {
            // Primary Key
            builder.HasKey(x => x.Id);

            // Properties
            builder.Property(x => x.PurchaseOrderCode)
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(x => x.Status)
                   .HasConversion<int>();

            // Relationships
            builder.HasMany(x => x.Items)
                   .WithOne(x => x.PurchaseOrder)
                   .HasForeignKey(x => x.PurchaseOrderId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.PurchaseOrderCode).IsUnique();

            builder.HasIndex(x => x.SupplierId);
            builder.HasIndex(x => x.Date);
        }
    }
}
