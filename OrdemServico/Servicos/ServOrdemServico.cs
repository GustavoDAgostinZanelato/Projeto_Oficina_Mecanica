using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OrdemServicoApp.Data;
using OrdemServicoApp.DTO;
using OrdemServicoApp.Models;

namespace OrdemServicoApp.Servicos;

public class ServOrdemServico
{
    private readonly DataContext _dataContext;
    private readonly IHttpClientFactory _httpClientFactory;

    public ServOrdemServico(DataContext dataContext, IHttpClientFactory httpClientFactory)
    {
        _dataContext = dataContext;
        _httpClientFactory = httpClientFactory;
    }

    public Task<List<OrdemServico>> ListarAsync(CancellationToken cancellationToken) =>
        _dataContext.OrdensServico
            .Include(ordem => ordem.Itens)
            .OrderByDescending(ordem => ordem.DataCriacaoServico)
            .ToListAsync(cancellationToken);

    public Task<OrdemServico?> ObterAsync(int id, CancellationToken cancellationToken) =>
        _dataContext.OrdensServico
            .Include(ordem => ordem.Itens)
            .FirstOrDefaultAsync(ordem => ordem.Id == id, cancellationToken);

    public async Task<OrdemServico> CriarAsync(CriarOrdemServicoDTO dados, CancellationToken cancellationToken)
    {
        await ValidarExistenciaAsync("Clientes", $"/api/clientes/{dados.ClienteId}", cancellationToken);
        await ValidarExistenciaAsync("Veiculos", $"/api/veiculos/{dados.VeiculoId}", cancellationToken);

        var agora = DateTime.UtcNow;
        var ordem = new OrdemServico
        {
            ClienteId = dados.ClienteId,
            VeiculoId = dados.VeiculoId,
            DataCriacaoServico = dados.DataCriacaoServico ?? agora,
            DataInicioServico = dados.DataInicioServico ?? agora,
            Status = "Aberta",
            Observacoes = Normalizar(dados.Observacoes),
            TaxaServico = dados.TaxaServico ?? 0,
            ValorBaseServico = 0,
            ValorTotal = dados.TaxaServico ?? 0
        };

        _dataContext.OrdensServico.Add(ordem);
        await _dataContext.SaveChangesAsync(cancellationToken);
        return ordem;
    }

    public async Task<OrdemServico?> AtualizarAsync(int id, AtualizarOrdemServicoDTO dados, CancellationToken cancellationToken)
    {
        var ordem = await ObterAsync(id, cancellationToken);
        if (ordem is null)
        {
            return null;
        }

        ValidarStatus(dados.Status);
        await ValidarExistenciaAsync("Clientes", $"/api/clientes/{dados.ClienteId}", cancellationToken);
        await ValidarExistenciaAsync("Veiculos", $"/api/veiculos/{dados.VeiculoId}", cancellationToken);

        ordem.ClienteId = dados.ClienteId;
        ordem.VeiculoId = dados.VeiculoId;
        ordem.Status = dados.Status.Trim();
        ordem.Observacoes = Normalizar(dados.Observacoes);
        ordem.TaxaServico = dados.TaxaServico ?? ordem.TaxaServico ?? 0;
        ordem.DataInicioServico = dados.DataInicioServico ?? ordem.DataInicioServico;
        ordem.DataConclusaoServico = dados.DataConclusaoServico;
        Recalcular(ordem);

        await _dataContext.SaveChangesAsync(cancellationToken);
        return ordem;
    }

