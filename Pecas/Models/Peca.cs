namespace Pecas.Models;

public class Peca
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal PrecoUnitario { get; set; }
    public int QuantidadeEstoque { get; set; }
}
