using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using ValidacionFERedsisOnBase.Facturas;

namespace SPRBConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Ejecutando");
            string pathXml = "C:\\PG_files\\Factura.xml";
            var factura = Factura.Create(pathXml, "2020wwe");
            Console.WriteLine($"Cufe Factura: {factura.CUFE}");
            Console.WriteLine($"Proveedor Factura: {factura.Proveedor.Nombre}");
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
