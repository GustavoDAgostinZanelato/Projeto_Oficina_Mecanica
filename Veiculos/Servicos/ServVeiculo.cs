using AppTemplate;

namespace VeiculosApp.Servicos
{
    public class ServVeiculo
    {
        private DataContext _dataContext;

        public ServVeiculo(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public void InserirVeiculo(Veiculo novoVeiculo)
        {
            ValidarVeiculo(novoVeiculo);

            _dataContext.Add(novoVeiculo);
            _dataContext.SaveChanges();
        }

        public void ValidarVeiculo(Veiculo veiculo)
        {
            if (string.IsNullOrWhiteSpace(veiculo.Placa))
            {
                throw new Exception("Falta informar a placa do veículo");
            }

            veiculo.Placa = veiculo.Placa.Trim().ToUpper();

            var placaJaExiste = _dataContext.Veiculos
                .Any(p => p.Placa == veiculo.Placa && p.Id != veiculo.Id);

            if (string.IsNullOrWhiteSpace(veiculo.Modelo))
            {
                throw new Exception("Falta informar o modelo do veículo");
            }

            if(veiculo.Ano <= 0)
            {
                throw new Exception("Informe um ano válido");
            }

            if(veiculo.ClienteId <= 0)
            {
                throw new Exception("Falta informar uma cliente proprietário");
            }

            if (placaJaExiste)
            {
                throw new Exception("Já existe um veículo cadastrado com essa placa");
            }
        }

        public List<Veiculo> ListarVeiculo()
        {
            return _dataContext.Veiculos.ToList();
        }

        public Veiculo BuscarVeiculoPorId(int Id)
        {
            var veiculo = _dataContext.Veiculos.FirstOrDefault(p => p.Id == Id);

            if(veiculo == null)
            {
                throw new Exception("Veículo não encontrado");
            }

            return veiculo;
        }

        public void EditarVeiculo(Veiculo veiculo)
        {
            ValidarVeiculo(veiculo);

            _dataContext.Veiculos.Update(veiculo);
            _dataContext.SaveChanges();
        }
    }
}