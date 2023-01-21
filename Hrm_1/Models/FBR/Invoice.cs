using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Inventory.Models.FBR
{
    public class Invoice
    {
        [StringLength(30)]
        public string FBRInvoiceNumber { get; set; }

        [Required]
        public long POSID { get; set; }

        [Required]
        [StringLength(50)]
        public string USIN { get; set; }

        [Required]
        //[DataType(DataType.DateTime)]
        public DateTime DateTime { get; set; }

        [StringLength(150)]
        public string BuyerName { get; set; }

        [StringLength(20)]
        public string BuyerPhoneNumber { get; set; }

        [Required]
        public decimal TotalSaleValue { get; set; }
        [Required]

        public double TotalQuantity { get; set; }

        [Required]
        public float TotalTaxCharged { get; set; }

        public decimal Discount { get; set; }

        [Required]
        public decimal TotalBillAMount { get; set; }

        [Required]
        public int PaymentMode { get; set; } //1. Cash, 2.Card, 3.Gift Voucher, 4.Loyality Card, 5.Mixed

        [StringLength(50)]
        public string RefUSIN { get; set; }

        [Required]
        public int InvoiceType { get; set; } //1. New, 2.Debit, 3.Credit

        [Required]
        public List<InvoiceItems> Items { get; set; }
        public UserInfo User { get; set; }

    }
}