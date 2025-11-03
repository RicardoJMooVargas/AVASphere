using AVASphere.ApplicationCore.Sales.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;
using System.Text.Json;

namespace AVASphere.Infrastructure.Sales.Configuration;

public class QuotationEntititeConfig : IEntityTypeConfiguration<Quotation>
{
    public void Configure(EntityTypeBuilder<Quotation> entity)
    {
        entity.ToTable("Quotations");

        // PK entero autoincremental
        entity.HasKey(q => q.QuotationId);
        entity.Property(q => q.QuotationId)
        .HasColumnName("IdQuotation")
        .ValueGeneratedOnAdd();

        entity.Property(q => q.SaleDate)
            .HasColumnName("SaleDate")
            .IsRequired();

        entity.Property(q => q.Status)
            .HasColumnName("Status")
            .IsRequired()
            .HasMaxLength(50);

        // SalesExecutives: guardado como JSONB (lista de strings o ints)
        entity.Property(q => q.SalesExecutives)
            .HasColumnName("SalesExecutives")
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'[]'::jsonb");

        entity.Property(q => q.Folio)
        .HasColumnName("Folio");

        entity.Property(q => q.CustomerId)
            .HasColumnName("IdCustomer")
            .IsRequired();

        entity.Property(q => q.GeneralComment)
            .HasColumnName("GeneralComment")
            .HasColumnType("text");

        entity.Property(q => q.Followups)
        .HasColumnName("FollowupsJson")
           .HasColumnType("jsonb")
            .HasDefaultValueSql("'[]'::jsonb");

        entity.Property(q => q.CreatedAt)
            .HasColumnName("CreatedAt");

        entity.Property(q => q.UpdatedAt)
            .HasColumnName("UpdatedAt");

        // Relación con Customer (bidireccional)
        entity.HasOne(q => q.Customer)
              .WithMany(c => c.Quotations)
              .HasForeignKey(q => q.CustomerId)
              .HasConstraintName("FK_Quotations_Customers_IdCustomer")
              .OnDelete(DeleteBehavior.Restrict);



    }
}