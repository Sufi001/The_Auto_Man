using System;

namespace Inventory.ViewModels
{
    public class DocumentDetailViewModel
    {
        public string DocNo { get; set; }
        public string Barcode { get; set; }
        public string Uanno { get; set; }
        public decimal Cost { get; set; }
        public decimal Qty { get; set; }
        public decimal Usize { get; set; }
        public decimal Discount { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal Amount { get; set; }
        public decimal Retail { get; set; }
        public decimal? ProductPackRetail { get; set; }
        public decimal? ProductPackQty { get; set; }
        public decimal? ProductUnitRetail { get; set; }
        public decimal? FreeQty { get; set; }
        public decimal? CtnPcs { get; set; }
        public decimal? CtnQty { get; set; }
        public string Description { get; set; }
        public string Totalamount { get; set; }
        public string TotalQty { get; set; }
        //[Required]
        public string Warehouse { get; set; }
        public string WarehouseName { get; set; }
        //[Required]
        public int? Colour { get; set; }
        public string ColourName { get; set; }
        public string staffcode { get; set; }
        //public string DateTimeSlot { get; set; }
        public DateTime? DateTimeSlot { get; set; }
        public decimal? QtyinHand { get; set; }

    }
}