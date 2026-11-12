using Clientes.Models;
using Microsoft.EntityFrameworkCore;

namespace Clientes.Data;

public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
    public DbSet<Cliente> Clientes => Set<Cliente>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var cliente = modelBuilder.Entity<Cliente>();

        cliente.ToTable("Clientes");
        cliente.HasKey(item => item.Id);
        cliente.HasIndex(item => item.CpfCnpj).IsUnique();
        cliente.Property(item => item.Nome).IsRequired().HasMaxLength(150);
        cliente.Property(item => item.CpfCnpj).IsRequired().HasMaxLength(14);
        cliente.Property(item => item.Telefone).HasMaxLength(20);
        cliente.Property(item => item.Email).HasMaxLength(150);
        cliente.Property(item => item.Endereco).HasMaxLength(300);

        base.OnModelCreating(modelBuilder);
    }
}
