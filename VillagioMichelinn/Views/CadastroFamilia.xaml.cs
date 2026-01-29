using System.Net.Http.Json;

namespace VillagioMichelinn
{
    public partial class CadastroFamilia : ContentPage
    {
        private readonly HttpClient _httpClient;

        public CadastroFamilia()
        {
            InitializeComponent();
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://villagiodb.runasp.net/")
            };
        }

        private async void OnCadastrarClicked(object sender, EventArgs e)
        {
            string nome = NomeEntry.Text;
            string telefone = TelefoneEntry.Text;
            string senha = SenhaEntry.Text;

            if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(telefone) || string.IsNullOrWhiteSpace(senha))
            {
                await DisplayAlert("Erro", "Preencha todos os campos!", "OK");
                return;
            }

            var familia = new
            {
                NomeResponsavel = nome,
                Telefone = telefone,
                Senha = senha
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/familias/cadastrar", familia);

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Sucesso", "Família cadastrada com sucesso!", "OK");
                    await Navigation.PushAsync(new Agendamento());
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("Erro", $"Falha ao cadastrar: {error}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro de conexão: {ex.Message}", "OK");
            }
        }

        private async void OnEntrarClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new EntrarFamilia());
        }
    }
}
