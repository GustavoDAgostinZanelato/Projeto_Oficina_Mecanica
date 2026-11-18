using Microsoft.EntityFrameworkCore;
using PedidoServico.Models;

namespace PedidoServico.Data;

public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
    public DbSet<OrdemServico> OrdensServico => Set<OrdemServico>();
    public DbSet<ItemOrdemServico> ItensOrdemServico => Set<ItemOrdemServico>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var ordem = modelBuilder.Entity<OrdemServico>();
        ordem.ToTable("OrdensServico");
        ordem.HasKey(item => item.Id);
        ordem.Property(item => item.Status).IsRequired().HasMaxLength(30);
        ordem.Property(item => item.Observacoes).HasMaxLength(2000);

        var item = modelBuilder.Entity<ItemOrdemServico>();
        item.ToTable("ItensOrdemServico");
        item.HasKey(item => item.Id);
        item.HasIndex(item => new { item.OrdemServicoId, item.PecaId });
        item.Property(item => item.PrecoUnitarioMomento).IsRequired();

        ordem.HasMany(item => item.Itens)
            .WithOne(item => item.OrdemServico)
            .HasForeignKey(item => item.OrdemServicoId)
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }
}
