using System;

namespace VillagioMichelinn
{
    public class Reserva
    {
        public string NomeCliente { get; set; } = string.Empty;
        public DateTime Data { get; set; }
        public string Horario { get; set; } = string.Empty;
        public string TipoPacote { get; set; } = string.Empty;
        public int QtdeAdulto { get; set; }
        public int QtdeMeia { get; set; }
        public int QtdeNaoPagante { get; set; }
        public decimal ValorTotal { get; set; }

        public string ResumoDataHorario =>
            $"{Data:dd/MM/yyyy} - {Horario}";

        public string ResumoValor =>
            ValorTotal.ToString("C", Agendamento.ptBR);
    }
}
