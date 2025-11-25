using AVASphere.ApplicationCore.Projects.Entities.General;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AVASphere.Infrastructure.Projects.Configuration;

public class ProjectQuoteEntitieConfig : IEntityTypeConfiguration<ProjectQuote>
{
    public void Configure(EntityTypeBuilder<ProjectQuote> entity)
    {
        entity.ToTable("ProjectQuote");
        entity.HasKey(e => e.IdProjectQuotes);
        
        entity.Property(e => e.GrandTotal)
            .IsRequired();
        
        entity.Property(e => e.TotalTaxes)
            .IsRequired();
        
        // Relación 1-1 con Project
        // ProjectQuote tiene la FK hacia Project
        entity.HasOne(pq => pq.Project)
            .WithOne(p => p.ProjectQuote)
            .HasForeignKey<ProjectQuote>(pq => pq.IdProject)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Relación 1-N con IndividualProjectQuote
        entity.HasMany(pq => pq.IndividualProjectQuotes)
            .WithOne(ipq => ipq.ProjectQuote)
            .HasForeignKey(ipq => ipq.IdProjectQuotes)
            .OnDelete(DeleteBehavior.Cascade);
    }
}