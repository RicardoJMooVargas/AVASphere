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
        
        entity.Property(e => e.IdProduct)
            .IsRequired();
            
        entity.Property(e => e.IdWarehouse)
            .IsRequired();
            
        // Configurar relaciones explícitamente para evitar duplicados
        entity.HasOne(e => e.Product)
            .WithMany()
            .HasForeignKey(e => e.IdProduct)
            .OnDelete(DeleteBehavior.Restrict);
            
        entity.HasOne(e => e.Warehouse)
            .WithMany()
            .HasForeignKey(e => e.IdWarehouse)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

