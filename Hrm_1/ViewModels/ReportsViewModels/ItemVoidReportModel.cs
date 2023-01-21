using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inventory.ViewModels.ReportsViewModels
{
    public class ItemVoidReportModel
    {
        public string TRANS_NO { get; set; }
        public string USER_NAME { get; set; }
        public string DESCRIPTION { get; set; }
        public string BARCODE { get; set; }
        public DateTime? SCAN_TIME { get; set; }
        public decimal? UNIT_RETAIL { get; set; }
        public decimal? QTY { get; set; }
        public decimal? PRICE { get; set; }
        public string BRANCH { get; set; }
    }
}