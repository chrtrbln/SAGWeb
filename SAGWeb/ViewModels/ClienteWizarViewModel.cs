using System.ComponentModel.DataAnnotations;

namespace SAGWeb.ViewModels
{
    public class ClienteWizardViewModel
    {
        [Required]
        public string Nombre { get; set; }

        public string Empresa { get; set; }
        public string Departamento { get; set; }
        public string Municipio { get; set; }
        public string Direccion { get; set; }

        [EmailAddress]
        public string Correo { get; set; }

        public string Telefono { get; set; }
    }
}
