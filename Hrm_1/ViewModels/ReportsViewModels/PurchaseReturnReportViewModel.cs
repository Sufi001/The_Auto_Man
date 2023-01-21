using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inventory.ViewModels.ReportsViewModels
{
    public class PurchaseReturnReportViewModel
    {
        public string DOC_NO { get; set; }
        public string SUPL_NAME { get; set; }
        public string BARCODE { get; set; }
        public string DESCRIPTION { get; set; }
        public string UNIT_SIZE { get; set; }
        public decimal? COST { get; set; }
        public decimal? UNIT_RETAIL { get; set; }
        public decimal? REORDER_LVL { get; set; }
        public DateTime? EXPIRY_DATE { get; set; }
        public decimal? DISC { get; set; }
        public decimal? CURRENT_QTY { get; set; }
        public decimal? QTY { get; set; }
        public string SGROUP_NAME { get; set; }
        public string DEPT_NAME { get; set; }
        public string GROUP_NAME { get; set; }
        public decimal TRANSFER_OUT_QTY { get; set; }
        public decimal TRANSFER_IN_QTY { get; set; }
        public int SALE_QTY { get; set; }
        public string location { get; set; }
        public decimal? UNIT_COST { get; set; }
        public string DEPT_CODE { get; set; }
        public string GROUP_CODE { get; set; }
        public string SGROUP_CODE { get; set; }
        public string SUPL_CODE { get; set; }
        public decimal? GST_AMOUNT { get; set; }
        public string BRANCH_FROM { get; set; }
        public string BRANCH_TO { get; set; }
    }
}