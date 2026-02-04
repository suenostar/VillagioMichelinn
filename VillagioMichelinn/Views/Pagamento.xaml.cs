using System;
using Microsoft.Maui.Controls;

// Toolkit Popups
using CommunityToolkit.Maui.Views;
// Seu popup customizado
using VillagioMichelinn.Popups;

namespace VillagioMichelinn
{
    public partial class Pagamento : ContentPage
    {
        private string _pixChave = "0101093764674";

        public Pagamento()
        {
            InitializeComponent();
        }

        private async void OnCopiarPixClicked(object sender, EventArgs e)
        {
            try
            {
                await Clipboard.SetTextAsync(_pixChave);

                var popup = new PixPopup(
                    titulo: "PIX copiado",
                    mensagem: "A chave PIX foi copiada para a área de transferência.",
                    textoBotao: "Entendi"
                );

                await this.ShowPopupAsync(popup);
            }
            catch
            {
                var popup = new PixPopup(
                    titulo: "Erro",
                    mensagem: "Não foi possível copiar a chave PIX.",
                    textoBotao: "OK"
                );

                await this.ShowPopupAsync(popup);
            }
        }

        private async void OnInicioClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Inicio());
        }
    }
}