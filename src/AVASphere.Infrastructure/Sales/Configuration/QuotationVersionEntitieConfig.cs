using AVASphere.ApplicationCore.Sales.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AVASphere.Infrastructure.Sales.Configuration
{
    internal class QuotationVersionEntitieConfig : IEntityTypeConfiguration<QuotationVersion>
    {
        public void Configure(EntityTypeBuilder<QuotationVersion> entity)
        {
            entity.ToTable("QuotationVersions");

            // PK entero autoincremental
            entity.HasKey(qv => qv.QuotationVersionId);
            entity.Property(qv => qv.QuotationVersionId)
                .HasColumnName("IdQuotationVersion")
                .ValueGeneratedOnAdd();

            entity.Property(qv => qv.VersionNumber)
                .HasColumnName("VersionNumber")
                .IsRequired();

            entity.Property(qv => qv.Subtotal)
                .HasColumnName("Subtotal")
                .HasPrecision(18, 2);

            entity.Property(qv => qv.TaxAmount)
                .HasColumnName("TaxAmount")
                .HasPrecision(18, 2);

            entity.Property(qv => qv.TotalAmount)
                .HasColumnName("TotalAmount")
                .HasPrecision(18, 2);

            entity.Property(qv => qv.GeneralComment)
                .HasColumnName("GeneralComment")
                .HasMaxLength(500);

            entity.Property(qv => qv.CreatedAt)
                .HasColumnName("CreatedAt")
                .IsRequired();

            entity.Property(qv => qv.CreatedBy)
                .HasColumnName("CreatedBy")
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(qv => qv.IdQuotation)
                .HasColumnName("IdQuotation")
                .IsRequired();

            // Products (List<SingleProductJson>) -> jsonb
            entity.Property(q => q.Products)
                  .HasColumnName("ProductsJson")
                  .HasColumnType("jsonb")
                  .HasDefaultValueSql("'[]'::jsonb");

            //Full Quotation object -> jsonb
            entity.Property(q => q.QuotationData)
                  .HasColumnName("QuotationData")
                  .HasColumnType("jsonb")
                  .HasDefaultValueSql("'{}'::jsonb");


            // FK a Quotation
            entity.HasOne(q => q.Quotation)
                  .WithMany(v => v.Versions)
                  .HasForeignKey(q => q.IdQuotation)
                  .HasConstraintName("QuotationVersion")
                  .OnDelete(DeleteBehavior.Restrict);


        }
    }
}