using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Turnos.C.Helpers;

namespace Turnos.C.Models
{
    public class Profesional : Persona
    {
        public Matricula Matricula { get; set; }
        
        public Prestacion Prestacion { get; set; }

        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        public int PrestacionId { get; set; }

        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        [DataType(DataType.Time)]
        public TimeOnly HoraInicio { get; set; }
        
        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        [DataType(DataType.Time)]
        public TimeOnly HoraFin { get; set; }
        
        public List<Turno>? Turnos { get; set; }
    }
}