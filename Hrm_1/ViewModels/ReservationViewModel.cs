using Inventory.ViewModels.Reservation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inventory.ViewModels
{
    public class ReservationViewModel
    {
        public ReservationMain main { get; set; }
        public ReservationDetail detail { get; set; }
        public List<ReservationDetail> detailList { get; set; }
        public List<ReservationMain> mainList { get; set; }
        public List<ReservationItems> reservationItems { get; set; }
        public List<ReservationItems> reservationAmenities { get; set; }
        public List<ReservationItems> itemList { get; set; }
        public List<ReservationItems>  amenityList{ get; set; }
    }
}