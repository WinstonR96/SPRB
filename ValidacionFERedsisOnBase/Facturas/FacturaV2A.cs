﻿using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ValidacionFERedsisOnBase.Facturas
{
    [XmlRoot("Factura")]
    public class FacturaV2A : FacturaV1
    {
        public string FechaVencimiento { get; set; } = string.Empty;
        public string NumOrdenCompra { get; set; } = string.Empty;

        public FacturaV2A()
        {
            TipoFactura = VersionFactura.V2A;
        }

        protected override bool GetDataFactura(XDocument xdoc, out string rejectionMessage)
        {
            string _rejectionMessage = string.Empty;
            if (!base.GetDataFactura(xdoc, out _rejectionMessage))
            {
                rejectionMessage = _rejectionMessage;
                return false;
            }

            SetFechaVencimiento(xdoc);
            //if (FechaVencimiento == string.Empty)
            //{
            //    rejectionMessage = "No se encontró la fecha de vencimiento de la factura";
            //    return false;
            //}

            SetNumOrdenCompra(xdoc);
            //if (NumOrdenCompra == string.Empty)
            //{
            //    rejectionMessage = "No se encontró el numero de la orden de compra";
            //    return false;
            //}

            rejectionMessage = string.Empty;
            return true;
        }

        protected override bool ValidateNitRedsis(XDocument xdoc, out string nit)
        {
            var customerNit = xdoc.Root.Elements().Where(e => e.Name.LocalName == "AccountingCustomerParty").SingleOrDefault()?
                            .Elements().Where(e => e.Name.LocalName == "Party").SingleOrDefault()?
                            .Elements().Where(e => e.Name.LocalName == "PartyLegalEntity").SingleOrDefault()?
                            .Elements().Where(e => e.Name.LocalName == "CompanyID").SingleOrDefault();

            if (customerNit != null && customerNit.Value != string.Empty)
            {
                nit = customerNit.Value.Trim();
                if (nit == NIT_REDSIS || nit == NIT_REDSIS_CV || nit == NIT_REDSIS_ || nit == NIT_REDSIS_CV_ || nit == NIT_REDSIS_C || nit == NIT_REDSIS_C_)
                    return true;
            }
            else
                nit = string.Empty;

            return false;
        }

        protected override void SetProveedor(XDocument xdoc)
        {
            Proveedor = null;
            var proveedor = xdoc.Root.Elements().Where(e => e.Name.LocalName == "AccountingSupplierParty").SingleOrDefault()?
                    .Elements().Where(e => e.Name.LocalName == "Party").SingleOrDefault();

            if (proveedor != null)
            {
                //cambio la ubicacion del nit
                var nit = proveedor
                    .Elements().Where(e => e.Name.LocalName == "PartyTaxScheme").SingleOrDefault()?
                    .Elements().Where(e => e.Name.LocalName == "CompanyID").SingleOrDefault();

                if (nit == null) return;

                var nombre = proveedor
                    .Elements().Where(e => e.Name.LocalName == "PartyTaxScheme").SingleOrDefault()?
                    .Elements().Where(e => e.Name.LocalName == "RegistrationName").SingleOrDefault();

                if (nombre == null) return;

                var ciudad = proveedor
                    .Elements().Where(e => e.Name.LocalName == "PhysicalLocation").SingleOrDefault()?
                    .Elements().Where(e => e.Name.LocalName == "Address").SingleOrDefault()?
                    .Elements().Where(e => e.Name.LocalName == "CityName").SingleOrDefault();

                string _dir = string.Empty;
                if (ciudad != null)
                {
                    var address = proveedor
                        .Elements().Where(e => e.Name.LocalName == "PhysicalLocation").SingleOrDefault()?
                        .Elements().Where(e => e.Name.LocalName == "Address").SingleOrDefault();

                    if (address == null) return;

                    var dirs = address.Elements().Where(e => e.Name.LocalName == "AddressLine").ToArray();

                    if (dirs.Length == 0) return;


                    foreach (var dir in dirs)
                    {
                        var dirValue = dir.Elements().Where(e => e.Name.LocalName == "Line").SingleOrDefault();

                        if (dirValue == null) return;

                        _dir += dirValue.Value + "\n";
                    }
                }


                Proveedor = new Proveedor
                {
                    Nit = nit.Value,
                    Nombre = nombre.Value,
                    Ciudad = ciudad != null ? ciudad.Value : string.Empty,
                    Direccion = _dir
                };

            }
        }

        protected override void SetTaxesInfo(XDocument xdoc)
        {
            bool taxExcIsZero = false;
            TaxesInfo = null;

            var taxes = xdoc.Root.Elements().Where(e => e.Name.LocalName == "LegalMonetaryTotal").SingleOrDefault();

            if (taxes == null)
                return;

            var lineExtensionAmount = taxes.Elements().Where(e => e.Name.LocalName == "LineExtensionAmount").SingleOrDefault();

            if (lineExtensionAmount == null) return;

            var taxExclusiveAmount = taxes.Elements().Where(e => e.Name.LocalName == "TaxExclusiveAmount").SingleOrDefault();

            if (taxExclusiveAmount == null) return;

            float value;
            if (!float.TryParse(taxExclusiveAmount.Value, out value)) return;

            taxExcIsZero = value == 0;


            var taxInclusiveAmount = taxes.Elements().Where(e => e.Name.LocalName == "TaxInclusiveAmount").SingleOrDefault();

            if (taxInclusiveAmount == null) return;

            var allowanceTotalAmount = taxes.Elements().Where(e => e.Name.LocalName == "AllowanceTotalAmount").SingleOrDefault();
            var chargeTotalAmount = taxes.Elements().Where(e => e.Name.LocalName == "ChargeTotalAmount").SingleOrDefault();

            var payableAmount = taxes.Elements().Where(e => e.Name.LocalName == "PayableAmount").SingleOrDefault();

            if (payableAmount == null) return;


            var taxesList = GetTaxesTotales(xdoc);
            if (!taxExcIsZero && taxesList.Count == 0)
            {
                return;
            }

            TaxesInfo = new TaxesInfo
            {
                LineExtensionAmount = lineExtensionAmount.Value,
                TaxExclusiveAmount = taxExclusiveAmount.Value,
                TaxInclusiveAmount = taxInclusiveAmount.Value,
                AllowanceTotalAmount = allowanceTotalAmount?.Value,
                ChargeTotalAmount = chargeTotalAmount?.Value,
                PayableAmount = payableAmount.Value,

                TaxesTotals = new List<TaxTotal>(taxesList.ToArray())

            };
        }

        protected override List<TaxTotal> GetTaxesTotales(XDocument xdoc)
        {
            var taxes = xdoc.Root.Elements().Where(e => e.Name.LocalName == "TaxTotal").ToArray();

            List<TaxTotal> _taxesTotales = new List<TaxTotal>();
            bool error = false;
            foreach (var tax in taxes)
            {
                List<TaxSubtotal> _subtotals = new List<TaxSubtotal>();
                string _taxAmount = string.Empty;

                var subtotals = tax.Elements().Where(e => e.Name.LocalName == "TaxSubtotal").ToArray();

                foreach (var subtotal in subtotals)
                {
                    //TaxCategory _categories 
                    string _schemeId = string.Empty;
                    string _percent = string.Empty;

                    var category = subtotal.Elements().Where(e => e.Name.LocalName == "TaxCategory").SingleOrDefault();

                    if (category == null)
                    {
                        error = true;
                        break;
                    }

                    var schemeId = category.Elements().Where(e => e.Name.LocalName == "TaxScheme").SingleOrDefault()?
                                .Elements().Where(e => e.Name.LocalName == "ID").SingleOrDefault();

                    if (schemeId == null || schemeId.Value == string.Empty)
                    {
                        error = true;
                        break;
                    }

                    var percent = category.Elements().Where(e => e.Name.LocalName == "Percent").SingleOrDefault();

                    if (percent == null || percent.Value == string.Empty)
                    {
                        error = true;
                        break;
                    }

                    _schemeId = schemeId.Value;
                    _percent = percent.Value;

                    TaxCategory c = new TaxCategory
                    {
                        SchemeID = _schemeId,
                        Percent = _percent
                    };

                    string _subtotalTaxAmount = string.Empty;
                    var subTotaltaxAmount = subtotal.Elements().Where(e => e.Name.LocalName == "TaxAmount").SingleOrDefault();

                    if (subTotaltaxAmount == null || subTotaltaxAmount.Value == string.Empty)
                    {
                        error = true;
                        break;
                    }

                    _subtotalTaxAmount = subTotaltaxAmount.Value;

                    TaxSubtotal s = new TaxSubtotal
                    {
                        TaxCategory = c,
                        TaxAmount = _subtotalTaxAmount
                    };

                    _subtotals.Add(s);
                }

                if (error)
                    break;

                var taxAmount = tax.Elements().Where(e => e.Name.LocalName == "TaxAmount").SingleOrDefault();

                if (taxAmount == null || taxAmount.Value == string.Empty)
                {
                    error = true;
                    break;
                }

                _taxAmount = taxAmount.Value;

                TaxTotal t = new TaxTotal
                {
                    TaxesSubtotals = _subtotals,
                    TaxAmount = _taxAmount
                };

                _taxesTotales.Add(t);
            }

            if (error)
                _taxesTotales.Clear();

            return _taxesTotales;
            //TaxesTotales = new List<TaxTotal>(_taxesTotales);
        }

        protected virtual void SetFechaVencimiento(XDocument xdoc)
        {
            FechaVencimiento = string.Empty;
            var fecha = xdoc.Root.Elements().Where(e => e.Name.LocalName == "DueDate").SingleOrDefault();
            if (fecha != null)
                FechaVencimiento = fecha.Value;
        }

        protected virtual void SetNumOrdenCompra(XDocument xdoc)
        {
            NumOrdenCompra = string.Empty;
            var orden = xdoc.Root.Elements().Where(e => e.Name.LocalName == "OrderReference").SingleOrDefault()?
                    .Elements().Where(e => e.Name.LocalName == "ID").SingleOrDefault();

            if (orden != null)
                NumOrdenCompra = orden.Value;
        }
    }
}
