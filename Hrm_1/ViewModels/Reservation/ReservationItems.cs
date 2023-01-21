using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inventory.ViewModels.Reservation
{
    public class ReservationItems
    {
        public long RES_ID { get; set; }
        public string BARCODE { get; set; }
        public string DESCRIPTION { get; set; }
        public Nullable<decimal> UNIT_RETAIL { get; set; }
        public string ROOM_CODE { get; set; }
        public string ROOM_NAME { get; set; }
        public int? QUANTITY { get; set; }
        public decimal? AMOUNT { get; set; }
        public DateTime? DATETIME { get; set; }
        public string STATUS { get; set; }
        public string COMMENTS { get; set; }
        public string REMARKS { get; set; }
    }
}