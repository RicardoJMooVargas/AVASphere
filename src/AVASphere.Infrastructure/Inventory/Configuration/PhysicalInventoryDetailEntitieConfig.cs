using AVASphere.ApplicationCore.Inventory.Entities.General;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AVASphere.Infrastructure.Inventory.Configuration;

public class PhysicalInventoryDetailEntitieConfig : IEntityTypeConfiguration<PhysicalInventoryDetail>
{
    public void Configure(EntityTypeBuilder<PhysicalInventoryDetail> entity)
    {
        entity.ToTable("PhysicalInventoryDetail");
        entity.HasKey(e => e.IdPhysicalInventoryDetail);
        
        entity.Property(e => e.SystemQuantity)
            .IsRequired();
        
        entity.Property(e => e.PhysicalQuantity)
            .IsRequired();
        
        entity.Property(e => e.Difference)
            .IsRequired();
        
        // Relación con PhysicalInventory
        entity.HasOne(pid => pid.PhysicalInventory)
            .WithMany(pi => pi.PhysicalInventoryDetails)
            .HasForeignKey(pid => pid.IdPhysicalInventory)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Relación con Product
        entity.HasOne(pid => pid.Product)
            .WithMany()
            .HasForeignKey(pid => pid.IdProduct)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Relación con LocationDetails (opcional)
        entity.HasOne(pid => pid.LocationDetails)
            .WithMany(ld => ld.PhysicalInventoryDetails)
            .HasForeignKey(pid => pid.IdLocationDetails)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
    }
}

