using AVASphere.ApplicationCore.Sales.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;
using System.Text.Json;

namespace AVASphere.Infrastructure.Sales.Configuration;

public class QuotationEntititeConfig
{
    public void Configure(EntityTypeBuilder<Quotation> builder)
    {
        builder.ToTable("Quotations");

        // PK entero autoincremental
        builder.HasKey(q => q.QuotationId);
        builder.Property(q => q.QuotationId)
        .HasColumnName("quotation_id")
        .ValueGeneratedOnAdd();

        builder.Property(q => q.SaleDate)
            .HasColumnName("sale_date")
            .IsRequired();

        builder.Property(q => q.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasMaxLength(50);

        // SalesExecutives: guardado como JSONB (lista de strings o ints)
        builder.Property(q => q.SalesExecutives)
            .HasColumnName("sales_executives")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => string.IsNullOrEmpty(v) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
            )
            .IsRequired();

        builder.Property(q => q.Folio)
        .HasColumnName("folio");

        builder.Property(q => q.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(q => q.GeneralComment)
            .HasColumnName("general_comment")
            .HasMaxLength("text");

        builder.Property(q => q.Followups)
            .HasColumnName("followups")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => string.IsNullOrEmpty(v) ? new List<QuotationFollowupsJson>() : JsonSerializer.Deserialize<List<QuotationFollowupsJson>>(v, (JsonSerializerOptions?)null) ?? new List<QuotationFollowupsJson>()
            );

        builder.Property(q => q.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(q => q.UpdatedAt)
            .HasColumnName("updated_at");



    }
}