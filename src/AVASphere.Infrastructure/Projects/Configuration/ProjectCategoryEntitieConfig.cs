using AVASphere.ApplicationCore.Projects.Entities.Catalogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AVASphere.Infrastructure.Projects.Configuration;

    public class ProjectCategoryEntitieConfig : IEntityTypeConfiguration<ProjectCategory>
    {
        public void Configure(EntityTypeBuilder<ProjectCategory> entity)
        {
            // Ejemplo de configuración:
            entity.ToTable("Project Category"); 
            entity.HasKey(e => e.IdProjectCategory); 
            
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsRequired();
            
            entity.Property(e => e.NormalizedName)
                .HasMaxLength(100)
                .IsRequired();
            
            // RELACIONES
           /* entity.HasMany(pc => pc.IndividualProjectQuotes) // ICollection<IndividualProjectQuote>
                .WithOne(ipq => ipq.ProjectCategory) // IndividualProjectQuote.ProjectCategory
                .HasForeignKey(ipq => ipq.IdProjectCategory) // FK en IndividualProjectQuote
                .OnDelete(DeleteBehavior.Cascade);
                */
        }
    }
