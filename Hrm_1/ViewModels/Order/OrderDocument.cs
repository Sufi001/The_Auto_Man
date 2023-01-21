using Inventory.ViewModels.Item;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inventory.ViewModels.Order
{
    public class OrderDocument
    {
        public bool Action { get; set; }
        public string Status { get; set; }
        public string OrderNo { get; set; }
        public string ShipTo { get; set; }
        public string CustomerMessage { get; set; }
        public string OrderNote { get; set; }
        public string Date { get; set; }
        public string DeliveryDate { get; set; }
        public decimal? Total { get; set; }
        public List<ItemViewModel> PurchasedItems { get; set; }

    }
}