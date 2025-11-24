using AVASphere.ApplicationCore.Projects.Entities.General;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AVASphere.Infrastructure.Projects.Configuration;

public class TechnicalDesignEntitieConfig : IEntityTypeConfiguration<TechnicalDesign>
{
    public void Configure(EntityTypeBuilder<TechnicalDesign> entity)
    {
        entity.ToTable("TechnicalDesign");
        entity.HasKey(e => e.IdTechnicalDesign);
        
        entity.Property(e => e.SavedDesign)
            .HasColumnType("text");
        entity.Property(e => e.imageUrl)
            .HasMaxLength(500);
        // json Relacionado con Modelo
        entity.OwnsMany(p => p.SolutionsJsons, sa => 
        {
            sa.ToJson();
            sa.Property(c => c.Index).HasColumnName("SolutionsJsons_Index");
            sa.Property(c => c.Name).HasColumnName("SolutionsJsons_Name");
            sa.Property(c => c.NormalizedName).HasColumnName("SolutionsJsons_NormalizedName");
        });
        
        // FK a ProjectCategory
        entity.HasOne(td => td.ProjectCategory)
            .WithMany(pc => pc.TechnicalDesigns)
            .HasForeignKey(td => td.IdProjectCategory)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

