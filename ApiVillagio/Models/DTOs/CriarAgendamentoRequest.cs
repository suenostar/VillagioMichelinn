using System.ComponentModel.DataAnnotations;

namespace ApiVillagio.Models.DTOs
{
    public sealed class CriarAgendamentoRequest
    {
      
        [Required]
        [RegularExpression(@"^\d{4}-(0[1-9]|1[0-2])-(0[1-9]|[12]\d|3[01])$")]
        public string Data { get; set; } = default!;

        
        [Required]
        [RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$")]
        public string Horario { get; set; } = default!;

        public int AgenciaId { get; set; }
    }
}
