using System;
using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ValidacionFERedsisOnBase.Facturas
{
    public enum VersionFactura
    {
        V1 = 0,
        V2A = 1,
        NoValida = 3
    }

    public abstract class Factura
    {
        public const string V1_NAMESPACE = @"http://www.dian.gov.co/contratos/facturaelectronica/v1";
        public const string V2A_NAMESPACE = @"urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";
        public const string CONTAINER_NAMESPACE = @"urn:oasis:names:specification:ubl:schema:xsd:AttachedDocument-2";

        public VersionFactura TipoFactura { get; set; }
        public string MailMessageID { get; set; } = string.Empty;
        public string UBLVersion { get; set; } = string.Empty;
        public string CUFE { get; set; } = string.Empty;
        public string NumFactura { get; set; } = string.Empty;
        public Proveedor Proveedor { get; set; } = new Proveedor();
        public string Observaciones { get; set; } = string.Empty;

        public static Factura Create(string xmlFile, string mailMessageID)
        {
            Factura factura = null;
            XDocument xdoc = null;

            try
            {

                xdoc = XDocument.Load(xmlFile);
            }
            catch (Exception e)
            {
                Console.Write(e);
                factura = new FacturaRechazada($"{e.Message}\n{e.ToString()}");
                factura.MailMessageID = mailMessageID;
                return factura;

            }

            //string rootName = xdoc.Root.Name.LocalName;
            string rootNamespaceName = xdoc.Root.Name.NamespaceName;

            switch (rootNamespaceName)
            {
                case V1_NAMESPACE:
                    factura = new FacturaV1();
                    break;
                case V2A_NAMESPACE:
                    factura = new FacturaV2A();
                    break;
                case CONTAINER_NAMESPACE:
                    factura = new Contenedor();
                    break;
                default:
                    //factura = new FacturaRechazada("Namespace de la factura no identificado");
                    //break;
                    return null;
            }

            if (factura.TipoFactura == VersionFactura.V1 || factura.TipoFactura == VersionFactura.V2A)
            {
                try
                {
                    string rejectionMessage = string.Empty;
                    if (!factura.GetDataFactura(xdoc, out rejectionMessage))
                        factura = factura.Rechazar(rejectionMessage);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    factura = new FacturaRechazada($"{e.Message}\n{e.ToString()}");
                }
            }

            factura.MailMessageID = mailMessageID;
            return factura;
        }

        public static void ParsearHtml(string pathHtml, Factura factura)
        {
            if (File.Exists(pathHtml))
            {
                try
                {
                    HtmlAgilityPack.HtmlDocument documentHTML = new HtmlAgilityPack.HtmlDocument();
                    documentHTML.Load(pathHtml);
                    documentHTML.GetElementbyId("proveedor").InnerHtml = factura.Proveedor.Nombre;
                    documentHTML.GetElementbyId("nit").InnerHtml = factura.Proveedor.Nit;
                    documentHTML.GetElementbyId("direccion").InnerHtml = factura.Proveedor.Direccion;
                    documentHTML.GetElementbyId("cufe").InnerHtml = factura.CUFE;
                    //TODO: verificar de donde sale este dato
                    documentHTML.GetElementbyId("nitpst").InnerHtml = "";
                    documentHTML.GetElementbyId("nfactura").InnerHtml = factura.NumFactura;
                    //TODO: verificar de donde sale este dato
                    documentHTML.GetElementbyId("ordencompra").InnerHtml = "";
                    documentHTML.GetElementbyId("fechaemision").InnerHtml = "";
                    documentHTML.GetElementbyId("horaemision").InnerHtml = "";
                    documentHTML.GetElementbyId("facturasautorizadas").InnerHtml = "";
                    documentHTML.GetElementbyId("autorizacionfactura").InnerHtml = "";
                    documentHTML.GetElementbyId("periodoautorizacion").InnerHtml = "";
                    documentHTML.GetElementbyId("origenfactura").InnerHtml = "";
                    documentHTML.GetElementbyId("codigomoneda").InnerHtml = "";
                    //TODO: Validar listado de items
                    documentHTML.GetElementbyId("nitem").InnerHtml = "";
                    documentHTML.GetElementbyId("cantidaditem").InnerHtml = "";
                    documentHTML.GetElementbyId("descripcionitem").InnerHtml = "";
                    documentHTML.GetElementbyId("valorunitarioitem").InnerHtml = "";
                    documentHTML.GetElementbyId("valortotalitem").InnerHtml = "";
                    documentHTML.GetElementbyId("observaciones").InnerHtml = factura.Observaciones;
                    documentHTML.GetElementbyId("basegravable").InnerHtml = "";
                    documentHTML.GetElementbyId("preciototal").InnerHtml = "";
                    documentHTML.Save(pathHtml);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Se produjo un error parseando html: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"El archivo {pathHtml} no fue encontrado");
            }
        }

        public static VersionFactura GetVersionFactura(string xmlFile)
        {
            XDocument xdoc = null;
            try
            {
                xdoc = XDocument.Load(xmlFile);
            }
            catch (Exception e)
            {
                Console.Write(e);
                return VersionFactura.NoValida;
            }

            string rootNamespaceName = xdoc.Root.Name.NamespaceName;
            switch (rootNamespaceName)
            {
                case V1_NAMESPACE:
                    return VersionFactura.V1;
                case V2A_NAMESPACE:
                    return VersionFactura.V2A;
                default:
                    return VersionFactura.NoValida;
            }
        }

        protected abstract bool GetDataFactura(XDocument xdoc, out string rejectionMessage);

        public string ToXML()
        {
            try
            {
                var stringwriter = new Utf8StringWriter();
                var serializer = new XmlSerializer(this.GetType());
                serializer.Serialize(stringwriter, this);
                return stringwriter.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine("error: " + e.Message + "\n" + e);
                return null;
            }
        }

        //public void WriteUTF8XML(string filePath)
        //{
        //    var serializer = new XmlSerializer(this.GetType());
        //    string utf8;
        //    using (StringWriter writer = new Utf8StringWriter())
        //    {
        //        serializer.Serialize(writer, this);
        //        utf8 = writer.ToString();
        //    }
        //    Console.WriteLine(utf8);
        //    //var memoryStream = new MemoryStream();
        //    //var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);

        //    //serializer.Serialize(streamWriter, this);
        //    //var utfEncodedXML = memoryStream.ToArray();
        //    using (StreamWriter sw = new StreamWriter(filePath))
        //    {
        //        sw.Write(utf8);
        //    }
        //}

        public class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding => Encoding.UTF8;
        }

        public FacturaRechazada Rechazar(string rejectionMessage)
        {
            FacturaRechazada rechazada = new FacturaRechazada(rejectionMessage);

            rechazada.MailMessageID = MailMessageID;
            rechazada.CUFE = CUFE;
            rechazada.NumFactura = NumFactura;
            rechazada.UBLVersion = UBLVersion;
            rechazada.Proveedor = Proveedor;

            return rechazada;
        }
    }
}
