using AVASphere.ApplicationCore.Inventory.Entities.General;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AVASphere.Infrastructure.Inventory.Configuration;

public class WarehouseTransferDetailEntitieConfig : IEntityTypeConfiguration<WarehouseTransferDetail>
{
    public void Configure(EntityTypeBuilder<WarehouseTransferDetail> entity)
    {
        entity.ToTable("WarehouseTransferDetail");
        entity.HasKey(e => e.IdTransferDetail);
        
        entity.Property(e => e.TransferDate)
            .IsRequired();
        
        entity.Property(e => e.Quantity)
            .IsRequired();
        
        // Relación con Product
        entity.HasOne(wtd => wtd.Product)
            .WithMany()
            .HasForeignKey(wtd => wtd.IdProduct)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Relación con WarehouseTransfer
        entity.HasOne(wtd => wtd.WarehouseTransfer)
            .WithMany(wt => wt.WarehouseTransferDetails)
            .HasForeignKey(wtd => wtd.IdWarehouseTransfer)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

