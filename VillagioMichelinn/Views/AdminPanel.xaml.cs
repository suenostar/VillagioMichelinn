using System;
using System.Collections.ObjectModel;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace VillagioMichelinn
{
    public partial class AdminPanel : ContentPage
    {
        private readonly CultureInfo ptBR = new("pt-BR");

        // Lista de reservas só para exibição (visual)
        public ObservableCollection<ReservaVisual> Reservas { get; set; } = new();

        public AdminPanel()
        {
            InitializeComponent();

            // Bind da CollectionView
            BindingContext = this;

            // Carrega preços atuais a partir do PrecosConfig

            EntryPrecoAdulto.Text = PrecosConfig.PrecoAdultoPasseio.ToString("N2", ptBR);
            EntryPrecoMeia.Text = PrecosConfig.PrecoMeiaPasseio.ToString("N2", ptBR);
            EntryPrecoCafeAdulto.Text = PrecosConfig.PrecoCafeAdulto.ToString("N2", ptBR);


            AtualizarLabelCafeMeia();

            // Carrega algumas reservas fictícias (apenas visual)
            CarregarReservasFake();
        }

        // =================== Preços ===================
        private void AtualizarLabelCafeMeia()
        {
            if (decimal.TryParse(EntryPrecoCafeAdulto.Text?.Replace("R$", "").Trim(),
                                 NumberStyles.Any,
                                 ptBR,
                                 out decimal precoCafeAdulto))
            {
                decimal meia = precoCafeAdulto / 2m;
                LabelPrecoCafeMeiaInfo.Text =
                    $"Meia Café será R$ {meia.ToString("N2", ptBR)} (metade do Adulto).";
            }
            else
            {
                LabelPrecoCafeMeiaInfo.Text =
                    "Meia Café será calculada como metade do Adulto.";
            }
        }

        private async void OnSalvarPrecosClicked(object sender, EventArgs e)
        {
            // Tenta ler os valores digitados
            bool okAdulto = decimal.TryParse(
                EntryPrecoAdulto.Text?.Replace("R$", "").Trim(),
                NumberStyles.Any,
                ptBR,
                out decimal novoPrecoAdulto);

            bool okMeia = decimal.TryParse(
                EntryPrecoMeia.Text?.Replace("R$", "").Trim(),
                NumberStyles.Any,
                ptBR,
                out decimal novoPrecoMeia);

            bool okCafeAdulto = decimal.TryParse(
                EntryPrecoCafeAdulto.Text?.Replace("R$", "").Trim(),
                NumberStyles.Any,
                ptBR,
                out decimal novoPrecoCafeAdulto);

            if (!okAdulto || !okMeia || !okCafeAdulto)
            {
                await DisplayAlert("Erro",
                    "Verifique se todos os preços foram preenchidos corretamente.",
                    "OK");
                return;
            }

            // Atualiza configuração global de preços
            PrecosConfig.PrecoAdultoPasseio = novoPrecoAdulto;
            PrecosConfig.PrecoMeiaPasseio = novoPrecoMeia;
            PrecosConfig.PrecoCafeAdulto = novoPrecoCafeAdulto;
            // PrecoCafeMeia é calculado pela propriedade na classe PrecosConfig (metade do adulto)

            AtualizarLabelCafeMeia();

            await DisplayAlert("Sucesso", "Preços atualizados com sucesso!", "OK");
        }

        private void EntryPrecoCafeAdulto_TextChanged(object sender, TextChangedEventArgs e)
        {
            AtualizarLabelCafeMeia();
        }

        // =================== Botão para ir até Agendamento Agência ===================
        private async void OnIrAgendamentoAgenciaClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AgendamentoAgencia());
        }


        private async void OnIrAgendamentoClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Agendamento());
        }


        // =================== Reservas (visual) ===================
        private void CarregarReservasFake()
        {
            Reservas.Clear();

            Reservas.Add(new ReservaVisual
            {
                Descricao = "Passeio - 3 Adultos, 1 Meia",
                Detalhes = "Data: 10/03/2026  -  Horário: 09:00",
                Valor = "Total: R$ 52,50"
            });

            Reservas.Add(new ReservaVisual
            {
                Descricao = "Café da manhã - 2 Adultos, 2 Meias",
                Detalhes = "Data: 15/03/2026  -  Horário: 08:00",
                Valor = "Total: R$ 210,00"
            });

            Reservas.Add(new ReservaVisual
            {
                Descricao = "Passeio - 1 Adulto",
                Detalhes = "Data: 20/03/2026  -  Horário: 11:00",
                Valor = "Total: R$ 15,00"
            });
        }

        private async void OnCancelarReservaVisualClicked(object sender, EventArgs e)
        {
            // SOMENTE VISUAL: não altera nada em banco / memória
            await DisplayAlert("Cancelar Reserva",
                "Este botão é apenas visual por enquanto. A lógica de cancelamento ainda não foi implementada.",
                "OK");
        }
    }

    // Modelo simples só para mostrar na CollectionView
    public class ReservaVisual
    {
        public string Descricao { get; set; } = string.Empty;
        public string Detalhes { get; set; } = string.Empty;
        public string Valor { get; set; } = string.Empty;
    }
}
