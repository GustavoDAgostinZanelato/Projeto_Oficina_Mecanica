using Microsoft.AspNetCore.Mvc;
using Pecas.DTO;
using Pecas.Models;
using Pecas.Servicos;

namespace Pecas.Controllers;

[ApiController]
[Route("api/pecas")]
public class PecasController : ControllerBase
{
    private readonly ServPeca _servPeca;

    public PecasController(ServPeca servPeca)
    {
        _servPeca = servPeca;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PecaDTO>>> Listar(CancellationToken cancellationToken)
    {
        var pecas = await _servPeca.ListarAsync(cancellationToken);
        return Ok(pecas.Select(Converter));
    }

    [HttpGet("{id:int}", Name = nameof(ObterPorId))]
    public async Task<ActionResult<PecaDTO>> ObterPorId(int id, CancellationToken cancellationToken)
    {
        var peca = await _servPeca.ObterAsync(id, cancellationToken);
        return peca is null ? NotFound() : Ok(Converter(peca));
    }

    [HttpPost]
    public async Task<ActionResult<PecaDTO>> Criar(
        CriarPecaDTO dados,
        CancellationToken cancellationToken)
    {
        try
        {
            var peca = await _servPeca.CriarAsync(dados, cancellationToken);
            return CreatedAtAction(nameof(ObterPorId), new { id = peca.Id }, Converter(peca));
        }
        catch (CodigoPecaDuplicadoException exception)
        {
            return CodigoDuplicado(exception);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PecaDTO>> Atualizar(
        int id,
        AtualizarPecaDTO dados,
        CancellationToken cancellationToken)
    {
        try
        {
            var peca = await _servPeca.AtualizarAsync(id, dados, cancellationToken);
            return peca is null ? NotFound() : Ok(Converter(peca));
        }
        catch (CodigoPecaDuplicadoException exception)
        {
            return CodigoDuplicado(exception);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int id, CancellationToken cancellationToken)
    {
        var removida = await _servPeca.RemoverAsync(id, cancellationToken);
        return removida ? NoContent() : NotFound();
    }

    [HttpPost("{id:int}/baixas")]
    public async Task<ActionResult<PecaDTO>> BaixarEstoque(
        int id,
        BaixaEstoqueDTO dados,
        CancellationToken cancellationToken)
    {
        try
        {
            var peca = await _servPeca.BaixarEstoqueAsync(id, dados.Quantidade, cancellationToken);
            return Ok(Converter(peca));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (EstoqueInsuficienteException exception)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Estoque insuficiente",
                Detail = exception.Message
            });
        }
    }

    private static ConflictObjectResult CodigoDuplicado(CodigoPecaDuplicadoException exception) =>
        new(new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Código de peça duplicado",
            Detail = exception.Message
        });

    private static PecaDTO Converter(Peca peca) => new()
    {
        Id = peca.Id,
        Codigo = peca.Codigo,
        Nome = peca.Nome,
        Descricao = peca.Descricao,
        PrecoUnitario = peca.PrecoUnitario,
        QuantidadeEstoque = peca.QuantidadeEstoque
    };
}
