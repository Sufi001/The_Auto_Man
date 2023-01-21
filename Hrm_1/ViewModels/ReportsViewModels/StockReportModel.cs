using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inventory.ViewModels.ReportsViewModels
{
    public class StockReportModel
    {
        public string BARCODE { get; set; }
        public decimal? GRN_QTY { get; set; }
        public decimal GRN_AMOUNT { get; set; }
        public decimal? GRF_QTY { get; set; }
        public decimal GRF_AMOUNY { get; set; }
        public decimal? SALE_QTY { get; set; }
        public decimal SALE_AMOUNT { get; set; }
        public decimal? TRANSFER_IN_QTY { get; set; }
        public decimal TRANSFER_IN_AMOUNT { get; set; }
        public decimal? TRANSFER_OUT_QTY { get; set; }
        public decimal TRANSFER_OUT_AMOUNT { get; set; }
        public string ItemName { get; set; }
        public decimal? UNIT_RETAIL { get; set; }
        public string DEPT_NAME { get; set; }
        public string GROUP_NAME { get; set; }
        public string SGROUP_NAME { get; set; }
        public string DEPT_CODE { get; set; }
        public string GROUP_CODE { get; set; }
        public string SRGOUP_CODE { get; set; }
        public string SUPL_NAME { get; set; }
        public string SUPL_CODE { get; set; }
        public decimal? CURRENT_QTY { get; set; }
        public decimal? CURRENT_amount { get; set; }
        public decimal? OPEN_QTY { get; set; }
        public decimal? OPEN_COST { get; set; }
        public decimal? WAST_QTY { get; set; }
        public decimal? WAST_AMOUNT { get; set; }
        public decimal? LOSS_QTY { get; set; }
        public decimal? LOSS_AMOUNT { get; set; }
        public decimal? FREE_UNIT_IN_QTY { get; set; }
        public decimal? FREE_UNIT_IN_VALUE { get; set; }
        public decimal? FREE_UNIT_OUT_QTY { get; set; }
        public decimal? FREE_UNIT_OUT_VALUE { get; set; }
        public string BRANCH_ID { get; set; }
        public string BRANCH_NAME { get; set; }
        public decimal? B1 { get; set; }
        public decimal? B2 { get; set; }
        public decimal? B3 { get; set; }
        public decimal? B1VALUE { get; set; }
        public decimal? B2VALUE { get; set; }
        public decimal? B3VALUE { get; set; }
        public decimal? BRANCH_TOTAL { get; set; }
        public decimal? BRANCH_TOTAL_VALUE { get; set; }

        public decimal? GAIN_QTY { get; set; }
        public decimal? GAIN_AMOUNT { get; set; }
    }
}