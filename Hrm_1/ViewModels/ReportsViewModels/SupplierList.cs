using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inventory.ViewModels.ReportsViewModels
{
    public class SupplierList
    {

        public  string Sup_Code { get; set; }
        public string SupplierName { get; set; }
        public string Barcode { get; set; }
        public decimal? Gst_Amt { get; set; }
        public decimal? ActAmt { get; set; }
        public decimal? DisAmt { get; set; }

        public DateTime DOC { get; set; }
        public decimal? Qty { get; set; }
        public decimal? Unitsize { get; set; }
        public decimal Cost { get; set; }
        public string Description { get; set; }

    }
}