using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using VillagioMichelinn.Services;

namespace VillagioMichelinn
{
    public partial class Agendamento : ContentPage
    {
        private DateTime currentMonth = DateTime.Now;
        private Button? selectedDayButton = null;
        private Button? selectedHorarioButton = null;

        // =================== Quantidades Passeio ===================
        private int adulto = 0;
        private int meia = 0;
        private int naoPagante = 0;

        // Preços do Passeio (vindo do Painel Admin via PrecosConfig)
        private decimal precoAdulto => PrecosConfig.PrecoAdultoPasseio;
        private decimal precoMeia => PrecosConfig.PrecoMeiaPasseio;

        // =================== Quantidades Café da Manhã ===================
        private int cafeAdulto = 0;
        private int cafeMeia = 0;
        private int cafeNaoPagante = 0;

        // Preços do Café da manhã (vindo do Painel Admin)
        private decimal precoCafeManha => PrecosConfig.PrecoCafeAdulto; // valor cheio
        private decimal precoCafeManhaMeia => PrecosConfig.PrecoCafeMeia;   // metade do valor

        // =================== Combo Família ===================
        private int comboAdulto = 0;
        private int comboMeia = 0;
        private int comboNaoPagante = 0;

        private decimal precoComboFamilia = 82m;

        // =================== Outros ===================
        private bool agendamentoFamilia = true;
        public static readonly CultureInfo ptBR = new("pt-BR");
        private decimal totalAtual = 0m;
        private const int AntecedenciaMinimaFamilia = 3;

        // ======== VALIDADORES (ISO DATE e HH:MM) ========
        private static readonly Regex RxIsoDate = new(@"^\d{4}-(0[1-9]|1[0-2])-(0[1-9]|[12]\d|3[01])$");
        private static readonly Regex RxTime24 = new(@"^([01]\d|2[0-3]):[0-5]\d$");
        private static bool IsValidIsoDate(string s) => !string.IsNullOrWhiteSpace(s) && RxIsoDate.IsMatch(s);
        private static bool IsValidTime24(string s) => !string.IsNullOrWhiteSpace(s) && RxTime24.IsMatch(s);

