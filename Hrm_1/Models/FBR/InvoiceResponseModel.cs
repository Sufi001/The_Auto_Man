using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inventory.Models.FBR
{
    public class InvoiceResponseModel
    {
        public string InvoiceNumber { get; set; }
        public int Code { get; set; }
        public string Response { get; set; }
    }
}