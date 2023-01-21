using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inventory.ViewModels.ReportsViewModels
{
    public class PurchaseReportViewModel
    {
        public PurchaseReportMainViewModel ReportMain { get; set; }
        public List<PurchaseReportDetailViewModel> SummaryList { get; set; }
        public List<PurchaseReportDetailViewModel> DetailList { get; set; }
    }

    public class PurchaseReportMainViewModel
    {
        public string ReportName { get; set; }
        public string Suppliers { get; set; }
        public string SupplierCode { get; set; }
        public string ViewSuplierCode { get; set; }
        public string DepartmentCode { get; set; }
        public string ViewDepartmentCode { get; set; }
        public string btn { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public string Group_CODE { get; set; }
        public string SubGroup_CODE { get; set; }
        public string Loc_Code { get; set; }
        public IEnumerable<GROUP> Groups { get; set; }
        public IEnumerable<SUB_GROUPS> Sub_Groups { get; set; }
        public IEnumerable<LOCATION> Locations { get; set; }

    }
    public class PurchaseReportDetailViewModel
    {
        public string DepartmentCode { get; set; }
        public string DepartmentName { get; set; }
        public string Barcode { get; set; }
        public string SupplierCode { get; set; }
        public string Description { get; set; }
        public Decimal? UnitSize { get; set; }
        public Decimal? Qty { get; set; }
        public Decimal? Amount { get; set; }
        public Decimal? DiscAmt { get; set; }
        public Decimal? GstAmt { get; set; }
        public Decimal? NetAmt { get; set; }
        public Decimal? Cost { get; set; }
        public string SupplierName { get; set; }
        public Decimal? ActAmt { get; set; }
        public Decimal? FreeQty { get; set; }
        public Decimal? FreeQtyAmt { get; set; }
        public string Grn_Date { get; set; }
        public string Grn_No  { get; set; }
        public string GroupCode  { get; set; }
        public string SGroupCode  { get; set; }
        public DateTime grnDate { get; set; }
    }

}