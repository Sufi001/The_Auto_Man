using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    public class SalesVM
    {
        public string SupplierName { get; set; }
        public string SupplierContactNo { get; set; }
        public string Phone { get; set; }
        public DateTime DocumentDate { get; set; }
        public string Address { get; set; }
        public string SupplierPhoneNo { get; set; }
        public string SupplierCode { get; set; }
        public string DocumentNo { get; set; }

        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public decimal Qty { get; set; }
        public decimal Rate { get; set; }
        public decimal? Amount { get; set; }
        public string PartyType { get; set; }
        public decimal Unit_Cost { get; set; }
        public DateTime BookingDate { get; set; }
        public string UanNo { get; set; }
        public decimal? CTNSize { get; set; }
        public decimal? FreeQty { get; set; }
        public decimal? Advance { get; set; }
        public decimal Payment { get; set; }
        public decimal Discount { get; set; }
        public string SalesMan { get; set; }
        public string QtyCTN { get; set; }

    }
}