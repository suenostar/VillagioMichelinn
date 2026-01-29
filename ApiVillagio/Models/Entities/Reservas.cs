namespace ApiVillagio.Models.Entities
{

    public class Reserva
    {
        public int Id { get; set; }
        public int FamiliaId { get; set; }
        public Familia Familia { get; set; }
        public DateTime DataReserva { get; set; }
    }

}
