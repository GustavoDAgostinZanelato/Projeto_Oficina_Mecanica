using Microsoft.EntityFrameworkCore;
using VeiculosApp.Servicos;

namespace AppTemplate
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<Veiculo> Veiculos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Veiculo>().HasKey(p => p.Id);

            modelBuilder.Entity<Veiculo>()
                .HasIndex(p => p.Placa)
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}