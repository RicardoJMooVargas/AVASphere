﻿﻿using Microsoft.EntityFrameworkCore;
using AVASphere.ApplicationCore.Common.Entities.Catalogs;
using AVASphere.ApplicationCore.Common.Entities.General;
using AVASphere.ApplicationCore.Projects.Entities.Catalogs;
using AVASphere.ApplicationCore.Projects.Entities.General;
using AVASphere.Infrastructure.Common.Configuration;
using AVASphere.Infrastructure.Projects.Configuration;
using AVASphere.Infrastructure.Sales.Configuration;
using AVASphere.ApplicationCore.Sales.Entities;
using AVASphere.ApplicationCore.Common.Entities.Products;
using AVASphere.ApplicationCore.Common.Entities.Jsons;
using AVASphere.ApplicationCore.Projects.Entities.jsons;
using AVASphere.ApplicationCore.Inventory.Entities.General;


namespace AVASphere.Infrastructure;

public class MasterDbContext : DbContext
{
    public MasterDbContext(DbContextOptions<MasterDbContext> options)
        : base(options)
    {
    }
    // MODULO DE COMMON
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Rol> Rols { get; set; } = null!;
    public DbSet<Area> Areas { get; set; } = null!;
    public DbSet<ConfigSys> ConfigSys { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<Property> Properties { get; set; } = null!;
    public DbSet<PropertyValue> PropertyValues { get; set; } = null!;
    public DbSet<Supplier> Suppliers { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<ProductProperties> ProductProperties { get; set; } = null!;
    public DbSet<StorageStructure> StorageStructures { get; set; } = null!;
    //public DbSet<BranchOffice> BranchOffices { get; set; } = null!;

    // MODULO DE SALES
    public DbSet<Quotation> Quotations { get; set; } = null!;
    public DbSet<Sale> Sales { get; set; } = null!;
    public DbSet<QuotationVersion> QuotationVersions { get; set; } = null!;
    public DbSet<SaleQuotation> SaleQuotations { get; set; } = null!;

    // MODULO PROJECTS
    public DbSet<ProjectCategory> ProjectCategory { get; set; } = null!;
    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<ProjectQuote> ProjectQuotes { get; set; } = null!;
    public DbSet<IndividualProjectQuote> IndividualProjectQuotes { get; set; } = null!;
    public DbSet<ListOfCategories> ListOfCategories { get; set; } = null!;
    //public DbSet<ListOfProductsByCategory> ListOfProductsByCategory { get; set; } = null!;
    public DbSet<TechnicalDesign> TechnicalDesigns { get; set; } = null!;
    public DbSet<IndividualListingProperties> IndividualListingProperties { get; set; } = null!;
    public DbSet<ListOfProductsToQuot> ListOfProductsToQuot { get; set; } = null!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // MODULO DE COMMON
        modelBuilder.ApplyConfiguration(new ConfigSysEntitieConfig());
        modelBuilder.ApplyConfiguration(new UserEntitieConfig());
        modelBuilder.ApplyConfiguration(new RolEntitieConfig());
        modelBuilder.ApplyConfiguration(new AreaEntitieConfig());
        modelBuilder.ApplyConfiguration(new CustomerEntitieConfig());
        modelBuilder.ApplyConfiguration(new PropertyEntitieConfig());
        modelBuilder.ApplyConfiguration(new PropertyValueEntitieConfig());
        modelBuilder.ApplyConfiguration(new SupplierEntitieConfig());
        modelBuilder.ApplyConfiguration(new ProductEntitieConfig());
        modelBuilder.ApplyConfiguration(new ProductPropertiesEntitieConfig());
        //modelBuilder.ApplyConfiguration(new BranchOfficeEntitieConfig());
        
        // Ignorar clases JSON que no son entidades
        modelBuilder.Ignore<PaymentMethodsJson>();
        modelBuilder.Ignore<PaymentTermsJson>();
        modelBuilder.Ignore<ContactsJson>();
        modelBuilder.Ignore<DirectionJson>();
        modelBuilder.Ignore<CategoriesJson>();
        modelBuilder.Ignore<CodeJson>();
        modelBuilder.Ignore<CostsJson>();
        modelBuilder.Ignore<SolutionsJson>();
        modelBuilder.Ignore<SettingsCustomerJson>();
        modelBuilder.Ignore<ColorsJson>();
        modelBuilder.Ignore<NotUseModuleJson>();
        modelBuilder.Ignore<SingleProductJson>();
        modelBuilder.Ignore<PriceSnapshotJson>();
        modelBuilder.Ignore<SaleJson>();
        modelBuilder.Ignore<AuxNoteDataJson>();
        modelBuilder.Ignore<QuotationDataJson>();
        modelBuilder.Ignore<QuotationFollowupsJson>();
        modelBuilder.Ignore<AppointmentJson>();
        modelBuilder.Ignore<VisitsJson>();
        
        // MODULO DE SALES
        modelBuilder.ApplyConfiguration(new QuotationEntitieConfig());
        modelBuilder.ApplyConfiguration(new SaleEntitieConfig());
        modelBuilder.ApplyConfiguration(new QuotationVersionEntitieConfig());
        modelBuilder.ApplyConfiguration(new SaleQuotationEntitieConfig());

        modelBuilder.Entity<QuotationVersion>()
       .HasOne(v => v.Quotation)
       .WithMany(q => q.Versions)
       .HasForeignKey(v => v.IdQuotation)
       .OnDelete(DeleteBehavior.Cascade);

        // MODULO DE PROJECTS
        modelBuilder.ApplyConfiguration(new ProjectCategoryEntitieConfig());
        modelBuilder.ApplyConfiguration(new ProjectEntitieConfig());
        modelBuilder.ApplyConfiguration(new ProjectQuoteEntitieConfig());
        modelBuilder.ApplyConfiguration(new IndividualProjectQuoteEntitieConfig());
        modelBuilder.ApplyConfiguration(new ListOfCategoriesEntitieConfig());
        // modelBuilder.ApplyConfiguration(new ListOfProductsByCategoryEntitieConfig()); DELETED and DEPRECATED
        modelBuilder.ApplyConfiguration(new TechnicalDesignEntitieConfig());
        modelBuilder.ApplyConfiguration(new IndividualListingPropertiesEntitieConfig());
        modelBuilder.ApplyConfiguration(new ListOfProductsToQuotEntitieConfig());
    }
}