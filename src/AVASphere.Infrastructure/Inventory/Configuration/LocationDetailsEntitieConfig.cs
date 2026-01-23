using AVASphere.ApplicationCore.Inventory.Entities.General;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AVASphere.Infrastructure.Inventory.Configuration;

public class LocationDetailsEntitieConfig : IEntityTypeConfiguration<LocationDetails>
{
    public void Configure(EntityTypeBuilder<LocationDetails> entity)
    {
        entity.ToTable("LocationDetails");
        entity.HasKey(e => e.IdLocationDetails);
        
        entity.Property(e => e.TypeStorageSystem)
            .IsRequired()
            .HasMaxLength(100);
        
        entity.Property(e => e.Section)
            .IsRequired()
            .HasMaxLength(100);
        
        entity.Property(e => e.VerticalLevel)
            .IsRequired();
        
        // Relación con Area
        entity.HasOne(ld => ld.Area)
            .WithMany()
            .HasForeignKey(ld => ld.IdArea)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Relación con StorageStructure
        entity.HasOne(ld => ld.StorageStructure)
            .WithMany()
            .HasForeignKey(ld => ld.IdStorageStructure)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Relación 1-N con PhysicalInventoryDetail
        entity.HasMany(ld => ld.PhysicalInventoryDetails)
            .WithOne(pid => pid.LocationDetails)
            .HasForeignKey(pid => pid.IdLocationDetails)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
