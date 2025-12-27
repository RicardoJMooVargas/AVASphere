using AVASphere.ApplicationCore.Projects.Entities.Catalogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AVASphere.Infrastructure.Projects.Configuration;

public class ProjectCategoryEntitieConfig : IEntityTypeConfiguration<ProjectCategory>
{
    public void Configure(EntityTypeBuilder<ProjectCategory> entity)
    {
        entity.ToTable("ProjectCategory");
        entity.HasKey(e => e.IdProjectCategory); 
        
        entity.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();
        
        entity.Property(e => e.NormalizedName)
            .HasMaxLength(100)
            .IsRequired();
        
        // FK a ConfigSys
        entity.HasOne(pc => pc.ConfigSys)
            .WithMany(cs => cs.ProjectCategories)
            .HasForeignKey(pc => pc.IdConfigSys)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Relación 1-N con IndividualProjectQuote
        entity.HasMany(pc => pc.IndividualProjectQuotes)
            .WithOne(ipq => ipq.ProjectCategory)
            .HasForeignKey(ipq => ipq.IdProjectCategory)
            .OnDelete(DeleteBehavior.Restrict);
       
        // Relación 1-N con ListOfCategories
        entity.HasMany(pc => pc.ListOfCategories)
            .WithOne(loc => loc.ProjectCategory)
            .HasForeignKey(loc => loc.IdProjectCategory)
            .OnDelete(DeleteBehavior.Restrict);
       
        // Relación 1-N con ListOfProductsByCategory
        /*
        entity.HasMany(pc => pc.ListOfProductsByCategory)
            .WithOne(lopbc => lopbc.ProjectCategory)
            .HasForeignKey(lopbc => lopbc.IdProjectCategory)
            .OnDelete(DeleteBehavior.Restrict);
        */
       
        // Relación 1-N con TechnicalDesign
        entity.HasMany(pc => pc.TechnicalDesigns)
            .WithOne(td => td.ProjectCategory)
            .HasForeignKey(td => td.IdProjectCategory)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
