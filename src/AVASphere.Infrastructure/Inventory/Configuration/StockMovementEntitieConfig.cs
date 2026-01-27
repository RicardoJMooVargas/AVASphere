//ACTUALIZADO A LA VERSION 0.2 DE LA DB
using AVASphere.ApplicationCore.Inventory.Entities.General;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AVASphere.Infrastructure.Inventory.Configuration;

public class StockMovementEntitieConfig : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> entity)
    {
        entity.ToTable("StockMovement");
        entity.HasKey(e => e.IdStockMovement);
        
        entity.Property(e => e.MovementType)
            .IsRequired();
        
        entity.Property(e => e.Quantity)
            .IsRequired();
        
        entity.Property(e => e.ReferenceType)
            .IsRequired();
        
        entity.Property(e => e.Description)
            .HasMaxLength(50);
        
        entity.Property(e => e.CreatedDate)
            .IsRequired();
        
        entity.Property(e => e.ByUser)
            .IsRequired();
        
        // Relación con Product
        entity.HasOne(sm => sm.Product)
            .WithMany()
            .HasForeignKey(sm => sm.IdProduct)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Relación con Warehouse
        entity.HasOne(sm => sm.Warehouse)
            .WithMany()
            .HasForeignKey(sm => sm.IdWarehouse)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

