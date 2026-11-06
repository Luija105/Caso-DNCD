using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;

[Route("api/[controller]")]
[ApiController]
public class CasosController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<CasosController> _logger;

    public CasosController(IConfiguration configuration, ILogger<CasosController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

[HttpGet]
public async Task<IActionResult> Get([FromQuery] string? documento)
{
    try
    {
        var supabaseUrl = _configuration["Supabase:Url"];
        var supabaseKey = _configuration["Supabase:Key"];

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("apikey", supabaseKey);
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {supabaseKey}");

        // Construimos la URL filtrando por número de expediente o documento de identidad
        string url = $"{supabaseUrl}/rest/v1/casos_dncd?select=*";

        if (!string.IsNullOrEmpty(documento))
        {
            string textClean = Uri.EscapeDataString(documento.Trim());
            // Usamos 'or' para buscar tanto en el expediente como en la cédula de forma exacta
            url += $"&or=(numero_expediente.eq.{textClean},documento_identidad.eq.{textClean})";
        }

        var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Error en Supabase: {error}", errorContent);
            return StatusCode(500, new { error = "Error al consultar Supabase" });
        }

        var jsonString = await response.Content.ReadAsStringAsync();
        
        // Devuelve el JSON directamente al WinForms
        return Content(jsonString, "application/json");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Excepción al conectar con Supabase vía REST.");
        Console.WriteLine("--- ERROR DE SUPABASE REST: " + ex.Message);
        return StatusCode(500, new { error = ex.Message });
    }
}

// DTO para mapear la respuesta
// DTO para mapear la respuesta
    public class CasoResponseDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("numero_expediente")]
        public string NumeroExpediente { get; set; } = string.Empty;

        [JsonPropertyName("nombre_ciudadano")]
        public string NombreCiudadano { get; set; } = string.Empty;

        [JsonPropertyName("documento_identidad")]
        public string DocumentoIdentidad { get; set; } = string.Empty;

        [JsonPropertyName("tipo_caso")]
        public string TipoCaso { get; set; } = string.Empty;

        [JsonPropertyName("descripcion")]
        public string Descripcion { get; set; } = string.Empty;

        [JsonPropertyName("fecha_registro")]
        public DateTime FechaRegistro { get; set; }
    }
}
    
    
