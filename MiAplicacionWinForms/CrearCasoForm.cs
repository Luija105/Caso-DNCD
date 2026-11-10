using System.Text;
using System.Text.Json;

namespace MiAplicacionWinForms;

public sealed class CrearCasoForm : Form
{
    private readonly TextBox txtCiudadano = new();
    private readonly TextBox txtDocumento = new();
    private readonly TextBox txtTipo = new();
    private readonly TextBox txtDescripcion = new();
    private readonly Button btnCrear = new();

    public CrearCasoForm()
    {
        Text = "Crear nuevo caso - DNCD";
        Size = new Size(500, 320);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.WhiteSmoke;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        var lblExpediente = new Label
        {
            Text = "Expediente: se genera automáticamente",
            Location = new Point(20, 25),
            AutoSize = true,
            ForeColor = Color.DarkSlateGray
        };
        Controls.Add(lblExpediente);
        CrearCampo("Ciudadano:", txtCiudadano, 65);
        CrearCampo("Documento:", txtDocumento, 105);
        CrearCampo("Tipo de caso:", txtTipo, 145);

        var lblDescripcion = new Label
        {
            Text = "Descripción:",
            Location = new Point(20, 188),
            AutoSize = true,
            Font = new Font(Font, FontStyle.Bold)
        };
        txtDescripcion.Location = new Point(175, 185);
        txtDescripcion.Size = new Size(285, 55);
        txtDescripcion.Multiline = true;
        Controls.Add(lblDescripcion);
        Controls.Add(txtDescripcion);

        btnCrear.Text = "Crear caso";
        btnCrear.Location = new Point(370, 270);
        btnCrear.Size = new Size(90, 30);
        btnCrear.BackColor = Color.FromArgb(0, 120, 212);
        btnCrear.ForeColor = Color.White;
        btnCrear.FlatStyle = FlatStyle.Flat;
        btnCrear.Click += BtnCrear_Click;
        Controls.Add(btnCrear);
    }

    private void CrearCampo(string etiqueta, TextBox campo, int y)
    {
        Controls.Add(new Label
        {
            Text = etiqueta,
            Location = new Point(20, y + 3),
            AutoSize = true,
            Font = new Font(Font, FontStyle.Bold)
        });
        campo.Location = new Point(175, y);
        campo.Width = 285;
        Controls.Add(campo);
    }

    private async void BtnCrear_Click(object? sender, EventArgs e)
    {
        var caso = new CasoDto
        {
            NombreCiudadano = txtCiudadano.Text.Trim(),
            DocumentoIdentidad = txtDocumento.Text.Trim(),
            TipoCaso = txtTipo.Text.Trim(),
            Descripcion = txtDescripcion.Text.Trim()
        };

        if (string.IsNullOrWhiteSpace(caso.NombreCiudadano) ||
            string.IsNullOrWhiteSpace(caso.DocumentoIdentidad) ||
            string.IsNullOrWhiteSpace(caso.TipoCaso) ||
            string.IsNullOrWhiteSpace(caso.Descripcion))
        {
            MessageBox.Show("Completa todos los campos.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            btnCrear.Enabled = false;
            using var client = new HttpClient();
            using var content = new StringContent(
                JsonSerializer.Serialize(caso),
                Encoding.UTF8,
                "application/json");
            var response = await client.PostAsync("http://localhost:5000/api/casos", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                MessageBox.Show(
                    $"La API devolvió {(int)response.StatusCode} ({response.ReasonPhrase}).\n\n{responseContent}",
                    "Error al crear caso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show("Caso creado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudo crear el caso: {ex.Message}", "Error de conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnCrear.Enabled = true;
        }
    }
}
