using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class PurchaseOrderItemConfiguration : IEntityTypeConfiguration<PurchaseOrderItem>
    {
        public void Configure(EntityTypeBuilder<PurchaseOrderItem> builder)
        {
            builder.Property(x => x.SinglePrice)
                   .HasColumnType("decimal(18,2)");

            builder.Property(x => x.ItemName)
                   .HasMaxLength(200)
                   .IsRequired();
        }
    }
}
