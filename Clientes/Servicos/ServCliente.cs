using Clientes.Data;
using Clientes.DTO;
using Clientes.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Clientes.Servicos;

public class ServCliente
{
    private readonly DataContext _dataContext;

    public ServCliente(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public Task<List<Cliente>> ListarAsync(CancellationToken cancellationToken) =>
        _dataContext.Clientes
            .OrderBy(cliente => cliente.Nome)
            .ToListAsync(cancellationToken);

    public Task<Cliente?> ObterAsync(int id, CancellationToken cancellationToken) =>
        _dataContext.Clientes.FirstOrDefaultAsync(cliente => cliente.Id == id, cancellationToken);

    public async Task<Cliente> CriarAsync(CriarClienteDTO dados, CancellationToken cancellationToken)
    {
        var cliente = new Cliente
        {
            Nome = dados.Nome.Trim(),
            CpfCnpj = SomenteDigitos(dados.CpfCnpj),
            Telefone = Normalizar(dados.Telefone),
            Email = Normalizar(dados.Email),
            Endereco = Normalizar(dados.Endereco)
        };

        _dataContext.Clientes.Add(cliente);
        try
        {
            await _dataContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (EhCpfCnpjDuplicado(exception))
        {
            throw new CpfCnpjDuplicadoException(cliente.CpfCnpj);
        }

        return cliente;
    }

    public async Task<Cliente?> AtualizarAsync(
        int id,
        AtualizarClienteDTO dados,
        CancellationToken cancellationToken)
    {
        var cliente = await _dataContext.Clientes
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (cliente is null)
        {
            return null;
        }

        cliente.Nome = dados.Nome.Trim();
        cliente.CpfCnpj = SomenteDigitos(dados.CpfCnpj);
        cliente.Telefone = Normalizar(dados.Telefone);
        cliente.Email = Normalizar(dados.Email);
        cliente.Endereco = Normalizar(dados.Endereco);

        try
        {
            await _dataContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (EhCpfCnpjDuplicado(exception))
        {
            throw new CpfCnpjDuplicadoException(cliente.CpfCnpj);
        }

        return cliente;
    }

    public async Task<bool> RemoverAsync(int id, CancellationToken cancellationToken)
    {
        var cliente = await _dataContext.Clientes
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (cliente is null)
        {
            return false;
        }

        _dataContext.Clientes.Remove(cliente);
        await _dataContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static string SomenteDigitos(string valor) =>
        new(valor.Where(char.IsDigit).ToArray());

    private static string? Normalizar(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    private static bool EhCpfCnpjDuplicado(DbUpdateException exception) =>
        exception.InnerException is SqliteException
        {
            SqliteErrorCode: 19,
            SqliteExtendedErrorCode: 2067
        };
}
