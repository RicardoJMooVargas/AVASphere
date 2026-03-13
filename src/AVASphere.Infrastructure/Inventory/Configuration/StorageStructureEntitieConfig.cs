using AVASphere.ApplicationCore.Inventory.Entities.General;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AVASphere.Infrastructure.Inventory.Configuration;

public class StorageStructureEntitieConfig : IEntityTypeConfiguration<StorageStructure>
{
    public void Configure(EntityTypeBuilder<StorageStructure> entity)
    {
        entity.ToTable("StorageStructure");
        entity.HasKey(e => e.IdStorageStructure);
        
        entity.Property(e => e.CodeRack)
            .IsRequired()
            .HasMaxLength(100);
        
        entity.Property(e => e.TypeStorageSystem)
            .IsRequired()
            .HasMaxLength(100);
        
        entity.Property(e => e.OneSection)
            .IsRequired();
        
        entity.Property(e => e.HasLevel)
            .IsRequired();
        
        entity.Property(e => e.HasSubLevel)
            .IsRequired();
        
        entity.Property(e => e.IdWarehouse)
            .IsRequired();
        
        entity.Property(e => e.IdArea)
            .IsRequired(false); // Nullable
            
        // Configurar relaciones explícitamente para evitar duplicados
        entity.HasOne(e => e.Warehouse)
            .WithMany(w => w.StorageStructures)
            .HasForeignKey(e => e.IdWarehouse)
            .OnDelete(DeleteBehavior.Restrict);
            
        // Relación con Area - solo una configuración
        entity.HasOne(e => e.Area)
            .WithMany() // Sin especificar la navegación inversa para evitar duplicados
            .HasForeignKey(e => e.IdArea)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
            
        entity.HasMany(e => e.LocationDetails)
            .WithOne(ld => ld.StorageStructure)
            .HasForeignKey(ld => ld.IdStorageStructure)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
