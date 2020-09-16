using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ValidacionFERedsisOnBase.Facturas
{
    [XmlRoot("Factura")]
    public class FacturaRechazada : FacturaV2A
    {
        private FacturaRechazada()
        {
            TipoFactura = VersionFactura.NoValida;
        }

        public FacturaRechazada(string rejectionMessage) : this()
        {
            //Observaciones = rejectionMessage;
            Notas = rejectionMessage;
        }



        protected override bool GetDataFactura(XDocument xdoc, string pathTemp, List<string> nits, out string rejectionMessage)
        {
            string SaltoLinea = "<br/>";
            rejectionMessage = "No es posible obtener la data de una factura no valida"+ SaltoLinea;
            return false;
        }
        
    }
}
