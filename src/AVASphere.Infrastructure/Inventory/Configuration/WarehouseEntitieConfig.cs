using AVASphere.ApplicationCore.Inventory.Entities.General;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AVASphere.Infrastructure.Inventory.Configuration;

public class WarehouseEntitieConfig : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> entity)
    {
        entity.ToTable("Warehouse");
        entity.HasKey(e => e.IdWarehouse);
        
        entity.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(255);
        
        entity.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(50);
        
        entity.Property(e => e.Location)
            .HasMaxLength(500);
        
        entity.Property(e => e.IsMain)
            .IsRequired();
        
        entity.Property(e => e.Active)
            .IsRequired();
        
        // Relación 1-N con PhysicalInventory
        entity.HasMany(w => w.PhysicalInventories)
            .WithOne(pi => pi.Warehouse)
            .HasForeignKey(pi => pi.IdWarehouse)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Relación 1-N con Inventory
        entity.HasMany(w => w.Inventories)
            .WithOne(i => i.Warehouse)
            .HasForeignKey(i => i.IdWarehouse)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
