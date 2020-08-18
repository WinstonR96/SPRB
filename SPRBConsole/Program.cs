using System;
using System.IO;
using System.ServiceModel;
using ValidacionFERedsisOnBase.Facturas;


namespace SPRBConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Ejecutando");
            string pathXml = "C:\\PG_files\\Factura.xml";
            string html = "C:\\Users\\Redsis\\Desktop\\Plantilla.html";
            var factura = Factura.Create(pathXml, "2020wwe");
            switch (factura.TipoFactura)
            {
                case VersionFactura.V1:
                    FacturaV1 fv1 = factura as FacturaV1;
                    Factura.ParsearHtml(html, fv1);
                    break;
                case VersionFactura.V2A:
                    FacturaV2A fv2 = factura as FacturaV2A;
                    Factura.ParsearHtml(html, fv2);
                    break;
                case VersionFactura.NoValida:                    
                    Factura.ParsearHtml(html, factura);
                    break;
            }
            Factura.ParsearHtml(html, factura);
            Console.WriteLine("Finalizado");
            Console.ReadLine();
        }

        private static void ParsearHtml(string pathHtml, Factura factura)
        {
            string html = "C:\\Users\\Redsis\\Desktop\\Plantilla.html";
            if (File.Exists(html))
            {
                try
                {
                    HtmlAgilityPack.HtmlDocument documentHTML = new HtmlAgilityPack.HtmlDocument();
                    documentHTML.Load(pathHtml);
                    Console.WriteLine($"Html: {html}");
                    documentHTML.GetElementbyId("proveedor").InnerHtml = factura.Proveedor.Nombre;
                    documentHTML.GetElementbyId("nit").InnerHtml = factura.Proveedor.Nit;
                    documentHTML.GetElementbyId("direccion").InnerHtml = factura.Proveedor.Direccion;
                    documentHTML.GetElementbyId("cufe").InnerHtml = factura.CUFE;
                    //TODO: verificar de donde sale este dato
                    documentHTML.GetElementbyId("nitpst").InnerHtml = "";
                    documentHTML.GetElementbyId("nfactura").InnerHtml = factura.NumFactura;
                    //TODO: verificar de donde sale este dato
                    documentHTML.GetElementbyId("ordencompra").InnerHtml =  "";
                    documentHTML.GetElementbyId("fechaemision").InnerHtml =  "";
                    documentHTML.GetElementbyId("horaemision").InnerHtml =  "";
                    documentHTML.GetElementbyId("facturasautorizadas").InnerHtml =  "";
                    documentHTML.GetElementbyId("autorizacionfactura").InnerHtml =  "";
                    documentHTML.GetElementbyId("periodoautorizacion").InnerHtml =  "";
                    documentHTML.GetElementbyId("origenfactura").InnerHtml =  "";
                    documentHTML.GetElementbyId("codigomoneda").InnerHtml =  "";
                    //TODO: Validar listado de items
                    documentHTML.GetElementbyId("nitem").InnerHtml =  "";
                    documentHTML.GetElementbyId("cantidaditem").InnerHtml =  "";
                    documentHTML.GetElementbyId("descripcionitem").InnerHtml =  "";
                    documentHTML.GetElementbyId("valorunitarioitem").InnerHtml =  "";
                    documentHTML.GetElementbyId("valortotalitem").InnerHtml =  "";
                    documentHTML.GetElementbyId("observaciones").InnerHtml =  factura.Observaciones;
                    documentHTML.GetElementbyId("basegravable").InnerHtml =  "";
                    documentHTML.GetElementbyId("preciototal").InnerHtml =  "";
                    documentHTML.Save(html);
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Se produjo un error parseando html: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"El archivo {pathHtml} no fue encontrado");
            }
        }

        public void ConectarSoap()
        {
            string END_POINT_STR = "http://ONBASE_PRUEB:Interfaz2020*@SPRB-SBX.sprb.com:8010/sap/bc/srt/rfc/sap/zecollectsap/400/zecollectsap/zecollectsap";
            BasicHttpBinding binding = new BasicHttpBinding();//Para HTTP
                                                              //BasicHttpsBinding binding = new BasicHttpsBinding();//Para HTTPS
            binding.Name = "zecollectsap";
            binding.MaxBufferPoolSize = 2147483647;
            binding.MaxReceivedMessageSize = 2147483647;
            string endpointStr = END_POINT_STR;
            EndpointAddress endpoint = new EndpointAddress(endpointStr);
            SprbSoap.ZECOLLECTSAPClient ws = new SprbSoap.ZECOLLECTSAPClient(binding, endpoint);
            SprbSoap.ZfiGetCarteraCliente zfiGetCarteraCliente = new SprbSoap.ZfiGetCarteraCliente();
            zfiGetCarteraCliente.Entitycode = "10301";
            zfiGetCarteraCliente.Reference1 = "8901074873";
            zfiGetCarteraCliente.Servicecode = "100110";
            var response = ws.ZfiGetCarteraCliente(zfiGetCarteraCliente);

            Console.WriteLine($"Probando {response.Returncode}");
        }
    }
}
