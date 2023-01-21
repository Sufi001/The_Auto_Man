using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inventory.ViewModels.ReportsViewModels
{
    public class SalesBySupplierReportModel
    {
        public string SupplierName { get; set; }
        public string SUPL_CODE { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public decimal? Qty { get; set; }
        public decimal? Unit_Cost { get; set; }
        public decimal? Rate { get; set; }
        public decimal? Discount { get; set; }
        public string DocumentNo { get; set; }
        public DateTime DocumentDate { get; set; }
        public decimal? Unit_Retail { get; set; }
        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public bool VisCondition { get; set; }
        public string SalesMan { get; set; }

    }
}