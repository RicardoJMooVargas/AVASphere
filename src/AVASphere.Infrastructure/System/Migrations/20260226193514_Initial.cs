﻿using System;
using System.Collections.Generic;
using AVASphere.ApplicationCore.Common.Entities.Catalogs;
using AVASphere.ApplicationCore.Common.Entities.General;
using AVASphere.ApplicationCore.Common.Entities.Jsons;
using AVASphere.ApplicationCore.Common.Entities.Products;
using AVASphere.ApplicationCore.Projects.Entities.General;
using AVASphere.ApplicationCore.Projects.Entities.jsons;
using AVASphere.ApplicationCore.Sales.Entities;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using ContactsJsonGeneral = AVASphere.ApplicationCore.Common.Entities.General.ContactsJson;
using ContactsJsonCatalog = AVASphere.ApplicationCore.Common.Entities.Catalogs.ContactsJson;
using SolutionsJsonProject = AVASphere.ApplicationCore.Projects.Entities.jsons.SolutionsJson;

#nullable disable

namespace AVASphere.Infrastructure.System.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Area",
                columns: table => new
                {
                    IdArea = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Area", x => x.IdArea);
                });

            migrationBuilder.CreateTable(
                name: "ConfigSys",
                columns: table => new
                {
                    IdConfigSys = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BranchName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LogoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Colors = table.Column<ICollection<ColorsJson>>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    NotUseModules = table.Column<ICollection<NotUseModuleJson>>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigSys", x => x.IdConfigSys);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    IdCustomer = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExternalId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    TaxId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SettingsCustomerJson = table.Column<SettingsCustomerJson>(type: "jsonb", nullable: true, defaultValueSql: "'{}'::jsonb"),
                    DirectionJson = table.Column<DirectionJson>(type: "jsonb", nullable: true, defaultValueSql: "'{}'::jsonb"),
                    PaymentMethodsJson = table.Column<PaymentMethodsJson>(type: "jsonb", nullable: true, defaultValueSql: "'{}'::jsonb"),
                    PaymentTermsJson = table.Column<PaymentTermsJson>(type: "jsonb", nullable: true, defaultValueSql: "'{}'::jsonb")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers_IdCustomer", x => x.IdCustomer);
                });

            migrationBuilder.CreateTable(
                name: "Property",
                columns: table => new
                {
                    IdProperty = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Property", x => x.IdProperty);
                });

            migrationBuilder.CreateTable(
                name: "Supplier",
                columns: table => new
                {
                    IdSupplier = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CompanyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TaxId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PersonType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BusinessId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CurrencyCoin = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    DeliveryDays = table.Column<double>(type: "double precision", nullable: true),
                    RegistrationDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Observations = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                     ContactsJson = table.Column<ContactsJsonCatalog>(type: "jsonb", nullable: true, defaultValueSql: "'{}'::jsonb"),
                    PaymentTermsJson = table.Column<PaymentTermsJson>(type: "jsonb", nullable: true, defaultValueSql: "'{}'::jsonb"),
                    PaymentMethodsJson = table.Column<PaymentMethodsJson>(type: "jsonb", nullable: true, defaultValueSql: "'{}'::jsonb")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Supplier", x => x.IdSupplier);
                });

            migrationBuilder.CreateTable(
                name: "Warehouse",
                columns: table => new
                {
                    IdWarehouse = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Location = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsMain = table.Column<double>(type: "double precision", nullable: false),
                    Active = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warehouse", x => x.IdWarehouse);
                });

            migrationBuilder.CreateTable(
                name: "WarehouseTransfer",
                columns: table => new
                {
                    IdWarehouseTransfer = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TransferDate = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Observations = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IdWarehouseFrom = table.Column<double>(type: "double precision", nullable: false),
                    IdWarehouseTo = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseTransfer", x => x.IdWarehouseTransfer);
                });

            migrationBuilder.CreateTable(
                name: "Rol",
                columns: table => new
                {
                    IdRol = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Permissions = table.Column<List<Permission>>(type: "jsonb", nullable: true, defaultValueSql: "'[]'::jsonb"),
                    Modules = table.Column<List<Module>>(type: "jsonb", nullable: true, defaultValueSql: "'[]'::jsonb"),
                    IdArea = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rol", x => x.IdRol);
                    table.ForeignKey(
                        name: "FK_Rol_Area_IdArea",
                        column: x => x.IdArea,
                        principalTable: "Area",
                        principalColumn: "IdArea",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectCategory",
                columns: table => new
                {
                    IdProjectCategory = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IdConfigSys = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectCategory", x => x.IdProjectCategory);
                    table.ForeignKey(
                        name: "FK_ProjectCategory_ConfigSys_IdConfigSys",
                        column: x => x.IdConfigSys,
                        principalTable: "ConfigSys",
                        principalColumn: "IdConfigSys",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Project",
                columns: table => new
                {
                    IdProject = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdProjectQuote = table.Column<int>(type: "integer", nullable: false),
                    IdConfigSys = table.Column<int>(type: "integer", nullable: false),
                    IdCustomer = table.Column<int>(type: "integer", nullable: false),
                    CurrentHito = table.Column<int>(type: "integer", nullable: false),
                    WrittenAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ExactAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AppointmentJson = table.Column<AppointmentJson>(type: "jsonb", nullable: true),
                    VisitsJson = table.Column<ICollection<VisitsJson>>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project", x => x.IdProject);
                    table.ForeignKey(
                        name: "FK_Project_ConfigSys_IdConfigSys",
                        column: x => x.IdConfigSys,
                        principalTable: "ConfigSys",
                        principalColumn: "IdConfigSys",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Project_Customers_IdCustomer",
                        column: x => x.IdCustomer,
                        principalTable: "Customers",
                        principalColumn: "IdCustomer",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Quotations",
                columns: table => new
                {
                    IdQuotation = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdCustomer = table.Column<int>(type: "integer", nullable: false),
                    SaleDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SalesExecutives = table.Column<List<string>>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    Folio = table.Column<int>(type: "integer", nullable: false),
                    GeneralComment = table.Column<string>(type: "text", nullable: true),
                    FollowupsJson = table.Column<List<QuotationFollowupsJson>>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    ProductsJson = table.Column<List<SingleProductJson>>(type: "jsonb", nullable: true, defaultValueSql: "'[]'::jsonb"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LinkedSaleId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LinkedSaleFolio = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IdConfigSys = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quotations", x => x.IdQuotation);
                    table.ForeignKey(
                        name: "FK_Quotations_ConfigSys_IdConfigSys",
                        column: x => x.IdConfigSys,
                        principalTable: "ConfigSys",
                        principalColumn: "IdConfigSys",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Quotations_Customers_IdCustomer",
                        column: x => x.IdCustomer,
                        principalTable: "Customers",
                        principalColumn: "IdCustomer",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Sales",
                columns: table => new
                {
                    IdSale = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdCustomer = table.Column<int>(type: "integer", nullable: false),
                    SalesExecutive = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SaleDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Folio = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DeliveryDriver = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    HomeDelivery = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeliveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SatisfactionLevel = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    SatisfactionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, defaultValue: "not specified"),
                    Comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AfterSalesFollowupDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    LinkedQuotationsJson = table.Column<List<QuotationReference>>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    ProductsJson = table.Column<List<SingleProductJson>>(type: "jsonb", nullable: true, defaultValueSql: "'[]'::jsonb"),
                    AuxNoteDataJson = table.Column<AuxNoteDataJson>(type: "jsonb", nullable: true),
                    IdConfigSys = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sales", x => x.IdSale);
                    table.ForeignKey(
                        name: "FK_Sales_Customers_IdCustomer",
                        column: x => x.IdCustomer,
                        principalTable: "Customers",
                        principalColumn: "IdCustomer",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "IdConfigSys",
                        column: x => x.IdConfigSys,
                        principalTable: "ConfigSys",
                        principalColumn: "IdConfigSys",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PropertyValue",
                columns: table => new
                {
                    IdPropertyValue = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Value = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    FatherValue = table.Column<int>(type: "integer", nullable: true),
                    Type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IdProperty = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyValue", x => x.IdPropertyValue);
                    table.ForeignKey(
                        name: "FK_PropertyValue_PropertyValue_FatherValue",
                        column: x => x.FatherValue,
                        principalTable: "PropertyValue",
                        principalColumn: "IdPropertyValue",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PropertyValue_Property_IdProperty",
                        column: x => x.IdProperty,
                        principalTable: "Property",
                        principalColumn: "IdProperty",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Product",
                columns: table => new
                {
                    IdProduct = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MainName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Quantity = table.Column<double>(type: "double precision", nullable: false),
                    Taxes = table.Column<double>(type: "double precision", nullable: false),
                    ImageUrls = table.Column<ICollection<ProductImageJson>>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    IdSupplier = table.Column<int>(type: "integer", nullable: false),
                    CodeJson = table.Column<ICollection<CodeJson>>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    CostsJson = table.Column<ICollection<CostsJson>>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    CategoriesJsons = table.Column<ICollection<CategoriesJson>>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                     SolutionsJsons = table.Column<ICollection<SolutionsJsonProject>>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    AuxDataJson = table.Column<AuxDataJson>(type: "jsonb", nullable: true, defaultValueSql: "'{}'::jsonb")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Product", x => x.IdProduct);
                    table.ForeignKey(
                        name: "FK_Product_Supplier_IdSupplier",
                        column: x => x.IdSupplier,
                        principalTable: "Supplier",
                        principalColumn: "IdSupplier",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PhysicalInventory",
                columns: table => new
                {
                    IdPhysicalInventory = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InventoryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    Observations = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IdWarehouse = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhysicalInventory", x => x.IdPhysicalInventory);
                    table.ForeignKey(
                        name: "FK_PhysicalInventory_Warehouse_IdWarehouse",
                        column: x => x.IdWarehouse,
                        principalTable: "Warehouse",
                        principalColumn: "IdWarehouse",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StorageStructure",
                columns: table => new
                {
                    IdStorageStructure = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CodeRack = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TypeStorageSystem = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OneSection = table.Column<bool>(type: "boolean", nullable: false),
                    HasLevel = table.Column<bool>(type: "boolean", nullable: false),
                    HasSubLevel = table.Column<bool>(type: "boolean", nullable: false),
                    IdWarehouse = table.Column<int>(type: "integer", nullable: false),
                    IdArea = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorageStructure", x => x.IdStorageStructure);
                    table.ForeignKey(
                        name: "FK_StorageStructure_Area_IdArea",
                        column: x => x.IdArea,
                        principalTable: "Area",
                        principalColumn: "IdArea",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StorageStructure_Warehouse_IdWarehouse",
                        column: x => x.IdWarehouse,
                        principalTable: "Warehouse",
                        principalColumn: "IdWarehouse",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    IdUser = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    HashPassword = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Aux = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreateAt = table.Column<DateOnly>(type: "date", nullable: true),
                    Verified = table.Column<bool>(type: "boolean", nullable: true),
                    IdRol = table.Column<int>(type: "integer", nullable: false),
                    IdConfigSys = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.IdUser);
                    table.ForeignKey(
                        name: "FK_User_ConfigSys_IdConfigSys",
                        column: x => x.IdConfigSys,
                        principalTable: "ConfigSys",
                        principalColumn: "IdConfigSys",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_User_Rol_IdRol",
                        column: x => x.IdRol,
                        principalTable: "Rol",
                        principalColumn: "IdRol",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TechnicalDesign",
                columns: table => new
                {
                    IdTechnicalDesign = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SavedDesign = table.Column<string>(type: "text", nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IdProjectCategory = table.Column<int>(type: "integer", nullable: false),
                    SolutionsJsons = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechnicalDesign", x => x.IdTechnicalDesign);
                    table.ForeignKey(
                        name: "FK_TechnicalDesign_ProjectCategory_IdProjectCategory",
                        column: x => x.IdProjectCategory,
                        principalTable: "ProjectCategory",
                        principalColumn: "IdProjectCategory",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ListOfCategories",
                columns: table => new
                {
                    IdListOfCategories = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdProject = table.Column<int>(type: "integer", nullable: false),
                    IdProjectCategory = table.Column<int>(type: "integer", nullable: false),
                    SolutionsJson = table.Column<SolutionsJsonProject>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListOfCategories", x => x.IdListOfCategories);
                    table.ForeignKey(
                        name: "FK_ListOfCategories_ProjectCategory_IdProjectCategory",
                        column: x => x.IdProjectCategory,
                        principalTable: "ProjectCategory",
                        principalColumn: "IdProjectCategory",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ListOfCategories_Project_IdProject",
                        column: x => x.IdProject,
                        principalTable: "Project",
                        principalColumn: "IdProject",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectQuote",
                columns: table => new
                {
                    IdProjectQuotes = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GrandTotal = table.Column<double>(type: "double precision", nullable: false),
                    TotalTaxes = table.Column<double>(type: "double precision", nullable: false),
                    IdProject = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectQuote", x => x.IdProjectQuotes);
                    table.ForeignKey(
                        name: "FK_ProjectQuote_Project_IdProject",
                        column: x => x.IdProject,
                        principalTable: "Project",
                        principalColumn: "IdProject",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuotationVersions",
                columns: table => new
                {
                    IdQuotationVersion = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    GeneralComment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ProductsJson = table.Column<List<SingleProductJson>>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IdQuotation = table.Column<int>(type: "integer", nullable: false),
                    QuotationDataJson = table.Column<QuotationDataJson>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuotationVersions", x => x.IdQuotationVersion);
                    table.ForeignKey(
                        name: "QuotationVersion",
                        column: x => x.IdQuotation,
                        principalTable: "Quotations",
                        principalColumn: "IdQuotation",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SaleQuotations",
                columns: table => new
                {
                    IdSaleQuotation = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdQuotation = table.Column<int>(type: "integer", nullable: false),
                    IdSale = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ProductsJson = table.Column<List<SingleProductJson>>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    PriceSnapshotJson = table.Column<PriceSnapshotJson>(type: "jsonb", nullable: true, defaultValueSql: "'{}'::jsonb"),
                    GeneralComment = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleQuotations", x => x.IdSaleQuotation);
                    table.ForeignKey(
                        name: "FK_SaleQuotation_Quotation_IdQuotation",
                        column: x => x.IdQuotation,
                        principalTable: "Quotations",
                        principalColumn: "IdQuotation",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SaleQuotation_Sale_IdSale",
                        column: x => x.IdSale,
                        principalTable: "Sales",
                        principalColumn: "IdSale",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductProperties",
                columns: table => new
                {
                    IdProductProperties = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomValue = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IdProduct = table.Column<int>(type: "integer", nullable: false),
                    IdPropertyValue = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductProperties", x => x.IdProductProperties);
                    table.ForeignKey(
                        name: "FK_ProductProperties_Product_IdProduct",
                        column: x => x.IdProduct,
                        principalTable: "Product",
                        principalColumn: "IdProduct",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductProperties_PropertyValue_IdPropertyValue",
                        column: x => x.IdPropertyValue,
                        principalTable: "PropertyValue",
                        principalColumn: "IdPropertyValue",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockMovement",
                columns: table => new
                {
                    IdStockMovement = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MovementType = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<double>(type: "double precision", nullable: false),
                    ReferenceType = table.Column<double>(type: "double precision", nullable: false),
                    Description = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ByUser = table.Column<int>(type: "integer", nullable: false),
                    IdProduct = table.Column<int>(type: "integer", nullable: false),
                    IdWarehouse = table.Column<int>(type: "integer", nullable: false),
                    WarehouseIdWarehouse = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMovement", x => x.IdStockMovement);
                    table.ForeignKey(
                        name: "FK_StockMovement_Product_IdProduct",
                        column: x => x.IdProduct,
                        principalTable: "Product",
                        principalColumn: "IdProduct",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockMovement_Warehouse_IdWarehouse",
                        column: x => x.IdWarehouse,
                        principalTable: "Warehouse",
                        principalColumn: "IdWarehouse",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockMovement_Warehouse_WarehouseIdWarehouse",
                        column: x => x.WarehouseIdWarehouse,
                        principalTable: "Warehouse",
                        principalColumn: "IdWarehouse");
                });

            migrationBuilder.CreateTable(
                name: "WarehouseTransferDetail",
                columns: table => new
                {
                    IdTransferDetail = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TransferDate = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<double>(type: "double precision", nullable: false),
                    IdProduct = table.Column<int>(type: "integer", nullable: false),
                    IdWarehouseTransfer = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseTransferDetail", x => x.IdTransferDetail);
                    table.ForeignKey(
                        name: "FK_WarehouseTransferDetail_Product_IdProduct",
                        column: x => x.IdProduct,
                        principalTable: "Product",
                        principalColumn: "IdProduct",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WarehouseTransferDetail_WarehouseTransfer_IdWarehouseTransf~",
                        column: x => x.IdWarehouseTransfer,
                        principalTable: "WarehouseTransfer",
                        principalColumn: "IdWarehouseTransfer",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Inventory",
                columns: table => new
                {
                    IdInventory = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Stock = table.Column<double>(type: "double precision", nullable: false),
                    StockMin = table.Column<double>(type: "double precision", nullable: false),
                    StockMax = table.Column<double>(type: "double precision", nullable: false),
                    LocationDetail = table.Column<double>(type: "double precision", nullable: true),
                    StatusInventoryProduct = table.Column<string>(type: "text", nullable: true),
                    IdPhysicalInventory = table.Column<int>(type: "integer", nullable: true),
                    IdProduct = table.Column<int>(type: "integer", nullable: false),
                    IdWarehouse = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventory", x => x.IdInventory);
                    table.ForeignKey(
                        name: "FK_Inventory_PhysicalInventory_IdPhysicalInventory",
                        column: x => x.IdPhysicalInventory,
                        principalTable: "PhysicalInventory",
                        principalColumn: "IdPhysicalInventory",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Inventory_Product_IdProduct",
                        column: x => x.IdProduct,
                        principalTable: "Product",
                        principalColumn: "IdProduct",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Inventory_Warehouse_IdWarehouse",
                        column: x => x.IdWarehouse,
                        principalTable: "Warehouse",
                        principalColumn: "IdWarehouse",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LocationDetails",
                columns: table => new
                {
                    IdLocationDetails = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Section = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    VerticalLevel = table.Column<int>(type: "integer", nullable: false),
                    IdArea = table.Column<int>(type: "integer", nullable: false),
                    IdStorageStructure = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationDetails", x => x.IdLocationDetails);
                    table.ForeignKey(
                        name: "FK_LocationDetails_Area_IdArea",
                        column: x => x.IdArea,
                        principalTable: "Area",
                        principalColumn: "IdArea",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LocationDetails_StorageStructure_IdStorageStructure",
                        column: x => x.IdStorageStructure,
                        principalTable: "StorageStructure",
                        principalColumn: "IdStorageStructure",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IndividualProjectQuote",
                columns: table => new
                {
                    IdIndividualProjectQuote = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Quantity = table.Column<double>(type: "double precision", nullable: false),
                    UnitPrice = table.Column<double>(type: "double precision", nullable: false),
                    Amount = table.Column<double>(type: "double precision", nullable: false),
                    Total = table.Column<double>(type: "double precision", nullable: false),
                    StatusProcess = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    IdProjectQuotes = table.Column<int>(type: "integer", nullable: false),
                    IdProjectCategory = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndividualProjectQuote", x => x.IdIndividualProjectQuote);
                    table.ForeignKey(
                        name: "FK_IndividualProjectQuote_ProjectCategory_IdProjectCategory",
                        column: x => x.IdProjectCategory,
                        principalTable: "ProjectCategory",
                        principalColumn: "IdProjectCategory",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IndividualProjectQuote_ProjectQuote_IdProjectQuotes",
                        column: x => x.IdProjectQuotes,
                        principalTable: "ProjectQuote",
                        principalColumn: "IdProjectQuotes",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PhysicalInventoryDetail",
                columns: table => new
                {
                    IdPhysicalInventoryDetail = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SystemQuantity = table.Column<double>(type: "double precision", nullable: false),
                    PhysicalQuantity = table.Column<double>(type: "double precision", nullable: false),
                    Difference = table.Column<double>(type: "double precision", nullable: false),
                    StatusInventoryProduct = table.Column<string>(type: "text", nullable: true),
                    IdPhysicalInventory = table.Column<int>(type: "integer", nullable: false),
                    IdProduct = table.Column<int>(type: "integer", nullable: false),
                    IdLocationDetails = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhysicalInventoryDetail", x => x.IdPhysicalInventoryDetail);
                    table.ForeignKey(
                        name: "FK_PhysicalInventoryDetail_LocationDetails_IdLocationDetails",
                        column: x => x.IdLocationDetails,
                        principalTable: "LocationDetails",
                        principalColumn: "IdLocationDetails",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PhysicalInventoryDetail_PhysicalInventory_IdPhysicalInvento~",
                        column: x => x.IdPhysicalInventory,
                        principalTable: "PhysicalInventory",
                        principalColumn: "IdPhysicalInventory",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PhysicalInventoryDetail_Product_IdProduct",
                        column: x => x.IdProduct,
                        principalTable: "Product",
                        principalColumn: "IdProduct",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IndividualListingProperties",
                columns: table => new
                {
                    IdIndividualListingProperties = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdIndividualProjectQuote = table.Column<int>(type: "integer", nullable: false),
                    IdProductProperties = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndividualListingProperties", x => x.IdIndividualListingProperties);
                    table.ForeignKey(
                        name: "FK_IndividualListingProperties_IndividualProjectQuote_IdIndivi~",
                        column: x => x.IdIndividualProjectQuote,
                        principalTable: "IndividualProjectQuote",
                        principalColumn: "IdIndividualProjectQuote",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IndividualListingProperties_ProductProperties_IdProductProp~",
                        column: x => x.IdProductProperties,
                        principalTable: "ProductProperties",
                        principalColumn: "IdProductProperties",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ListOfProductsToQuot",
                columns: table => new
                {
                    IdListOfProductsToQuot = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdIndividualProjectQuotes = table.Column<int>(type: "integer", nullable: false),
                    IdProduct = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListOfProductsToQuot", x => x.IdListOfProductsToQuot);
                    table.ForeignKey(
                        name: "FK_ListOfProductsToQuot_IndividualProjectQuote_IdIndividualPro~",
                        column: x => x.IdIndividualProjectQuotes,
                        principalTable: "IndividualProjectQuote",
                        principalColumn: "IdIndividualProjectQuote",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ListOfProductsToQuot_Product_IdProduct",
                        column: x => x.IdProduct,
                        principalTable: "Product",
                        principalColumn: "IdProduct",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Customers_DirectionJson",
                table: "Customers",
                column: "DirectionJson")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Email",
                table: "Customers",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_ExternalId",
                table: "Customers",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_SettingsCustomerJson",
                table: "Customers",
                column: "SettingsCustomerJson")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_TaxId",
                table: "Customers",
                column: "TaxId");

            migrationBuilder.CreateIndex(
                name: "IX_IndividualListingProperties_IdIndividualProjectQuote",
                table: "IndividualListingProperties",
                column: "IdIndividualProjectQuote");

            migrationBuilder.CreateIndex(
                name: "IX_IndividualListingProperties_IdProductProperties",
                table: "IndividualListingProperties",
                column: "IdProductProperties");

            migrationBuilder.CreateIndex(
                name: "IX_IndividualProjectQuote_IdProjectCategory",
                table: "IndividualProjectQuote",
                column: "IdProjectCategory");

            migrationBuilder.CreateIndex(
                name: "IX_IndividualProjectQuote_IdProjectQuotes",
                table: "IndividualProjectQuote",
                column: "IdProjectQuotes");

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_IdPhysicalInventory",
                table: "Inventory",
                column: "IdPhysicalInventory");

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_IdProduct",
                table: "Inventory",
                column: "IdProduct");

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_IdWarehouse",
                table: "Inventory",
                column: "IdWarehouse");

            migrationBuilder.CreateIndex(
                name: "IX_ListOfCategories_IdProject",
                table: "ListOfCategories",
                column: "IdProject");

            migrationBuilder.CreateIndex(
                name: "IX_ListOfCategories_IdProjectCategory",
                table: "ListOfCategories",
                column: "IdProjectCategory");

            migrationBuilder.CreateIndex(
                name: "IX_ListOfProductsToQuot_IdIndividualProjectQuotes",
                table: "ListOfProductsToQuot",
                column: "IdIndividualProjectQuotes");

            migrationBuilder.CreateIndex(
                name: "IX_ListOfProductsToQuot_IdProduct",
                table: "ListOfProductsToQuot",
                column: "IdProduct");

            migrationBuilder.CreateIndex(
                name: "IX_LocationDetails_IdArea",
                table: "LocationDetails",
                column: "IdArea");

            migrationBuilder.CreateIndex(
                name: "IX_LocationDetails_IdStorageStructure",
                table: "LocationDetails",
                column: "IdStorageStructure");

            migrationBuilder.CreateIndex(
                name: "IX_PhysicalInventory_IdWarehouse",
                table: "PhysicalInventory",
                column: "IdWarehouse");

            migrationBuilder.CreateIndex(
                name: "IX_PhysicalInventoryDetail_IdLocationDetails",
                table: "PhysicalInventoryDetail",
                column: "IdLocationDetails");

            migrationBuilder.CreateIndex(
                name: "IX_PhysicalInventoryDetail_IdPhysicalInventory",
                table: "PhysicalInventoryDetail",
                column: "IdPhysicalInventory");

            migrationBuilder.CreateIndex(
                name: "IX_PhysicalInventoryDetail_IdProduct",
                table: "PhysicalInventoryDetail",
                column: "IdProduct");

            migrationBuilder.CreateIndex(
                name: "IX_Product_IdSupplier",
                table: "Product",
                column: "IdSupplier");

            migrationBuilder.CreateIndex(
                name: "IX_ProductProperties_IdProduct",
                table: "ProductProperties",
                column: "IdProduct");

            migrationBuilder.CreateIndex(
                name: "IX_ProductProperties_IdPropertyValue",
                table: "ProductProperties",
                column: "IdPropertyValue");

            migrationBuilder.CreateIndex(
                name: "IX_Project_IdConfigSys",
                table: "Project",
                column: "IdConfigSys");

            migrationBuilder.CreateIndex(
                name: "IX_Project_IdCustomer",
                table: "Project",
                column: "IdCustomer");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectCategory_IdConfigSys",
                table: "ProjectCategory",
                column: "IdConfigSys");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectQuote_IdProject",
                table: "ProjectQuote",
                column: "IdProject",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PropertyValue_FatherValue",
                table: "PropertyValue",
                column: "FatherValue");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyValue_IdProperty",
                table: "PropertyValue",
                column: "IdProperty");

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_IdConfigSys",
                table: "Quotations",
                column: "IdConfigSys");

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_IdCustomer",
                table: "Quotations",
                column: "IdCustomer");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationVersions_IdQuotation",
                table: "QuotationVersions",
                column: "IdQuotation");

            migrationBuilder.CreateIndex(
                name: "IX_Rol_IdArea",
                table: "Rol",
                column: "IdArea");

            migrationBuilder.CreateIndex(
                name: "IX_SaleQuotations_IdQuotation",
                table: "SaleQuotations",
                column: "IdQuotation");

            migrationBuilder.CreateIndex(
                name: "IX_SaleQuotations_IdSale",
                table: "SaleQuotations",
                column: "IdSale");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_IdConfigSys",
                table: "Sales",
                column: "IdConfigSys");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_IdCustomer",
                table: "Sales",
                column: "IdCustomer");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovement_IdProduct",
                table: "StockMovement",
                column: "IdProduct");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovement_IdWarehouse",
                table: "StockMovement",
                column: "IdWarehouse");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovement_WarehouseIdWarehouse",
                table: "StockMovement",
                column: "WarehouseIdWarehouse");

            migrationBuilder.CreateIndex(
                name: "IX_StorageStructure_IdArea",
                table: "StorageStructure",
                column: "IdArea");

            migrationBuilder.CreateIndex(
                name: "IX_StorageStructure_IdWarehouse",
                table: "StorageStructure",
                column: "IdWarehouse");

            migrationBuilder.CreateIndex(
                name: "IX_TechnicalDesign_IdProjectCategory",
                table: "TechnicalDesign",
                column: "IdProjectCategory");

            migrationBuilder.CreateIndex(
                name: "IX_User_IdConfigSys",
                table: "User",
                column: "IdConfigSys");

            migrationBuilder.CreateIndex(
                name: "IX_User_IdRol",
                table: "User",
                column: "IdRol");

            migrationBuilder.CreateIndex(
                name: "IX_User_UserName",
                table: "User",
                column: "UserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseTransferDetail_IdProduct",
                table: "WarehouseTransferDetail",
                column: "IdProduct");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseTransferDetail_IdWarehouseTransfer",
                table: "WarehouseTransferDetail",
                column: "IdWarehouseTransfer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IndividualListingProperties");

            migrationBuilder.DropTable(
                name: "Inventory");

            migrationBuilder.DropTable(
                name: "ListOfCategories");

            migrationBuilder.DropTable(
                name: "ListOfProductsToQuot");

            migrationBuilder.DropTable(
                name: "PhysicalInventoryDetail");

            migrationBuilder.DropTable(
                name: "QuotationVersions");

            migrationBuilder.DropTable(
                name: "SaleQuotations");

            migrationBuilder.DropTable(
                name: "StockMovement");

            migrationBuilder.DropTable(
                name: "TechnicalDesign");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "WarehouseTransferDetail");

            migrationBuilder.DropTable(
                name: "ProductProperties");

            migrationBuilder.DropTable(
                name: "IndividualProjectQuote");

            migrationBuilder.DropTable(
                name: "LocationDetails");

            migrationBuilder.DropTable(
                name: "PhysicalInventory");

            migrationBuilder.DropTable(
                name: "Quotations");

            migrationBuilder.DropTable(
                name: "Sales");

            migrationBuilder.DropTable(
                name: "Rol");

            migrationBuilder.DropTable(
                name: "WarehouseTransfer");

            migrationBuilder.DropTable(
                name: "Product");

            migrationBuilder.DropTable(
                name: "PropertyValue");

            migrationBuilder.DropTable(
                name: "ProjectCategory");

            migrationBuilder.DropTable(
                name: "ProjectQuote");

            migrationBuilder.DropTable(
                name: "StorageStructure");

            migrationBuilder.DropTable(
                name: "Supplier");

            migrationBuilder.DropTable(
                name: "Property");

            migrationBuilder.DropTable(
                name: "Project");

            migrationBuilder.DropTable(
                name: "Area");

            migrationBuilder.DropTable(
                name: "Warehouse");

            migrationBuilder.DropTable(
                name: "ConfigSys");

            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}
