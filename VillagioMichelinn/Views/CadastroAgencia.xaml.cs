using System.Net.Http.Json;


namespace Teste
{
    public partial class CadastroAgencia : ContentPage
    {
        private readonly HttpClient _httpClient;

        public CadastroAgencia()
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
            string email = EmailEntry.Text;
            string telefone = TelefoneEntry.Text;
            string cnpj = CnpjEntry.Text;
            string senha = SenhaEntry.Text;

            if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(telefone) || string.IsNullOrWhiteSpace(cnpj) ||
                string.IsNullOrWhiteSpace(senha))
            {
                await DisplayAlert("Erro", "Preencha todos os campos!", "OK");
                return;
            }

            var agencia = new
            {
                Nome = nome,
                Email = email,
                Telefone = telefone,
                Cnpj = cnpj,
                Senha = senha
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/Agencias/cadastrar", agencia);

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Sucesso", "Agência cadastrada com sucesso!", "OK");
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
            await Navigation.PushAsync(new EntrarAgencia());
        }
    }
}