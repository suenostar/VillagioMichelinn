namespace ApiVillagio.Models.Entities
{
    public class Agencia
    {
        public int Id { get; set; }
        public string Nome { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Telefone { get; set; } = default!;
        public string Cnpj { get; set; } = default!; 
        public string Senha { get; set; } = default!;

        public ICollection<Agendamento> Agendamentos { get; set; } = new List<Agendamento>();
    }
}