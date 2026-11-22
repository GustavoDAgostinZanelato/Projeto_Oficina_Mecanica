namespace OrdemServicoApp.Servicos;

public sealed class EstoqueInsuficienteException(int solicitada)
    : Exception($"Estoque insuficiente para a quantidade solicitada: {solicitada}.");

public sealed class IntegracaoException : Exception
{
    public IntegracaoException(string microsservico, int statusCode)
        : base($"Falha na integração com o microsserviço de {microsservico}. Status HTTP: {statusCode}.")
    {
    }

    public IntegracaoException(string microsservico, Exception causa)
        : base($"Microsserviço de {microsservico} indisponível ou sem resposta no tempo esperado.", causa)
    {
    }
}

public sealed class RecursoNaoEncontradoException(string microsservico, string caminho)
    : Exception($"Recurso não encontrado no microsserviço de {microsservico}: {caminho}.");

public sealed class OperacaoInvalidaException(string mensagem) : Exception(mensagem);
