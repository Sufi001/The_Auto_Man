//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Inventory
{
    using System;
    using System.Collections.Generic;
    
    public partial class GRFS_DETAIL
    {
        public string GRF_NO { get; set; }
        public string BARCODE { get; set; }
        public decimal COST { get; set; }
        public Nullable<decimal> DISCOUNT { get; set; }
        public Nullable<decimal> QTY { get; set; }
        public Nullable<decimal> GST_AMOUNT { get; set; }
        public Nullable<int> COLOUR { get; set; }
        public string WAREHOUSE { get; set; }
        public string TRANSFER_STATUS { get; set; }
        public Nullable<decimal> FREE_QTY { get; set; }
    
        public virtual GRFS_MAIN GRFS_MAIN { get; set; }
        public virtual PRODUCT PRODUCT { get; set; }
    }
}
