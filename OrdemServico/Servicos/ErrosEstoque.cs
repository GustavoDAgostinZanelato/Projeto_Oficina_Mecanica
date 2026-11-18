namespace PedidoServico.Servicos;

public sealed class EstoqueInsuficienteException(int solicitada)
    : Exception($"Estoque insuficiente para a quantidade solicitada: {solicitada}.");

public sealed class IntegracaoException(string microsservico, int statusCode)
    : Exception($"Falha na integração com o microsserviço de {microsservico}. Status HTTP: {statusCode}.");

public sealed class RecursoNaoEncontradoException(string microsservico, string caminho)
    : Exception($"Recurso não encontrado no microsserviço de {microsservico}: {caminho}.");
