using System.Net.Http.Json;

namespace Teste
{
    public partial class EntrarFamilia : ContentPage
    {
        private readonly HttpClient _httpClient;

        public EntrarFamilia()
        {
            InitializeComponent();
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://villagiodb.runasp.net/")
            };
        }

        private async void OnEntrarClicked(object sender, EventArgs e)
        {
            string telefone = TelefoneEntry.Text;
            string senha = SenhaEntry.Text;

            if (string.IsNullOrWhiteSpace(telefone) || string.IsNullOrWhiteSpace(senha))
            {
                await DisplayAlert("Erro", "Preencha todos os campos!", "OK");
                return;
            }

            var loginData = new { Telefone = telefone, Senha = senha };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/familias/login", loginData);

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Sucesso", "Login realizado!", "OK");
                    await Navigation.PushAsync(new Agendamento());
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("Erro", $"Falha no login: {error}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro de conexão: {ex.Message}", "OK");
            }
        }
    }
}