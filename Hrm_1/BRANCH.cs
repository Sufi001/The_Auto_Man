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
    
    public partial class BRANCH
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public BRANCH()
        {
            this.PROD_LOC_REPORT = new HashSet<PROD_LOC_REPORT>();
            this.TRANSFER_MAIN = new HashSet<TRANSFER_MAIN>();
            this.TRANSFER_MAIN1 = new HashSet<TRANSFER_MAIN>();
            this.WAST_MAIN = new HashSet<WAST_MAIN>();
        }
    
        public string BRANCH_CODE { get; set; }
        public string BRANCH_NAME { get; set; }
        public string ADDRESS { get; set; }
        public string CITY_CODE { get; set; }
        public string EMAIL { get; set; }
        public string STATUS { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PROD_LOC_REPORT> PROD_LOC_REPORT { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TRANSFER_MAIN> TRANSFER_MAIN { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TRANSFER_MAIN> TRANSFER_MAIN1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<WAST_MAIN> WAST_MAIN { get; set; }
    }
}
