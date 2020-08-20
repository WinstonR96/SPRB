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
            string plantilla = "C:\\Users\\Redsis\\Desktop\\Plantilla.html";
            string html = "C:\\Users\\Redsis\\Desktop\\";
            var factura = Factura.Create(pathXml, "2020wwe");
            switch (factura.TipoFactura)
            {
                case VersionFactura.V1:
                    FacturaV1 fv1 = factura as FacturaV1;
                    Factura.ParsearHtml(plantilla, fv1, html);
                    break;
                case VersionFactura.V2A:
                    FacturaV2A fv2 = factura as FacturaV2A;
                    Factura.ParsearHtml(plantilla, fv2, html);
                    break;
                case VersionFactura.NoValida:                    
                    Factura.ParsearHtml(plantilla, factura, html);
                    break;
            }
            //Factura.ParsearHtml(html, factura);
            Console.WriteLine("Finalizado");
            Console.ReadLine();
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
