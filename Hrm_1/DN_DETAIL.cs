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
    
    public partial class DN_DETAIL
    {
        public string DN_NO { get; set; }
        public string BARCODE { get; set; }
        public string UAN_NO { get; set; }
        public decimal UNITS_SOLD { get; set; }
        public decimal UNIT_RETAIL { get; set; }
        public decimal UNIT_COST { get; set; }
        public decimal AMOUNT { get; set; }
        public string VOID { get; set; }
        public string EXCH_FLAG { get; set; }
        public Nullable<decimal> UNIT_DISC { get; set; }
        public decimal GST_AMOUNT { get; set; }
        public string PRICE_OVR { get; set; }
        public string DEAL_CODE { get; set; }
        public Nullable<decimal> CTN_QTY { get; set; }
        public Nullable<decimal> ITEM_DIS { get; set; }
        public Nullable<decimal> BULK_RETAIL { get; set; }
        public Nullable<int> DEAL_COUNT { get; set; }
        public Nullable<decimal> FREE_QTY { get; set; }
        public string MODEL { get; set; }
        public Nullable<decimal> DIS_AMOUNT { get; set; }
        public string DESCRIPTION { get; set; }
        public string S_NO { get; set; }
        public string SUPL_CODE { get; set; }
        public Nullable<decimal> OLD_COST { get; set; }
        public Nullable<decimal> OLD_RETAIL { get; set; }
        public Nullable<System.DateTime> S_TIME { get; set; }
        public Nullable<System.DateTime> E_TIME { get; set; }
        public Nullable<decimal> SERVICE_AMT { get; set; }
        public string PACKING { get; set; }
        public string BILL_CHECK { get; set; }
        public string PAGE_SIZE { get; set; }
        public string PAPER_GSM { get; set; }
        public string WAREHOUSE { get; set; }
        public Nullable<int> COLOUR { get; set; }
        public string TRANSFER_STATUS { get; set; }
    
        public virtual DN_MAIN DN_MAIN { get; set; }
        public virtual PRODUCT PRODUCT { get; set; }
    }
}
