using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ApiDncdCaso.Models
{
    public class CasoDto
    {
        [JsonPropertyName("numero_expediente")]
        public string NumeroExpediente { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre del ciudadano es obligatorio.")]
        [JsonPropertyName("nombre_ciudadano")]
        public string NombreCiudadano { get; set; } = string.Empty;

        [Required(ErrorMessage = "El documento de identidad es obligatorio.")]
        [JsonPropertyName("documento_identidad")]
        public string DocumentoIdentidad { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de caso es obligatorio.")]
        [JsonPropertyName("tipo_caso")]
        public string TipoCaso { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [JsonPropertyName("descripcion")]
        public string Descripcion { get; set; } = string.Empty;
    }

    public class CasoResponseDto : CasoDto
    {
        public int Id { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string Estado { get; set; } = "Abierto";
    }
}