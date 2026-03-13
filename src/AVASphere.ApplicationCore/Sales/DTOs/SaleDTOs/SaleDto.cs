using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AVASphere.ApplicationCore.Sales.DTOs
{
    public class SaleExternalDto
    {
        // Datos de cabecera
        public string NF { get; set; } = "F";
        public string Caja { get; set; } = "12";
        public string Serie { get; set; } = "CR";
        public string Folio { get; set; } = string.Empty;
        public string Fecha { get; set; } = string.Empty;
        public string Hora { get; set; } = string.Empty;
        public string ZN { get; set; } = string.Empty;
        public string Agente { get; set; } = string.Empty;
        public string CodeClient { get; set; } = string.Empty;
        public string NombreCliente { get; set; } = string.Empty;
        public string TelCliente { get; set; } = string.Empty;
        public string EmailCliente { get; set; } = string.Empty;
        public string DireccionCliente { get; set; } = string.Empty;
        public string PoblacionCliente { get; set; } = string.Empty;

        // Totales
        public decimal Importe { get; set; }
        public decimal Descuento { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Total { get; set; }

        // Productos
        public List<SaleProductExternalDto> Productos { get; set; } = new();

        // 🔹 Nuevo campo para la configuración
        public int IdConfigSys { get; set; }
    }

    public class SaleProductExternalDto
    {
        public int Mov { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Unidad { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public decimal Precio { get; set; }
        public decimal Dcto { get; set; }
        public decimal Impto { get; set; }
        public decimal Total { get; set; }
    }


}