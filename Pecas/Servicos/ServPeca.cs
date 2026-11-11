using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Pecas.Data;
using Pecas.DTO;
using Pecas.Models;

namespace Pecas.Servicos;

public class ServPeca
{
    private readonly DataContext _dataContext;

    public ServPeca(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public Task<List<Peca>> ListarAsync(CancellationToken cancellationToken) =>
        _dataContext.Pecas
            .OrderBy(peca => peca.Nome)
            .ToListAsync(cancellationToken);

    public Task<Peca?> ObterAsync(int id, CancellationToken cancellationToken) =>
        _dataContext.Pecas.FirstOrDefaultAsync(peca => peca.Id == id, cancellationToken);

    public async Task<Peca> CriarAsync(CriarPecaDTO dados, CancellationToken cancellationToken)
    {
        var peca = new Peca
        {
            Codigo = dados.Codigo.Trim(),
            Nome = dados.Nome.Trim(),
            Descricao = NormalizarDescricao(dados.Descricao),
            PrecoUnitario = dados.PrecoUnitario,
            QuantidadeEstoque = dados.QuantidadeEstoque
        };

        _dataContext.Pecas.Add(peca);
        try
        {
            await _dataContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (EhCodigoDuplicado(exception))
        {
            throw new CodigoPecaDuplicadoException(peca.Codigo);
        }

        return peca;
    }

    public async Task<Peca?> AtualizarAsync(
        int id,
        AtualizarPecaDTO dados,
        CancellationToken cancellationToken)
    {
        var peca = await _dataContext.Pecas
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (peca is null)
        {
            return null;
        }

        peca.Codigo = dados.Codigo.Trim();
        peca.Nome = dados.Nome.Trim();
        peca.Descricao = NormalizarDescricao(dados.Descricao);
        peca.PrecoUnitario = dados.PrecoUnitario;
        peca.QuantidadeEstoque = dados.QuantidadeEstoque;

        try
        {
            await _dataContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (EhCodigoDuplicado(exception))
        {
            throw new CodigoPecaDuplicadoException(peca.Codigo);
        }

        return peca;
    }

    public async Task<bool> RemoverAsync(int id, CancellationToken cancellationToken)
    {
        var peca = await _dataContext.Pecas
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (peca is null)
        {
            return false;
        }

        _dataContext.Pecas.Remove(peca);
        await _dataContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<Peca> BaixarEstoqueAsync(
        int id,
        int quantidade,
        CancellationToken cancellationToken)
    {
        var pecasAtualizadas = await _dataContext.Pecas
            .Where(peca => peca.Id == id && peca.QuantidadeEstoque >= quantidade)
            .ExecuteUpdateAsync(
                atualizacao => atualizacao.SetProperty(
                    peca => peca.QuantidadeEstoque,
                    peca => peca.QuantidadeEstoque - quantidade),
                cancellationToken);

        if (pecasAtualizadas == 0)
        {
            var pecaExistente = await _dataContext.Pecas
                .AsNoTracking()
                .FirstOrDefaultAsync(peca => peca.Id == id, cancellationToken);

            if (pecaExistente is null)
            {
                throw new KeyNotFoundException($"Peça com ID {id} não encontrada.");
            }

            throw new EstoqueInsuficienteException(quantidade, pecaExistente.QuantidadeEstoque);
        }

        return await _dataContext.Pecas
            .AsNoTracking()
            .FirstAsync(peca => peca.Id == id, cancellationToken);
    }

    private static string? NormalizarDescricao(string? descricao) =>
        string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim();

    private static bool EhCodigoDuplicado(DbUpdateException exception) =>
        exception.InnerException is SqliteException
        {
            SqliteErrorCode: 19,
            SqliteExtendedErrorCode: 2067
        };
}
