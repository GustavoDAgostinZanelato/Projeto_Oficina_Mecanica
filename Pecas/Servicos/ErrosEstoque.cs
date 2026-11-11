namespace Pecas.Servicos;

public sealed class CodigoPecaDuplicadoException(string codigo)
    : Exception($"Já existe uma peça cadastrada com o código '{codigo}'.");

public sealed class EstoqueInsuficienteException(int solicitada, int disponivel)
    : Exception($"Estoque insuficiente. Solicitado: {solicitada}; disponível: {disponivel}.");
