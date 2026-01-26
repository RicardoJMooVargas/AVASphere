using AVASphere.ApplicationCore.Inventory.Entities.General;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AVASphere.Infrastructure.Inventory.Configuration;

public class WarehouseTransferEntitieConfig : IEntityTypeConfiguration<WarehouseTransfer>
{
    public void Configure(EntityTypeBuilder<WarehouseTransfer> entity)
    {
        entity.ToTable("WarehouseTransfer");
        entity.HasKey(e => e.IdWarehouseTransfer);
        
        entity.Property(e => e.TransferDate)
            .IsRequired();
        
        entity.Property(e => e.Status)
            .IsRequired()
            .HasMaxLength(50);
        
        entity.Property(e => e.Observations)
            .HasMaxLength(50);
        
        entity.Property(e => e.IdWarehouseFrom)
            .IsRequired();
        
        entity.Property(e => e.IdWarehouseTo)
            .IsRequired();
        
        // Relación 1-N con WarehouseTransferDetail
        entity.HasMany(wt => wt.WarehouseTransferDetails)
            .WithOne(wtd => wtd.WarehouseTransfer)
            .HasForeignKey(wtd => wtd.IdWarehouseTransfer)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

