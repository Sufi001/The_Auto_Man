using System;

namespace Inventory.ViewModels.Item
{
    public class ItemViewModel
    {
        public string Barcode { get; set; }
        public string Description { get; set; }
        public string UrduName { get; set; }
        public string Uanno { get; set; }
        public decimal? Cost { get; set; }
        public decimal? Retail { get; set; }
        public decimal? Ctnpcs { get; set; }
        public decimal? PackQty { get; set; }
        public decimal? PackRetail { get; set; }
        public decimal? UnitSize { get; set; }
        public decimal Qty { get; set; }
        public string ReferenceCode { get; set; }
        public string Rating { get; set; }
        public string Review { get; set; }
        public string Mail { get; set; }
        public string Status { get; set; }
        public string DocNo { get; set; }
        public DateTime DocDate { get; set; }
        public decimal? Discount { get; set; }
        public string Comments { get; set; }
        public string BranchId { get; set; }
        public decimal? CurrentQty { get; set; }

    }
}