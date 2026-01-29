namespace ApiVillagio.Models.Entities
{

    public class Pagamento
    {
        public int Id { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataPagamento { get; set; }
    }

}
