using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inventory.ViewModels.Order
{
    public class OrderProcessingViewModel
    {
        public OrderCount OC { get; set; }
        public List<OrderDocument> list { get; set; }
    }
}