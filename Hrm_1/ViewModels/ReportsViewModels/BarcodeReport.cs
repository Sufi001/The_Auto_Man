using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inventory.ViewModels.ReportsViewModels
{
    public class BarcodeReport
    {
        public string Barcode { get; set; }
        public string Name { get; set; }
        public byte[] Img { get; set; }
        public int Qty { get; set; }
        public string Exp { get; set; }
        public string Price { get; set; }
    }
}