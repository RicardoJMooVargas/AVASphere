﻿using AVASphere.ApplicationCore.Projects.Entities.General;
using AVASphere.ApplicationCore.Projects.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AVASphere.Infrastructure.Projects.Configuration;

public class IndividualProjectQuoteEntitieConfig : IEntityTypeConfiguration<IndividualProjectQuote>
{
    public void Configure(EntityTypeBuilder<IndividualProjectQuote> entity)
    {
        entity.ToTable("IndividualProjectQuote");
        entity.HasKey(e => e.IdIndividualProjectQuote);
        
        entity.Property(e => e.Description)
            .HasMaxLength(500);
        /*
        entity.Property(e => e.Category)
            .HasMaxLength(100);
        */
        entity.Property(e => e.Quantity)
            .IsRequired();
        
        entity.Property(e => e.UnitPrice)
            .IsRequired();
        
        entity.Property(e => e.Amount)
            .IsRequired();
        
        entity.Property(e => e.Total)
            .IsRequired();
        
        entity.Property(e => e.StatusProcess)
            .HasDefaultValue(StatusIndividualProject.NotStarted);
        
        // FK a ProjectQuote
        entity.HasOne(ipq => ipq.ProjectQuote)
            .WithMany(pq => pq.IndividualProjectQuotes)
            .HasForeignKey(ipq => ipq.IdProjectQuotes)
            .OnDelete(DeleteBehavior.Cascade);
        
        // FK a ProjectCategory
        entity.HasOne(ipq => ipq.ProjectCategory)
            .WithMany(pc => pc.IndividualProjectQuotes)
            .HasForeignKey(ipq => ipq.IdProjectCategory)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Relación 1-N con IndividualListingProperties
        entity.HasMany(ipq => ipq.IndividualListingProperties)
            .WithOne(ilp => ilp.IndividualProjectQuote)
            .HasForeignKey(ilp => ilp.IdIndividualProjectQuote)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Relación 1-N con ListOfProductsToQuot
        entity.HasMany(ipq => ipq.ListOfProductsToQuot)
            .WithOne(loptq => loptq.IndividualProjectQuotes)
            .HasForeignKey(loptq => loptq.IdIndividualProjectQuotes)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