    public async Task<bool> RemoverAsync(int id, CancellationToken cancellationToken)
    {
        var ordem = await _dataContext.OrdensServico
            .Include(item => item.Itens)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (ordem is null)
        {
            return false;
        }

        _dataContext.OrdensServico.Remove(ordem);
        await _dataContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<OrdemServico> AdicionarPecaAsync(int ordemId, AdicionarPecaDTO dados, CancellationToken cancellationToken)
    {
        var ordem = await ObterAsync(ordemId, cancellationToken);
        if (ordem is null)
        {
            throw new KeyNotFoundException($"Ordem de serviço com ID {ordemId} não encontrada.");
        }

        if (StatusFinalizado(ordem.Status))
        {
            throw new OperacaoInvalidaException(
                $"Não é possível adicionar peças a uma ordem de serviço com status '{ordem.Status}'.");
        }

        var pecaClient = _httpClientFactory.CreateClient("Pecas");
        using var consulta = await EnviarAsync("Peças",
            () => pecaClient.GetAsync($"/api/pecas/{dados.PecaId}", cancellationToken));
        if (consulta.StatusCode == HttpStatusCode.NotFound)
        {
            throw new KeyNotFoundException($"Peça com ID {dados.PecaId} não encontrada.");
        }
        if (!consulta.IsSuccessStatusCode)
        {
            throw new IntegracaoException("Peças", (int)consulta.StatusCode);
        }

        await using var content = await consulta.Content.ReadAsStreamAsync(cancellationToken);
        using var pecaJson = await JsonDocument.ParseAsync(content, cancellationToken: cancellationToken);
        var root = pecaJson.RootElement;
        var preco = root.GetProperty("precoUnitario").GetDecimal();

        var baixa = new StringContent(
            JsonSerializer.Serialize(new { quantidade = dados.Quantidade }),
            Encoding.UTF8,
            "application/json");
        using var respostaBaixa = await EnviarAsync("Peças",
            () => pecaClient.PostAsync($"/api/pecas/{dados.PecaId}/baixas", baixa, cancellationToken));

        if (respostaBaixa.StatusCode == HttpStatusCode.NotFound)
        {
            throw new KeyNotFoundException($"Peça com ID {dados.PecaId} não encontrada.");
        }
        if (respostaBaixa.StatusCode == HttpStatusCode.Conflict)
        {
            throw new EstoqueInsuficienteException(dados.Quantidade);
        }
        if (!respostaBaixa.IsSuccessStatusCode)
        {
            throw new IntegracaoException("Peças", (int)respostaBaixa.StatusCode);
        }

        var item = ordem.Itens.FirstOrDefault(item => item.PecaId == dados.PecaId);
        if (item is null)
        {
            item = new ItemOrdemServico
            {
                OrdemServicoId = ordem.Id,
                PecaId = dados.PecaId,
                Quantidade = dados.Quantidade,
                PrecoUnitarioMomento = preco
            };
            ordem.Itens.Add(item);
        }
        else
        {
            item.Quantidade += dados.Quantidade;
        }

        Recalcular(ordem);
        await _dataContext.SaveChangesAsync(cancellationToken);
        return ordem;
    }

    private async Task ValidarExistenciaAsync(string clientName, string path, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient(clientName);
        using var response = await EnviarAsync(clientName, () => client.GetAsync(path, cancellationToken));
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new RecursoNaoEncontradoException(clientName, path);
        }
        if (!response.IsSuccessStatusCode)
        {
            throw new IntegracaoException(clientName, (int)response.StatusCode);
        }
    }

    private static async Task<HttpResponseMessage> EnviarAsync(string microsservico, Func<Task<HttpResponseMessage>> requisicao)
    {
        try
        {
            return await requisicao();
        }
        catch (HttpRequestException exception)
        {
            throw new IntegracaoException(microsservico, exception);
        }
        catch (TaskCanceledException exception) when (!exception.CancellationToken.IsCancellationRequested)
        {
            // Timeout do HttpClient (e não cancelamento pelo chamador).
            throw new IntegracaoException(microsservico, exception);
        }
    }

    private static bool StatusFinalizado(string status) =>
        status.Trim().ToLowerInvariant() is "concluída" or "concluida" or "concluido" or "cancelada";

    private static void Recalcular(OrdemServico ordem)
    {
        ordem.ValorBaseServico = ordem.Itens.Sum(item => item.PrecoUnitarioMomento * item.Quantidade);
        ordem.ValorTotal = (ordem.TaxaServico ?? 0) + (ordem.ValorBaseServico ?? 0);
    }

    private static string? Normalizar(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void ValidarStatus(string status)
    {
        var normalized = status.Trim();
        if (!new[] { "Aberta", "Em Andamento", "Concluída", "Concluido", "Cancelada" }
            .Contains(normalized, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Status inválido. Use: Aberta, Em Andamento, Concluída ou Cancelada.");
        }
    }
}
