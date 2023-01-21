using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inventory.ViewModels.ReportsViewModels
{
    public class SupplierBalance
    {
        public string SUPL_CODE { get; set; }
        public string SUPL_NAME { get; set; }
        public string NARRATION { get; set; }
        public decimal? PAYMENT { get; set; }
        public decimal? RECEIPT { get; set; }
        public decimal? DISCOUNT { get; set; }
        public decimal? NET_BALANCE { get; set; }
        public decimal? BALANCE { get; set; }
        public decimal? OPENING_BALANCE { get; set; }
        public string BALANCE_TYPE { get; set; }
        public string INVOICE_NO { get; set; }
        public decimal? SUPL_TOTAL_BALANCE { get; set; }
        public DateTime? DATE { get; set; }
    }
}