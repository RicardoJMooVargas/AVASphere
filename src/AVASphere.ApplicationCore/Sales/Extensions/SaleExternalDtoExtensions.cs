using AVASphere.ApplicationCore.Common.DTOs;
using AVASphere.ApplicationCore.Common.Entities.General;
using AVASphere.ApplicationCore.Common.Entities.Jsons;
using AVASphere.ApplicationCore.Sales.DTOs;
using AVASphere.ApplicationCore.Sales.Entities;

namespace AVASphere.ApplicationCore.Common.Extensions;

public static class SaleExternalDtoExtensions
{
    public static Sale ToEntity(this SaleExternalDto dto, int customerId, string salesExecutive = "system", int idConfigSys = 1)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        return new Sale
        {
            SalesExecutive = salesExecutive,
            SaleDate = DateTime.UtcNow,
            Type = "External",
            IdCustomer = customerId,
            Folio = dto.Folio,
            TotalAmount = dto.Total,

            // Productos mapeados a SingleProductJson
            ProductsJson = dto.Productos?.Select(p => new SingleProductJson
            {
                Description = p.Descripcion,
                Quantity = (double)p.Cantidad,
                Unit = p.Unidad,
                UnitPrice = p.Precio,
                TotalPrice = p.Total
            }).ToList(),

            // Datos de nota externa
            AuxNoteDataJson = new AuxNoteDataJson
            {
                Cliente = dto.CodeClient,
                NombreCliente = dto.NombreCliente,
                Folio = dto.Folio,
                Fecha = dto.Fecha,
                Hora = dto.Hora,
                Serie = dto.Serie,
                Caja = dto.Caja,
                Zn = dto.ZN,
                Nf = dto.NF,
                Agente = dto.Agente,
                DireccionCliente = dto.DireccionCliente,
                PoblacionCliente = dto.PoblacionCliente,
                EmailCliente = dto.EmailCliente,
                TelCliente = dto.TelCliente,
                Importe = dto.Importe,
                Descuento = dto.Descuento,
                Impuesto = dto.Impuesto,
                Total = dto.Total,
                ExisteEnDB = false
            },

            IdConfigSys = dto.IdConfigSys,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}