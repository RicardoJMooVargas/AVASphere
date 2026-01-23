using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InventoryEntity = AVASphere.ApplicationCore.Inventory.Entities.General.Inventory;

namespace AVASphere.Infrastructure.Inventory.Configuration;

public class InventoryEntitieConfig : IEntityTypeConfiguration<InventoryEntity>
{
    public void Configure(EntityTypeBuilder<InventoryEntity> entity)
    {
        entity.ToTable("Inventory");
        entity.HasKey(e => e.IdInventory);
        
        entity.Property(e => e.Stock)
            .IsRequired();
        
        entity.Property(e => e.StockMin)
            .IsRequired();
        
        entity.Property(e => e.StockMax)
            .IsRequired();
        
        entity.Property(e => e.LocationDetail)
            .IsRequired();
        
        // Relación con PhysicalInventory
        entity.HasOne(i => i.PhysicalInventory)
            .WithMany(pi => pi.Inventories)
            .HasForeignKey(i => i.IdPhysicalInventory)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Relación con Product
        entity.HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.IdProduct)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Relación con Warehouse
        entity.HasOne(i => i.Warehouse)
            .WithMany(w => w.Inventories)
            .HasForeignKey(i => i.IdWarehouse)
            .OnDelete(DeleteBehavior.Restrict);
    }
}