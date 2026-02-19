using System;

namespace VillagioMichelinn
{
    // Classe estática para manter os preços globais
    public static class PrecosConfig
    {
        // Passeio
        public static decimal PrecoAdultoPasseio { get; set; } = 15m;
        public static decimal PrecoMeiaPasseio { get; set; } = 7.5m;

        // Café da manhã
        public static decimal PrecoCafeAdulto { get; set; } = 70m;

        // Meia do café sempre metade do adulto
        public static decimal PrecoCafeMeia => PrecoCafeAdulto / 2m;
    }
}
