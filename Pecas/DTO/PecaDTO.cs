namespace Pecas.DTO;

public class PecaDTO
{
    public int Id { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string Nome { get; init; } = string.Empty;
    public string? Descricao { get; init; }
    public decimal PrecoUnitario { get; init; }
    public int QuantidadeEstoque { get; init; }
}
