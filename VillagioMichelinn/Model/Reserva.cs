using System;

namespace VillagioMichelinn
{
    public sealed class Reserva
    {
        public string NomeCliente { get; set; } = string.Empty;

        public DateTime Data { get; set; }            
        public string Horario { get; set; } = "";     
        public DateTime DataHoraLocal { get; set; }
        public string TipoPacote { get; set; } = "";
        public int QtdeAdulto { get; set; }
        public int QtdeMeia { get; set; }
        public int QtdeNaoPagante { get; set; }

        public decimal ValorTotal { get; set; }
    }
}