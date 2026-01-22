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
        
        entity.Property(e => e.OneSection)
            .IsRequired();
        
        entity.Property(e => e.HasLevel)
            .IsRequired();
        
        entity.Property(e => e.HasSubLevel)
            .IsRequired();
        
        // Relación 1-N con LocationDetails
        entity.HasMany(ss => ss.LocationDetails)
            .WithOne(ld => ld.StorageStructure)
            .HasForeignKey(ld => ld.IdStorageStructure)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
