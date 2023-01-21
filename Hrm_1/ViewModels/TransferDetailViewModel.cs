using System;

namespace Inventory.ViewModels
{
    public class TransferDetailViewModel
    {
        public string DocNo { get; set; }
        public string Barcode { get; set; }
        public string Uanno { get; set; }
        public decimal Cost { get; set; }
        public decimal Qty { get; set; }
        public Nullable<decimal> Usize { get; set; }
        public decimal Retail { get; set; }
        public string Description { get; set; }
        public string Warehouse { get; set; }
        public string WarehouseName { get; set; }
        public string TotalQty { get; set; }
        public string Totalamount { get; set; }
        //[Required]
        public int? Colour { get; set; }
        public string ColourName { get; set; }
        public decimal? InHandQty { get; set; }
    }
}