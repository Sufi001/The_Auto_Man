namespace Inventory.ViewModels.ReportsViewModels
{
    public class ReportDetailViewModel
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public decimal Qty { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        public string GrossAmount { get; set; }
        public string TotalUnits { get; set; }
        public string PartyType { get; set; }
        public string Warehouse { get; set; }
        public decimal Discount { get; set; }
    }
}