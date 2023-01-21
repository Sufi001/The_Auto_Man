using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Inventory.ViewModels.Reservation
{
    public class ReservationMain
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
        //public Nullable<decimal> PER_DAY { get; set; }
        public List<COUNTRY> Countries { get; set; }
        public List<CITY> Cities { get; set; }
        public IEnumerable<SelectListItem> IdTypes { get; set; }
        public IList<ReservationDetail> detailList { get; set; }
    }
}