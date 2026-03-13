using AVASphere.ApplicationCore.Projects.Entities.General;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AVASphere.Infrastructure.Projects.Configuration;

public class ListOfProductsToQuotEntitieConfig : IEntityTypeConfiguration<ListOfProductsToQuot>
{
    public void Configure(EntityTypeBuilder<ListOfProductsToQuot> entity)
    {
        entity.ToTable("ListOfProductsToQuot");
        entity.HasKey(e => e.IdListOfProductsToQuot);
        // FK a IndividualProjectQuote
        entity.HasOne(loptq => loptq.IndividualProjectQuotes)
            .WithMany(ipq => ipq.ListOfProductsToQuot)
            .HasForeignKey(loptq => loptq.IdIndividualProjectQuotes)
            .OnDelete(DeleteBehavior.Cascade);
        // FK a Product
        entity.HasOne(loptq => loptq.Product)
            .WithMany(p => p.ProductImages)
            .HasForeignKey(loptq => loptq.IdProduct)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

