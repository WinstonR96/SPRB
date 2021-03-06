﻿using ExcelDataReader;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.ServiceModel;
using System.Xml.Linq;
using ValidacionFERedsisOnBase.Facturas;
using WsSoap;
using System.Linq;

namespace SPRBConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Ejecutando");
            string pathXml = "C:\\PG_files\\Factura6.xml";
            string nit1 = "791481826";
            string nit2 = "8600073229";
            string plantilla = "C:\\Users\\wrodriguez\\Desktop\\Plantilla.html";
            string html = "C:\\Users\\wrodriguez\\Desktop\\";
            string excelPath = "C:\\Users\\wrodriguez\\Desktop\\Nit.xlsx";
            string csvPath = @"C:\Users\wrodriguez\Documents\archivo.csv";
            try
            {
                //List<string> nits = ObtenerNits(excelPath);
                var facturas = procesarCsv(csvPath);
                foreach (var factura in facturas)
                {
                    factura.Resultado = "No encontrada";
                    //Console.WriteLine($"{factura.Id_Empresa} - {factura.Nro_Factura} - {factura.Nit} - {factura.Nro_Egreso} - {factura.Estado} - {factura.Resultado}");
                }
                Console.WriteLine(actualizarCsv(csvPath,facturas));
                ////Obtener correo
                //Console.WriteLine("Nit de prueba {0}",Api.ObtenerEmail(nit1));
                //Console.WriteLine("Nit de prueba {0}",Api.ObtenerEmail(nit2));
                //XDocument xDocument = XDocument.Load(pathXml);
                ////var factura = Factura.Create(pathXml, "2020wwe");
                //var factura = Factura.Create(xDocument, "2020wwe", html, nits);
                //Console.WriteLine(System.Environment.NewLine);
                //Console.WriteLine("------------- Información de Factura ------------");
                //Console.WriteLine(System.Environment.NewLine);
                //Console.WriteLine(factura.ToString());
                //Console.WriteLine(System.Environment.NewLine);
                //Console.WriteLine("-------------------------------------------------");
                //Console.WriteLine(System.Environment.NewLine);
                //switch (factura.TipoFactura)
                //{
                //    case VersionFactura.V1:
                //        FacturaV1 fv1 = factura as FacturaV1;
                //        Factura.ParsearHtml(plantilla, fv1, html);
                //        break;
                //    case VersionFactura.V2A:
                //        FacturaV2A fv2 = factura as FacturaV2A;
                //        Factura.ParsearHtml(plantilla, fv2, html);
                //        break;
                //    case VersionFactura.NoValida:
                //        Factura.ParsearHtml(plantilla, factura, html);
                //        break;
                //    default:
                //        FacturaRechazada fr = factura as FacturaRechazada;
                //        throw new Exception($"Factura Rechazada: {factura.Observaciones}");
                //}

                //FacturaV2A f2 = factura as FacturaV2A;
                //Console.WriteLine($"Nro orden: {f2.NumOrdenCompra}");
                //Console.WriteLine($"Fecha Vencimiento: {f2.FechaVencimiento}");
                //Console.WriteLine($"Valor total: {f2.TotalFactura}");

            }
            catch(Exception ex)
            {
                Console.WriteLine($"Ocurrio un error: {ex.Message}");
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

        public static List<string> ObtenerNits(string path)
        {
            int row = 0;
            int col = 0;
            List<string> nits = new List<string>();
            try
            {
                using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        do
                        {
                            int rowCount = 0;
                            while (reader.Read())
                            {
                                if (rowCount >= row)
                                {
                                    string cad = reader.GetString(col);
                                    if (string.IsNullOrEmpty(cad))
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        cad = cad.ToLower();
                                        nits.Add(cad);
                                    }
                                }
                                rowCount++;
                            }
                        } while (reader.NextResult());
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return nits;
        }

        public class Factura
        {
            public string Id_Empresa { get; set; }
            public string Nro_Factura { get; set; }
            public string Nro_Egreso { get; set; }
            public string Estado { get; set; }
            public string Nit { get; set; }
            public string Nomb_Propuesta { get; set; }
            public string Resultado { get; set; }

        }

        public static List<Factura> procesarCsv(string path)
        {
            int row = 0;
            int col = 0;
            List<Factura> facturas = new List<Factura>();
            try
            {
                using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateCsvReader(stream))
                    {
                        do
                        {
                            int rowCount = 0;
                            while (reader.Read())
                            {
                                if (rowCount >= row)
                                {
                                    string id_empresa = reader.GetString(col);
                                    string nro_factura = reader.GetString(col + 1);
                                    string nit = reader.GetString(col + 2);
                                    string nro_egreso = reader.GetString(col + 3);
                                    string estado = reader.GetString(col + 4);
                                    string nomb_propuesta = reader.GetString(col + 5);
                                    facturas.Add(new Factura()
                                    {
                                        Id_Empresa = id_empresa,
                                        Nro_Factura = nro_factura,
                                        Nit = nit,
                                        Nro_Egreso = nro_egreso,
                                        Estado = estado,
                                        Nomb_Propuesta = nomb_propuesta
                                    });

                                }
                                rowCount++;
                            }
                        } while (reader.NextResult());
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return facturas;
        }

        public static bool actualizarCsv(string path, List<Factura> facturas)
        {
            if (facturas == null || facturas.Count == 0) return false;
            try
            {
                string newLine = Environment.NewLine;
                using (var sw = new StreamWriter(path))
                {
                    foreach (Factura item in facturas)
                    {
                        string[] aux = new string[] { item.Id_Empresa, item.Nro_Factura, item.Nit, item.Nro_Egreso, item.Estado, item.Nomb_Propuesta, item.Resultado };
                        var row = string.Join(";", aux.ToArray());
                        sw.Write(row + newLine);
                    }
                    return true;
                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
