using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Turnos.C.Helpers;

namespace Turnos.C.Models
{
    public class Paciente : Persona
    {
        public Cobertura Cobertura { get; set; }
        public List<Turno>? Turnos { get; set; }
        public List<Formulario>? Formularios { get; set; }
    }
}