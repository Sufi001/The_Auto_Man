using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inventory.ViewModels.Room
{
    public class RoomDropDownModel
    {
        public string code { get; set; }
        public string name { get; set; }
        public ICollection<RESERVATION_DETAIL> detail { get; set; }
        public string alertMessage { get; set; }
        public decimal? rate { get; set; }
    }
}