// REVISADO
using AVASphere.ApplicationCore.Common.Entities.General;
using AVASphere.ApplicationCore.Projects.Enum;

namespace AVASphere.ApplicationCore.Projects.Entities.General;

public class Project
{
    public int IdProject { get; set; }
    public int IdProjectQuote { get; set; }
    public int IdConfigSys { get; set; }
    public int IdCustomer { get; set; }
    // Datos generales del proyecto
    public Hitos CurrentHito { get; set; } = Hitos.Appointment;
    public string? WrittenAddress { get; set; } 
    public string? ExactAddress { get; set; }
    // JSON
    public AppointmentJson? AppointmentJson { get; set; }
    public ICollection<VisitsJson>? VisitsJson { get; set; } = new List<VisitsJson>();
    //FK RELATIONS
    public ProjectQuote? ProjectQuote { get; set; }
    public Customer? Customer { get; set; }
    public ConfigSys ConfigSys { get; set; } = null!;
    // EXTERNAL RELATIONS
    public ICollection<ListOfCategories> ListOfCategories { get; set; } = new List<ListOfCategories>();
    
    // Iniciales del cliente + mes + año + categorias usando sus ID, hasta 4 + id de 6 digitos maximo
    // EJEMPLO: JD-0924-0001-000123
    // crear un get que me permita generar este codigo de proyecto
}

public class AppointmentJson
{
    public int Index { get; set; }
    public string? Name { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime Datetime { get; set; }
    public string? Notes { get; set; }
    public string? Direction { get; set; }
    public string? Reference { get; set; }
}

public class VisitsJson
{
    public int Index { get; set; }
    public DateTime DateVisite { get; set; }
    public string? Description { get; set; }
    public int? IdUser { get; set; }
    public string? Type { get; set; }  
    public DateTime CreatedAt { get; set; }
}
