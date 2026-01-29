using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using ApiVillagio.Models.Entities;

namespace ApiVillagio.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Agencia> Agencias { get; set; }
        public DbSet<Familia> Familias { get; set; }
        public DbSet<Agendamento> Agendamentos { get; set; }
        public DbSet<Pagamento> Pagamentos { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
    }
}