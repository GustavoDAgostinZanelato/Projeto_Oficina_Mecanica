namespace OrdemServicoApp.Models;

public class ItemOrdemServico
{
    public int Id { get; set; }
    public int OrdemServicoId { get; set; }
    public int PecaId { get; set; }
    public int Quantidade { get; set; }
    public decimal PrecoUnitarioMomento { get; set; }
    public OrdemServico? OrdemServico { get; set; }
}
