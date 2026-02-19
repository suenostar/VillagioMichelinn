using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace VillagioMichelinn
{
    public partial class AgendamentoAgencia : ContentPage
    {
        private DateTime currentMonth = DateTime.Now;
        private Button? selectedDayButton = null;
        private Button? selectedHorarioButton = null;
        private const int LimiteDiasAgencia = 15;

        // =================== Passeio ===================
        private int adulto = 0;
        private int meia = 0;
        private int naoPagante = 0;
        private decimal precoAdulto = 15m;
        private decimal precoMeia = 7.5m;

        // =================== Café da manhã ===================
        private int cafeAdulto = 0;
        private int cafeMeia = 0;
        private int cafeNaoPagante = 0;
        private decimal precoCafeManha = 70m; // preço cheio do café

        private static readonly CultureInfo ptBR = new("pt-BR");
        private decimal totalAtual = 0m;

        private readonly Dictionary<string, string> safraPorMes = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Janeiro", "Uva, Goiaba, Morango, Lichia" },
            { "Fevereiro", "Uva, Goiaba, Morango" },
            { "Março", "Uva, Goiaba" },
            { "Abril", "Goiaba, Morango" },
            { "Maio", "Uva, Goiaba, Morango" },
            { "Junho", "Uva, Goiaba, Morango" },
            { "Julho", "Uva, Goiaba, Morango" },
            { "Agosto", "Morango" },
            { "Setembro", "Morango" },
            { "Outubro", "Pêssego, Morango, Goiaba" },
            { "Novembro", "Pêssego, Morango, Goiaba" },
            { "Dezembro", "Uva, Goiaba, Morango, Lichia" }
        };

        private enum SelectedMode
        {
            None,
            Passeio,
            CafeManha
        }

        private SelectedMode modoSelecionado = SelectedMode.None;

        public AgendamentoAgencia()
        {
            InitializeComponent();             // <-- precisa existir por causa do XAML
            BuildCalendar(currentMonth);
            AtualizarSafra(currentMonth);
            SetHorariosEnabled(false);
            AtualizarTotal();
        }

        // =================== Calendário ===================
        private void BuildCalendar(DateTime month)
        {
            CalendarGrid.Children.Clear();
            MonthLabel.Text = month.ToString("MMMM yyyy", ptBR).ToUpper();

            var firstDay = new DateTime(month.Year, month.Month, 1);
            int daysInMonth = DateTime.DaysInMonth(month.Year, month.Month);
            int startColumn = (int)firstDay.DayOfWeek;
            int row = 0;
            int col = startColumn;
            var hoje = DateTime.Today;
            var minDate = hoje.AddDays(LimiteDiasAgencia);

            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(month.Year, month.Month, day);
                var btn = new Button
                {
                    Text = day.ToString(),
                    BackgroundColor = Colors.LightGreen,
                    TextColor = Colors.Black,
                    CornerRadius = 20,
                    WidthRequest = 40,
                    HeightRequest = 40,
                    FontSize = 12
                };

                if (date.Date < minDate)
                {
                    btn.BackgroundColor = Colors.LightGray;
                    btn.TextColor = Colors.DarkGray;
                    btn.IsEnabled = false;
                }

                if (date.Date == hoje && btn.IsEnabled)
                {
                    btn.BorderColor = Colors.DarkGreen;
                    btn.BorderWidth = 2;
                }

                btn.Clicked += OnDayClicked;
                CalendarGrid.Add(btn, col, row);
                col++;
                if (col > 6)
                {
                    col = 0;
                    row++;
                }
            }
        }

        private void AtualizarSafra(DateTime mesAtual)
        {
            string nomeMes = mesAtual.ToString("MMMM", ptBR);
            if (safraPorMes.TryGetValue(nomeMes, out var safra))
            {
                SafraLabel!.Text = $"Frutas disponíveis em {nomeMes}:\n{safra}";
            }
            else
            {
                SafraLabel!.Text = "Safra não disponível.";
            }
        }

        private void OnPreviousMonthClicked(object sender, EventArgs e)
        {
            currentMonth = currentMonth.AddMonths(-1);
            BuildCalendar(currentMonth);
            AtualizarSafra(currentMonth);

            if (selectedDayButton != null)
            {
                selectedDayButton.BackgroundColor = Colors.LightGreen;
                selectedDayButton = null;
            }
            selectedHorarioButton = null;
            SetHorariosEnabled(false);
        }

        private void OnNextMonthClicked(object sender, EventArgs e)
        {
            currentMonth = currentMonth.AddMonths(1);
            BuildCalendar(currentMonth);
            AtualizarSafra(currentMonth);

            if (selectedDayButton != null)
            {
                selectedDayButton.BackgroundColor = Colors.LightGreen;
                selectedDayButton = null;
            }
            selectedHorarioButton = null;
            SetHorariosEnabled(false);
        }

        private void OnDayClicked(object? sender, EventArgs e)
        {
            if (selectedDayButton != null)
                selectedDayButton.BackgroundColor = Colors.LightGreen;

            selectedDayButton = sender as Button;
            if (selectedDayButton != null)
            {
                selectedDayButton.BackgroundColor = Colors.Yellow;
                _ = DisplayAlert("Dia Selecionado",
                    $"Você escolheu {selectedDayButton.Text}/{currentMonth.Month}/{currentMonth.Year}",
                    "OK");
            }

            SetHorariosEnabled(true);
        }

        // =================== Horários ===================
        private void SetHorariosEnabled(bool enabled)
        {
            if (HorariosFlex is null) return;

            HorariosFlex.IsEnabled = enabled;

            foreach (var b in HorariosFlex.Children.OfType<Button>())
            {
                b.IsEnabled = enabled;
                if (!enabled)
                {
                    b.BackgroundColor = Color.FromArgb("#A4FF88");
                    b.TextColor = Colors.Black;
                }
            }

            if (!enabled)
                selectedHorarioButton = null;
        }

        private void OnHorarioClicked(object sender, EventArgs e)
        {
            var botaoClicado = (Button)sender;

            if (!botaoClicado.IsEnabled)
                return;

            if (selectedHorarioButton != null)
                selectedHorarioButton.BackgroundColor = Color.FromArgb("#A4FF88");

            botaoClicado.BackgroundColor = Colors.Yellow;
            selectedHorarioButton = botaoClicado;
        }

        // =================== Passeio - Quantidades ===================
        private void OnAdultoMais(object sender, EventArgs e)
        {
            SetMode(SelectedMode.Passeio);
            adulto++;
            AdultoCount!.Text = adulto.ToString();
            AtualizarTotal();
        }

        private void OnAdultoMenos(object sender, EventArgs e)
        {
            SetMode(SelectedMode.Passeio);
            if (adulto > 0) adulto--;
            AdultoCount!.Text = adulto.ToString();
            AtualizarTotal();
        }

        private void OnMeiaMais(object sender, EventArgs e)
        {
            SetMode(SelectedMode.Passeio);
            meia++;
            MeiaCount!.Text = meia.ToString();
            AtualizarTotal();
        }

        private void OnMeiaMenos(object sender, EventArgs e)
        {
            SetMode(SelectedMode.Passeio);
            if (meia > 0) meia--;
            MeiaCount!.Text = meia.ToString();
            AtualizarTotal();
        }

        private void OnNaoPaganteMais(object sender, EventArgs e)
        {
            SetMode(SelectedMode.Passeio);
            naoPagante++;
            NaoPaganteCount!.Text = naoPagante.ToString();
        }

        private void OnNaoPaganteMenos(object sender, EventArgs e)
        {
            SetMode(SelectedMode.Passeio);
            if (naoPagante > 0) naoPagante--;
            NaoPaganteCount!.Text = naoPagante.ToString();
        }

        // =================== Café - Quantidades ===================
        private void OnCafeAdultoMais(object sender, EventArgs e)
        {
            SetMode(SelectedMode.CafeManha);
            cafeAdulto++;
            CafeAdultoCount!.Text = cafeAdulto.ToString();
            AtualizarTotal();
        }

        private void OnCafeAdultoMenos(object sender, EventArgs e)
        {
            SetMode(SelectedMode.CafeManha);
            if (cafeAdulto > 0) cafeAdulto--;
            CafeAdultoCount!.Text = cafeAdulto.ToString();
            AtualizarTotal();
        }

        private void OnCafeMeiaMais(object sender, EventArgs e)
        {
            SetMode(SelectedMode.CafeManha);
            cafeMeia++;
            CafeMeiaCount!.Text = cafeMeia.ToString();
            AtualizarTotal();
        }

        private void OnCafeMeiaMenos(object sender, EventArgs e)
        {
            SetMode(SelectedMode.CafeManha);
            if (cafeMeia > 0) cafeMeia--;
            CafeMeiaCount!.Text = cafeMeia.ToString();
            AtualizarTotal();
        }

        private void OnCafeNaoPaganteMais(object sender, EventArgs e)
        {
            SetMode(SelectedMode.CafeManha);
            cafeNaoPagante++;
            CafeNaoPaganteCount!.Text = cafeNaoPagante.ToString();
        }

        private void OnCafeNaoPaganteMenos(object sender, EventArgs e)
        {
            SetMode(SelectedMode.CafeManha);
            if (cafeNaoPagante > 0) cafeNaoPagante--;
            CafeNaoPaganteCount!.Text = cafeNaoPagante.ToString();
        }

        // =================== Modo (Passeio x Café) ===================
        private void SetMode(SelectedMode novoModo)
        {
            if (modoSelecionado == novoModo) return;

            modoSelecionado = novoModo;

            switch (novoModo)
            {
                case SelectedMode.Passeio:
                    expPasseio.IsEnabled = true;
                    expCafe.IsEnabled = false;
                    ResetCafe();
                    break;

                case SelectedMode.CafeManha:
                    expPasseio.IsEnabled = false;
                    expCafe.IsEnabled = true;
                    ResetIngressos();
                    break;

                case SelectedMode.None:
                default:
                    expPasseio.IsEnabled = true;
                    expCafe.IsEnabled = true;
                    break;
            }

            AtualizarTotal();
        }

        private void ResetIngressos()
        {
            adulto = 0;
            meia = 0;
            naoPagante = 0;

            AdultoCount!.Text = "0";
            MeiaCount!.Text = "0";
            NaoPaganteCount!.Text = "0";
        }

        private void ResetCafe()
        {
            cafeAdulto = 0;
            cafeMeia = 0;
            cafeNaoPagante = 0;

            CafeAdultoCount!.Text = "0";
            CafeMeiaCount!.Text = "0";
            CafeNaoPaganteCount!.Text = "0";
        }

        // =================== Total ===================
        private void AtualizarTotal()
        {
            decimal total = 0m;

            switch (modoSelecionado)
            {
                case SelectedMode.Passeio:
                    total += adulto * precoAdulto;
                    total += meia * precoMeia;
                    // naoPagante não paga
                    break;

                case SelectedMode.CafeManha:
                    total += cafeAdulto * precoCafeManha;

                    // Meia café = metade do valor do café
                    decimal precoCafeMeia = precoCafeManha / 2;
                    total += cafeMeia * precoCafeMeia;
                    // cafeNaoPagante não paga
                    break;

                case SelectedMode.None:
                default:
                    total = 0m;
                    break;
            }

            totalAtual = total;
            TotalLabel!.Text = total.ToString("C", ptBR);
        }

        // =================== Trocar pacote ===================
        private void OnTrocarPacoteClicked(object sender, EventArgs e)
        {
            modoSelecionado = SelectedMode.None;

            ResetIngressos();
            ResetCafe();

            expPasseio.IsEnabled = true;
            expCafe.IsEnabled = true;

            expPasseio.IsExpanded = false;
            expCafe.IsExpanded = false;

            AtualizarTotal();
        }

        // =================== Pagamento ===================
        private async void OnPagarClicked(object sender, EventArgs e)
        {
            if (selectedDayButton is null)
            {
                await DisplayAlert("Atenção", "Escolha um dia no calendário.", "OK");
                return;
            }
            if (selectedHorarioButton is null)
            {
                await DisplayAlert("Atenção", "Escolha um horário.", "OK");
                return;
            }
            if (modoSelecionado == SelectedMode.None)
            {
                await DisplayAlert("Atenção", "Escolha um pacote: Passeio ou Café da manhã.", "OK");
                return;
            }

            switch (modoSelecionado)
            {
                case SelectedMode.Passeio:
                    if (adulto + meia <= 0)
                    {
                        await DisplayAlert("Atenção", "Adicione pelo menos 1 ingresso pago (Adulto ou Meia).", "OK");
                        return;
                    }
                    break;

                case SelectedMode.CafeManha:
                    if (cafeAdulto + cafeMeia <= 0)
                    {
                        await DisplayAlert("Atenção", "Adicione pelo menos 1 Café pago (Adulto ou Meia).", "OK");
                        return;
                    }
                    break;
            }

            int dia = int.Parse(selectedDayButton.Text!);
            var dataSelecionada = new DateTime(currentMonth.Year, currentMonth.Month, dia);
            string horarioSelecionado = selectedHorarioButton.Text ?? string.Empty;

            string tipoPacote = modoSelecionado switch
            {
                SelectedMode.Passeio => "Passeio (Agência)",
                SelectedMode.CafeManha => "Café da manhã (Agência)",
                _ => "Indefinido (Agência)"
            };

            // Aproveita os mesmos campos da Reserva tanto para passeio quanto para café
            int qtdeAdultoFinal = modoSelecionado == SelectedMode.Passeio ? adulto : cafeAdulto;
            int qtdeMeiaFinal = modoSelecionado == SelectedMode.Passeio ? meia : cafeMeia;
            int qtdeNaoPaganteFinal = modoSelecionado == SelectedMode.Passeio ? naoPagante : cafeNaoPagante;

            var reserva = new Reserva
            {
                NomeCliente = "Agência",
                Data = dataSelecionada,
                Horario = horarioSelecionado,
                TipoPacote = tipoPacote,
                QtdeAdulto = qtdeAdultoFinal,
                QtdeMeia = qtdeMeiaFinal,
                QtdeNaoPagante = qtdeNaoPaganteFinal,
                ValorTotal = totalAtual
            };

            ReservaStore.ReservaAtual = reserva;

            await DisplayAlert("Sucesso", "Sua reserva será registrada após pagamento! ", "OK");

            await Navigation.PushAsync(new Pagamento());
        }
    }
}
