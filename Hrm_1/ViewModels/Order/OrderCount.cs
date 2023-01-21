using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inventory.ViewModels.Order
{
    public class OrderCount
    {
        public int AllCount { get; set; }
        public int ProcessingCount { get; set; }
        public int DispatchCount { get; set; }
        public int TrashCount { get; set; }
        public int OnholdCount { get; set; }
        public int CompletedCount { get; set; }
        public int CancelledCount { get; set; }
    }
}