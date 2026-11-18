using System.ComponentModel.DataAnnotations;

namespace PedidoServico.DTO;

public class AdicionarPecaDTO
{
    [Range(1, int.MaxValue)]
    public int PecaId { get; init; }

    [Range(1, int.MaxValue)]
    public int Quantidade { get; init; } = 1;
}
