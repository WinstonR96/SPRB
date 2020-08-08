using System.Collections.Generic;

namespace ValidacionFERedsisOnBase
{
    public class TaxesInfo
    {
        public string LineExtensionAmount { get; set; } = string.Empty;
        public string TaxExclusiveAmount { get; set; } = string.Empty;
        public string TaxInclusiveAmount { get; set; } = string.Empty;
        public string AllowanceTotalAmount { get; set; } = string.Empty;
        public string ChargeTotalAmount { get; set; } = string.Empty;
        public string PayableAmount { get; set; } = string.Empty;

        public List<TaxTotal> TaxesTotals { get; set; }
    }

    public class TaxTotal
    {
        public string TaxAmount { get; set; } = string.Empty;
        public List<TaxSubtotal> TaxesSubtotals { get; set; } = new List<TaxSubtotal>();
    }

    public class TaxSubtotal
    {
        public string TaxAmount { get; set; } = string.Empty;
        public TaxCategory TaxCategory { get; set; } = new TaxCategory();
    }

    public class TaxCategory
    {
        public string SchemeID { get; set; } = string.Empty;
        public string Percent { get; set; } = string.Empty;
    }
}
