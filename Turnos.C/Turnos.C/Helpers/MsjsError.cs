namespace Turnos.C.Helpers
{
    public static class MsjsError
    {
        public const string ERROR_REQUERIDO = "El campo {0} es requerido";
        public const string ERROR_STRING_LENGTH = "{0} debe tener entre {2} y {1} caracteres";
        public const string ERROR_RANGO = "{0} debe respetar el rango entre {1} y {2}";
        public const string ERROR_SOLO_NUMEROS = "El campo {0} solo puede contener números.";
        public const string ERROR_SOLO_LETRAS = "El campo {0} solo puede contener letras.";
        public const string ERROR_CARACTERES_INVALIDOS = "El campo {0} solo puede contener letras y/o numeros.";
        public const string ERROR_PRESTACION_AMBAS = "Seleccione una prestación existente o cree una nueva, pero no ambas.";
        public const string ERROR_PRESTACION_NINGUNA = "Debe seleccionar una prestación o ingresar una nueva.";
        public const string ERROR_DURACION_INVALIDA = "La duración debe ser mayor a 0.";
        public const string ERROR_PRECIO_INVALIDO = "El precio debe ser mayor a 0.";

    }
}
