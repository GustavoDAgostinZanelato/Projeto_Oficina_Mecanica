using Microsoft.AspNetCore.Mvc;
using VeiculosApp.DTO;
using VeiculosApp.Servicos;

namespace VeiculosApp.Controllers
{
    [ApiController]
    [Route("api/veiculos")]
    public class VeiculosController : ControllerBase
    {
        private ServVeiculo _servVeiculo;

        public VeiculosController(ServVeiculo servVeiculo)
        {
            _servVeiculo = servVeiculo;
        }

        [HttpPost]
        public IActionResult Inserir(InserirVeiculoDTO inserirVeiculoDto)
        {
            try
            {
                var novoVeiculo = new Veiculo();

                novoVeiculo.ClienteId = inserirVeiculoDto.ClienteId;
                novoVeiculo.Placa = inserirVeiculoDto.Placa;
                novoVeiculo.Modelo = inserirVeiculoDto.Modelo;
                novoVeiculo.Ano = inserirVeiculoDto.Ano;
                novoVeiculo.Quilometragem = inserirVeiculoDto.Quilometragem;

                _servVeiculo.InserirVeiculo(novoVeiculo);

                return Ok(novoVeiculo);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        public IActionResult Listar()
        {
            try
            {
                var veiculos = _servVeiculo.ListarVeiculo();

                return Ok(veiculos);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("{id:int}")]
        public IActionResult BuscarPorId(int id)
        {
            try
            {
                var veiculo = _servVeiculo.BuscarVeiculoPorId(id);

                return Ok(veiculo);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut("{id:int}")]
        public IActionResult Editar(int id, EditarVeiculoDTO editarVeiculoDto)
        {
            try
            {
                var veiculo = _servVeiculo.BuscarVeiculoPorId(id);

                veiculo.ClienteId = editarVeiculoDto.ClienteId;
                veiculo.Placa = editarVeiculoDto.Placa;
                veiculo.Modelo = editarVeiculoDto.Modelo;
                veiculo.Ano = editarVeiculoDto.Ano;
                veiculo.Quilometragem = editarVeiculoDto.Quilometragem;

                _servVeiculo.EditarVeiculo(veiculo);

                return Ok(veiculo);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
