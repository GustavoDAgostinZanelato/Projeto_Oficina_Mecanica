using Microsoft.AspNetCore.Mvc;
using PedidoServico.DTO;
using PedidoServico.Models;
using PedidoServico.Servicos;

namespace PedidoServico.Controllers;

[ApiController]
[Route("api/ordens-servicos")]
public class OrdensServicoController : ControllerBase
{
    private readonly ServOrdemServico _servico;

    public OrdensServicoController(ServOrdemServico servico)
    {
        _servico = servico;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrdemServicoDTO>>> Listar(CancellationToken cancellationToken)
    {
        var ordens = await _servico.ListarAsync(cancellationToken);
        return Ok(ordens.Select(Converter));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrdemServicoDTO>> ObterPorId(int id, CancellationToken cancellationToken)
    {
        var ordem = await _servico.ObterAsync(id, cancellationToken);
        return ordem is null ? NotFound() : Ok(Converter(ordem));
    }

    [HttpPost]
    public async Task<ActionResult<OrdemServicoDTO>> Criar(CriarOrdemServicoDTO dados, CancellationToken cancellationToken)
    {
        try
        {
            var ordem = await _servico.CriarAsync(dados, cancellationToken);
            return CreatedAtAction(nameof(ObterPorId), new { id = ordem.Id }, Converter(ordem));
        }
        catch (RecursoNaoEncontradoException exception)
        {
            return BadRequest(new ProblemDetails { Status = 400, Title = "Referência inválida", Detail = exception.Message });
        }
        catch (IntegracaoException exception)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new ProblemDetails { Status = 502, Title = "Falha de integração", Detail = exception.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<OrdemServicoDTO>> Atualizar(int id, AtualizarOrdemServicoDTO dados, CancellationToken cancellationToken)
    {
        try
        {
            var ordem = await _servico.AtualizarAsync(id, dados, cancellationToken);
            return ordem is null ? NotFound() : Ok(Converter(ordem));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Status = 400, Title = "Dados inválidos", Detail = exception.Message });
        }
        catch (RecursoNaoEncontradoException exception)
        {
            return BadRequest(new ProblemDetails { Status = 400, Title = "Referência inválida", Detail = exception.Message });
        }
        catch (IntegracaoException exception)
        {
            return StatusCode(502, new ProblemDetails { Status = 502, Title = "Falha de integração", Detail = exception.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int id, CancellationToken cancellationToken)
    {
        var removida = await _servico.RemoverAsync(id, cancellationToken);
        return removida ? NoContent() : NotFound();
    }

    [HttpPost("{id:int}/pecas")]
    [HttpPost("{id:int}/itens")]
    public async Task<ActionResult<OrdemServicoDTO>> AdicionarPeca(int id, AdicionarPecaDTO dados, CancellationToken cancellationToken)
    {
        try
        {
            var ordem = await _servico.AdicionarPecaAsync(id, dados, cancellationToken);
            return Ok(Converter(ordem));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new ProblemDetails { Status = 404, Title = "Recurso não encontrado", Detail = exception.Message });
        }
        catch (EstoqueInsuficienteException exception)
        {
            return Conflict(new ProblemDetails { Status = 409, Title = "Estoque insuficiente", Detail = exception.Message });
        }
        catch (IntegracaoException exception)
        {
            return StatusCode(502, new ProblemDetails { Status = 502, Title = "Falha de integração", Detail = exception.Message });
        }
    }

    private static OrdemServicoDTO Converter(OrdemServico ordem) => new()
    {
        Id = ordem.Id,
        ClienteId = ordem.ClienteId,
        VeiculoId = ordem.VeiculoId,
        DataCriacaoServico = ordem.DataCriacaoServico,
        DataInicioServico = ordem.DataInicioServico,
        DataConclusaoServico = ordem.DataConclusaoServico,
        Status = ordem.Status,
        ValorTotal = ordem.ValorTotal,
        Observacoes = ordem.Observacoes,
        TaxaServico = ordem.TaxaServico,
        ValorBaseServico = ordem.ValorBaseServico,
        Itens = ordem.Itens.Select(item => new ItemOrdemServicoDTO
        {
            Id = item.Id,
            PecaId = item.PecaId,
            Quantidade = item.Quantidade,
            PrecoUnitarioMomento = item.PrecoUnitarioMomento
        }).ToList()
    };
}
