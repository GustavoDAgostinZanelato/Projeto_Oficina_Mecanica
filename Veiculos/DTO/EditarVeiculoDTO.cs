namespace VeiculosApp.DTO
{
    public class EditarVeiculoDTO
    {
        public int ClienteId { get; set; }
        public string Placa { get; set; }
        public string Modelo { get; set; }
        public int Ano { get; set; }
        public int? Quilometragem { get; set; }
    }
}
