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
    
    public partial class PRODUCT_UAN_LIST
    {
        public string BARCODE { get; set; }
        public string UAN_NO { get; set; }
        public Nullable<decimal> QTY { get; set; }
        public string TRANSFER_STATUS { get; set; }
        public Nullable<System.DateTime> UPDATE_DATE { get; set; }
        public string DEL_STATUS { get; set; }
    
        public virtual PRODUCT PRODUCT { get; set; }
    }
}
