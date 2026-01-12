using AVASphere.ApplicationCore.Projects.Entities.General;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AVASphere.Infrastructure.Projects.Configuration;

public class ProjectEntitieConfig : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> entity)
    {
        entity.ToTable("Project");
        entity.HasKey(e => e.IdProject);
        
        entity.Property(e => e.CurrentHito)
            .IsRequired();
        
        entity.Property(e => e.WrittenAddress)
            .HasMaxLength(500);
        
        entity.Property(e => e.ExactAddress)
            .HasMaxLength(500);
        
        // Configuración de propiedades JSON
        entity.Property(e => e.AppointmentJson)
            .HasColumnType("jsonb");

        entity.Property(e => e.VisitsJson)
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'[]'::jsonb")
            .IsRequired();
        
        // FK a ConfigSys (requerida)
        entity.HasOne(p => p.ConfigSys)
            .WithMany(cs => cs.Projects)
            .HasForeignKey(p => p.IdConfigSys)
            .OnDelete(DeleteBehavior.Restrict);
        
        // FK a Customer (requerida)
        entity.HasOne(p => p.Customer)
            .WithMany(c => c.Projects)
            .HasForeignKey(p => p.IdCustomer)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Relación 1-1 con ProjectQuote
        entity.HasOne(p => p.ProjectQuote)
            .WithOne(pq => pq.Project)
            .HasForeignKey<ProjectQuote>(pq => pq.IdProject)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Relación 1-N con ListOfCategories
        entity.HasMany(p => p.ListOfCategories)
            .WithOne(loc => loc.Project)
            .HasForeignKey(loc => loc.IdProject)
            .OnDelete(DeleteBehavior.Cascade);
    }
}