        private static bool TryCombineLocalDateTime(string isoDate, string time24, out DateTime localDateTime)
        {
            localDateTime = default;
            if (!IsValidIsoDate(isoDate) || !IsValidTime24(time24)) return false;

            if (DateTime.TryParseExact($"{isoDate} {time24}", "yyyy-MM-dd HH:mm",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            {
                // dt terá Kind = Unspecified -> trate como horário local do dispositivo
                localDateTime = dt;
                return true;
            }
            return false;
        }

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

        private enum SelectedMode
        {
            None,
            Passeio,
            CafeManha,
            ComboFamilia
        }

        private SelectedMode modoSelecionado = SelectedMode.None;

        public Agendamento()
        {
            InitializeComponent();
            agendamentoFamilia = true;
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
            int startColumn = (int)firstDay.DayOfWeek; // Domingo=0..Sábado=6

            int row = 0;
            int col = startColumn;

            var hoje = DateTime.Today;

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

                if (date.Date < hoje)
                {
                    btn.BackgroundColor = Colors.LightGray;
                    btn.TextColor = Colors.DarkGray;
                    btn.IsEnabled = false;
                }

                if (agendamentoFamilia &&
                    date.DayOfWeek >= DayOfWeek.Monday &&
                    date.DayOfWeek <= DayOfWeek.Friday)
                {
                    btn.BackgroundColor = Colors.Gray;
                    btn.IsEnabled = false;
                }

                if (agendamentoFamilia &&
                    (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) &&
                    btn.IsEnabled)
                {
                    var diferencaDias = (date.Date - hoje).TotalDays;

                    if (diferencaDias < AntecedenciaMinimaFamilia)
                    {
                        btn.BackgroundColor = Colors.LightGray;
                        btn.TextColor = Colors.DarkGray;
                        btn.IsEnabled = false;
                    }
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

                int dia = int.Parse(selectedDayButton.Text!);
                var dataSelecionada = new DateTime(currentMonth.Year, currentMonth.Month, dia);
                var dataIso = dataSelecionada.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

                _ = DisplayAlert("Dia Selecionado",
                    $"Você escolheu {dataIso}",
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

        // =================== Ingressos Passeio: + / - ===================
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

        private void OnComboAdultoMais(object sender, EventArgs e)
        {
            SetMode(SelectedMode.ComboFamilia);
            comboAdulto++;
            ComboAdultoCount!.Text = comboAdulto.ToString();
            AtualizarTotal();
        }

        private void OnComboAdultoMenos(object sender, EventArgs e)
        {
            SetMode(SelectedMode.ComboFamilia);
            if (comboAdulto > 0) comboAdulto--;
            ComboAdultoCount!.Text = comboAdulto.ToString();
            AtualizarTotal();
        }

        private void OnComboMeiaMais(object sender, EventArgs e)
        {
            SetMode(SelectedMode.ComboFamilia);
            comboMeia++;
            ComboMeiaCount!.Text = comboMeia.ToString();
            AtualizarTotal();
        }

        private void OnComboMeiaMenos(object sender, EventArgs e)
        {
            SetMode(SelectedMode.ComboFamilia);
            if (comboMeia > 0) comboMeia--;
            ComboMeiaCount!.Text = comboMeia.ToString();
            AtualizarTotal();
        }

        private void OnComboNaoPaganteMais(object sender, EventArgs e)
        {
            SetMode(SelectedMode.ComboFamilia);
            comboNaoPagante++;
            ComboNaoPaganteCount!.Text = comboNaoPagante.ToString();
        }

        private void OnComboNaoPaganteMenos(object sender, EventArgs e)
        {
            SetMode(SelectedMode.ComboFamilia);
            if (comboNaoPagante > 0) comboNaoPagante--;
            ComboNaoPaganteCount!.Text = comboNaoPagante.ToString();
        }

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

                    ClearCafeManha();
                    ClearComboFamilia();
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
            cafeAdulto = 0;
            cafeMeia = 0;
            cafeNaoPagante = 0;

            if (CafeAdultoCount != null) CafeAdultoCount.Text = "0";
            if (CafeMeiaCount != null) CafeMeiaCount.Text = "0";
            if (CafeNaoPaganteCount != null) CafeNaoPaganteCount.Text = "0";
        }

        private void ClearComboFamilia()
        {
            comboAdulto = 0;
            comboMeia = 0;
            comboNaoPagante = 0;

            if (ComboAdultoCount != null) ComboAdultoCount.Text = "0";
            if (ComboMeiaCount != null) ComboMeiaCount.Text = "0";
            if (ComboNaoPaganteCount != null) ComboNaoPaganteCount.Text = "0";
        }

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
                    decimal precoCafeMeiaCalc = precoCafeManha / 2m;
                    total += cafeAdulto * precoCafeManha;
                    total += cafeMeia * precoCafeMeiaCalc;
                    break;

                case SelectedMode.ComboFamilia:
                    decimal precoComboMeia = precoComboFamilia / 2m;
                    total += comboAdulto * precoComboFamilia;
                    total += comboMeia * precoComboMeia;
                    break;

                case SelectedMode.None:
                default:
                    total = 0m;
                    break;
            }

            totalAtual = total;
            TotalLabel!.Text = total.ToString("C", ptBR);
        }

        private void OnTrocarPacoteClicked(object sender, EventArgs e)
        {
            modoSelecionado = SelectedMode.None;
            ResetIngressos();
            ClearCafeManha();
            ClearComboFamilia();
            expPasseio.IsEnabled = true;
            expCafe.IsEnabled = true;
            expFamilia.IsEnabled = true;
            expPasseio.IsExpanded = false;
            expCafe.IsExpanded = false;
            expFamilia.IsExpanded = false;
            AtualizarTotal();
        }

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
                await DisplayAlert("Atenção", "Escolha um pacote: Passeio, Café da manhã ou Combo Família.", "OK");
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

                case SelectedMode.ComboFamilia:
                    if (comboAdulto + comboMeia <= 0)
                    {
                        await DisplayAlert("Atenção", "Adicione pelo menos 1 Combo pago (Adulto ou Meia).", "OK");
                        return;
                    }
                    break;
            }

            // ======= Data e Hora no padrão exigido =======
            int dia = int.Parse(selectedDayButton.Text!);
            var dataSelecionada = new DateTime(currentMonth.Year, currentMonth.Month, dia);
            string dataIso = dataSelecionada.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            string horarioSelecionado = selectedHorarioButton.Text ?? "";
            string horario24 = horarioSelecionado.Trim(); // já vem como "HH:MM"

            // Validações reforçadas
            if (!IsValidIsoDate(dataIso))
            {
                await DisplayAlert("Formato inválido", "Data deve ser AAAA-MM-DD (ISO 8601).", "OK");
                return;
            }
            if (!IsValidTime24(horario24))
            {
                await DisplayAlert("Formato inválido", "Horário deve ser HH:MM (24h).", "OK");
                return;
            }

            if (!TryCombineLocalDateTime(dataIso, horario24, out var dataHoraLocal))
            {
                await DisplayAlert("Erro", "Não foi possível combinar data e horário.", "OK");
                return;
            }

            string tipoPacote = modoSelecionado switch
            {
                SelectedMode.Passeio => "Passeio",
                SelectedMode.CafeManha => "Café da manhã",
                SelectedMode.ComboFamilia => "Combo Família",
                _ => "Indefinido"
            };

            int qtdAdulto = 0, qtdMeia = 0, qtdNaoPagante = 0;

            switch (modoSelecionado)
            {
                case SelectedMode.Passeio:
                    qtdAdulto = adulto;
                    qtdMeia = meia;
                    qtdNaoPagante = naoPagante;
                    break;

                case SelectedMode.CafeManha:
                    qtdAdulto = cafeAdulto;
                    qtdMeia = cafeMeia;
                    qtdNaoPagante = cafeNaoPagante;
                    break;

                case SelectedMode.ComboFamilia:
                    qtdAdulto = comboAdulto;
                    qtdMeia = comboMeia;
                    qtdNaoPagante = comboNaoPagante;
                    break;
            }

            // Resumo para o usuário confirmar
            var resumo = $"Data: {dataIso}\nHorário: {horario24}\nPacote: {tipoPacote}\n" +
                         $"Pessoas (Adulto/Meia/NP): {qtdAdulto}/{qtdMeia}/{qtdNaoPagante}\n" +
                         $"Total: {totalAtual.ToString("C", ptBR)}";

            bool confirmar = await DisplayAlert("Confirme seus dados", resumo, "Confirmar", "Voltar");
            if (!confirmar) return;

            var reserva = new Reserva
            {
                NomeCliente = "Cliente",
                Data = dataSelecionada,
                Horario = horario24,
                TipoPacote = tipoPacote,
                QtdeAdulto = qtdAdulto,
                QtdeMeia = qtdMeia,
                QtdeNaoPagante = qtdNaoPagante,
                ValorTotal = totalAtual,
                DataHoraLocal = dataHoraLocal
            };

            ReservaStore.ReservaAtual = reserva;

            

            await DisplayAlert("Sucesso", "Sua reserva será registrada após pagamento! ", "OK");
            await Navigation.PushAsync(new Pagamento());
        }
    }
}