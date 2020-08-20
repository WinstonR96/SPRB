namespace ValidacionFERedsisOnBase
{
    public class ControlFactura
    {
        public string Autorizacion { get; set; } = string.Empty;
        public PeriodoAutorizacion PeriodoAutorizacion { get; set; } = new PeriodoAutorizacion();
        public FacturasAutorizadas FacturasAutorizadas { get; set; } = new FacturasAutorizadas();

        public override string ToString()
        {
            return $"Autorizacion: {Autorizacion}\n" +
                   $"Periodo Autorizacion: \n{PeriodoAutorizacion}\n" +
                   $"Facturas Autorizadas: \n{FacturasAutorizadas}";
        }

    }

    public class PeriodoAutorizacion
    {
        public string FechaInicio { get; set; } = string.Empty;
        public string FechaFin { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"Fecha Inicio: {FechaInicio}\n" +
                   $"Fecha Fin: {FechaFin}";
        }
    }

    public class FacturasAutorizadas
    {
        public string Prefijo { get; set; } = string.Empty;
        public string RangoInicio { get; set; } = string.Empty;
        public string RangoFin { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"Prefijo: {Prefijo}\n" +
                   $"Rango Inicio: {RangoInicio}\n" +
                   $"Rango Fin: {RangoFin}";
        }
    }
}
