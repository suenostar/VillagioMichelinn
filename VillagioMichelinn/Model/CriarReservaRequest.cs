namespace VillagioMichelinn.Models
{
	public sealed class CriarReservaRequest
	{
		public int FamiliaId { get; set; }
		public string Data { get; set; } = string.Empty;     
		public string Horario { get; set; } = string.Empty;  
	}
}