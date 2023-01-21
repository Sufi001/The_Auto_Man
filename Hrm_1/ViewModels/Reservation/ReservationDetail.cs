using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inventory.ViewModels.Reservation
{
    public class ReservationDetail
    {
        public long RES_ID { get; set; }
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
        public Nullable<decimal> TOTAL_AMT { get; set; }
        public Nullable<int> ADULTS { get; set; }
        public Nullable<int> CHILDREN { get; set; }
        public Nullable<int> REVS_DAYS { get; set; }
        public string STATUS { get; set; }


        public List<ROOM_TYPE> RoomTypeList { get; set; }
        public List<ROOM_CATEGORY> RoomCategoryList { get; set; }
        public List<ROOM> RoomList { get; set; }
    }
}