﻿using System;
using System.Collections.Generic;
using System.Globalization;
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

        public static void ParsearHtml(string pathHtml, Factura factura, HtmlAgilityPack.HtmlDocument documentHTML = null)
        {
            if (File.Exists(pathHtml))
            {
                try
                {
                    if (documentHTML == null)
                    {
                        documentHTML = new HtmlAgilityPack.HtmlDocument();
                        documentHTML.Load(pathHtml);
                    }
                    documentHTML.GetElementbyId("proveedor").InnerHtml = factura.Proveedor.Nombre;
                    documentHTML.GetElementbyId("nit").InnerHtml = factura.Proveedor.Nit;
                    documentHTML.GetElementbyId("direccion").InnerHtml = factura.Proveedor.Direccion;
                    documentHTML.GetElementbyId("cufe").InnerHtml = factura.CUFE;
                    documentHTML.GetElementbyId("nfactura").InnerHtml = factura.NumFactura;
                    documentHTML.GetElementbyId("observaciones").InnerHtml = factura.Observaciones;
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

        public static void ParsearHtml(string pathHtml, FacturaV1 factura, HtmlAgilityPack.HtmlDocument documentHTML = null)
        {
            if (File.Exists(pathHtml))
            {
                try
                {
                    if (documentHTML == null)
                    {
                        documentHTML = new HtmlAgilityPack.HtmlDocument();
                        documentHTML.Load(pathHtml);
                    }
                    string NroItem = ObtenerNumeroItem(factura.Items);
                    string CantidadItems = ObtenerCantidadesItems(factura.Items);
                    string DescripcionItems = ObtenerDescripcionItems(factura.Items);
                    string ValorUnitarioItem = ObtenerValorUnitarioItem(factura.Items);
                    string ValorTotalItem = ObtenerValorTotalItem(factura.Items);
                    string BaseGravable = FormatMoneda(factura.BaseGravable);
                    string TotalFactura = FormatMoneda(factura.TotalFactura);
                    ParsearHtml(pathHtml, factura as Factura, documentHTML);
                    documentHTML.GetElementbyId("nitpst").InnerHtml = factura.PST;
                    documentHTML.GetElementbyId("fechaemision").InnerHtml = factura.FechaEmision;
                    documentHTML.GetElementbyId("horaemision").InnerHtml = factura.HoraEmision;
                    documentHTML.GetElementbyId("facturasautorizadas").InnerHtml = factura.ControlFactura.FacturasAutorizadas.RangoInicio + " - " + factura.ControlFactura.FacturasAutorizadas.RangoFin;
                    documentHTML.GetElementbyId("autorizacionfactura").InnerHtml = factura.ControlFactura.Autorizacion;
                    documentHTML.GetElementbyId("periodoautorizacion").InnerHtml = factura.ControlFactura.PeriodoAutorizacion.FechaInicio + " - " + factura.ControlFactura.PeriodoAutorizacion.FechaFin;
                    documentHTML.GetElementbyId("origenfactura").InnerHtml = factura.OrigenFactura;
                    documentHTML.GetElementbyId("codigomoneda").InnerHtml = factura.TipoMoneda;
                    documentHTML.GetElementbyId("nitem").InnerHtml = NroItem;
                    documentHTML.GetElementbyId("cantidaditem").InnerHtml = CantidadItems;
                    documentHTML.GetElementbyId("descripcionitem").InnerHtml = DescripcionItems;
                    documentHTML.GetElementbyId("valorunitarioitem").InnerHtml = ValorUnitarioItem;
                    documentHTML.GetElementbyId("valortotalitem").InnerHtml = ValorTotalItem;
                    documentHTML.GetElementbyId("basegravable").InnerHtml = BaseGravable;
                    documentHTML.GetElementbyId("preciototal").InnerHtml = TotalFactura;
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

        private static string FormatMoneda(string valor)
        {
            string value = "";
            double valorDecimal = Convert.ToDouble(valor, CultureInfo.InvariantCulture);
            value = string.Format(new CultureInfo("es-CO"), "{0:c}", valorDecimal);
            return value;
        }

        private static string ObtenerValorTotalItem(List<Item> items)
        {
            string ValorTotalItem = "";
            foreach (var item in items)
            {
                double cantItem = Convert.ToDouble(item.Cantidad, CultureInfo.InvariantCulture);
                double valorUnitItem = Convert.ToDouble(item.PriceAmount, CultureInfo.InvariantCulture);
                string auxTotal = string.Format(new CultureInfo("es-CO"), "{0:c}", cantItem * valorUnitItem);
                ValorTotalItem += $"{auxTotal}<br>";
            }
            return ValorTotalItem;
        }

        private static string ObtenerValorUnitarioItem(List<Item> items)
        {
            CultureInfo cultureInfo = new CultureInfo("es-CO");
            string ValorUnitItem = "";
            foreach (var item in items)
            {
                double convert = Convert.ToDouble(item.PriceAmount, CultureInfo.InvariantCulture);
                //TODO: Dar formato de moneda
                string aux = string.Format(cultureInfo,"{0:c}", convert);
                ValorUnitItem += $"{aux}<br>";
            }
            return ValorUnitItem;
        }

        private static string ObtenerDescripcionItems(List<Item> items)
        {
            string DescItem = "";
            foreach (var item in items)
            {
                DescItem += $"{item.Descripcion}<br>";
            }
            return DescItem;
        }

        private static string ObtenerNumeroItem(List<Item> items)
        {
            string NroItem = "";
            foreach (var item in items)
            {
                NroItem += $"{item.ID}<br>";
            }
            return NroItem;
        }

        private static string ObtenerCantidadesItems(List<Item> items)
        {
            string cantItems = "";
            foreach(var item in items)
            {
                cantItems += $"{item.Cantidad}<br>";
            }
            return cantItems;
        }

        public static void ParsearHtml(string pathHtml, FacturaV2A factura, HtmlAgilityPack.HtmlDocument documentHTML = null)
        {
            if (File.Exists(pathHtml))
            {
                try
                {
                    if (documentHTML == null)
                    {
                        documentHTML = new HtmlAgilityPack.HtmlDocument();
                        documentHTML.Load(pathHtml);
                    }
                    ParsearHtml(pathHtml, factura as FacturaV1, documentHTML);
                    documentHTML.GetElementbyId("ordencompra").InnerHtml = factura.NumOrdenCompra;
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
