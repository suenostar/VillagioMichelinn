using System;
using Microsoft.Maui.Controls;

namespace VillagioMichelinn
{
    public partial class Inicio : ContentPage
    {
        // ========= SECRETO: controle de taps no logo =========
        private int secretTapCount = 0;
        private DateTime lastTapTime = DateTime.MinValue;

        public Inicio()
        {
            InitializeComponent();
            CarregarReserva();
        }

        private void CarregarReserva()
        {
            var reserva = ReservaStore.ReservaAtual;

            if (reserva == null)
            {
                NomeReservaLabel.Text = "Nenhuma reserva encontrada";
                DataHorarioLabel.Text = "-";
                ValorLabel.Text = "-";
                return;
            }

            NomeReservaLabel.Text = reserva.NomeCliente;
            DataHorarioLabel.Text = reserva.ResumoDataHorario;
            ValorLabel.Text = reserva.ResumoValor;
        }

        private async void OnNreservaClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Agendamento());
        }

        private async void OnMenuClicked(object sender, EventArgs e)
        {
            CancelCard.Opacity = 0;
            CancelCard.IsVisible = true;
            await CancelCard.FadeTo(1, 150, Easing.CubicOut);
        }

        private async void OnCancelNo(object sender, EventArgs e)
        {
            await CancelCard.FadeTo(0, 120);
            CancelCard.IsVisible = false;
        }

        private async void OnCancelYes(object sender, EventArgs e)
        {
            await CancelCard.FadeTo(0, 120);
            CancelCard.IsVisible = false;

            ReservaStore.ReservaAtual = null;

            NomeReservaLabel.Text = "Reserva cancelada";
            DataHorarioLabel.Text = "-";
            ValorLabel.Text = "-";

            await DisplayAlert("Cancelado", "A reserva foi cancelada!", "OK");
        }

        // ========= TAP SECRETO NO LOGO =========
        private async void OnLogoTapped(object sender, TappedEventArgs e)
        {
            var now = DateTime.Now;

            // Se passaram mais de 2 segundos desde o último toque, zera o contador
            if ((now - lastTapTime).TotalSeconds > 2)
                secretTapCount = 0;

            secretTapCount++;
            lastTapTime = now;

            // Quando chegar em 5 toques rápidos, abre o painel admin
            if (secretTapCount >= 5)
            {
                secretTapCount = 0; // reseta para próxima vez

                // Se quiser colocar senha, dá para adicionar aqui
                // string senha = await DisplayPromptAsync("Admin", "Digite a senha:");
                // if (senha != "1234") return;

                await Navigation.PushAsync(new AdminPanel());
            }
        }
    }
}
