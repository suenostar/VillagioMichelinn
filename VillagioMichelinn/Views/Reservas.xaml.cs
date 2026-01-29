using System;
using Microsoft.Maui.Controls;

namespace VillagioMichelinn
{
    public partial class Reservas : ContentPage
    {
        public Reservas()
        {
            InitializeComponent();
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

            // TODO: remova a reserva aqui (backend/coleção) e atualize a lista
            await DisplayAlert("Cancelado", "A reserva foi cancelada!", "OK");
        }
    }
}