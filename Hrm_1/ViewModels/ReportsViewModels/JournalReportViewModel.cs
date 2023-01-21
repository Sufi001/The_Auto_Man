using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inventory.ViewModels.ReportsViewModels
{
    public class JournalReportViewModel
    {
        public string DOC_SEQ { get; set; }
        public string DOC_TYPE { get; set; }
        public DateTime DOC_DATE { get; set; }
        public string NARRATION { get; set; }
        public decimal? DEBIT { get; set; }
        public decimal? CREDIT { get; set; }
        public string PARTY_TYPE { get; set; }
        public string PARTY_CODE { get; set; }
        public string STATUS { get; set; }
        public decimal? AMOUNT { get; set; }
        public string RECEIVED_FROM { get; set; }
        public string AMOUNT_IN_WORDS { get; set; }
        public string BANK_NAME { get; set; }
        public DateTime? DATED { get; set; }
        public string PAYMENT_PURPOSE { get; set; }
        public string CHEQUE_NO { get; set; }
        public string COMPANY { get; set; }
        public string RES_ID { get; set; }
        public string PAYENT_MODE { get; set; }
        public string BILL_NO { get; set; }
        public string STAFF_CODE { get; set; }
        public string STAFF_NAME { get; set; }
        public string PARTY_NAME { get; set; }
        public string BOOK_NO { get; set; }

    }
}