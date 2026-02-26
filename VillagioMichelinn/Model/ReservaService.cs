using System.Net.Http.Json;
using VillagioMichelinn.Models;

namespace VillagioMichelinn.Services
{
    public class ReservaApiService
	{
		private readonly HttpClient _http;
		private const string BaseUrl = "https://villagiodb.runasp.net";

		public ReservaApiService()
		{
			_http = new HttpClient
			{
				BaseAddress = new Uri(BaseUrl)
			};
		}

		public async Task<(bool Sucesso, string Mensagem)> CriarReservaAsync(CriarReservaRequest req)
		{
			try
			{
				var response = await _http.PostAsJsonAsync("/api/Reservas", req);

				if (response.IsSuccessStatusCode)
				{
					return (true, "Reserva criada com sucesso.");
				}

				var erro = await response.Content.ReadAsStringAsync();
				return (false, $"Erro da API: {response.StatusCode} - {erro}");
			}
			catch (Exception ex)
			{
				return (false, $"Falha ao conectar na API: {ex.Message}");
			}
		}
	}
}