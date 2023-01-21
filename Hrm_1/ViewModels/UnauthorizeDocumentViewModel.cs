using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Inventory.ViewModels
{
    public class UnauthorizeDocumentViewModel
    {
        [Required]
        public string DocType { get; set; }
        [Required]
        public string DocNumber { get; set; }
        public IEnumerable<SelectListItem> Documents
        {
            get
            {
                return new List<SelectListItem>
                {
                    new SelectListItem{Text = "Purchase", Value = "Purchase"},
                    new SelectListItem{Text = "Purchase Return", Value = "PurchaseReturn"},
                    new SelectListItem{Text = "Sale", Value = "Sale"},
                    new SelectListItem{Text = "Sale Return", Value = "SaleReturn"},
                    new SelectListItem{Text = "Receipt", Value = "Receipt"},
                    new SelectListItem{Text = "Payment", Value = "Payment"},
                    new SelectListItem{Text = "Stock Transfer In", Value = "StockTransferIn"},
                    new SelectListItem{Text = "Stock Transfer Out", Value = "StockTransferOut"},
                    new SelectListItem{Text = "Waste", Value = "Waste"},
                    new SelectListItem{Text = "Gain", Value = "Gain"},
                };
            }
        }
    }
}