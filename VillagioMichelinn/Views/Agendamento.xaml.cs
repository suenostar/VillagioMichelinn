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

        // Ingressos (para modo Passeio)
        private int adulto = 0;
        private int meia = 0;
        private int naoPagante = 0;

        private decimal precoAdulto = 12m;
        private decimal precoMeia = 6m;

        // Cafés
        private decimal precoCafeCaipira = 75m;
        private decimal precoCafeRural = 60m;

        // Combos Agência
        private decimal precoPasseioAgenciaOpcao1 = 12m; // Opção 1 soma com 1 café (caipira ou rural)
        private decimal precoPasseioAgenciaOpcao2 = 15m; // Opção 2 é só passeio

        // Combo Família (já formatado no layout)
        private decimal precoComboFamiliaOpcao1 = 87m; // total pronto
        private decimal precoComboFamiliaOpcao2 = 15m; // só passeio

        // Visibilidade / Regra de agência x família
        private bool isAgenciaLogin = false;      // <- ajuste aqui conforme o login
        private bool agendamentoFamilia = true;   // true = família (bloqueia seg-sex), false = agência

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
            Passeio,                 // Ingressos + (opcional) Café do Grupo 1
            CafeManha,               // Café do Grupo 2 (sem ingressos)
            ComboFamiliaOpcao1,      // total fechado
            ComboFamiliaOpcao2,      // total fechado
            ComboAgenciaOpcao1,      // 1 café (agência) + taxa 12
            ComboAgenciaOpcao2       // só passeio 15
        }

        private SelectedMode modoSelecionado = SelectedMode.None;

        public Agendamento()
        {
            InitializeComponent();

            // 1) Defina aqui com base no login real:
            //    - true  => login é de agência
            //    - false => login é de família/visitante
            isAgenciaLogin = true;               // <-- ajuste isso dinamicamente (ex.: vindo do auth)
            agendamentoFamilia = !isAgenciaLogin;

            // 2) Mostra/oculta o expander da Agência com base no login
            if (FindByName("expAgencia") is View expAgenciaView)
                expAgenciaView.IsVisible = isAgenciaLogin;

            // 3) Agora sim, constrói o calendário e a UI dinâmica
            BuildCalendar(currentMonth);
            AtualizarSafra(currentMonth);

            // 4) Horários só habilitam depois de escolher o dia
            SetHorariosEnabled(false);

            // 5) Total inicial
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

                // Se for agendamento de família: bloquear seg-sex
                if (agendamentoFamilia &&
                    date.DayOfWeek >= DayOfWeek.Monday &&
                    date.DayOfWeek <= DayOfWeek.Friday)
                {
                    btn.BackgroundColor = Colors.Gray;
                    btn.IsEnabled = false;
                }

                // Destaque do dia de HOJE (se habilitado)
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

        // =================== Navegação de mês ===================
        private void OnPreviousMonthClicked(object sender, EventArgs e)
        {
            currentMonth = currentMonth.AddMonths(-1);
            BuildCalendar(currentMonth);
            AtualizarSafra(currentMonth);

            // Ao mudar o mês, zera seleção de dia e horários
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

        // =================== Seleção de dia/horário ===================
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

            // Habilita horários somente após escolher um dia
            SetHorariosEnabled(true);
        }

        private void SetHorariosEnabled(bool enabled)
        {
            if (HorariosFlex is null) return;

            // (opcional) bloquear cliques no contêiner inteiro
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
            // Ao mexer em ingressos, modo vira Passeio
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
            // Não entra no total, mas mantemos a contagem
            naoPagante++;
            NaoPaganteCount!.Text = naoPagante.ToString();
        }

        private void OnNaoPaganteMenos(object sender, EventArgs e)
        {
            if (naoPagante > 0) naoPagante--;
            NaoPaganteCount!.Text = naoPagante.ToString();
        }

        // =================== Café / Combos – handler ÚNICO ===================
        /// <summary>
        /// Único handler para todos CheckBox:
        /// - Grupo 1 (Passeio + Café): CafePasseioCaipiraCheckBox / CafePasseioRuralCheckBox
        /// - Grupo 2 (Café da manhã):  CafeManhaCaipiraCheckBox / CafeManhaRuralCheckBox
        /// - Combo Agência:            CFCPagenciaCheckBox / CFRLagenciaCheckBox / PasseioagenciaCheckBox
        /// - Combo Família:            CombofamiliaCheckBox / PasseiofamiliaCheckBox
        /// Garante exclusividade dentro de cada grupo e exclusividade entre pacotes.
        /// </summary>
        private void OnCafeCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (sender == null) return;

            // ===== Grupo 1: Passeio + Café da manhã =====
            if (sender == CafePasseioCaipiraCheckBox)
            {
                if (e.Value)
                {
                    SetMode(SelectedMode.Passeio);
                    CafePasseioRuralCheckBox.IsChecked = false;
                }
                AtualizarTotal();
                return;
            }
            if (sender == CafePasseioRuralCheckBox)
            {
                if (e.Value)
                {
                    SetMode(SelectedMode.Passeio);
                    CafePasseioCaipiraCheckBox.IsChecked = false;
                }
                AtualizarTotal();
                return;
            }

            // ===== Grupo 2: Café da manhã (avulso) =====
            if (sender == CafeManhaCaipiraCheckBox)
            {
                if (e.Value)
                {
                    SetMode(SelectedMode.CafeManha);
                    CafeManhaRuralCheckBox.IsChecked = false;
                }
                AtualizarTotal();
                return;
            }
            if (sender == CafeManhaRuralCheckBox)
            {
                if (e.Value)
                {
                    SetMode(SelectedMode.CafeManha);
                    CafeManhaCaipiraCheckBox.IsChecked = false;
                }
                AtualizarTotal();
                return;
            }

            // ===== Combo Agência =====
            if (sender == CFCPagenciaCheckBox) // café caipira + taxa 12
            {
                if (e.Value)
                {
                    SetMode(SelectedMode.ComboAgenciaOpcao1);
                    CFRLagenciaCheckBox.IsChecked = false;
                    PasseioagenciaCheckBox.IsChecked = false; // não pode simultâneo com opção 2
                }
                AtualizarTotal();
                return;
            }
            if (sender == CFRLagenciaCheckBox) // café rural + taxa 12
            {
                if (e.Value)
                {
                    SetMode(SelectedMode.ComboAgenciaOpcao1);
                    CFCPagenciaCheckBox.IsChecked = false;
                    PasseioagenciaCheckBox.IsChecked = false;
                }
                AtualizarTotal();
                return;
            }
            if (sender == PasseioagenciaCheckBox) // só passeio 15
            {
                if (e.Value)
                {
                    SetMode(SelectedMode.ComboAgenciaOpcao2);
                    CFCPagenciaCheckBox.IsChecked = false;
                    CFRLagenciaCheckBox.IsChecked = false;
                }
                AtualizarTotal();
                return;
            }

            // ===== Combo Família =====
            if (sender == CombofamiliaCheckBox) // total R$87
            {
                if (e.Value)
                {
                    SetMode(SelectedMode.ComboFamiliaOpcao1);
                    PasseiofamiliaCheckBox.IsChecked = false;
                }
                AtualizarTotal();
                return;
            }
            if (sender == PasseiofamiliaCheckBox) // só passeio R$15
            {
                if (e.Value)
                {
                    SetMode(SelectedMode.ComboFamiliaOpcao2);
                    CombofamiliaCheckBox.IsChecked = false;
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

            // Exclusividade entre pacotes:
            switch (novoModo)
            {
                case SelectedMode.Passeio:
                    expPasseio.IsEnabled = true;
                    expCafe.IsEnabled = false;
                    expFamilia.IsEnabled = false;
                    if (FindByName("expAgencia") is View expAgenciaView)
                        expAgenciaView.IsEnabled = false;

                    ClearCafeGrupo2();
                    ClearCombos();
                    break;

                case SelectedMode.CafeManha:
                    expPasseio.IsEnabled = false;
                    expCafe.IsEnabled = true;
                    expFamilia.IsEnabled = false;
                    if (FindByName("expAgencia") is View expAgenciaView2)
                        expAgenciaView2.IsEnabled = false;

                    ResetIngressos();
                    ClearCafeGrupo1();
                    ClearCombos();
                    break;

                case SelectedMode.ComboFamiliaOpcao1:
                case SelectedMode.ComboFamiliaOpcao2:
                    expPasseio.IsEnabled = false;
                    expCafe.IsEnabled = false;
                    expFamilia.IsEnabled = true;
                    if (FindByName("expAgencia") is View expAgenciaView3)
                        expAgenciaView3.IsEnabled = false;

                    ResetIngressos();
                    ClearCafeGrupo1();
                    ClearCafeGrupo2();
                    ClearComboAgencia();
                    break;

                case SelectedMode.ComboAgenciaOpcao1:
                case SelectedMode.ComboAgenciaOpcao2:
                    expPasseio.IsEnabled = false;
                    expCafe.IsEnabled = false;
                    expFamilia.IsEnabled = false;
                    if (FindByName("expAgencia") is View expAgenciaView4)
                        expAgenciaView4.IsEnabled = true;

                    ResetIngressos();
                    ClearCafeGrupo1();
                    ClearCafeGrupo2();
                    ClearComboFamilia();
                    break;

                case SelectedMode.None:
                default:
                    expPasseio.IsEnabled = true;
                    expCafe.IsEnabled = true;
                    expFamilia.IsEnabled = true;
                    if (FindByName("expAgencia") is View expAgenciaView5)
                        expAgenciaView5.IsEnabled = isAgenciaLogin;
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

        private void ClearCafeGrupo1()
        {
            if (CafePasseioCaipiraCheckBox != null) CafePasseioCaipiraCheckBox.IsChecked = false;
            if (CafePasseioRuralCheckBox != null) CafePasseioRuralCheckBox.IsChecked = false;
        }

        private void ClearCafeGrupo2()
        {
            if (CafeManhaCaipiraCheckBox != null) CafeManhaCaipiraCheckBox.IsChecked = false;
            if (CafeManhaRuralCheckBox != null) CafeManhaRuralCheckBox.IsChecked = false;
        }

        private void ClearComboAgencia()
        {
            if (CFCPagenciaCheckBox != null) CFCPagenciaCheckBox.IsChecked = false;
            if (CFRLagenciaCheckBox != null) CFRLagenciaCheckBox.IsChecked = false;
            if (PasseioagenciaCheckBox != null) PasseioagenciaCheckBox.IsChecked = false;
        }

        private void ClearComboFamilia()
        {
            if (CombofamiliaCheckBox != null) CombofamiliaCheckBox.IsChecked = false;
            if (PasseiofamiliaCheckBox != null) PasseiofamiliaCheckBox.IsChecked = false;
        }

        private void ClearCombos()
        {
            ClearComboAgencia();
            ClearComboFamilia();
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

                    if (CafePasseioCaipiraCheckBox?.IsChecked == true)
                        total += precoCafeCaipira;
                    if (CafePasseioRuralCheckBox?.IsChecked == true)
                        total += precoCafeRural;
                    break;

                case SelectedMode.CafeManha:
                    if (CafeManhaCaipiraCheckBox?.IsChecked == true)
                        total += precoCafeCaipira;
                    if (CafeManhaRuralCheckBox?.IsChecked == true)
                        total += precoCafeRural;
                    break;

                case SelectedMode.ComboFamiliaOpcao1:
                    total = precoComboFamiliaOpcao1; // total pronto
                    break;

                case SelectedMode.ComboFamiliaOpcao2:
                    total = precoComboFamiliaOpcao2; // total pronto
                    break;

                case SelectedMode.ComboAgenciaOpcao1:
                    // 1 café (caipira ou rural) + taxa 12
                    total += precoPasseioAgenciaOpcao1;
                    if (CFCPagenciaCheckBox?.IsChecked == true)
                        total += precoCafeCaipira;
                    else if (CFRLagenciaCheckBox?.IsChecked == true)
                        total += precoCafeRural;
                    break;

                case SelectedMode.ComboAgenciaOpcao2:
                    // só passeio 15
                    total = precoPasseioAgenciaOpcao2;
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
            // 0) Volta para estado neutro
            modoSelecionado = SelectedMode.None;

            // 1) Limpa seleções de pacotes/itens
            ResetIngressos();
            ClearCafeGrupo1();
            ClearCafeGrupo2();
            ClearCombos();

            // 2) Reabilita todos os grupos (respeitando login de agência)
            expPasseio.IsEnabled = true;
            expCafe.IsEnabled = true;
            expFamilia.IsEnabled = true;
            if (FindByName("expAgencia") is View expAgenciaView)
                expAgenciaView.IsEnabled = isAgenciaLogin;

            // 3) (Opcional) Colapsa todos os expansores para limpeza visual
            expPasseio.IsExpanded = false;
            expCafe.IsExpanded = false;
            expFamilia.IsExpanded = false;
            // Se o expander da Agência tiver x:Name="expAgencia" gerado, use:
            // expAgencia.IsExpanded = false;
            // Como fallback, tente via FindByName:
            var expAg = FindByName("expAgencia");
            if (expAg is CommunityToolkit.Maui.Views.Expander ag)
                ag.IsExpanded = false;

            // 4) (Opcional) Também limpar dia/horário — descomente se quiser
            // if (selectedDayButton != null) { selectedDayButton.BackgroundColor = Colors.LightGreen; selectedDayButton = null; }
            // if (selectedHorarioButton != null) { selectedHorarioButton.BackgroundColor = Color.FromArgb("#A4FF88"); selectedHorarioButton = null; }
            // SetHorariosEnabled(selectedDayButton != null);

            // 5) Recalcula total (zerado)
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
                await DisplayAlert("Atenção", "Escolha um pacote: Passeio, Café da manhã, Combo Família ou Combo Agência.", "OK");
                return;
            }

            // Validações por modo
            switch (modoSelecionado)
            {
                case SelectedMode.Passeio:
                    if (adulto + meia <= 0)
                    {
                        await DisplayAlert("Atenção", "Adicione pelo menos 1 ingresso pago (Adulto ou Meia) para o Passeio.", "OK");
                        return;
                    }
                    break;

                case SelectedMode.CafeManha:
                    if (!(CafeManhaCaipiraCheckBox?.IsChecked == true || CafeManhaRuralCheckBox?.IsChecked == true))
                    {
                        await DisplayAlert("Atenção", "Selecione 1 café (Caipira ou Rural) em 'Café da manhã'.", "OK");
                        return;
                    }
                    break;

                case SelectedMode.ComboFamiliaOpcao1:
                case SelectedMode.ComboFamiliaOpcao2:
                    // Já são exclusivos e fechados (ok)
                    break;

                case SelectedMode.ComboAgenciaOpcao1:
                    // precisa exatamente 1 café
                    bool agCafe1 = CFCPagenciaCheckBox?.IsChecked == true;
                    bool agCafe2 = CFRLagenciaCheckBox?.IsChecked == true;
                    if ((agCafe1 ? 1 : 0) + (agCafe2 ? 1 : 0) != 1)
                    {
                        await DisplayAlert("Atenção", "Na Agência Opção 1 selecione exatamente 1 café (Caipira ou Rural).", "OK");
                        return;
                    }
                    break;

                case SelectedMode.ComboAgenciaOpcao2:
                    if (PasseioagenciaCheckBox?.IsChecked != true)
                    {
                        await DisplayAlert("Atenção", "Na Agência Opção 2 selecione o passeio.", "OK");
                        return;
                    }
                    break;
            }

            // Tudo certo -> seguir para pagamento
            await Navigation.PushAsync(new Pagamento());
        }
    }
}

