using CommunityToolkit.Maui.Views;

namespace VillagioMichelinn.Popups;

public partial class PixPopup : Popup
{
    public string Titulo { get; set; } = "PIX copiado";
    public string Mensagem { get; set; } = "A chave PIX foi copiada para a área de transferência.";
    public string TextoBotao { get; set; } = "OK";

    public PixPopup(string? titulo = null, string? mensagem = null, string? textoBotao = null)
    {
        InitializeComponent();

        if (!string.IsNullOrWhiteSpace(titulo)) Titulo = titulo;
        if (!string.IsNullOrWhiteSpace(mensagem)) Mensagem = mensagem;
        if (!string.IsNullOrWhiteSpace(textoBotao)) TextoBotao = textoBotao;

        BindingContext = this;
    }

    private void OnFecharClicked(object? sender, EventArgs e)
    {
        Close();
    }
}