using System;
using Microsoft.Maui.Controls;

namespace VillagioMichelinn
{
    public partial class Inicio : ContentPage
    {
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

            // Limpa a reserva atual ao cancelar
            ReservaStore.ReservaAtual = null;

            NomeReservaLabel.Text = "Reserva cancelada";
            DataHorarioLabel.Text = "-";
            ValorLabel.Text = "-";

            await DisplayAlert("Cancelado", "A reserva foi cancelada!", "OK");
        }
    }
}