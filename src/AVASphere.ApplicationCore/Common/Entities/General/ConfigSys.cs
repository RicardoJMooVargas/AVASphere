using AVASphere.ApplicationCore.Projects.Entities.Catalogs;
using AVASphere.ApplicationCore.Projects.Entities.General;
using AVASphere.ApplicationCore.Sales.Entities;

namespace AVASphere.ApplicationCore.Common.Entities.General
{
    public class ConfigSys
    {
        public int IdConfigSys { get; set; }

        public string CompanyName { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;

        public ICollection<ColorsJson> Colors { get; set; } = new List<ColorsJson>();
        public ICollection<NotUseModuleJson> NotUseModules { get; set; } = new List<NotUseModuleJson>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // FK DESACTIVADO, FUERA DE USO
        //public int IdBranchOffice { get; set; } = 0;
        //public BranchOffice? BranchOffice { get; set; } = null;

        // 🔹 Relación 1-N
        // TOCA ENTIDAD ANCLA DE SU MODULO DEBE IR AQUÍ
        public ICollection<User> Users { get; set; } = new List<User>();
        // public ICollection<Project> Projects { get; set; } = new List<Project>();
        public ICollection<Quotation> Quotations { get; set; } = new List<Quotation>();
        public ICollection<Sale> Sales { get; set; } = new List<Sale>();

    }

    public class ColorsJson
    {
        public int Index { get; set; } = 0;
        public string NameColor { get; set; } = "Default";
        public string ColorCode { get; set; } = string.Empty;
        public string ColorRgb { get; set; } = string.Empty;
    }

    public class NotUseModuleJson
    {
        public int Index { get; set; } = 0;
        public string NameModule { get; set; } = "Default";
    }
}