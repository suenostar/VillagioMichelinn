using System;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;   // <-- IMPORTANTE: Preferences

namespace VillagioMichelinn
{
	public partial class EntrarFamilia : ContentPage
	{
		private readonly HttpClient _httpClient;

		public EntrarFamilia()
		{
			InitializeComponent();
			_httpClient = new HttpClient
			{
				BaseAddress = new Uri("https://villagiodb.runasp.net/")
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

			var loginData = new
			{
				Telefone = telefone,
				Senha = senha
			};

			try
			{
				var response = await _httpClient.PostAsJsonAsync("api/familias/login", loginData);

				if (response.IsSuccessStatusCode)
				{
					var dadosLogin = await response.Content.ReadFromJsonAsync<FamiliaLoginResponse>();

					if (dadosLogin == null)
					{
						await DisplayAlert("Erro", "Resposta inválida do servidor.", "OK");
						return;
					}

					Preferences.Set("FamiliaId", dadosLogin.Id);
					Preferences.Set("FamiliaNome", dadosLogin.NomeResponsavel ?? string.Empty);

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

		private async void OnTesteClicked(object sender, EventArgs e)
		{
			await Navigation.PushAsync(new Agendamento());
		}

		private async void OnTestesClicked(object sender, EventArgs e)
		{
			await Navigation.PushAsync(new Inicio());
		}

		private sealed class FamiliaLoginResponse
		{
			public string Message { get; set; } = string.Empty;
			public int Id { get; set; }
			public string? NomeResponsavel { get; set; }
		}
	}
}
