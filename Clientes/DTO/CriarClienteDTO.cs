using System.ComponentModel.DataAnnotations;

namespace Clientes.DTO;

public class CriarClienteDTO
{
    [Required]
    [StringLength(150)]
    public string Nome { get; init; } = string.Empty;

    // Aceita CPF (11 dígitos) ou CNPJ (14 dígitos), com ou sem pontuação.
    [Required]
    [RegularExpression(
        @"^(\d{11}|\d{14}|\d{3}\.\d{3}\.\d{3}-\d{2}|\d{2}\.\d{3}\.\d{3}/\d{4}-\d{2})$",
        ErrorMessage = "Informe um CPF (11 dígitos) ou CNPJ (14 dígitos) válido em formato.")]
    public string CpfCnpj { get; init; } = string.Empty;

    [StringLength(20)]
    public string? Telefone { get; init; }

    [EmailAddress]
    [StringLength(150)]
    public string? Email { get; init; }

    [StringLength(300)]
    public string? Endereco { get; init; }
}
