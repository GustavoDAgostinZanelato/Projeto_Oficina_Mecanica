using System.ComponentModel.DataAnnotations;

namespace Pecas.DTO;

public class BaixaEstoqueDTO
{
    [Range(1, int.MaxValue)]
    public int Quantidade { get; init; }
}
