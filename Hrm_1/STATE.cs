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
    
    public partial class STATE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public STATE()
        {
            this.TICKET_DETAIL = new HashSet<TICKET_DETAIL>();
        }
    
        public string ID { get; set; }
        public string NAME { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TICKET_DETAIL> TICKET_DETAIL { get; set; }
    }
}
