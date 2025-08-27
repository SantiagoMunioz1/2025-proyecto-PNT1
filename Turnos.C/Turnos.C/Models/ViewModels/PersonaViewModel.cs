using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Turnos.C.Models.ViewModels
{
    public class PersonaViewModel
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string DNI { get; set; }
        public string SelectedRole { get; set; }
        public List<SelectListItem> Roles { get; set; }


    }
}
