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
    
    public partial class PROD_BALANCE
    {
        public string BARCODE { get; set; }
        public Nullable<decimal> OPEN_QTY { get; set; }
        public Nullable<decimal> OPEN_COST { get; set; }
        public Nullable<decimal> GRN_QTY { get; set; }
        public Nullable<decimal> GRN_AMOUNT { get; set; }
        public Nullable<decimal> GRF_QTY { get; set; }
        public Nullable<decimal> GRF_AMOUNY { get; set; }
        public Nullable<decimal> WAST_QTY { get; set; }
        public Nullable<decimal> WAST_AMOUNT { get; set; }
        public Nullable<decimal> GAIN_QTY { get; set; }
        public Nullable<decimal> GAIN_AMOUNT { get; set; }
        public Nullable<decimal> SALE_QTY { get; set; }
        public Nullable<decimal> SALE_AMOUNT { get; set; }
        public Nullable<decimal> CURRENT_QTY { get; set; }
        public Nullable<decimal> AVG_COST { get; set; }
        public Nullable<decimal> LAST_COST { get; set; }
        public Nullable<decimal> TRANSFER_IN_QTY { get; set; }
        public Nullable<decimal> TRANSFER_IN_AMOUNT { get; set; }
        public Nullable<decimal> TRANSFER_OUT_QTY { get; set; }
        public Nullable<decimal> TRANSFER_OUT_AMOUNT { get; set; }
        public string TRANSFER_STATUS { get; set; }
        public Nullable<System.DateTime> UPDATE_DATE { get; set; }
    
        public virtual PRODUCT PRODUCT { get; set; }
    }
}