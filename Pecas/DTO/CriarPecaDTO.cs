using System.ComponentModel.DataAnnotations;

namespace Pecas.DTO;

public class CriarPecaDTO
{
    [Required]
    [StringLength(50)]
    public string Codigo { get; init; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Nome { get; init; } = string.Empty;

    [StringLength(1000)]
    public string? Descricao { get; init; }

    [Range(typeof(decimal), "0.01", "999999999", ParseLimitsInInvariantCulture = true)]
    public decimal PrecoUnitario { get; init; }

    [Range(0, int.MaxValue)]
    public int QuantidadeEstoque { get; init; }
}
