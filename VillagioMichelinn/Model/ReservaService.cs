using System.Net.Http.Json;
using System.Threading.Tasks;

namespace VillagioMichelinn.Services
{
    public sealed class ReservaService
    {
        // Ajuste para sua API:
        // - Android emulador + Kestrel local → http://10.0.2.2:5211
        // - Windows com API no mesmo host → http://localhost:5211
        private const string BASE_URL = "http://10.0.2.2:5211";

        private readonly HttpClient _http;

        public ReservaService(HttpClient? http = null)
        {
            _http = http ?? new HttpClient { BaseAddress = new Uri(BASE_URL) };
        }

        public async Task CriarReservaAsync(CriarReservaRequest request)
        {
            var resp = await _http.PostAsJsonAsync("/reservas", request);
            resp.EnsureSuccessStatusCode();
        }
    }
}