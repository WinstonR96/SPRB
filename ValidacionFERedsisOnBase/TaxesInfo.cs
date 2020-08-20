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

        public override string ToString()
        {
            return $"Line Extension Amount: {LineExtensionAmount}\n" +
                   $"Tax Exclusive Amount: {TaxExclusiveAmount}\n" +
                   $"Tax Inclusive Amount: {TaxInclusiveAmount}\n" +
                   $"Allowance Total Amount: {AllowanceTotalAmount}\n" +
                   $"Charge total Amount: {ChargeTotalAmount}\n" +
                   $"Payable Amount: {PayableAmount}\n" +
                   $"Taxes Totals: {TaxesTotals.Count}";
        }
    }

    public class TaxTotal
    {
        public string TaxAmount { get; set; } = string.Empty;
        public List<TaxSubtotal> TaxesSubtotals { get; set; } = new List<TaxSubtotal>();

        public override string ToString()
        {
            return $"Tax Amount: {TaxAmount}\n" +
                   $"Taxes Subtotals: {TaxesSubtotals.Count}";
        }
    }

    public class TaxSubtotal
    {
        public string TaxAmount { get; set; } = string.Empty;
        public TaxCategory TaxCategory { get; set; } = new TaxCategory();

        public override string ToString()
        {
            return $"Tax Amount: {TaxAmount}\n" +
                   $"Tax Category: {TaxCategory}";
        }
    }

    public class TaxCategory
    {
        public string SchemeID { get; set; } = string.Empty;
        public string Percent { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"SchemeId: {SchemeID}\n" +
                   $"Percent: {Percent}";
        }
    }
}
