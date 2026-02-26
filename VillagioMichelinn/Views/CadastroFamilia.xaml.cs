using System;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage; 

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
				BaseAddress = new Uri("https://villagiodb.runasp.net/") 
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
					var dados = await response.Content.ReadFromJsonAsync<CadastrarFamiliaResponse>();

					if (dados != null)
					{
						Preferences.Set("FamiliaId", dados.Id);
						Preferences.Set("FamiliaNome", nome);
					}

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

		private sealed class CadastrarFamiliaResponse
		{
			public string Message { get; set; } = string.Empty;
			public int Id { get; set; }
		}
	}
}