# Diagrama de Relaciones - Módulo Projects

## Estructura Completa de Relaciones

### Relaciones con ConfigSys (Central)
```
┌──────────────┐
│  ConfigSys   │ (Entidad Central)
└──────┬───────┘
       │
       ├─1:N──> Project (Restrict)
       │
       └─1:N──> ProjectCategory (Restrict)
```

### Relaciones Principales del Módulo Projects

```
┌──────────────┐
│  ConfigSys   │
└──────┬───────┘
       │
       │ 1:N (Restrict)
       ▼
┌──────────────┐                    ┌────────────────────┐
│   Project    │◄──1:1 (Cascade)────│   ProjectQuote     │
└──────┬───────┘                    └─────────┬──────────┘
       │                                      │
       │                                      │ 1:N (Cascade)
       │                                      ▼
       │                            ┌─────────────────────────┐
       │                            │ IndividualProjectQuote  │
       │                            └──────────┬──────────────┘
       │                                       │
       │ 1:N (Cascade)                        │ 1:N (Cascade)
       ▼                                       ├──> IndividualListingProperties
┌──────────────────┐                          │
│ ListOfCategories │                          └──> ListOfProductsToQuot
└──────┬───────────┘
       │
       │ 1:N (Cascade)
       ▼
┌──────────────┐
│   Customer   │
└──────────────┘
```

### Relaciones con ProjectCategory

```
┌──────────────────┐
│ ProjectCategory  │
└────────┬─────────┘
         │
         ├─1:N (Restrict)──> IndividualProjectQuote
         │
         ├─1:N (Restrict)──> ListOfCategories
         │
         ├─1:N (Restrict)──> ListOfProductsByCategory
         │
         └─1:N (Restrict)──> TechnicalDesign
```

## Tabla de Relaciones Detallada

| Entidad Padre | Entidad Hija | Tipo | Delete Behavior | FK en Hija |
|--------------|--------------|------|----------------|------------|
| **ConfigSys** | Project | 1:N | Restrict | IdConfigSys |
| **ConfigSys** | ProjectCategory | 1:N | Restrict | IdConfigSys |
| **Project** | ProjectQuote | 1:1 | Cascade | IdProject |
| **Project** | ListOfCategories | 1:N | Cascade | IdProject |
| **Project** | Customer | 1:N | Cascade | IdProject |
| **ProjectQuote** | IndividualProjectQuote | 1:N | Cascade | IdProjectQuotes |
| **ProjectCategory** | IndividualProjectQuote | 1:N | Restrict | IdProjectCategory |
| **ProjectCategory** | ListOfCategories | 1:N | Restrict | IdProjectCategory |
| **ProjectCategory** | ListOfProductsByCategory | 1:N | Restrict | IdProjectCategory |
| **ProjectCategory** | TechnicalDesign | 1:N | Restrict | IdProjectCategory |
| **IndividualProjectQuote** | IndividualListingProperties | 1:N | Cascade | IdIndividualProjectQuotes |
| **IndividualProjectQuote** | ListOfProductsToQuot | 1:N | Cascade | IdIndividualProjectQuotes |

## Explicación de Delete Behaviors

### Cascade Delete
Se usa cuando la entidad hija **NO tiene sentido sin el padre**:
- ProjectQuote sin Project → se elimina
- IndividualProjectQuote sin ProjectQuote → se elimina
- ListOfCategories sin Project → se elimina
- IndividualListingProperties sin IndividualProjectQuote → se elimina
- ListOfProductsToQuot sin IndividualProjectQuote → se elimina

### Restrict Delete
Se usa cuando queremos **prevenir eliminaciones accidentales**:
- ConfigSys con Projects o ProjectCategories activas → bloqueado
- ProjectCategory en uso por cualquier entidad → bloqueado

## Tipos de Datos Especiales

