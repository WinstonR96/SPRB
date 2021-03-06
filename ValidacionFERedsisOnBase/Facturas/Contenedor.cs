﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ValidacionFERedsisOnBase.Facturas
{
    [XmlRoot("Factura")]
    public class Contenedor : FacturaV2A
    {
        protected override bool GetDataFactura(XDocument xdoc, string pathTemp, List<string> nits, out string rejectionMessage)
        {
            rejectionMessage = string.Empty;
            string SaltoLinea = "<br/>";
            bool esValida = true;
            try
            {
                var invoiceXdocument = GetInvoice(xdoc, pathTemp);

                if (invoiceXdocument == null)
                {
                    rejectionMessage += "No se encontró el XML correspondiente a la factura electrónica";
                    rejectionMessage += SaltoLinea;
                    esValida = false;
                }

                string _rejectionMessage = string.Empty;
                if (!base.GetDataFactura(invoiceXdocument, pathTemp, nits, out _rejectionMessage))
                {
                    rejectionMessage = _rejectionMessage;
                    esValida = false;
                }                
                return esValida;
            }
            catch(Exception ex)
            {
                throw ex;
            }
            
        }

        private XDocument GetInvoice(XDocument xdoc, string path)
        {
            XDocument _xdocData = null;
            var attachment = xdoc.Root.Elements().Where(e => e.Name.LocalName == "Attachment").SingleOrDefault()?
                            .Elements().Where(e => e.Name.LocalName == "ExternalReference").SingleOrDefault()?
                            .Elements().Where(e => e.Name.LocalName == "Description").SingleOrDefault();

            if (attachment == null) return null;

            string attachmentXML = attachment.Value;
            //string tempXmlDataFile = "temp_" + DateTime.Now.Ticks + ".xml";
            string tempXmlDataFile = path + DateTime.Now.Ticks + ".xml";

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
