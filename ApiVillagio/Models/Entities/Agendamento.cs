namespace ApiVillagio.Models.Entities
{

    public class Agendamento
    {
        public int Id { get; set; }
        public DateTime Data { get; set; }
        public int AgenciaId { get; set; }
        public Agencia Agencia { get; set; }
    }

}
