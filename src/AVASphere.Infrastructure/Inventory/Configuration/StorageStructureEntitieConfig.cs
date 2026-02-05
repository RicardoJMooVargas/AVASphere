using AVASphere.ApplicationCore.Inventory.Entities.General;
using AVASphere.ApplicationCore.Common.Entities.Catalogs;
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
        
        entity.Property(e => e.OneSection)
            .IsRequired();
        
        entity.Property(e => e.HasLevel)
            .IsRequired();
        
        entity.Property(e => e.HasSubLevel)
            .IsRequired();
        
        entity.Property(e => e.IdWarehouse)
            .IsRequired();
        
        entity.Property(e => e.IdArea)
            .IsRequired(false); // Nullable - permite que un rack no tenga área asignada
        
        // Relación N-1 con Warehouse
        entity.HasOne(ss => ss.Warehouse)
            .WithMany()
            .HasForeignKey(ss => ss.IdWarehouse)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Relación N-1 con Area - nullable, si se elimina el área, IdArea se pone en null
        entity.HasOne(ss => ss.Area)
            .WithMany(a => a.StorageStructures)
            .HasForeignKey(ss => ss.IdArea)
            .OnDelete(DeleteBehavior.SetNull);
        
        // Relación 1-N con LocationDetails
        entity.HasMany(ss => ss.LocationDetails)
            .WithOne(ld => ld.StorageStructure)
            .HasForeignKey(ld => ld.IdStorageStructure)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
