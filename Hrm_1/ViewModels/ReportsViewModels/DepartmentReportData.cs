using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inventory.ViewModels.ReportsViewModels
{
    public class DepartmentReportData
    {
        public string DEPT_NAME { get; set; }
        public string GROUP_NAME { get; set; }
        public string SGROUP_NAME { get; set; }
        public string BARCODE { get; set; }
        public string DESCRIPTION { get; set; }
        public string TRANS_TYPE { get; set; }
        public decimal units_sold { get; set; }
        public decimal? cost_amount { get; set; }
        public decimal retail_amount { get; set; }
        public decimal? disc_amount { get; set; }
        public string net_amount { get; set; }
        public string net_margin { get; set; }
        public string priceoverridecount { get; set; }
        public string ItemsVoidCount { get; set; }
        public decimal priceoverrideamount { get; set; }
        public decimal VoidAmount { get; set; }
        public string DEPT_CODE { get; set; }
        public string GROUP_CODE { get; set; }
        public string SGROUP_CODE { get; set; }

    }
}