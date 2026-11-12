namespace VeiculosApp.DTO
{
    public class InserirVeiculoDTO
    {
        public int ClienteId { get; set; }
        public string Placa { get; set; }
        public string Modelo { get; set; }
        public int Ano { get; set; }
        public int? Quilometragem { get; set; }
    }
}
