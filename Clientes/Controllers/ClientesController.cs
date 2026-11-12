using Clientes.DTO;
using Clientes.Models;
using Clientes.Servicos;
using Microsoft.AspNetCore.Mvc;

namespace Clientes.Controllers;

[ApiController]
[Route("api/clientes")]
public class ClientesController : ControllerBase
{
    private readonly ServCliente _servCliente;

    public ClientesController(ServCliente servCliente)
    {
        _servCliente = servCliente;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClienteDTO>>> Listar(CancellationToken cancellationToken)
    {
        var clientes = await _servCliente.ListarAsync(cancellationToken);
        return Ok(clientes.Select(Converter));
    }

    // Rota consumida pelo microsserviço de Ordens de Serviço (busca simples 1).
    [HttpGet("{id:int}", Name = nameof(ObterPorId))]
    public async Task<ActionResult<ClienteDTO>> ObterPorId(int id, CancellationToken cancellationToken)
    {
        var cliente = await _servCliente.ObterAsync(id, cancellationToken);
        return cliente is null ? NotFound() : Ok(Converter(cliente));
    }

    [HttpPost]
    public async Task<ActionResult<ClienteDTO>> Criar(
        CriarClienteDTO dados,
        CancellationToken cancellationToken)
    {
        try
        {
            var cliente = await _servCliente.CriarAsync(dados, cancellationToken);
            return CreatedAtAction(nameof(ObterPorId), new { id = cliente.Id }, Converter(cliente));
        }
        catch (CpfCnpjDuplicadoException exception)
        {
            return CpfCnpjDuplicado(exception);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ClienteDTO>> Atualizar(
        int id,
        AtualizarClienteDTO dados,
        CancellationToken cancellationToken)
    {
        try
        {
            var cliente = await _servCliente.AtualizarAsync(id, dados, cancellationToken);
            return cliente is null ? NotFound() : Ok(Converter(cliente));
        }
        catch (CpfCnpjDuplicadoException exception)
        {
            return CpfCnpjDuplicado(exception);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int id, CancellationToken cancellationToken)
    {
        var removido = await _servCliente.RemoverAsync(id, cancellationToken);
        return removido ? NoContent() : NotFound();
    }

    private static ConflictObjectResult CpfCnpjDuplicado(CpfCnpjDuplicadoException exception) =>
        new(new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "CPF/CNPJ duplicado",
            Detail = exception.Message
        });

    private static ClienteDTO Converter(Cliente cliente) => new()
    {
        Id = cliente.Id,
        Nome = cliente.Nome,
        CpfCnpj = cliente.CpfCnpj,
        Telefone = cliente.Telefone,
        Email = cliente.Email,
        Endereco = cliente.Endereco
    };
}
