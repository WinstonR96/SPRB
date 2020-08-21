using System;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ValidacionFERedsisOnBase.Facturas
{
    [XmlRoot("Factura")]
    public class FacturaRechazada : Factura
    {
        private FacturaRechazada()
        {
            TipoFactura = VersionFactura.NoValida;
        }

        public FacturaRechazada(string rejectionMessage) : this()
        {
            Observaciones = rejectionMessage;
        }

        protected override bool GetDataFactura(XDocument xdoc, string pathTemp, out string rejectionMessage)
        {
            rejectionMessage = "No es posible obtener la data de una factura no valida";
            return false;
        }
    }
}
