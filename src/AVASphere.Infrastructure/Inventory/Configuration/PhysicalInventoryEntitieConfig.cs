using AVASphere.ApplicationCore.Inventory.Entities.General;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AVASphere.Infrastructure.Inventory.Configuration;

public class PhysicalInventoryEntitieConfig : IEntityTypeConfiguration<PhysicalInventory>
{
    public void Configure(EntityTypeBuilder<PhysicalInventory> entity)
    {
        entity.ToTable("PhysicalInventory");
        entity.HasKey(e => e.IdPhysicalInventory);
        
        entity.Property(e => e.InventoryDate)
            .IsRequired();
        
        entity.Property(e => e.Status)
            .IsRequired()
            .HasMaxLength(50);
        
        entity.Property(e => e.CreatedBy)
            .IsRequired();
        
        entity.Property(e => e.Observations)
            .HasMaxLength(1000);
        
        // Relación con Warehouse
        entity.HasOne(pi => pi.Warehouse)
            .WithMany(w => w.PhysicalInventories)
            .HasForeignKey(pi => pi.IdWarehouse)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Relación 1-N con Inventory
        entity.HasMany(pi => pi.Inventories)
            .WithOne(i => i.PhysicalInventory)
            .HasForeignKey(i => i.IdPhysicalInventory)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Relación 1-N con PhysicalInventoryDetail
        entity.HasMany(pi => pi.PhysicalInventoryDetails)
            .WithOne(pid => pid.PhysicalInventory)
            .HasForeignKey(pid => pid.IdPhysicalInventory)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
