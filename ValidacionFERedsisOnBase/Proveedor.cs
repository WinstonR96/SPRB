namespace ValidacionFERedsisOnBase
{
    public class Proveedor
    {
        public string Nit { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Ciudad { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"Nit: {Nit}\n Nombre: {Nombre}\n Ciudad: {Ciudad}\n Direccion: {Direccion}";
        }
    }
}
