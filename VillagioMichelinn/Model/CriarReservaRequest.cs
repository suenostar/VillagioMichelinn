using System.ComponentModel.DataAnnotations;

namespace VillagioMichelinn
{
    public sealed class CriarReservaRequest
    {
        
        [Required]
        [RegularExpression(@"^\d{4}-(0[1-9]|1[0-2])-(0[1-9]|[12]\d|3[01])$")]
        public string Data { get; set; } = default!;

        
        [Required]
        [RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$")]
        public string Horario { get; set; } = default!;

        [Required] public string TipoPacote { get; set; } = default!;
        public int QtdeAdulto { get; set; }
        public int QtdeMeia { get; set; }
        public int QtdeNaoPagante { get; set; }
        public decimal ValorTotal { get; set; }
        public int? AgenciaId { get; set; }
        public int? FamiliaId { get; set; }
    }
}