using System.ComponentModel.DataAnnotations;

namespace OrdemServicoApp.DTO;

public class AdicionarPecaDTO
{
    [Range(1, int.MaxValue)]
    public int PecaId { get; init; }

    [Range(1, int.MaxValue)]
    public int Quantidade { get; init; } = 1;
}
