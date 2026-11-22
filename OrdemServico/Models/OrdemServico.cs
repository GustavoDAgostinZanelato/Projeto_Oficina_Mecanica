namespace OrdemServicoApp.Models;

public class OrdemServico
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public int VeiculoId { get; set; }
    public DateTime DataCriacaoServico { get; set; }
    public DateTime DataInicioServico { get; set; }
    public DateTime? DataConclusaoServico { get; set; }
    public string Status { get; set; } = "Aberta";
    public decimal? ValorTotal { get; set; }
    public string? Observacoes { get; set; }
    public decimal? TaxaServico { get; set; }
    public decimal? ValorBaseServico { get; set; }
    public ICollection<ItemOrdemServico> Itens { get; set; } = new List<ItemOrdemServico>();
}
