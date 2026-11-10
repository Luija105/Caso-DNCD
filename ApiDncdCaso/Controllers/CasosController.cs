using Microsoft.AspNetCore.Mvc;
using ApiDncdCaso.Models;
using System.Text.RegularExpressions;
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

[HttpPost]
public async Task<IActionResult> Create([FromBody] CasoDto caso)
{
    var supabaseUrl = _configuration["Supabase:Url"];
    var supabaseKey = _configuration["Supabase:Key"];

    if (string.IsNullOrWhiteSpace(supabaseUrl) || string.IsNullOrWhiteSpace(supabaseKey))
    {
        _logger.LogError("La configuración de Supabase está incompleta.");
        return StatusCode(500, new { error = "La configuración de Supabase está incompleta." });
    }

    try
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("apikey", supabaseKey);
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {supabaseKey}");
        client.DefaultRequestHeaders.Add("Prefer", "return=representation");

        caso.NumeroExpediente = await GenerarNumeroExpedienteAsync(client, supabaseUrl, supabaseKey);
        var json = JsonSerializer.Serialize(caso);
        using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"{supabaseUrl}/rest/v1/casos_dncd", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Error al crear caso en Supabase: {error}", responseContent);
            return StatusCode((int)response.StatusCode, new { error = responseContent });
        }

        return Content(responseContent, "application/json", System.Text.Encoding.UTF8);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Excepción al crear caso en Supabase.");
        return StatusCode(500, new { error = "No fue posible crear el caso." });
    }
}

private async Task<string> GenerarNumeroExpedienteAsync(HttpClient client, string supabaseUrl, string supabaseKey)
{
    var year = DateTime.UtcNow.Year;
    var url = $"{supabaseUrl}/rest/v1/casos_dncd?select=numero_expediente&numero_expediente=like.EXP-{year}-*";
    var response = await client.GetAsync(url);
    var responseContent = await response.Content.ReadAsStringAsync();

    if (!response.IsSuccessStatusCode)
    {
        _logger.LogError("Error al obtener expedientes existentes: {error}", responseContent);
        throw new InvalidOperationException("No fue posible generar el número de expediente.");
    }

    var expedientes = JsonSerializer.Deserialize<List<ExpedienteExistenteDto>>(responseContent) ?? [];
    var ultimoNumero = expedientes
        .Select(x => Regex.Match(x.NumeroExpediente ?? string.Empty, $"^EXP-{year}-(\\d+)$"))
        .Where(match => match.Success)
        .Select(match => int.Parse(match.Groups[1].Value))
        .DefaultIfEmpty(0)
        .Max();

    return $"EXP-{year}-{ultimoNumero + 1:D3}";
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

    private sealed class ExpedienteExistenteDto
    {
        [JsonPropertyName("numero_expediente")]
        public string? NumeroExpediente { get; set; }
    }
}
    
    
