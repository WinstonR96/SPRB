using System;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ValidacionFERedsisOnBase.Facturas
{
    [XmlRoot("Factura")]
    public class Contenedor : FacturaV2A
    {
        protected override bool GetDataFactura(XDocument xdoc, out string rejectionMessage)
        {
            var invoiceXdocument = GetInvoice(xdoc);

            if (invoiceXdocument == null)
            {
                rejectionMessage = "No se encontró el XML correspondiente a la factura electrónica";
                return false;
            }

            string _rejectionMessage = string.Empty;
            if (!base.GetDataFactura(invoiceXdocument, out _rejectionMessage))
            {
                rejectionMessage = _rejectionMessage;
                return false;
            }

            rejectionMessage = "";
            return true;
        }

        private XDocument GetInvoice(XDocument xdoc)
        {
            XDocument _xdocData = null;
            var attachment = xdoc.Root.Elements().Where(e => e.Name.LocalName == "Attachment").SingleOrDefault()?
                            .Elements().Where(e => e.Name.LocalName == "ExternalReference").SingleOrDefault()?
                            .Elements().Where(e => e.Name.LocalName == "Description").SingleOrDefault();

            if (attachment == null) return null;

            string attachmentXML = attachment.Value;
            string tempXmlDataFile = "temp_" + DateTime.Now.Ticks + ".xml";

            using (var sw = new System.IO.StreamWriter(tempXmlDataFile))
                sw.Write(attachmentXML);

            try
            {
                _xdocData = XDocument.Load(tempXmlDataFile);
            }
            catch (Exception e)
            {
                Console.Write(e);
                System.IO.File.Delete(tempXmlDataFile);
                return null;
            }

            if (_xdocData?.Root.Name.NamespaceName != V2A_NAMESPACE)
            {
                System.IO.File.Delete(tempXmlDataFile);
                return null;
            }

            System.IO.File.Delete(tempXmlDataFile);
            return _xdocData;
        }
    }
}
