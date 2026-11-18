namespace Clientes.DTO;

public class ClienteDTO
{
    public int Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string CpfCnpj { get; init; } = string.Empty;
    public string? Telefone { get; init; }
    public string? Email { get; init; }
    public string? Endereco { get; init; }
}
