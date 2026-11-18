namespace Clientes.Servicos;

public sealed class CpfCnpjDuplicadoException(string cpfCnpj)
    : Exception($"Já existe um cliente cadastrado com o CPF/CNPJ '{cpfCnpj}'.");
