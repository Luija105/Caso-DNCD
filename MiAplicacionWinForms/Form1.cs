using System.Text.Json;
using System.Text.Json.Serialization;

namespace MiAplicacionWinForms;

public partial class Form1 : Form
{
    // Controles de la interfaz
    private TextBox txtEntrada;
    private Button btnBuscar;
    private Label lblEstadoBusqueda;

    // Campos de visualización de datos
    private Label lblExpedienteVal;
    private Label lblCiudadanoVal;
    private Label lblDocumentoVal;
    private Label lblTipoVal;
    private Label lblEstadoVal;
    private TextBox txtDescripcionVal;



    public Form1()
    {
        InitializeComponent();
        ConfigurarInterfaz();
    }

    private void ConfigurarInterfaz()
    {
        this.Text = "Consulta de Casos - DNCD";
        this.Size = new Size(520, 420);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.WhiteSmoke;

        // Título y Campo de Búsqueda
        var lblTitulo = new Label { Text = "Ingrese Cédula o Expediente:", Location = new Point(20, 20), AutoSize = true, Font = new Font(Font, FontStyle.Bold) };
        
        txtEntrada = new TextBox
        {
            Location = new Point(20, 45),
            Width = 350,
            PlaceholderText = "Ej. 001-1234567-8 o EXP-2026-001"
        };

        btnBuscar = new Button
        {
            Text = "Buscar",
            Location = new Point(380, 43),
            Width = 90,
            BackColor = Color.FromArgb(0, 120, 212),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnBuscar.Click += BtnBuscar_Click;

        lblEstadoBusqueda = new Label
        {
            Text = "Estado: Esperando consulta...",
            Location = new Point(20, 80),
            AutoSize = true,
            ForeColor = Color.Gray
        };

        // Estructura visual de los datos
        int posY = 120;
        CrearFilaDetalle("Nro. Expediente:", ref posY, out lblExpedienteVal);
        CrearFilaDetalle("Ciudadano:", ref posY, out lblCiudadanoVal);
        CrearFilaDetalle("Documento:", ref posY, out lblDocumentoVal);
        CrearFilaDetalle("Tipo de Caso:", ref posY, out lblTipoVal);
        CrearFilaDetalle("Estado:", ref posY, out lblEstadoVal);

        // Caja de texto para la Descripción
        var lblDescTitle = new Label { Text = "Descripción:", Location = new Point(20, posY), AutoSize = true, Font = new Font(Font, FontStyle.Bold) };
        txtDescripcionVal = new TextBox
        {
            Location = new Point(140, posY),
            Width = 330,
            Height = 65,
            Multiline = true,
            ReadOnly = true,
            BackColor = Color.White
        };

        // Agregar todo al formulario
        this.Controls.Add(lblTitulo);
        this.Controls.Add(txtEntrada);
        this.Controls.Add(btnBuscar);
        this.Controls.Add(lblEstadoBusqueda);
        this.Controls.Add(lblDescTitle);
        this.Controls.Add(txtDescripcionVal);
    }

    private void CrearFilaDetalle(string etiqueta, ref int y, out Label lblValor)
    {
        var lblTitle = new Label { Text = etiqueta, Location = new Point(20, y), AutoSize = true, Font = new Font(Font, FontStyle.Bold) };
        lblValor = new Label { Text = "---", Location = new Point(140, y), AutoSize = true, ForeColor = Color.DarkSlateGray };
        
        this.Controls.Add(lblTitle);
        this.Controls.Add(lblValor);
        y += 32;
    }

    private async void BtnBuscar_Click(object? sender, EventArgs e)
    {
        string texto = txtEntrada.Text.Trim();
        if (string.IsNullOrEmpty(texto))
        {
            MessageBox.Show("Por favor, escriba un número de cédula o expediente.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        lblEstadoBusqueda.Text = "Buscando en el servidor...";
        lblEstadoBusqueda.ForeColor = Color.Blue;

        try
        {
            using var client = new HttpClient();
            string url = $"http://localhost:5000/api/casos?documento={texto}";

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                
                var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var casos = JsonSerializer.Deserialize<List<CasoResponseDto>>(jsonString, opciones);

                if (casos != null && casos.Count > 0)
                {
                    var caso = casos[0];

                    // Asignación correcta de valores a los controles definidos arriba
                    lblExpedienteVal.Text = caso.NumeroExpediente;
                    lblCiudadanoVal.Text = caso.NombreCiudadano;
                    lblDocumentoVal.Text = caso.DocumentoIdentidad;
                    lblTipoVal.Text = caso.TipoCaso;
                    txtDescripcionVal.Text = caso.Descripcion;
                    lblEstadoVal.Text = "Activo"; // O el valor que corresponda de tu lógica

                    lblEstadoBusqueda.Text = "¡Registro encontrado con éxito!";
                    lblEstadoBusqueda.ForeColor = Color.Green;
                }
                else
                {
                    LimpiarCampos();
                    lblEstadoBusqueda.Text = "No se encontraron registros con ese criterio.";
                    lblEstadoBusqueda.ForeColor = Color.DarkOrange;
                }
            }
            else
            {
                LimpiarCampos();
                lblEstadoBusqueda.Text = "Error al consultar la API.";
                lblEstadoBusqueda.ForeColor = Color.Red;
            }
        }
        catch (Exception ex)
        {
            LimpiarCampos();
            lblEstadoBusqueda.Text = "Sin conexión con el servidor.";
            lblEstadoBusqueda.ForeColor = Color.Red;
            MessageBox.Show($"No se pudo conectar con la API: {ex.Message}", "Error de Conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LimpiarCampos()
    {
        lblExpedienteVal.Text = "---";
        lblCiudadanoVal.Text = "---";
        lblDocumentoVal.Text = "---";
        lblTipoVal.Text = "---";
        lblEstadoVal.Text = "---";
        txtDescripcionVal.Text = string.Empty;
    }
}

// Estructura de datos que recibe el JSON de la API
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