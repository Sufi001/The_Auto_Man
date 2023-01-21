using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inventory.ViewModels
{
    public class DashboardViewModel
    {
        public string TodaySale { get; set; }
        public string TodaySaleB1 { get; set; }
        public string TodaySaleB2 { get; set; }
        public string TodaySaleB3 { get; set; }
        public string TodaySaleTotal { get; set; }
        public string YesterdaySale { get; set; }
        public string YesterdaySaleB1 { get; set; }
        public string YesterdaySaleB2 { get; set; }
        public string YesterdaySaleB3 { get; set; }
        public string YesterdaySaleTotal { get; set; }
        public string WebTodaySale { get; set; }
        public string WebPendingOrderCount { get; set; }
        public string TodayPurchase { get; set; }
        public string YesterdayPurchase { get; set; }
        public string NetPayable { get; set; }
        public string NetReceiveable { get; set; }
        public string TodayRecovery { get; set; }
        public string TodayOrder { get; set; }
        public string TodayPayment { get; set; }
        public string TodayReceipt { get; set; }
        public string MonthlyPurchase { get; set; }
        public string StockValue { get; set; }
        public string StockValueB1 { get; set; }
        public string StockValueB2 { get; set; }
        public string StockValueB3 { get; set; }
        public string StockQty { get; set; }
        public string StockQtyB1 { get; set; }
        public string StockQtyB2 { get; set; }
        public string StockQtyB3 { get; set; }

        public string StockTransferQty { get; set; }
        public string StockTransferQtyB1 { get; set; }
        public string StockTransferQtyB2 { get; set; }
        public string StockTransferQtyB3 { get; set; }

        public string StockTransferValue { get; set; }
        public string StockTransferValueB1 { get; set; }
        public string StockTransferValueB2 { get; set; }
        public string StockTransferValueB3 { get; set; }

        public string BillVoidQty { get; set; }
        public string BillVoidQtyB1 { get; set; }
        public string BillVoidQtyB2 { get; set; }
        public string BillVoidQtyB3 { get; set; }

        public string BillVoidValue { get; set; }
        public string BillVoidValueB1 { get; set; }
        public string BillVoidValueB2 { get; set; }
        public string BillVoidValueB3 { get; set; }

        public string ItemVoidQty { get; set; }
        public string ItemVoidQtyB1 { get; set; }
        public string ItemVoidQtyB2 { get; set; }
        public string ItemVoidQtyB3 { get; set; }

        public string ItemVoidValue { get; set; }
        public string ItemVoidValueB1 { get; set; }
        public string ItemVoidValueB2 { get; set; }
        public string ItemVoidValueB3 { get; set; }

        public string CurrentMonthSale  { get; set; }
        public string CurrentMonthSaleB1 { get; set; }
        public string CurrentMonthSaleB2 { get; set; }
        public string CurrentMonthSaleB3 { get; set; }

        public string StockReceiveQty { get; set; }
        public string StockReceiveQtyB1 { get; set; }
        public string StockReceiveQtyB2 { get; set; }
        public string StockReceiveQtyB3 { get; set; }

        public string StockReceiveValue { get; set; }
        public string StockReceiveValueB1 { get; set; }
        public string StockReceiveValueB2 { get; set; }
        public string StockReceiveValueB3 { get; set; }


        public string NegitiveStockItemCount { get; set; }
        public string NegitiveStockItemCountB1 { get; set; }
        public string NegitiveStockItemCountB2 { get; set; }
        public string NegitiveStockItemCountB3 { get; set; }

        public string PreviousMonthSale { get; set; }
        public string PreviousMonthSaleB1 { get; set; }
        public string PreviousMonthSaleB2 { get; set; }
        public string PreviousMonthSaleB3 { get; set; }

        public List<JsChart> WeeklySales { get; set; }
        public List<JsChart> MonthlySales { get; set; }
        public List<JsChart> StockByDepartment { get; set; }
        public List<JsChart> SalesByDepartment { get; set; }
    }

    public class JsChart
    {
        public int DayNumber { get; set; }
        public int MonthNumber { get; set; }
        public string Month { get; set; }
        public string DeptCode { get; set; }
        public string DeptName { get; set; }
        public string Day { get; set; }
        public decimal? Amount { get; set; }
    }

}