### JSON Columns (PostgreSQL jsonb)
| Entidad | Campo | Tipo | Descripción |
|---------|-------|------|-------------|
| Project | AppointmentJson | jsonb | Datos de cita inicial |
| Project | VisitsJson | jsonb[] | Array de visitas |
| ListOfCategories | SolutionsJson | jsonb | Soluciones por categoría |
| ListOfCategories | Properties | text | Propiedades adicionales |
| ListOfCategories | Products | text | Productos adicionales |
| ListOfProductsByCategory | SolutionsJson | jsonb | Soluciones de productos |

## Claves Primarias (PK)

| Entidad | Primary Key |
|---------|-------------|
| Project | IdProject |
| ProjectQuote | IdProjectQuotes |
| ProjectCategory | IdProjectCategory |
| IndividualProjectQuote | IdIndividualProjectQuote |
| ListOfCategories | IdListOfCategories |
| ListOfProductsByCategory | IdListOfProductsByCategory |
| TechnicalDesign | IdTechnicalDesign |
| IndividualListingProperties | IdIndividualListingProperties |
| ListOfProductsToQuot | IdListOfProductsToQuot |

## Navegación Bidireccional

Todas las relaciones tienen navegación en **ambos sentidos**:

### Ejemplo: Project ↔ ProjectQuote
```csharp
// En Project
public ProjectQuote? ProjectQuote { get; set; }

// En ProjectQuote
public int IdProject { get; set; }
public Project Project { get; set; } = null!;
```

### Ejemplo: ProjectCategory ↔ IndividualProjectQuote
```csharp
// En ProjectCategory
public ICollection<IndividualProjectQuote> IndividualProjectQuotes { get; set; }

// En IndividualProjectQuote
public int IdProjectCategory { get; set; }
public ProjectCategory ProjectCategory { get; set; } = null!;
```

## Consideraciones Importantes

1. **Relación 1-1 única**: Solo hay una relación 1-1 en el sistema (Project ↔ ProjectQuote)
2. **ConfigSys como raíz**: Todas las entidades principales dependen de ConfigSys
3. **Cascadas controladas**: Solo se eliminan en cascada entidades que no tienen valor independiente
4. **Categorías protegidas**: Las categorías están protegidas contra eliminación accidental
5. **JSON para flexibilidad**: Campos JSON permiten estructura dinámica sin migraciones constantes

## Índices Recomendados (Para futuras optimizaciones)

```sql
-- FK más usadas
CREATE INDEX idx_project_configsys ON "Project"("IdConfigSys");
CREATE INDEX idx_projectquote_project ON "ProjectQuote"("IdProject");
CREATE INDEX idx_individualprojectquote_projectquote ON "IndividualProjectQuote"("IdProjectQuotes");
CREATE INDEX idx_individualprojectquote_category ON "IndividualProjectQuote"("IdProjectCategory");
CREATE INDEX idx_listofcategories_project ON "ListOfCategories"("IdProject");
CREATE INDEX idx_listofcategories_category ON "ListOfCategories"("IdProjectCategory");

-- Búsquedas en JSON (PostgreSQL)
CREATE INDEX idx_project_appointmentjson ON "Project" USING gin("AppointmentJson");
CREATE INDEX idx_project_visitsjson ON "Project" USING gin("VisitsJson");
```

## Archivos de Configuración

Todos ubicados en: `AVASphere.Infrastructure/Projects/Configuration/`

1. ✅ ProjectCategoryEntitieConfig.cs
2. ✅ ProjectEntitieConfig.cs
3. ✅ ProjectQuoteEntitieConfig.cs
4. ✅ IndividualProjectQuoteEntitieConfig.cs
5. ✅ ListOfCategoriesEntitieConfig.cs
6. ✅ ListOfProductsByCategoryEntitieConfig.cs
7. ✅ TechnicalDesignEntitieConfig.cs
8. ✅ IndividualListingPropertiesEntitieConfig.cs
9. ✅ ListOfProductsToQuotEntitieConfig.cs

**Todas las configuraciones están completas y sin errores de compilación.**

