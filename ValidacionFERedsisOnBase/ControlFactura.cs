namespace ValidacionFERedsisOnBase
{
    public class ControlFactura
    {
        public string Autorizacion { get; set; } = string.Empty;
        public PeriodoAutorizacion PeriodoAutorizacion { get; set; } = new PeriodoAutorizacion();
        public FacturasAutorizadas FacturasAutorizadas { get; set; } = new FacturasAutorizadas();

    }

    public class PeriodoAutorizacion
    {
        public string FechaInicio { get; set; } = string.Empty;
        public string FechaFin { get; set; } = string.Empty;
    }

    public class FacturasAutorizadas
    {
        public string Prefijo { get; set; } = string.Empty;
        public string RangoInicio { get; set; } = string.Empty;
        public string RangoFin { get; set; } = string.Empty;
    }
}
