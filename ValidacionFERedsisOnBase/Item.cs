namespace ValidacionFERedsisOnBase
{
    public class Item
    {
        public string ID { get; set; } = string.Empty;
        public string Cantidad { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string PriceAmount { get; set; } = string.Empty;
        public string LineExtensionAmount { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"Id: {ID}\n" +
                   $"Cantidad: {Cantidad}\n" +
                   $"Descripcion: {Descripcion}\n" +
                   $"Price Amount: {PriceAmount}\n" +
                   $"Line Extension Amount: {LineExtensionAmount}";
        }
    }
}
