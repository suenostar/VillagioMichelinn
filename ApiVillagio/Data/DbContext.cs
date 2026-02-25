using ApiVillagio.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiVillagio.Data
{
    public class DbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public DbContext(DbContextOptions<DbContext> options) : base(options) { }

        public DbSet<Agencia> Agencias => Set<Agencia>();
        public DbSet<Familia> Familias => Set<Familia>();
        public DbSet<Agendamento> Agendamentos => Set<Agendamento>();
        public DbSet<Pagamento> Pagamentos => Set<Pagamento>();
        public DbSet<Reserva> Reservas => Set<Reserva>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Tabelas
            modelBuilder.Entity<Agencia>().ToTable("Agencias");
            modelBuilder.Entity<Familia>().ToTable("Familias");
            modelBuilder.Entity<Agendamento>().ToTable("Agendamentos");
            modelBuilder.Entity<Pagamento>().ToTable("Pagamentos");
            modelBuilder.Entity<Reserva>().ToTable("Reservas");

            // Agencias
            modelBuilder.Entity<Agencia>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Nome).IsRequired().HasMaxLength(100);
                e.Property(x => x.Email).IsRequired().HasMaxLength(100);
                e.Property(x => x.Telefone).IsRequired().HasMaxLength(20);
                e.Property(x => x.Cnpj).IsRequired().HasMaxLength(20);
                e.HasIndex(x => x.Cnpj).IsUnique();
                e.Property(x => x.Senha).IsRequired().HasMaxLength(200);
            });

            // Familias
            modelBuilder.Entity<Familia>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.NomeResponsavel).IsRequired().HasMaxLength(100);
                e.Property(x => x.Telefone).IsRequired().HasMaxLength(20);
                e.HasIndex(x => x.Telefone).IsUnique();
                e.Property(x => x.Senha).IsRequired().HasMaxLength(200);
            });

            // Agendamentos
            modelBuilder.Entity<Agendamento>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Data)
                    .IsRequired()
                    .HasColumnType("datetime2(0)"); 
                e.HasOne(x => x.Agencia)
                    .WithMany(a => a.Agendamentos)
                    .HasForeignKey(x => x.AgenciaId)
                    .OnDelete(DeleteBehavior.Restrict);

                
                e.HasIndex(x => new { x.AgenciaId, x.Data }).IsUnique();
            });

            // Pagamentos
            modelBuilder.Entity<Pagamento>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Valor)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();
                e.Property(x => x.DataPagamento)
                    .HasColumnType("datetime2(0)")
                    .IsRequired();
            });

            // Reservas
            modelBuilder.Entity<Reserva>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.DataReserva)
                    .HasColumnType("datetime2(0)")
                    .IsRequired();

                e.HasOne(x => x.Familia)
                    .WithMany(f => f.Reservas)
                    .HasForeignKey(x => x.FamiliaId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(x => new { x.FamiliaId, x.DataReserva }).IsUnique();
            });
        }
    }
}
