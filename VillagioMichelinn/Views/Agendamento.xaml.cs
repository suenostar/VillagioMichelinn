using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace VillagioMichelinn
{
    public partial class Agendamento : ContentPage
    {
        // =================== Estado geral ===================
        private DateTime currentMonth = DateTime.Now;
        private Button? selectedDayButton = null;
        private Button? selectedHorarioButton = null;

        // Ingressos (Passeio)
        private int adulto = 0;
        private int meia = 0;
        private int naoPagante = 0;

        private decimal precoAdulto = 15m;
        private decimal precoMeia = 7.5m;

        // Café da manhã (avulso) – único tipo
        private decimal precoCafeManha = 70m;

        // Combo Família (única opção)
        private decimal precoComboFamilia = 82m; // R$70 + R$12

        // Família: calendário bloqueia seg-sex
        private bool agendamentoFamilia = true;

        private static readonly CultureInfo ptBR = new("pt-BR");

        // Safra por mês
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

        // =================== Modo selecionado ===================
        private enum SelectedMode
        {
            None,
            Passeio,        // Ingressos (adulto/meia)
            CafeManha,      // Café avulso (1 tipo)
            ComboFamilia    // Total fechado R$82
        }

        private SelectedMode modoSelecionado = SelectedMode.None;

        public Agendamento()
        {
            InitializeComponent();

            // Família por padrão (bloqueia seg-sex no calendário)
            agendamentoFamilia = true;

            // Monta calendário e UI dinâmica
            BuildCalendar(currentMonth);
            AtualizarSafra(currentMonth);

            // Horários só habilitam após escolher o dia
            SetHorariosEnabled(false);

            // Total inicial
            AtualizarTotal();
        }

        // =================== Calendário (INALTERADO) ===================
        private void BuildCalendar(DateTime month)
        {
            CalendarGrid.Children.Clear();
            MonthLabel.Text = month.ToString("MMMM yyyy", ptBR).ToUpper();

            var firstDay = new DateTime(month.Year, month.Month, 1);
            int daysInMonth = DateTime.DaysInMonth(month.Year, month.Month);
            int startColumn = (int)firstDay.DayOfWeek; // Domingo=0..Sábado=6

            int row = 0;
            int col = startColumn;

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

                // Desabilita dias passados
                if (date.Date < DateTime.Today)
                {
                    btn.BackgroundColor = Colors.LightGray;
                    btn.TextColor = Colors.DarkGray;
                    btn.IsEnabled = false;
                }

                // Família: bloquear seg-sex
                if (agendamentoFamilia &&
                    date.DayOfWeek >= DayOfWeek.Monday &&
                    date.DayOfWeek <= DayOfWeek.Friday)
                {
                    btn.BackgroundColor = Colors.Gray;
                    btn.IsEnabled = false;
                }

                // Destaque do dia atual (se habilitado)
                if (date.Date == DateTime.Today && btn.IsEnabled)
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

            // Zera seleção de dia/horário
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
        // =================== FIM: Calendário (INALTERADO) ===================

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

        // =================== Ingressos: + / - ===================
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
            naoPagante++;
            NaoPaganteCount!.Text = naoPagante.ToString();
        }

        private void OnNaoPaganteMenos(object sender, EventArgs e)
        {
            if (naoPagante > 0) naoPagante--;
            NaoPaganteCount!.Text = naoPagante.ToString();
        }

        // =================== Café / Combo Família ===================
        private void OnCafeCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (sender == null) return;

            // Café da manhã – único checkbox
            if (sender == CafeManhaCheckBox)
            {
                if (e.Value)
                {
                    SetMode(SelectedMode.CafeManha);
                    CombofamiliaCheckBox.IsChecked = false;
                }
                else
                {
                    if (modoSelecionado == SelectedMode.CafeManha)
                        modoSelecionado = SelectedMode.None;
                }
                AtualizarTotal();
                return;
            }

            // Combo Família (única opção)
            if (sender == CombofamiliaCheckBox)
            {
                if (e.Value)
                {
                    SetMode(SelectedMode.ComboFamilia);
                    ResetIngressos();
                    ClearCafeManha();
                }
                else
                {
                    if (modoSelecionado == SelectedMode.ComboFamilia)
                        modoSelecionado = SelectedMode.None;
                }
                AtualizarTotal();
                return;
            }
        }

        // =================== Gerenciamento de modo/limpezas ===================
        private void SetMode(SelectedMode novoModo)
        {
            if (modoSelecionado == novoModo) return;

            modoSelecionado = novoModo;

            switch (novoModo)
            {
                case SelectedMode.Passeio:
                    expPasseio.IsEnabled = true;
                    expCafe.IsEnabled = false;
                    expFamilia.IsEnabled = false;

                    // Limpa outros pacotes
                    ClearComboFamilia();
                    ClearCafeManha();
                    break;

                case SelectedMode.CafeManha:
                    expPasseio.IsEnabled = false;
                    expCafe.IsEnabled = true;
                    expFamilia.IsEnabled = false;

                    ResetIngressos();
                    ClearComboFamilia();
                    break;

                case SelectedMode.ComboFamilia:
                    expPasseio.IsEnabled = false;
                    expCafe.IsEnabled = false;
                    expFamilia.IsEnabled = true;

                    ResetIngressos();
                    ClearCafeManha();
                    break;

                case SelectedMode.None:
                default:
                    expPasseio.IsEnabled = true;
                    expCafe.IsEnabled = true;
                    expFamilia.IsEnabled = true;
                    break;
            }

            AtualizarTotal();
        }

        private void ResetIngressos()
        {
            adulto = 0; meia = 0; naoPagante = 0;
            AdultoCount!.Text = "0";
            MeiaCount!.Text = "0";
            NaoPaganteCount!.Text = "0";
        }

        private void ClearCafeManha()
        {
            if (CafeManhaCheckBox != null) CafeManhaCheckBox.IsChecked = false;
        }

        private void ClearComboFamilia()
        {
            if (CombofamiliaCheckBox != null) CombofamiliaCheckBox.IsChecked = false;
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
                    break;

                case SelectedMode.CafeManha:
                    if (CafeManhaCheckBox?.IsChecked == true)
                        total += precoCafeManha;
                    break;

                case SelectedMode.ComboFamilia:
                    total = precoComboFamilia;
                    break;

                case SelectedMode.None:
                default:
                    total = 0m;
                    break;
            }

            TotalLabel!.Text = total.ToString("C", ptBR);
        }

        // =================== Trocar pacote (reset neutro) ===================
        private void OnTrocarPacoteClicked(object sender, EventArgs e)
        {
            // Volta para estado neutro
            modoSelecionado = SelectedMode.None;

            // Limpa seleções e reabilita grupos
            ResetIngressos();
            ClearCafeManha();
            ClearComboFamilia();

            expPasseio.IsEnabled = true;
            expCafe.IsEnabled = true;
            expFamilia.IsEnabled = true;

            // Colapsa todos
            expPasseio.IsExpanded = false;
            expCafe.IsExpanded = false;
            expFamilia.IsExpanded = false;

            AtualizarTotal();
        }

        // =================== Pagamento ===================
        private async void OnPagarClicked(object sender, EventArgs e)
        {
            // Regras gerais
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
                await DisplayAlert("Atenção", "Escolha um pacote: Passeio, Café da manhã ou Combo Família.", "OK");
                return;
            }

            // Validações por modo
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
                    if (CafeManhaCheckBox?.IsChecked != true)
                    {
                        await DisplayAlert("Atenção", "Marque o Café da manhã.", "OK");
                        return;
                    }
                    break;

                case SelectedMode.ComboFamilia:
                    if (CombofamiliaCheckBox?.IsChecked != true)
                    {
                        await DisplayAlert("Atenção", "Marque o Combo Família.", "OK");
                        return;
                    }
                    break;
            }

            // OK -> segue para pagamento
            await Navigation.PushAsync(new Pagamento());
        }
    }
}