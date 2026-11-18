using System.ComponentModel.DataAnnotations;

namespace PedidoServico.DTO;

public class CriarOrdemServicoDTO
{
    [Range(1, int.MaxValue)]
    public int ClienteId { get; init; }

    [Range(1, int.MaxValue)]
    public int VeiculoId { get; init; }

    public DateTime? DataCriacaoServico { get; init; }

    public DateTime? DataInicioServico { get; init; }

    [StringLength(2000)]
    public string? Observacoes { get; init; }

    [Range(typeof(decimal), "0", "999999999", ParseLimitsInInvariantCulture = true)]
    public decimal? TaxaServico { get; init; }
}
