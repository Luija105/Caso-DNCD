using System.ComponentModel.DataAnnotations;

namespace ApiDncdCaso.Models
{
    public class CasoDto
    {
        [Required(ErrorMessage = "El número de expediente es obligatorio.")]
        public string NumeroExpediente { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre del ciudadano es obligatorio.")]
        public string NombreCiudadano { get; set; } = string.Empty;

        [Required(ErrorMessage = "El documento de identidad es obligatorio.")]
        public string DocumentoIdentidad { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de caso es obligatorio.")]
        public string TipoCaso { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        public string Descripcion { get; set; } = string.Empty;
    }

    public class CasoResponseDto : CasoDto
    {
        public int Id { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string Estado { get; set; } = "Abierto";
    }
}