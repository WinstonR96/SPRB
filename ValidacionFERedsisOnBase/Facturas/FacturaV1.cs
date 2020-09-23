using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ValidacionFERedsisOnBase.Facturas
{
    [XmlRoot("Factura")]
    public class FacturaV1 : Factura
    {
        protected const string NIT_REDSIS = "802014278";
        protected const string NIT_REDSIS_C = "8020142780";
        protected const string NIT_REDSIS_C_ = "802.014.278.0";
        protected const string NIT_REDSIS_ = "802.014.278";
        protected const string NIT_REDSIS_CV = "802014278-0";
        protected const string NIT_REDSIS_CV_ = "802.014.278-0";

        public ControlFactura ControlFactura { get; set; } = new ControlFactura();
        public string OrigenFactura { get; set; } = string.Empty;
        public string FechaEmision { get; set; } = string.Empty;
        public string HoraEmision { get; set; } = string.Empty;
        public string TipoMoneda { get; set; } = string.Empty;
        public string PST { get; set; } = string.Empty;
        public string TotalFactura { get; set; } = string.Empty;
        public string BaseGravable { get; set; } = string.Empty;
        public TaxesInfo TaxesInfo { get; set; } = new TaxesInfo();
        public List<Item> Items { get; set; } = new List<Item>();

        public FacturaV1()
        {
            TipoFactura = VersionFactura.V1;
        }

        protected override bool GetDataFactura(XDocument xdoc, string pathTemp, List<string> nits, out string rejectionMessage)
        {
            rejectionMessage = string.Empty;
            string SaltoLinea = "<br/>";
            bool esValida = true;
            try
            {
                SetUBLVersion(xdoc);
                if (UBLVersion == string.Empty)
                {
                    rejectionMessage += "No se enontró la version UBL";
                    rejectionMessage += SaltoLinea;
                    esValida = false;
                }

                SetCUFE(xdoc);
                if (CUFE == string.Empty)
                {
                    rejectionMessage += "No se encontró el CUFE";
                    rejectionMessage += SaltoLinea;
                    esValida = false;
                }

                SetNumeroFactura(xdoc);
                if (NumFactura == string.Empty)
                {
                    rejectionMessage += "No se encontró el número de factura";
                    rejectionMessage += SaltoLinea;
                    esValida = false;
                }

                string errorProveedor = string.Empty;
                SetProveedor(xdoc, out errorProveedor);
                if (errorProveedor.Length > 0)
                {
                    rejectionMessage += errorProveedor;
                    rejectionMessage += SaltoLinea;
                    esValida = false;
                }

                string nit;
                if(!ValidateNitSPRB(xdoc, nits, out nit))
                {
                    if (nit == string.Empty)
                    {
                        rejectionMessage += "No se pudo validar el nit en la factura";
                        rejectionMessage += SaltoLinea;
                    }
                    else
                    {
                        rejectionMessage += $"Ningun nit no corresponde al de la factura '{nit}'";
                        rejectionMessage += SaltoLinea;
                    }

                    esValida = false;
                }

                //if (!ValidateNitRedsis(xdoc, out nit))
                //{
                //    if (nit == string.Empty)
                //        rejectionMessage = "No se pudo validar el nit de redsis en la factura";
                //    else
                //        rejectionMessage = $"El Nit de Redsis '{NIT_REDSIS}' no corresponde al de la factura '{nit}'";

                //    return false;
                //}

                SetControlFactura(xdoc);
                if (ControlFactura == null)
                {
                    rejectionMessage += "No se pudo obtener el control de la factura (InvoiceControl)";
                    rejectionMessage += SaltoLinea;
                    esValida = false;
                }

                SetOrigenFactura(xdoc);
                if (OrigenFactura == string.Empty)
                {
                    rejectionMessage += "No se encontró el origen de la factura";
                    rejectionMessage += SaltoLinea;
                    esValida = false;
                }

                SetFechaEmision(xdoc);
                if (FechaEmision == string.Empty)
                {
                    rejectionMessage += "No se encontró la Fecha de emisión de la factura";
                    rejectionMessage += SaltoLinea;
                    esValida = false;
                }

                SetHoraEmision(xdoc);
                if (HoraEmision == string.Empty)
                {
                    rejectionMessage += "No se encontró la hora de emisión de la factura";
                    rejectionMessage += SaltoLinea;
                    esValida = false;
                }

                SetTipoMoneda(xdoc);
                if (TipoMoneda == string.Empty)
                {
                    rejectionMessage += "No se encontró el tipo de Moneda";
                    rejectionMessage += SaltoLinea;
                    esValida = false;
                }

                SetPST(xdoc);
                if (PST == string.Empty)
                {
                    rejectionMessage += "No se encontró el PST";
                    rejectionMessage += SaltoLinea;
                    esValida = false;
                }

                SetTotalFactura(xdoc);
                if (TotalFactura == string.Empty)
                {
                    rejectionMessage += "No se encontró el total de la factura";
                    rejectionMessage += SaltoLinea;
                    esValida = false;
                }

                SetBaseGravable(xdoc);
                if (BaseGravable == string.Empty)
                {
                    rejectionMessage += "No se encontró la base gravable";
                    rejectionMessage += SaltoLinea;
                    esValida = false;
                }

                SetTaxesInfo(xdoc);
                if (TaxesInfo == null)
                {
                    rejectionMessage += "Es posible que no se haya encontrado ningún Taxtotal en la factura o que\n" +
                                        "no se haya podido obtener todas las propiedades para cada uno";
                    rejectionMessage += SaltoLinea;

                    esValida = false;
                }

                SetItems(xdoc);
                if (Items?.Count == 0)
                {
                    rejectionMessage += "Es posible que no se haya encontrado ningún item en la factura o que\n" +
                                        "no se haya podido obtener todas las propiedades para cada item";
                    rejectionMessage += SaltoLinea;

                    esValida = false;
                }

                SetObservaciones(xdoc);

                string errorCliente = string.Empty;
                SetCliente(xdoc, out errorCliente);
                if (errorCliente.Length > 0)
                {
                    rejectionMessage += errorCliente;
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

        private string QuitarCaracteresNit(string nit)
        {
            string patron = @"[^\w]";
            Regex regex = new Regex(patron);
            return regex.Replace(nit, "");
        }

        protected virtual bool ValidateNitSPRB(XDocument xdoc, List<string> nits, out string nit)
        {
            var customerNit = xdoc.Root.Elements().Where(e => e.Name.LocalName == "AccountingCustomerParty").SingleOrDefault()?
                            .Elements().Where(e => e.Name.LocalName == "Party").SingleOrDefault()?
                            .Elements().Where(e => e.Name.LocalName == "PartyIdentification").SingleOrDefault()?
                            .Elements().Where(e => e.Name.LocalName == "ID").SingleOrDefault();

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

        protected virtual void SetUBLVersion(XDocument xdoc)
        {
            UBLVersion = string.Empty;
            var ubl = xdoc.Root.Elements().Where(e => e.Name.LocalName == "UBLVersionID").SingleOrDefault();

            if (ubl != null)
                UBLVersion = ubl.Value;
        }

        protected virtual void SetCUFE(XDocument xdoc)
        {
            CUFE = string.Empty;
            var cufe = xdoc.Root.Elements().Where(e => e.Name.LocalName == "UUID").SingleOrDefault();
            if (cufe != null)
                CUFE = cufe.Value;
        }

        protected virtual void SetCliente(XDocument xdoc, out string errorCliente)
        {
            errorCliente = string.Empty;
            Cliente = null;
            var cliente = xdoc.Root.Elements().Where(e => e.Name.LocalName == "AccountingCustomerParty").SingleOrDefault()?
                    .Elements().Where(e => e.Name.LocalName == "Party").SingleOrDefault();

            if (cliente != null)
            {
                var nit = cliente
                    .Elements().Where(e => e.Name.LocalName == "PartyIdentification").SingleOrDefault()?
                    .Elements().Where(e => e.Name.LocalName == "ID").SingleOrDefault();

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

        protected virtual void SetNumeroFactura(XDocument xdoc)
        {
            NumFactura = string.Empty;
            var idElem = xdoc.Root.Elements().Where(e => e.Name.LocalName == "ID").SingleOrDefault();
            if (idElem != null)
                NumFactura = idElem.Value;
        }

        protected virtual void SetProveedor(XDocument xdoc, out string errorProveedor)
        {
            errorProveedor = string.Empty;
            Proveedor = null;
            var proveedor = xdoc.Root.Elements().Where(e => e.Name.LocalName == "AccountingSupplierParty").SingleOrDefault()?
                    .Elements().Where(e => e.Name.LocalName == "Party").SingleOrDefault();

            if (proveedor != null)
            {
                var nit = proveedor
                    .Elements().Where(e => e.Name.LocalName == "PartyIdentification").SingleOrDefault()?
                    .Elements().Where(e => e.Name.LocalName == "ID").SingleOrDefault();

                if (nit == null || nit.Value == string.Empty) errorProveedor += "No se encontro nit del proveedor";

                var nombre = proveedor
                    .Elements().Where(e => e.Name.LocalName == "PartyName").SingleOrDefault()?
                    .Elements().Where(e => e.Name.LocalName == "Name").SingleOrDefault();

                if (nombre == null || nombre.Value == string.Empty) errorProveedor += "No se encontro nombre del proveedor";

                var ciudad = proveedor
                    .Elements().Where(e => e.Name.LocalName == "PhysicalLocation").SingleOrDefault()?
                    .Elements().Where(e => e.Name.LocalName == "Address").SingleOrDefault()?
                    .Elements().Where(e => e.Name.LocalName == "CityName").SingleOrDefault();

                if (ciudad == null || ciudad.Value == string.Empty) errorProveedor += "No se encontro ciudad del proveedor";

                var dir = proveedor
                    .Elements().Where(e => e.Name.LocalName == "PhysicalLocation").SingleOrDefault()?
                    .Elements().Where(e => e.Name.LocalName == "Address").SingleOrDefault()?
                    .Elements().Where(e => e.Name.LocalName == "AddressLine").SingleOrDefault()?
                    .Elements().Where(e => e.Name.LocalName == "Line").SingleOrDefault();

                if (dir == null || dir.Value == string.Empty) errorProveedor += "No se encontro direccion del proveedor";


                Proveedor = new Proveedor
                {
                    Nit = (nit != null) ? nit.Value : "",
                    Nombre = (nombre != null) ? nombre.Value : "",
                    Ciudad = (ciudad != null) ? ciudad.Value : "",
                    Direccion = (dir != null) ? dir.Value : "",
                };

            }
        }

        protected virtual bool ValidateNitRedsis(XDocument xdoc, out string nit)
        {
            var customerNit = xdoc.Root.Elements().Where(e => e.Name.LocalName == "AccountingCustomerParty").SingleOrDefault()?
                            .Elements().Where(e => e.Name.LocalName == "Party").SingleOrDefault()?
                            .Elements().Where(e => e.Name.LocalName == "PartyIdentification").SingleOrDefault()?
                            .Elements().Where(e => e.Name.LocalName == "ID").SingleOrDefault();

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

        protected virtual void SetOrigenFactura(XDocument xdoc)
        {
            OrigenFactura = string.Empty;
            var origen = xdoc.Root.Elements().Where(e => e.Name.LocalName == "UBLExtensions").SingleOrDefault()?
                        .Descendants().Where(e => e.Name.LocalName == "DianExtensions").SingleOrDefault()?
                        .Elements().Where(e => e.Name.LocalName == "InvoiceSource").SingleOrDefault()?
                        .Elements().Where(e => e.Name.LocalName == "IdentificationCode").SingleOrDefault();

            if (origen != null)
                OrigenFactura = origen.Value;
        }

        protected virtual void SetFechaEmision(XDocument xdoc)
        {
            FechaEmision = string.Empty;
            var fecha = xdoc.Root.Elements().Where(e => e.Name.LocalName == "IssueDate").SingleOrDefault();
            if (fecha != null)
                FechaEmision = fecha.Value;
        }

        protected virtual void SetHoraEmision(XDocument xdoc)
        {
            HoraEmision = string.Empty;
            var hora = xdoc.Root.Elements().Where(e => e.Name.LocalName == "IssueTime").SingleOrDefault();
            if (hora != null)
                HoraEmision = hora.Value;
        }

        protected virtual void SetTipoMoneda(XDocument xdoc)
        {
            TipoMoneda = string.Empty;

            var tipoMoneda = xdoc.Root.Elements().Where(e => e.Name.LocalName == "DocumentCurrencyCode").SingleOrDefault();
            if (tipoMoneda != null)
                TipoMoneda = tipoMoneda.Value;
        }

        protected virtual void SetPST(XDocument xdoc)
        {
            PST = string.Empty;
            var pst = xdoc.Root.Elements().Where(e => e.Name.LocalName == "UBLExtensions").SingleOrDefault()?
                    .Descendants().Where(d => d.Name.LocalName == "DianExtensions").SingleOrDefault()?
                    //.Elements().Where(e => e.Name.LocalName == "UBLExtension").SingleOrDefault()?
                    //.Elements().Where(e => e.Name.LocalName == "ExtensionContent").SingleOrDefault()?
                    //.Elements().Where(e => e.Name.LocalName == "DianExtensions").SingleOrDefault()?
                    .Elements().Where(e => e.Name.LocalName == "SoftwareProvider").SingleOrDefault()?
                    .Elements().Where(e => e.Name.LocalName == "ProviderID").SingleOrDefault();

            if (pst != null)
                PST = pst.Value;
        }

        protected virtual void SetTotalFactura(XDocument xdoc)
        {
            TotalFactura = string.Empty;
            var total = xdoc.Root.Elements().Where(e => e.Name.LocalName == "LegalMonetaryTotal").SingleOrDefault()?
                .Elements().Where(e => e.Name.LocalName == "PayableAmount").SingleOrDefault();

            if (total != null)
                TotalFactura = total.Value;
        }

        protected virtual void SetBaseGravable(XDocument xdoc)
        {
            BaseGravable = string.Empty;
            var baseGravable = xdoc.Root.Elements().Where(e => e.Name.LocalName == "LegalMonetaryTotal").SingleOrDefault()?
                .Elements().Where(e => e.Name.LocalName == "LineExtensionAmount").SingleOrDefault();

            if (baseGravable != null)
                BaseGravable = baseGravable.Value;
        }

        protected virtual void SetControlFactura(XDocument xdoc)
        {
            ControlFactura = null;
            var control = xdoc.Root.Elements().Where(e => e.Name.LocalName == "UBLExtensions").SingleOrDefault()?
                        .Descendants().Where(e => e.Name.LocalName == "DianExtensions").SingleOrDefault()?
                        .Elements().Where(e => e.Name.LocalName == "InvoiceControl").SingleOrDefault();

            if (control != null)
            {
                var auth = control.Elements().Where(e => e.Name.LocalName == "InvoiceAuthorization").SingleOrDefault();
                if (auth == null || auth.Value == string.Empty) return;

                var startDate = control.Descendants().Where(e => e.Name.LocalName == "StartDate").SingleOrDefault();
                if (startDate == null || startDate.Value == string.Empty) return;

                var endDate = control.Descendants().Where(e => e.Name.LocalName == "EndDate").SingleOrDefault();
                if (endDate == null || endDate.Value == string.Empty) return;

                var prefix = control.Descendants().Where(e => e.Name.LocalName == "Prefix").SingleOrDefault();

                var from = control.Descendants().Where(e => e.Name.LocalName == "From").SingleOrDefault();
                if (from == null || from.Value == string.Empty) return;

                var to = control.Descendants().Where(e => e.Name.LocalName == "To").SingleOrDefault();
                if (to == null || to.Value == string.Empty) return;

                ControlFactura = new ControlFactura
                {
                    Autorizacion = auth.Value,
                    PeriodoAutorizacion = new PeriodoAutorizacion
                    {
                        FechaInicio = startDate.Value,
                        FechaFin = endDate.Value
                    },
                    FacturasAutorizadas = new FacturasAutorizadas
                    {
                        Prefijo = prefix != null ? prefix.Value : string.Empty,
                        RangoInicio = from.Value,
                        RangoFin = to.Value
                    }
                };
            }
        }

        protected virtual void SetTaxesInfo(XDocument xdoc)
        {
            TaxesInfo = null;

            var taxesList = GetTaxesTotales(xdoc);
            if (taxesList.Count == 0)
            {
                return;
            }

            TaxesInfo = new TaxesInfo
            {
                TaxesTotals = new List<TaxTotal>(taxesList.ToArray())
            };
        }

        protected virtual List<TaxTotal> GetTaxesTotales(XDocument xdoc)
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

                    var percent = subtotal.Elements().Where(e => e.Name.LocalName == "Percent").SingleOrDefault();

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

        protected virtual void SetItems(XDocument xdoc)
        {
            var items = xdoc.Root.Elements().Where(e => e.Name.LocalName == "InvoiceLine").ToArray();

            List<Item> _items = new List<Item>();
            bool error = false;
            foreach (var item in items)
            {
                var id = item.Elements().Where(e => e.Name.LocalName == "ID").SingleOrDefault();
                if (id == null || id.Value == string.Empty)
                {
                    error = true;
                    break;
                }

                var cantidad = item.Elements().Where(e => e.Name.LocalName == "InvoicedQuantity").SingleOrDefault();
                if (cantidad == null || cantidad.Value == string.Empty)
                {
                    error = true;
                    break;
                }

                var descripciones = item.Elements().Where(e => e.Name.LocalName == "Item").SingleOrDefault()?
                                .Elements().Where(e => e.Name.LocalName == "Description").ToArray();

                if (descripciones.Length == 0)
                {
                    error = true;
                    break;
                }

                string _desc = string.Empty;
                foreach (var d in descripciones)
                {
                    if (d.Value == string.Empty)
                    {
                        error = true;
                        break;
                    }
                    _desc += d.Value + "\n";
                }

                if (error)
                    break;

                var priceAmount = item.Elements().Where(e => e.Name.LocalName == "Price").SingleOrDefault()?
                                .Elements().Where(e => e.Name.LocalName == "PriceAmount").SingleOrDefault();
                if (priceAmount == null || priceAmount.Value == string.Empty)
                {
                    error = true;
                    break;
                }

                var lineExtensionAmount = item.Elements().Where(e => e.Name.LocalName == "LineExtensionAmount").SingleOrDefault();
                if (lineExtensionAmount == null || lineExtensionAmount.Value == string.Empty)
                {
                    error = true;
                    break;
                }

                Item i = new Item
                {
                    ID = id.Value,
                    Cantidad = cantidad.Value,
                    Descripcion = _desc,
                    PriceAmount = priceAmount.Value,
                    LineExtensionAmount = lineExtensionAmount.Value
                };

                _items.Add(i);
            }

            if (error)
                return;

            Items = new List<Item>(_items);
        }

        protected virtual void SetObservaciones(XDocument xdoc)
        {
            Observaciones = string.Empty;
            var obs = xdoc.Root.Elements().Where(e => e.Name.LocalName == "Note").ToArray();

            if (obs.Length > 0)
            {
                string todas = string.Empty;
                foreach (var nota in obs)
                {
                    todas += nota.Value + "\n";
                }

                Observaciones = todas;
            }
        }

        public override string ToString()
        {
            return $"Version de Factura: {TipoFactura}\n" +
                   $"Mail Message ID: {MailMessageID}\n" +
                   $"UBL Version : {UBLVersion}\n" +
                   $"CUFE: {CUFE}\n" +
                   $"# Factura: {NumFactura}\n" +
                   $"Notas: {Notas}\n" +
                   $"Observaciones: {Observaciones}\n" +
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

        protected override FacturaRechazada Rechazar(string rejectionMessage)
        {
            FacturaRechazada rechazada = new FacturaRechazada(rejectionMessage)
            {
                TipoFactura = TipoFactura,
                MailMessageID = MailMessageID,
                UBLVersion = UBLVersion,
                CUFE = CUFE,
                NumFactura = NumFactura,
                Proveedor = Proveedor,
                Observaciones = Observaciones,
                Cliente = Cliente,
                ControlFactura = ControlFactura,
                OrigenFactura = OrigenFactura,
                FechaEmision = FechaEmision,
                HoraEmision = HoraEmision,
                TipoMoneda = TipoMoneda,
                PST = PST,
                TotalFactura = TotalFactura,
                BaseGravable = BaseGravable,
                TaxesInfo = TaxesInfo,
                Items = Items,
            };
            return rechazada;
        }
    }
}
