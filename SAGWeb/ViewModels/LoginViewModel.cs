using System.ComponentModel.DataAnnotations;

namespace SAGWeb.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        public string Nombre { get; set; }

        [Required]
        public string Pin { get; set; }
    }
}
