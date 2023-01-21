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
    
    public partial class ROOM
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ROOM()
        {
            this.AMENITY_PRODUCT_DETAIL = new HashSet<AMENITY_PRODUCT_DETAIL>();
            this.AMENITY_PRODUCT_DETAIL_HISTORY = new HashSet<AMENITY_PRODUCT_DETAIL_HISTORY>();
            this.RESERVATION_DETAIL = new HashSet<RESERVATION_DETAIL>();
            this.RESERVATION_DETAIL_HISTORY = new HashSet<RESERVATION_DETAIL_HISTORY>();
        }
    
        public string ROOM_CODE { get; set; }
        public string ROOM_NAME { get; set; }
        public string STATUS { get; set; }
        public System.DateTime DOC { get; set; }
        public string INSERTED_BY { get; set; }
        public string UPDATED_BY { get; set; }
        public Nullable<System.DateTime> UPDATE_DATE { get; set; }
        public string TYPE { get; set; }
        public string CATEGORY { get; set; }
        public decimal RATE { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AMENITY_PRODUCT_DETAIL> AMENITY_PRODUCT_DETAIL { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AMENITY_PRODUCT_DETAIL_HISTORY> AMENITY_PRODUCT_DETAIL_HISTORY { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RESERVATION_DETAIL> RESERVATION_DETAIL { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RESERVATION_DETAIL_HISTORY> RESERVATION_DETAIL_HISTORY { get; set; }
        public virtual ROOM_CATEGORY ROOM_CATEGORY { get; set; }
        public virtual ROOM_TYPE ROOM_TYPE { get; set; }
    }
}
