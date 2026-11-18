using System.ComponentModel.DataAnnotations;

namespace PedidoServico.DTO;

public class AtualizarOrdemServicoDTO
{
    [Range(1, int.MaxValue)]
    public int ClienteId { get; init; }

    [Range(1, int.MaxValue)]
    public int VeiculoId { get; init; }

    [Required]
    [StringLength(30)]
    public string Status { get; init; } = string.Empty;

    [StringLength(2000)]
    public string? Observacoes { get; init; }

    [Range(typeof(decimal), "0", "999999999", ParseLimitsInInvariantCulture = true)]
    public decimal? TaxaServico { get; init; }

    public DateTime? DataInicioServico { get; init; }
    public DateTime? DataConclusaoServico { get; init; }
}
