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
    
    public partial class INV_STOCK_TAKE_ENTRY
    {
        public int STOCK_ID { get; set; }
        public Nullable<System.DateTime> STOCK_DTE { get; set; }
        public string BARCODE { get; set; }
        public Nullable<decimal> SHEET_NO { get; set; }
        public Nullable<System.DateTime> ENTRY_DTE { get; set; }
        public Nullable<decimal> UNIT_COST { get; set; }
        public Nullable<decimal> UNIT_RETAIL { get; set; }
        public Nullable<decimal> QTY { get; set; }
    }
}
