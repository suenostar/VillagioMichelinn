using System.Collections.Generic;

namespace VillagioMichelinn
{
    public static class ReservaStore
    {
        // Por enquanto vamos guardar só UMA reserva (a mais recente)
        public static Reserva? ReservaAtual { get; set; }

        // Futuro: você pode trocar por uma lista:
        // public static List<Reserva> Reservas { get; } = new();
    }
}