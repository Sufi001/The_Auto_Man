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
    
    public partial class GRN_MAIN
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public GRN_MAIN()
        {
            this.GRN_DETAIL = new HashSet<GRN_DETAIL>();
        }
    
        public string GRN_NO { get; set; }
        public System.DateTime GRN_DATE { get; set; }
        public string PO_NO { get; set; }
        public string STATUS { get; set; }
        public string SUPL_CODE { get; set; }
        public string LOC_ID { get; set; }
        public System.DateTime DOC { get; set; }
        public string INSERTED_BY { get; set; }
        public Nullable<System.DateTime> UPDATE_DATE { get; set; }
        public string UPDATED_BY { get; set; }
        public string DISC_STATUS { get; set; }
        public string DOC_TYPE { get; set; }
        public Nullable<decimal> CASH { get; set; }
        public string JV_NO { get; set; }
        public string COMMENTS { get; set; }
        public string BILTY_NO { get; set; }
        public Nullable<decimal> BILTY_AMT { get; set; }
        public string DELIVERY_PLACE { get; set; }
        public string INV_NO { get; set; }
        public string SALESTAX { get; set; }
        public Nullable<decimal> DIS_AMT { get; set; }
        public Nullable<decimal> GST_AMT { get; set; }
        public Nullable<decimal> AMOUNT { get; set; }
        public string PAYMENT_TYPE { get; set; }
        public string WAREHOUSE { get; set; }
        public string TRANSFER_STATUS { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<GRN_DETAIL> GRN_DETAIL { get; set; }
        public virtual SUPPLIER SUPPLIER { get; set; }
    }
}
