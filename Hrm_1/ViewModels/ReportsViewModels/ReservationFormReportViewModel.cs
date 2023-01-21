using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inventory.ViewModels.ReportsViewModels
{
    public class ReservationFormReportViewModel
    {
        public long RES_ID { get; set; }
        public DateTime DOC_DATE { get; set; }
        public string SUPL_CODE { get; set; }
        public string FIRST_NAME { get; set; }
        public string LAST_NAME { get; set; }
        public string CITY { get; set; }
        public string ADDRESS { get; set; }
        public string COUNTRY { get; set; }
        public string PHONE { get; set; }
        public string E_MAIL { get; set; }
        public string ID_TYPE { get; set; }
        public string ID_NO { get; set; }
        public string STATUS { get; set; }
        public Nullable<System.DateTime> DOB { get; set; }
        public Nullable<decimal> DISCOUNT { get; set; }
        public Nullable<decimal> TOTAL_AMT { get; set; }
        public Nullable<decimal> BALANCE { get; set; }
        public Nullable<decimal> AMOUNT_PAID { get; set; }
        public string COMPANY { get; set; }
        public string ROOM_TYPE { get; set; }
        public string ROOM_CODE { get; set; }
        public string ROOM_NAME { get; set; }
        public string ROOM_CATEGORY { get; set; }
        public string TABLES_CODE { get; set; }
        public DateTime? CHECKIN_DATETIME { get; set; }
        public Nullable<System.DateTime> ESTIMATED_CHECKOUT_DATETIME { get; set; }
        public DateTime? CHECKOUT_DATETIME { get; set; }
        public string RES_STS { get; set; }
        public Nullable<decimal> RATE_PER_DAY { get; set; }
        public Nullable<decimal> OTHER_CHARGES { get; set; }
        public Nullable<decimal> SERVICES_CHARGES { get; set; }
        public Nullable<decimal> GST { get; set; }
        public Nullable<decimal> DETAIL_TOTAL { get; set; }
        public Nullable<int> ADULTS { get; set; }
        public Nullable<int> CHILDREN { get; set; }
        public Nullable<int> REVS_DAYS { get; set; }
        public string DETAIL_STATUS { get; set; }

        public string BARCODE { get; set; }
        public string DESCRIPTION { get; set; }
        public Nullable<decimal> UNIT_RETAIL { get; set; }
        public string ITEM_ROOM_CODE { get; set; }
        public string ITEM_ROOM_NAME { get; set; }
        public int? QUANTITY { get; set; }
        public decimal? AMOUNT { get; set; }
        public DateTime? DATETIME { get; set; }
        public string ITEM_STATUS { get; set; }

    }
}