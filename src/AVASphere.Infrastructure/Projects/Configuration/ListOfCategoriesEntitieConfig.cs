using AVASphere.ApplicationCore.Projects.Entities.General;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AVASphere.Infrastructure.Projects.Configuration;

public class ListOfCategoriesEntitieConfig : IEntityTypeConfiguration<ListOfCategories>
{
    public void Configure(EntityTypeBuilder<ListOfCategories> entity)
    {
        entity.ToTable("ListOfCategories");
        entity.HasKey(e => e.IdListOfCategories);
        
        // Configuración de propiedades JSON
        entity.Property(e => e.SolutionsJson)
            .HasColumnType("jsonb")
            .IsRequired();
        
        // FK a Project
        entity.HasOne(loc => loc.Project)
            .WithMany(p => p.ListOfCategories)
            .HasForeignKey(loc => loc.IdProject)
            .OnDelete(DeleteBehavior.Cascade);
        
        // FK a ProjectCategory
        entity.HasOne(loc => loc.ProjectCategory)
            .WithMany(pc => pc.ListOfCategories)
            .HasForeignKey(loc => loc.IdProjectCategory)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

