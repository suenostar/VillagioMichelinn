namespace ApiVillagio.Models.Entities
{
    public class Familia
    {
        public int Id { get; set; }
        public string NomeResponsavel { get; set; } = default!;
        public string Telefone { get; set; } = default!;
        public string Senha { get; set; } = default!;

        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    }
}
