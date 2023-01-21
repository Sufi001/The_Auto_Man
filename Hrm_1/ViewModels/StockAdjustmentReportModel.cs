using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inventory.ViewModels
{
    public class StockAdjustmentReportModel
    {
        public string DocNo { get; set; }
        public DateTime DocDate { get; set; }
        public string DeptCode { get; set; }
        public string DeptName { get; set; }
        public string Barcode { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public decimal Qty { get; set; }
        public decimal? Cost { get; set; }
        public decimal Amount { get; set; }
        public string BranchId { get; set; }
        public string BranchName { get; set; }


    }
}