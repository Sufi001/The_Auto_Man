using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Inventory.Models.FBR
{
    public class InvoiceItems
    {
        [Required]
        [StringLength(50)]
        public string ItemCode { get; set; }

        [Required]
        [StringLength(150)]
        public string ItemName { get; set; }

        [Required]
        [StringLength(8)]
        public string PCTCode { get; set; } //Pakistan Custom Tariff

        [Required]
        public double Quantity { get; set; }

        [Required]
        public float TaxRate { get; set; }

        [Required]
        public decimal SaleValue { get; set; }

        public decimal Discount { get; set; }

        [Required]
        public decimal TaxCharged { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        [Required]
        public int InvoiceType { get; set; } //1. New, 2.Debit, 3.Credit

        [StringLength(50)]
        public string RefUSIN { get; set; }
    }
}