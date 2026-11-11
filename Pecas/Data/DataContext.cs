using Microsoft.EntityFrameworkCore;
using Pecas.Models;

namespace Pecas.Data;

public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
    public DbSet<Peca> Pecas => Set<Peca>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var peca = modelBuilder.Entity<Peca>();

        peca.ToTable("Pecas");
        peca.HasKey(item => item.Id);
        peca.HasIndex(item => item.Codigo).IsUnique();
        peca.Property(item => item.Codigo).IsRequired().HasMaxLength(50);
        peca.Property(item => item.Nome).IsRequired().HasMaxLength(150);
        peca.Property(item => item.Descricao).HasMaxLength(1000);
        peca.Property(item => item.PrecoUnitario).IsRequired();
        peca.Property(item => item.QuantidadeEstoque).IsRequired();

        base.OnModelCreating(modelBuilder);
    }
}
