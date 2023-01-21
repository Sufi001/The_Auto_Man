using System;
using System.Collections.Generic;
using System.Linq;

namespace Inventory.ViewModels
{
    public class DocumentListViewModel
    {
        public string DocNo { get; set; }
        public DateTime Doc { get; set; }
        public string Location { get; set; }
        public string ProductName { get; set; }
        public decimal Qty { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string Supplier { get; set; }
        
    }
}