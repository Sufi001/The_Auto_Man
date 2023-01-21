using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Inventory.ViewModels.Room
{
    public class RoomViewModel
    {
        public string ROOM_CODE { get; set; }
        public string ROOM_NAME { get; set; }
        public string STATUS { get; set; }
        public System.DateTime DOC { get; set; }
        public string INSERTED_BY { get; set; }
        public string UPDATED_BY { get; set; }
        public Nullable<System.DateTime> UPDATE_DATE { get; set; }
        public string TYPE { get; set; }
        public string CATEGORY { get; set; }
        public decimal? RATE { get; set; }
        public List<ROOM_CATEGORY> RoomCategories { get; set; }
        public List<ROOM_TYPE> RoomTypes { get; set; }
        public IEnumerable<SelectListItem> RoomStatus { get; set; }
        public List<ROOM> roomsList { get; set; }
    }
}