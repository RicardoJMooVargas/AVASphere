# Sales Page - Visualización de Cotizaciones

## 📱 Layout de sales_page.dart

```
┌─────────────────────────────────────────────────────────────────┐
│ MÓDULO DE VENTAS                                         [☰]    │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌──────────────────────────┬──────────────────────────────────┐ │
│  │   FORMULARIO             │   PANEL + LISTA                  │ │
│  │   (Izquierda)            │   (Derecha)                      │ │
│  │                          │                                  │ │
│  │  ╔════════════════════╗  │  ┌────────────────────────────┐  │ │
│  │  ║ NUEVA COTIZACIÓN   ║  │  │ 📊 ESTADÍSTICAS            │  │ │
│  │  ├────────────────────┤  │  ├────────────────────────────┤  │ │
│  │  │ • Folio            │  │  │ Total: N cotizaciones      │  │ │
│  │  │ • Fecha de Venta   │  │  │ ✓ Aprobadas: X            │  │ │
│  │  │ • Comentario       │  │  │ ⏰ Pendientes: Y           │  │ │
│  │  │                    │  │  │ ✗ Rechazadas: Z           │  │ │
│  │  ├────────────────────┤  │  └────────────────────────────┘  │ │
│  │  │ CLIENTE            │  │                                  │ │
│  │  │ • Búsqueda         │  │  ┌────────────────────────────┐  │ │
│  │  │ • Nombre           │  │  │ 📋 COTIZACIONES            │  │ │
│  │  │ • Email/Teléfono   │  │  ├────────────────────────────┤  │ │
│  │  │                    │  │  │ Folio | Cliente | Fecha    │  │ │
│  │  ├────────────────────┤  │  ├────────────────────────────┤  │ │
│  │  │ EJECUTIVOS         │  │  │ [Tarjeta 1]                │  │ │
│  │  │ + Agregar          │  │  │  • Folio: 001             │  │ │
│  │  │ - Eliminar         │  │  │  • Cliente: XYZ           │  │ │
│  │  │                    │  │  │  • Ejecutivos: Juan, Ana  │  │ │
│  │  ├────────────────────┤  │  │  • Botones: Ver/Editar... │  │ │
│  │  │ SEGUIMIENTOS       │  │  │                            │  │ │
│  │  │ + Agregar          │  │  │ [Tarjeta 2]                │  │ │
│  │  │ - Eliminar         │  │  │  ...                       │  │ │
│  │  │                    │  │  │                            │  │ │
│  │  │ [Crear] [Limpiar]  │  │  │ [Refrescar]                │  │ │
│  │  ╚════════════════════╝  │  └────────────────────────────┘  │ │
│  │                          │                                  │ │
│  └──────────────────────────┴──────────────────────────────────┘ │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

## 🎯 Componentes Visibles

### Lado Izquierdo - Formulario (QuotationFormWidget)

- **Sección 1: Nueva Cotización**

  - Campo: Folio (número)
  - Campo: Fecha de Venta (date picker)
  - Campo: Comentario General (textarea)

- **Sección 2: Información del Cliente**

  - Búsqueda de cliente existente (suggest)
  - Campo: Nombre Completo
  - Campo: Código
  - Campo: Teléfono
  - Campo: Email

- **Sección 3: Ejecutivos de Ventas** (colapsible)

  - Agregar ejecutivos dinámicamente
  - Eliminar ejecutivos individuales

- **Sección 4: Seguimientos** (colapsible)

  - Agregar seguimientos
  - Comentario + Fecha del seguimiento
  - Eliminar seguimientos

- **Botones de Acción**
  - [Crear Cotización]
  - [Limpiar Formulario]

### Lado Derecho - Estadísticas y Lista

#### Panel de Estadísticas (rightHeader)

```
┌─────────────────────────────────────┐
│  Total de Cotizaciones: 15          │
├─────────────────────────────────────┤
│  ✓ Aprobadas: 8                    │
│  ⏰ Pendientes: 5                   │
│  ✗ Rechazadas: 2                   │
└─────────────────────────────────────┘
```

#### Lista de Cotizaciones (rightBody - QuotationListWidget)

Cada tarjeta muestra:

```
┌────────────────────────────────────────────┐
│ Folio: 001               [Estado: Pending]  │
├────────────────────────────────────────────┤
│ 👤 Cliente: XYZ Company                   │
│ 📅 Fecha: 28/11/2025                      │
│ 👥 Ejecutivos: Juan López, Ana García    │
│ 💬 Seguimientos: 3                        │
├────────────────────────────────────────────┤
│ [Ver] [Editar] [Eliminar]                 │
└────────────────────────────────────────────┘
```

## 🔄 Flujo de Interacción

### 1. **Crear Nueva Cotización**

```
Usuario rellena formulario izquierda
    ↓
Click "Crear Cotización"
    ↓
Se envía POST al backend
    ↓
Se añade a la lista (derecha)
    ↓
Se limpian campos automáticamente
```

### 2. **Ver Detalles**

```
Usuario hace click "Ver" en tarjeta
    ↓
Se abre modal QuotationDetailWidget
    ↓
Muestra información completa + seguimientos
    ↓
Opción: Editar, Agregar Seguimiento, Cerrar
```

### 3. **Actualizar Cotización** (TODO)

```
Usuario hace click "Editar"
    ↓
Se llena formulario con datos existentes
    ↓
Usuario modifica datos
    ↓
Click "Actualizar"
    ↓
PUT al backend
```

### 4. **Eliminar Cotización** (TODO)

```
Usuario hace click "Eliminar"
    ↓
Confirmación: "¿Estás seguro?"
    ↓
DELETE al backend
    ↓
Se remueve de la lista
```

### 5. **Refrescar Datos**

```
Click botón "Refrescar" en sidebar
    ↓
GET /api/QuotationManager/GetAll/Quotations
    ↓
Se actualiza lista en tiempo real
```

## 📦 Estados Reactivos (GetX - Obx)

El formulario y la lista están envueltos en `Obx()` para reactividad automática:

```dart
Obx(() => QuotationFormWidget(...))  // Actualiza si cambia isCreating
Obx(() => QuotationListWidget(...))  // Actualiza si cambia quotations
```

Esto permite:

- Mostrar/ocultar indicadores de carga
- Actualizar lista instantáneamente
- Reflejar cambios sin recarga manual

## 🚀 Estados Reales

**Cuando isLoading = true:**

- Muestra CircularProgressIndicator
- Desactiva botones

**Cuando quotations está vacío:**

- Muestra mensaje "No hay cotizaciones"
- Botón "Refrescar"

**Cuando hay datos:**

- Muestra tarjetas de cada cotización
- Permite interacción con botones

## 🎨 Tema y Colores

- **Color Primario**: AppColors.primaryColor
- **Verde**: Aprobadas (✓)
- **Naranja**: Pendientes (⏰)
- **Rojo**: Rechazadas (✗)
- **Gris**: Fondo y neutrales

## ✅ Estados Implementados

✅ Crear cotización
✅ Mostrar lista de cotizaciones
✅ Ver detalles en modal
✅ Estadísticas en tiempo real
✅ Refrescar datos
🔄 Editar cotización (TODO - métodos stub)
🔄 Eliminar cotización (TODO - métodos stub)
🔄 Agregar/Eliminar seguimientos (TODO)
