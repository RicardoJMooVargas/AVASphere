using AVASphere.ApplicationCore.Sales.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AVASphere.Infrastructure.Sales.Configuration
{
    internal class SaleQuotationEntitieConfig : IEntityTypeConfiguration<SaleQuotation>
    {
        public void Configure(EntityTypeBuilder<SaleQuotation> entity)
        {
            entity.ToTable("SaleQuotations");

            // PK entero autoincremental
            entity.HasKey(sq => sq.IdSaleQuotation);
            entity.Property(sq => sq.IdSaleQuotation)
                .HasColumnName("IdSaleQuotation")
                .ValueGeneratedOnAdd();

            entity.Property(sq => sq.CreatedAt)
                .HasColumnName("CreatedAt");

            entity.Property(sq => sq.CreatedBy)
                .HasColumnName("CreatedBy");

            entity.Property(sq => sq.GeneralComment)
            .HasColumnName("GeneralComment");

            entity.Property(sq => sq.Products)
                  .HasColumnName("ProductsJson")
                  .HasColumnType("jsonb")
                  .HasDefaultValueSql("'[]'::jsonb");

            entity.Property(s => s.PriceSnapshot)
                  .HasColumnName("PriceSnapshotJson")
                  .HasColumnType("jsonb")
                  .HasDefaultValueSql("'{}'::jsonb");

            entity.Property(sq => sq.IdQuotation)
                .HasColumnName("IdQuotation");

            entity.Property(s => s.IdSale)
                 .HasColumnName("IdSale");

            // FK a Quotation
            entity.HasOne(s => s.Quotation)
                  .WithMany()
                  .HasForeignKey(s => s.IdQuotation)
                  .HasConstraintName("FK_SaleQuotation_Quotation_IdQuotation")
                  .OnDelete(DeleteBehavior.Restrict);


        }
    }
}