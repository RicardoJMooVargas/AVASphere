using System;
using System.Collections.Generic;
using AVASphere.ApplicationCore.Common.Entities.General;
using AVASphere.ApplicationCore.Common.Entities.Jsons;
using AVASphere.ApplicationCore.Sales.Entities;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

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
                    PhoneNumber = table.Column<int>(type: "integer", nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    TaxId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SettingsCustomerJson = table.Column<SettingsCustomerJson>(type: "jsonb", nullable: true, defaultValueSql: "'{}'::jsonb"),
                    DirectionJson = table.Column<DirectionJson>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    PaymentMethodsJson = table.Column<PaymentMethodsJson>(type: "jsonb", nullable: true, defaultValueSql: "'{}'::jsonb"),
                    PaymentTermsJson = table.Column<PaymentTermsJson>(type: "jsonb", nullable: true, defaultValueSql: "'{}'::jsonb")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers_IdCustomer", x => x.IdCustomer);
                });

            migrationBuilder.CreateTable(
                name: "ProjectCategory",
                columns: table => new
                {
                    IdProjectCategory = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectCategory", x => x.IdProjectCategory);
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
                name: "Quotations",
                columns: table => new
                {
                    IdQuotation = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SaleDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SalesExecutives = table.Column<List<string>>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    Folio = table.Column<int>(type: "integer", nullable: false),
                    IdCustomer = table.Column<int>(type: "integer", nullable: false),
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
                    SalesExecutive = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SaleDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IdCustomer = table.Column<int>(type: "integer", nullable: false),
                    Folio = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DeliveryDriver = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    HomeDelivery = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeliveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SatisfactionLevel = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    SatisfactionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                name: "User",
                columns: table => new
                {
                    IdUser = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Password = table.Column<string>(type: "text", nullable: true),
                    HashPassword = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Aux = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreateAt = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Verified = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
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
                    QuotationData = table.Column<Quotation>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb")
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
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ProductsJson = table.Column<List<SingleProductJson>>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    PriceSnapshotJson = table.Column<PriceSnapshotJson>(type: "jsonb", nullable: true, defaultValueSql: "'{}'::jsonb"),
                    GeneralComment = table.Column<string>(type: "text", nullable: true),
                    IdQuotation = table.Column<int>(type: "integer", nullable: false),
                    IdSale = table.Column<int>(type: "integer", nullable: false)
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
                name: "IX_Sales_IdConfigSys",
                table: "Sales",
                column: "IdConfigSys");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_IdCustomer",
                table: "Sales",
                column: "IdCustomer");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectCategory");

            migrationBuilder.DropTable(
                name: "QuotationVersions");

            migrationBuilder.DropTable(
                name: "SaleQuotations");

            migrationBuilder.DropTable(
                name: "Sales");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Quotations");

            migrationBuilder.DropTable(
                name: "Rol");

            migrationBuilder.DropTable(
                name: "ConfigSys");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Area");
        }
    }
}
