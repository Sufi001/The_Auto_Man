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
    
    public partial class SUPPLIER
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SUPPLIER()
        {
            this.GRFS_MAIN = new HashSet<GRFS_MAIN>();
            this.GRN_MAIN = new HashSet<GRN_MAIN>();
            this.JOURNALs = new HashSet<JOURNAL>();
            this.RESERVATION_MAIN = new HashSet<RESERVATION_MAIN>();
            this.STOCK_MAIN = new HashSet<STOCK_MAIN>();
            this.SUPPLIER_PRODUCTS = new HashSet<SUPPLIER_PRODUCTS>();
            this.TICKET_MAIN = new HashSet<TICKET_MAIN>();
            this.TRANS_MN = new HashSet<TRANS_MN>();
        }
    
        public string SUPL_CODE { get; set; }
        public string SUPL_NAME { get; set; }
        public string ADDRESS { get; set; }
        public string CONTACT_PERSON { get; set; }
        public string PHONE { get; set; }
        public string FAX { get; set; }
        public string PHONE2 { get; set; }
        public string MOBILE { get; set; }
        public string EMAIL { get; set; }
        public string STATUS { get; set; }
        public string LOCATION { get; set; }
        public string STN_NO { get; set; }
        public string NTN_NO { get; set; }
        public string GST_TYPE { get; set; }
        public string PAY_TERMS { get; set; }
        public Nullable<decimal> PAY_DAYS { get; set; }
        public System.DateTime DOC { get; set; }
        public string INSERTED_BY { get; set; }
        public Nullable<System.DateTime> UPDATE_DATE { get; set; }
        public string UPDATED_BY { get; set; }
        public Nullable<decimal> BALANCE { get; set; }
        public string PARTY_TYPE { get; set; }
        public Nullable<int> DISC_CATEGORY_ID { get; set; }
        public Nullable<System.DateTime> DOB { get; set; }
        public string ACCOUNT_CODE { get; set; }
        public string CUST_TYPE { get; set; }
        public string LEAD_DAYS { get; set; }
        public string GENDER { get; set; }
        public Nullable<decimal> ADVANCE { get; set; }
        public string CITY_CODE { get; set; }
        public string AREA_CODE { get; set; }
        public string DISCOUNT { get; set; }
        public string NAME_URDU { get; set; }
        public Nullable<decimal> OPENING { get; set; }
        public string TRANSFER_STATUS { get; set; }
        public string CONTACT_NAME { get; set; }
        public string CNIC { get; set; }
        public string BUSINESS_NAME { get; set; }
        public string BUSINESS_URL { get; set; }
        public string LAT { get; set; }
        public string LON { get; set; }
        public string COMMENTS { get; set; }
        public string PROMO_CODE { get; set; }
        public Nullable<System.DateTime> PROMO_EXPIRY { get; set; }
    
        public virtual AREA AREA { get; set; }
        public virtual CITY CITY { get; set; }
        public virtual GL_CHART GL_CHART { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<GRFS_MAIN> GRFS_MAIN { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<GRN_MAIN> GRN_MAIN { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<JOURNAL> JOURNALs { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RESERVATION_MAIN> RESERVATION_MAIN { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<STOCK_MAIN> STOCK_MAIN { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SUPPLIER_PRODUCTS> SUPPLIER_PRODUCTS { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TICKET_MAIN> TICKET_MAIN { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TRANS_MN> TRANS_MN { get; set; }
    }
}