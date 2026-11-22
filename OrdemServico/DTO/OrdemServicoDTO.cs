namespace OrdemServicoApp.DTO;

public class OrdemServicoDTO
{
    public int Id { get; init; }
    public int ClienteId { get; init; }
    public int VeiculoId { get; init; }
    public DateTime DataCriacaoServico { get; init; }
    public DateTime DataInicioServico { get; init; }
    public DateTime? DataConclusaoServico { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal? ValorTotal { get; init; }
    public string? Observacoes { init; get; }
    public decimal? TaxaServico { get; init; }
    public decimal? ValorBaseServico { get; init; }
    public List<ItemOrdemServicoDTO> Itens { get; init; } = [];
}

public class ItemOrdemServicoDTO
{
    public int Id { get; init; }
    public int PecaId { get; init; }
    public int Quantidade { get; init; }
    public decimal PrecoUnitarioMomento { get; init; }
}
