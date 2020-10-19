namespace WsSoap
{
    public static class Api
    {
        public static string ObtenerEmail(string nit)
        {
            string email = "";
            SPRBWs.ZWS_ONBASE zWS_ONBASE = new SPRBWs.ZWS_ONBASE();
            SPRBWs.Zmodf0001 zmodf0001 = new SPRBWs.Zmodf0001();
            zmodf0001.Nit = nit;
            var response = zWS_ONBASE.Zmodf0001(zmodf0001);
            email = response.Email;
            return email;
        }
    }
}
