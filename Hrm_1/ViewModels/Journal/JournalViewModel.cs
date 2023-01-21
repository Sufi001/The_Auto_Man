using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Inventory.ViewModels.Journal
{
    public class JournalViewModel
    {
        public string Doc_Seq { get; set; }
        public DateTime? VoucherDate { get; set; }
        public decimal? Amount { get; set; }
        public string ReceivedFrom { get; set; }
        public string Narration { get; set; }
        public string AmountInWords { get; set; }
        public string BankName { get; set; }
        public DateTime? Dated { get; set; }
        public string PaymentPurpose { get; set; }
        public string ChequeNo { get; set; }
        public string Company { get; set; }
        public long RES_ID { get; set; }
        public string Doc_Type { get; set; }
        public string Party_Type { get; set; }
        public string PaymentMode { get; set; }
        public string Status { get; set; }
        public string RoomCode { get; set; }
        public string RoomName { get; set; }
        public string BookNo { get; set; }
        public string BillNo { get; set; }
        public string AccountCode { get; set; }
        public string AccountCodeIE { get; set; }
        public string AccountTitle { get; set; } 
        public string AccountTitleIE { get; set; }

        public string StaffCode { get; set; }
        public string CustomerCode { get; set; }
        public decimal? Balance { get; set; }

        public IEnumerable<SelectListItem> CustomerList { get; set; }
        public IEnumerable<SelectListItem> SupplierList { get; set; }
        public IEnumerable<SelectListItem> AccountListIncome { get; set; }
        public IEnumerable<SelectListItem> AccountListExpense { get; set; }
        public IEnumerable<SelectListItem> StaffList { get; set; }
        public IEnumerable<SelectListItem> docTypeList { get; set; }
        public IEnumerable<SelectListItem> TransactionMode { get; set; }
        public IEnumerable<SelectListItem> AccountListCB { get; set; }
    }
}