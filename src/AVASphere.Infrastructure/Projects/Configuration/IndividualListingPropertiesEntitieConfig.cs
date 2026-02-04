﻿using AVASphere.ApplicationCore.Projects.Entities.General;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AVASphere.Infrastructure.Projects.Configuration;

public class IndividualListingPropertiesEntitieConfig : IEntityTypeConfiguration<IndividualListingProperties>
{
    public void Configure(EntityTypeBuilder<IndividualListingProperties> entity)
    {
        entity.ToTable("IndividualListingProperties");
        entity.HasKey(e => e.IdIndividualListingProperties);
        
        // FK a IndividualProjectQuote (requerida)
        entity.HasOne(ilp => ilp.IndividualProjectQuote)
            .WithMany(ipq => ipq.IndividualListingProperties)
            .HasForeignKey(ilp => ilp.IdIndividualProjectQuote)
            .OnDelete(DeleteBehavior.Cascade);
        
        // FK a ProductProperties (requerida)
        entity.HasOne(ilp => ilp.ProductProperties)
            .WithMany(pp => pp.IndividualListingProperties)
            .HasForeignKey(ilp => ilp.IdProductProperties)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

