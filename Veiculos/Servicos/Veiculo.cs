namespace VeiculosApp.Servicos
{
    public class Veiculo
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string Placa { get; set; }
        public string Modelo { get; set; } 
        public int Ano { get; set; }
        public int? Quilometragem { get; set; }
    }
}
