using System;
using System.Security.Cryptography;
using System.Text;

namespace ValidacionFERedsisOnBase
{
    public class DatosCUFE
    {
        public string NumFac { get; set; } = string.Empty;
        public string FecFac { get; set; } = string.Empty;
        public string HorFac { get; set; } = string.Empty;
        public string ValFac { get; set; } = string.Empty;
        public string CodImp1 { get; set; } = string.Empty;
        public string ValImp1 { get; set; } = string.Empty;
        public string CodImp2 { get; set; } = string.Empty;
        public string ValImp2 { get; set; } = string.Empty;
        public string CodImp3 { get; set; } = string.Empty;
        public string ValImp3 { get; set; } = string.Empty;
        public string ValTot { get; set; } = string.Empty;
        public string NitFE { get; set; } = string.Empty;
        public string NumAdq { get; set; } = string.Empty;
        public string ClTec { get; set; } = string.Empty;
        public string TipoAmbie { get; set; } = string.Empty;

        public static void Ejemplo()
        {
            DatosCUFE dCufe = new DatosCUFE
            {
                NumFac = "323200000129",
                FecFac = "2019-01-16",
                HorFac = "10:53:10-05:00",
                ValFac = "1500000.00",
                CodImp1 = "01",
                ValImp1 = "285000.00",
                CodImp2 = "04",
                ValImp2 = "0.00",
                CodImp3 = "03",
                ValImp3 = "0.00",
                ValTot = "1785000.00",
                NitFE = "700085371",
                NumAdq = "800199436",
                ClTec = "693ff6f2a553c3646a063436fd4dd9ded0311471",
                TipoAmbie = "1",

            };

            dCufe.GenerarCUFE();
        }

        private void GenerarCUFE()
        {
            string composicion = NumFac + FecFac + HorFac + ValFac + CodImp1 +
                                ValImp1 + CodImp2 + ValImp2 + CodImp3 + ValImp3 +
                                ValTot + NitFE + NumAdq + ClTec + TipoAmbie;

            Console.WriteLine(composicion == "3232000001292019-01-1610:53:10-05:001500000.0001285000.00040.00030.001785000.00700085371800199436693ff6f2a553c3646a063436fd4dd9ded03114711");

            string cufe = SHA_384(composicion);


            Console.WriteLine(cufe == "8bb918b19ba22a694f1da11c643b5e9de39adf60311cf179179e9b33381030bcd4c3c3f156c506ed5908f9276f5bd9b4");
        }

        private string SHA_384(string cad)
        {
            string result = string.Empty;

            try
            {
                using (SHA384 sHA384 = SHA384.Create())
                {
                    byte[] sourceBytes = Encoding.UTF8.GetBytes(cad);
                    byte[] hashBytes = sHA384.ComputeHash(sourceBytes);
                    result = BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLower();

                    Console.WriteLine(result);
                }
            }
            catch (Exception e)
            {
                result = e.ToString();
            }

            return result;
        }
    }
}
