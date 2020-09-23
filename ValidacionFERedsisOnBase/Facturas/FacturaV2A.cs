using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

        protected override bool GetDataFactura(XDocument xdoc, string pathTemp, List<string> nits, out string rejectionMessage)
        {
            rejectionMessage = string.Empty;
            string SaltoLinea = "<br/>";
            bool esValida = true;
            try
            {
                string _rejectionMessage = string.Empty;
                if (!base.GetDataFactura(xdoc, pathTemp, nits, out _rejectionMessage))
                {
                    rejectionMessage = _rejectionMessage;
                    esValida = false;
                }

                _rejectionMessage = string.Empty;
                SetFechaVencimiento(xdoc, out _rejectionMessage);
                if (_rejectionMessage.Length > 0)
                {
                    rejectionMessage += _rejectionMessage;
                    rejectionMessage += SaltoLinea;
                    esValida = false;
                }

                SetNumOrdenCompra(xdoc);
                if (NumOrdenCompra == string.Empty)
                {
                    rejectionMessage += "No se encontró el numero de la orden de compra";
                    rejectionMessage += SaltoLinea;
                    esValida = false;
                }

                //rejectionMessage = string.Empty;
                return esValida;
            }
            catch(Exception ex)
            {
                throw ex;
            }
            
        }

        protected override FacturaRechazada Rechazar(string rejectionMessage)
        {
            
            FacturaRechazada rechazada = base.Rechazar(rejectionMessage);
            rechazada.FechaVencimiento = FechaVencimiento;
            rechazada.NumOrdenCompra = NumOrdenCompra;
            return rechazada;
        }

        private string QuitarCaracteresNit(string nit)
        {
            string patron = @"[^\w]";
            Regex regex = new Regex(patron);
            return regex.Replace(nit, "");
        }

        protected override bool ValidateNitSPRB(XDocument xdoc, List<string> nits, out string nit)
        {
            var customerNit = xdoc.Root.Elements().Where(e => e.Name.LocalName == "AccountingCustomerParty").SingleOrDefault()?
                            .Elements().Where(e => e.Name.LocalName == "Party").SingleOrDefault()?
                            .Elements().Where(e => e.Name.LocalName == "PartyLegalEntity").SingleOrDefault()?
                            .Elements().Where(e => e.Name.LocalName == "CompanyID").SingleOrDefault();

            if (customerNit != null && customerNit.Value != string.Empty)
            {
                nit = QuitarCaracteresNit(customerNit.Value.Trim());
                foreach (var nitSPRB in nits)
                {

                    if (QuitarCaracteresNit(nitSPRB).Contains(nit))
                        return true;
                }
            }
            else
                nit = string.Empty;

            return false;
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

        protected override void SetProveedor(XDocument xdoc, out string errorProveedor)
        {
            errorProveedor = string.Empty;
            Proveedor = null;
            var proveedor = xdoc.Root.Elements().Where(e => e.Name.LocalName == "AccountingSupplierParty").SingleOrDefault()?
                    .Elements().Where(e => e.Name.LocalName == "Party").SingleOrDefault();

            if (proveedor != null)
            {
                //cambio la ubicacion del nit
                var nit = proveedor
                    .Elements().Where(e => e.Name.LocalName == "PartyTaxScheme").SingleOrDefault()?
                    .Elements().Where(e => e.Name.LocalName == "CompanyID").SingleOrDefault();

                if (nit == null) errorProveedor += "No se encontro nit del proveedor";

                var nombre = proveedor
                    .Elements().Where(e => e.Name.LocalName == "PartyTaxScheme").SingleOrDefault()?
                    .Elements().Where(e => e.Name.LocalName == "RegistrationName").SingleOrDefault();

                if (nombre == null) errorProveedor += "No se encontro nombre del proveedor";

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

                    if (address == null) errorProveedor += "No se encontro direccion del proveedor";

                    var dirs = address.Elements().Where(e => e.Name.LocalName == "AddressLine").ToArray();

                    if (dirs.Length == 0) errorProveedor += "No se encontro direccion del proveedor";


                    foreach (var dir in dirs)
                    {
                        var dirValue = dir.Elements().Where(e => e.Name.LocalName == "Line").SingleOrDefault();

                        if (dirValue == null) errorProveedor += "No se encontro direccion del proveedor";

                        _dir += dirValue.Value + "\n";
                    }
                }


                Proveedor = new Proveedor
                {
                    Nit = (nit != null) ? nit.Value : "",
                    Nombre = (nombre != null) ? nombre.Value : "",
                    Ciudad = (ciudad != null) ? ciudad.Value : "",
                    Direccion = (!string.IsNullOrEmpty(_dir)) ? _dir : "",
                };

            }
        }

        protected override void SetCliente(XDocument xdoc, out string errorCliente)
        {
            errorCliente = string.Empty;
            Cliente = null;
            var cliente = xdoc.Root.Elements().Where(e => e.Name.LocalName == "AccountingCustomerParty").SingleOrDefault()?
                    .Elements().Where(e => e.Name.LocalName == "Party").SingleOrDefault();

            if (cliente != null)
            {
                var nit = cliente
                    .Elements().Where(e => e.Name.LocalName == "PartyTaxScheme").SingleOrDefault()?
                    .Elements().Where(e => e.Name.LocalName == "CompanyID").SingleOrDefault();

                if (nit == null || nit.Value == string.Empty) errorCliente += "No se encontro el nit del cliente";

                var nombre = cliente
                    .Elements().Where(e => e.Name.LocalName == "PartyName").SingleOrDefault()?
                    .Elements().Where(e => e.Name.LocalName == "Name").SingleOrDefault();

                if (nombre == null || nombre.Value == string.Empty) errorCliente += "No se encontro el nombre del cliente";

                var ciudad = cliente
                    .Elements().Where(e => e.Name.LocalName == "PhysicalLocation").SingleOrDefault()?
                    .Elements().Where(e => e.Name.LocalName == "Address").SingleOrDefault()?
                    .Elements().Where(e => e.Name.LocalName == "CityName").SingleOrDefault();

                if (ciudad == null || ciudad.Value == string.Empty) errorCliente += "No se encontro la ciudad del cliente";

                var dir = cliente
                    .Elements().Where(e => e.Name.LocalName == "PhysicalLocation").SingleOrDefault()?
                    .Elements().Where(e => e.Name.LocalName == "Address").SingleOrDefault()?
                    .Elements().Where(e => e.Name.LocalName == "AddressLine").SingleOrDefault()?
                    .Elements().Where(e => e.Name.LocalName == "Line").SingleOrDefault();

                if (dir == null || dir.Value == string.Empty) errorCliente += "No se encontro la dirección del cliente";


                Cliente = new Cliente
                {
                    Nit = (nit != null) ? nit.Value : "",
                    Nombre = (nombre != null) ? nombre.Value : "",
                    Ciudad = (ciudad != null) ? ciudad.Value : "",
                    Direccion = (dir != null) ? dir.Value : "",
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

        protected virtual void SetFechaVencimiento(XDocument xdoc, out string errorFechaVencimiento)
        {
            errorFechaVencimiento = string.Empty;
            FechaVencimiento = string.Empty;
            string annioAux = "1920";
            var fecha = xdoc.Root.Elements().Where(e => e.Name.LocalName == "DueDate").SingleOrDefault();
            if (fecha != null)
            {
                string fv = fecha.Value;
                string annio = fv.Substring(0, 4);
                string mes = fv.Substring(5, 2);
                string dia = fv.Substring(8, 2);
                if (int.Parse(annio) < 1754)
                {
                    errorFechaVencimiento = $"el año: {annio} es invalido, se asigno el año: {annioAux} de manera preventiva";
                    annio = annioAux;
                }
                FechaVencimiento = annio+"-"+mes+"-"+dia;
            }
            else
            {
                errorFechaVencimiento = "No se encontró la fecha de vencimiento de la factura";
            }
        }

        protected virtual void SetNumOrdenCompra(XDocument xdoc)
        {
            NumOrdenCompra = string.Empty;
            var orden = xdoc.Root.Elements().Where(e => e.Name.LocalName == "OrderReference").SingleOrDefault()?
                    .Elements().Where(e => e.Name.LocalName == "ID").SingleOrDefault();

            if (orden != null)
                NumOrdenCompra = orden.Value;
        }

        public override string ToString()
        {
            return $"Version de Factura: {TipoFactura}\n" +
                   $"Mail Message ID: {MailMessageID}\n" +
                   $"UBL Version : {UBLVersion}\n" +
                   $"CUFE: {CUFE}\n" +
                   $"# Factura: {NumFactura}\n" +
                   $"Observaciones: {Observaciones}\n" +
                   $"Notas: {Notas}\n" +
                   $"Fecha vencimiento: {FechaVencimiento}\n" +
                   $"Nro Orden de compra: {NumOrdenCompra}\n" +
                   $"Tipo Moneda: {TipoMoneda}\n" +
                   $"Origen Factura: {OrigenFactura}\n" +
                   $"Fecha Emision: {FechaEmision}\n" +
                   $"Hora Emision: {HoraEmision}\n" +
                   $"Control Factura: \n{ControlFactura}\n" +
                   $"Total Factura: {TotalFactura}\n" +
                   $"Base Gravable: {BaseGravable}\n" +
                   $"PST: {PST}\n" +
                   $"Cliente: \n{Cliente}\n" +
                   $"Proveedor: \n{Proveedor}\n" +
                   $"Taxes Info: \n{TaxesInfo}\n" +
                   $"Items: {Items.Count}\n";
        }
    }
}
