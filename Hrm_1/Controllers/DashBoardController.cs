using Inventory.Models.FBR;
using Inventory.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
namespace Inventory.Controllers
{
    [Authorize]
    public class DashBoardController : Controller
    {
        DataContext db;
        DateTime yesterday = DateTime.Now.AddDays(-1).Date;
        DateTime today = DateTime.Now.Date;
        DateTime tomorow = DateTime.Now.AddDays(1).Date;
        public DashBoardController()
        {
            db = new DataContext();
        }
        // GET: DashBoard
        public ActionResult Index()
        {
            try
            {

           
            DashboardViewModel DVM = new DashboardViewModel();
            var date = DateTime.Now.Date;
            DVM.TodaySale = TodaySale("");
            DVM.TodaySaleB1 = TodaySale("01");
            DVM.TodaySaleB2 = TodaySale("02");
            DVM.TodaySaleB3 = TodaySale("03");
            DVM.TodaySaleTotal = (Convert.ToInt32(DVM.TodaySaleB1) + Convert.ToInt32(DVM.TodaySaleB2) + Convert.ToInt32(DVM.TodaySaleB3)).ToString();
            DVM.YesterdaySale = YesterdaySale("");
            DVM.YesterdaySaleB1 = YesterdaySale("01");
            DVM.YesterdaySaleB2 = YesterdaySale("02");
            DVM.YesterdaySaleB3 = YesterdaySale("03");
            DVM.YesterdaySaleTotal = (Convert.ToInt32(DVM.YesterdaySaleB1) + Convert.ToInt32(DVM.YesterdaySaleB2) + Convert.ToInt32(DVM.YesterdaySaleB3)).ToString();
            DVM.TodayPurchase = TodayPurchase();
            DVM.YesterdayPurchase = YesterdayPurchase();
            DVM.MonthlyPurchase = MonthlyPurchase();

            

            DVM.NetPayable = NetPayable();

            if (string.IsNullOrEmpty(DVM.NetPayable))
                DVM.NetPayable = "0";


            DVM.NetReceiveable = NetReceiveable();

            if (string.IsNullOrEmpty(DVM.NetReceiveable))
                DVM.NetReceiveable = "0";

            DVM.TodayOrder = TodayOrder();

            DVM.TodayPayment = TodayPayment();
            if (string.IsNullOrEmpty(DVM.TodayPayment))
                DVM.TodayPayment = "0";

            DVM.TodayReceipt = TodayReceipt();
            if (string.IsNullOrEmpty(DVM.TodayReceipt))
                DVM.TodayReceipt = "0";

            DVM.TodayRecovery = TodayRecovery();

                DVM.StockValue = CurrentStockValue("");
                DVM.StockValueB1 = CurrentStockValue("01");
                DVM.StockValueB2 = CurrentStockValue("02");
                DVM.StockValueB3 = CurrentStockValue("03");

                DVM.StockQty = CurrentStockQty("");
                DVM.StockQtyB1 = CurrentStockQty("01");
                DVM.StockQtyB2 = CurrentStockQty("02");
                DVM.StockQtyB3 = CurrentStockQty("03");

                DVM.WebTodaySale = WebTodaySale();
                DVM.WebPendingOrderCount = WebPendingOrderCount();

                



                DVM.BillVoidQty = BillVoidQty("");
                DVM.BillVoidQtyB1 = BillVoidQty("01");
                DVM.BillVoidQtyB2 = BillVoidQty("02");
                DVM.BillVoidQtyB3 = BillVoidQty("03");

                DVM.BillVoidValue = BillVoidValue("");
                DVM.BillVoidValueB1 = BillVoidValue("01");
                DVM.BillVoidValueB2 = BillVoidValue("02");
                DVM.BillVoidValueB3 = BillVoidValue("03");


                DVM.ItemVoidQty = ItemVoidQty("");
                DVM.ItemVoidQtyB1 = ItemVoidQty("01");
                DVM.ItemVoidQtyB2 = ItemVoidQty("02");
                DVM.ItemVoidQtyB3 = ItemVoidQty("03");

                DVM.ItemVoidValue = ItemVoidValue("");
                DVM.ItemVoidValueB1 = ItemVoidValue("01");
                DVM.ItemVoidValueB2 = ItemVoidValue("02");
                DVM.ItemVoidValueB3 = ItemVoidValue("03");

                DVM.CurrentMonthSale = CurrentMonthSale("");
                DVM.CurrentMonthSaleB1 = CurrentMonthSale("01");
                DVM.CurrentMonthSaleB2 = CurrentMonthSale("02");
                DVM.CurrentMonthSaleB3 = CurrentMonthSale("03");

                DVM.StockTransferQty = TodayStockTransferQty("04");
                DVM.StockTransferQtyB1 = TodayStockTransferQty("01");
                DVM.StockTransferQtyB2 = TodayStockTransferQty("02");
                DVM.StockTransferQtyB3 = TodayStockTransferQty("03");

                DVM.StockTransferValue = TodayStockTransferValue("0");
                DVM.StockTransferValueB1 = TodayStockTransferValue("01");
                DVM.StockTransferValueB2 = TodayStockTransferValue("02");
                DVM.StockTransferValueB3 = TodayStockTransferValue("03");


                DVM.StockReceiveQty = TodayStockReceiveQty("04");
                DVM.StockReceiveQtyB1 = TodayStockReceiveQty("01");
                DVM.StockReceiveQtyB2 = TodayStockReceiveQty("02");
                DVM.StockReceiveQtyB3 = TodayStockReceiveQty("03");

                DVM.StockReceiveValue = TodayStockReceiveValue("04");
                DVM.StockReceiveValueB1 = TodayStockReceiveValue("01");
                DVM.StockReceiveValueB2 = TodayStockReceiveValue("02");
                DVM.StockReceiveValueB3 = TodayStockReceiveValue("03");

                DVM.NegitiveStockItemCount = NegitiveStockItemCount("");
                DVM.NegitiveStockItemCountB1 = NegitiveStockItemCount("01");
                DVM.NegitiveStockItemCountB2 = NegitiveStockItemCount("02");
                DVM.NegitiveStockItemCountB3 = NegitiveStockItemCount("03");

                DVM.PreviousMonthSale = PreviousMonthSale("");
                DVM.PreviousMonthSaleB1 = PreviousMonthSale("01");
                DVM.PreviousMonthSaleB2 = PreviousMonthSale("02");
                DVM.PreviousMonthSaleB3 = PreviousMonthSale("03");


                DVM.WeeklySales = WeeklySales();
            DVM.MonthlySales = MonthlySales();
            DVM.StockByDepartment = StockByDepartment();
            DVM.SalesByDepartment = SaleByDepartment();

            return View(DVM);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string TodaySale(string BranchId)
        {
            decimal? amount;

            if (!string.IsNullOrEmpty(BranchId))
            {
                amount = db.TRANS_MN.Where(x => x.TRANS_DATE >= today && x.TRANS_DATE < tomorow && x.CANCEL == "F"
                && x.TILL_NO == BranchId && x.STATUS == Constants.DocumentStatus.AuthorizedDocument)
                .Sum(x => x.CASH_AMT);

            }
            else
            {
                amount = db.TRANS_MN.Where(x => x.TRANS_DATE >= today && x.TRANS_DATE < tomorow && x.CANCEL == "F"
                && x.STATUS == Constants.DocumentStatus.AuthorizedDocument)
                .Sum(x => x.CASH_AMT);

            }

            if (amount.HasValue)
                return amount.Value.ToString("0");
            else
                return "0";
        }

        public string YesterdaySale(string BranchId)
        {
            decimal? amount;



            if (!string.IsNullOrEmpty(BranchId))
            {
                amount = db.TRANS_MN.Where(x => x.TRANS_DATE >= yesterday && x.TRANS_DATE < today && x.CANCEL == "F"
                && x.TILL_NO == BranchId && x.STATUS == Constants.DocumentStatus.AuthorizedDocument)
                .Sum(x => x.CASH_AMT);
            }
            else
            {
                amount = db.TRANS_MN.Where(x => x.TRANS_DATE >= yesterday && x.TRANS_DATE < today && x.CANCEL == "F"
                && x.STATUS == Constants.DocumentStatus.AuthorizedDocument)
                .Sum(x => x.CASH_AMT);
            }
            if (amount.HasValue)
                return amount.Value.ToString("0");
            else
                return "0";
        }

        public string TodayPurchase()
        {

            var amount = db.GRN_MAIN.Where(x => x.GRN_DATE >= today && x.GRN_DATE < tomorow
            && x.STATUS == Constants.DocumentStatus.AuthorizedDocument).Sum(x => x.AMOUNT);

            if (amount.HasValue)
                return amount.Value.ToString("0");
            else
                return "0";
        }

        public string YesterdayPurchase()
        {

            var amount = db.GRN_MAIN.Where(x => x.GRN_DATE >= yesterday && x.GRN_DATE < today

            && x.STATUS == Constants.DocumentStatus.AuthorizedDocument).Sum(x => x.AMOUNT);

            if (amount.HasValue)
                return amount.Value.ToString("0");
            else
                return "0";
        }

        public string MonthlyPurchase()
        {
            var amount = db.GRN_MAIN.Where(x => x.GRN_DATE.Month == DateTime.Now.Month && x.STATUS == Constants.DocumentStatus.AuthorizedDocument).Sum(x => x.AMOUNT);

            if (amount.HasValue)
                return amount.Value.ToString("0");
            else
                return "0";
        }

        public string TodayPayment()
        {
            var amount = db.JOURNALs.Where(x => x.DOC_TYPE == "P" &&
             x.TRANS_DTE >= today && x.TRANS_DTE < tomorow

            ).Sum(x => x.AMOUNT);
            if (amount.HasValue)
                return amount.Value.ToString("0");
            else
                return "0";
        }
        public string TodayReceipt()
        {
            var amount = db.JOURNALs.Where(x => x.DOC_TYPE == "R" &&
            x.TRANS_DTE >= today && x.TRANS_DTE < tomorow
            ).Sum(x => x.AMOUNT);
            if (amount.HasValue)
                return amount.Value.ToString("0");
            else
                return "0";
        }

        public string WebTodaySale()
        {
            var amount = db.TRANS_MN.Where(x => x.TRANS_DATE >= today && x.TRANS_DATE < tomorow

            && x.SALE_TYPE == "WebStore" && x.STATUS == Constants.DocumentStatus.AuthorizedDocument).Sum(x => x.CASH_AMT);
            if (amount.HasValue)
                return amount.Value.ToString("0");
            else
                return "0";

        }

        public string WebPendingOrderCount()
        {
            var count = db.TRANS_MN.Where(x => x.STATUS != "3" && x.SALE_TYPE == "WebStore").Count().ToString();
            return count;
        }

        public string NetPayable()
        {
            var amount = db.SUPPLIERs.Where(x => x.PARTY_TYPE == Constants.CustomerSupplier.Supplier && x.STATUS == Constants.DocumentStatus.AuthorizedDocument).Sum(x => (decimal?)(x.BALANCE ?? 0) + (decimal?)(x.OPENING ?? 0));
            if (amount.HasValue)
                return amount.Value.ToString("0");
            else
                return "0";
        }
        public string NetReceiveable()
        {
            var amount = db.SUPPLIERs.Where(x => x.PARTY_TYPE == Constants.CustomerSupplier.Customer && x.STATUS == Constants.DocumentStatus.AuthorizedDocument).Sum(x => (decimal?)(x.BALANCE ?? 0) + (decimal?)(x.OPENING ?? 0));
            if (amount.HasValue)
                return amount.Value.ToString("0");
            else
                return "0";
        }
        public string TodayRecovery()
        {
            return "0";
        }
        public string TodayOrder()
        {
            var count = db.DN_MAIN.Where(x => x.DN_DATE >= today && x.DN_DATE < tomorow).Count().ToString();
            return count;

        }
        public string CurrentStockValue(string BranchId)
        {
            string Value = "";
            if (string.IsNullOrEmpty(BranchId))
            {
                var data = db.PROD_BALANCE.ToList();
                var barcodes = data.Select(x => x.BARCODE).ToList();
                var products = db.PRODUCTS.Where(x => barcodes.Contains(x.BARCODE)).ToList();

                decimal? TotalAmount = 0;
                foreach (var item in data)
                {
                    var productUnitCost = products.Where(x => x.BARCODE == item.BARCODE).Select(x => x.UNIT_COST).SingleOrDefault();
                    if (productUnitCost != null)
                    {
                        TotalAmount += ((item.GRN_QTY ?? 0) + (item.TRANSFER_IN_QTY ?? 0)+ (item.GAIN_QTY ?? 0) - (item.GRF_QTY ?? 0) - (item.TRANSFER_OUT_QTY ?? 0) - (item.WAST_QTY ?? 0)) * productUnitCost;
                    }
                }

                Value = string.Format("{0:0}", TotalAmount);
            }
            else
            {
                var data = db.PROD_LOC_REPORT.Where(x => x.BRANCH_ID == BranchId).Select(x => new { Barcode = x.BARCODE, Qty = x.CURRENT_QTY }).ToList();
                var barcodes = data.Select(x => x.Barcode).ToList();
                var products = db.PRODUCTS.Where(x => barcodes.Contains(x.BARCODE)).ToList();

                decimal? TotalAmount = 0;
                foreach (var item in data)
                {
                    var productUnitCost = products.Where(x => x.BARCODE == item.Barcode).Select(x => x.UNIT_COST).SingleOrDefault();
                    if (productUnitCost != null)
                    {
                        TotalAmount += item.Qty * productUnitCost;
                    }
                }

                Value = string.Format("{0:0}", TotalAmount);
            }
            return Value;
        }
        public string CurrentStockQty(string BranchId)
        {
            string Value = "";
            if (string.IsNullOrEmpty(BranchId))
            {
                var data = db.PROD_BALANCE.ToList();
                decimal? v = 0;
                foreach (var item in data)
                {
                    v += ((item.GRN_QTY ?? 0) + (item.TRANSFER_IN_QTY ?? 0)+ (item.GAIN_QTY ?? 0) - (item.GRF_QTY ?? 0) - (item.TRANSFER_OUT_QTY ?? 0) -(item.WAST_QTY ?? 0));

                }

                Value = string.Format("{0:0}", v);
            }
            else
            {
                var data = db.PROD_LOC_REPORT.Where(x => x.BRANCH_ID == BranchId).Sum(x => (decimal?)x.CURRENT_QTY) ?? 0;
                Value = string.Format("{0:0}", data);
            }
            return Value;
        }


        public string CurrentMonthSale(string BranchId)
        {
            decimal? amount;

            if (!string.IsNullOrEmpty(BranchId))
            {
                amount = db.TRANS_MN.Where(x => x.TRANS_DATE.Year == today.Year && x.TRANS_DATE.Month == today.Month && x.CANCEL == "F"
                && x.TILL_NO == BranchId && x.STATUS == Constants.DocumentStatus.AuthorizedDocument)
                .Sum(x => x.CASH_AMT);
            }
            else
            {
                amount = db.TRANS_MN.Where(x => x.TRANS_DATE.Year == today.Year && x.TRANS_DATE.Month == today.Month && x.CANCEL == "F"
                && x.STATUS == Constants.DocumentStatus.AuthorizedDocument)
                .Sum(x => x.CASH_AMT);
            }
            if (amount.HasValue)
                return amount.Value.ToString("0");
            else
                return "0";
        }

        public string PreviousMonthSale(string BranchId)
        {
            decimal? amount;

            var month = DateTime.Now.Month - 1 < 0 ? 12 : DateTime.Now.Month - 1; 
            var year = (DateTime.Now.Month - 1) < 0 ? DateTime.Now.Year - 1 : DateTime.Now.Year; 


            if (!string.IsNullOrEmpty(BranchId))
            {
                amount = db.TRANS_MN.Where(x => x.TRANS_DATE.Month == month && x.TRANS_DATE.Year == year && x.CANCEL == "F"
                && x.TILL_NO == BranchId && x.STATUS == Constants.DocumentStatus.AuthorizedDocument)
                .Sum(x => x.CASH_AMT);
            }
            else
            {
                amount = db.TRANS_MN.Where(x => x.TRANS_DATE.Month == month && x.TRANS_DATE.Year == year && x.CANCEL == "F"
                && x.STATUS == Constants.DocumentStatus.AuthorizedDocument)
                .Sum(x => x.CASH_AMT);
            }
            if (amount.HasValue)
                return amount.Value.ToString("0");
            else
                return "0";
        }


        public string TodayStockTransferQty(string BranchId)
        {
            string Value = "";
            decimal Qty = 0;
            var data = db.TRANSFER_MAIN.Include(x => x.TRANSFER_DETAIL).Where(x => x.DOC_DATE >= today && x.DOC_DATE < tomorow
            && x.STATUS == Constants.DocumentStatus.AuthorizedDocument
            && x.BRANCH_ID_FROM == BranchId).ToList();
            foreach (var main in data)
            {
                Qty += main.TRANSFER_DETAIL.Sum(x => x.QTY);
            }
            Value = string.Format("{0:0}", Qty);
            return Value;
        }
        public string TodayStockTransferValue(string BranchId)
        {
            string Value = "";
            decimal Val = 0;
           
                var data = db.TRANSFER_MAIN.Include(x => x.TRANSFER_DETAIL).Where(x => x.DOC_DATE >= today && x.DOC_DATE < tomorow
                && x.STATUS == Constants.DocumentStatus.AuthorizedDocument
                && x.BRANCH_ID_FROM == BranchId).ToList();
                foreach (var main in data)
                {
                    Val += main.TRANSFER_DETAIL.Sum(x => x.QTY * x.COST);
                }

            Value = string.Format("{0:0}", Val);
            return Value;
        }

        public string TodayStockReceiveQty(string BranchId)
        {
            string Value = "";
            decimal Qty = 0;
            var data = db.TRANSFER_MAIN.Include(x => x.TRANSFER_DETAIL).Where(x => x.DOC_DATE >= today && x.DOC_DATE < tomorow
            && x.STATUS == Constants.DocumentStatus.AuthorizedDocument
            && x.BRANCH_ID_TO == BranchId).ToList();
            foreach (var main in data)
            {
                Qty += main.TRANSFER_DETAIL.Sum(x => x.QTY);
            }
            Value = string.Format("{0:0}", Qty);
            return Value;
        }
        public string TodayStockReceiveValue(string BranchId)
        {
            string Value = "";
            decimal Val = 0;

            var data = db.TRANSFER_MAIN.Include(x => x.TRANSFER_DETAIL).Where(x => x.DOC_DATE >= today && x.DOC_DATE < tomorow
            && x.STATUS == Constants.DocumentStatus.AuthorizedDocument
            && x.BRANCH_ID_TO == BranchId).ToList();
            foreach (var main in data)
            {
                Val += main.TRANSFER_DETAIL.Sum(x => x.QTY * x.COST);
            }

            Value = string.Format("{0:0}", Val);
            return Value;
        }

        public string NegitiveStockItemCount(string BranchId)
        {
            string Value = "";
            decimal Qty = 0;
            if (!string.IsNullOrEmpty(BranchId))
                Qty = db.PROD_LOC_REPORT.Where(x => x.BRANCH_ID == BranchId && x.CURRENT_QTY < 0).Count();
            else
                Qty = db.PROD_BALANCE.Where(x =>
               ((x.GRN_QTY ?? 0) + (x.TRANSFER_IN_QTY ?? 0) + (x.GAIN_QTY ?? 0) -
               (x.GRF_QTY ?? 0) - (x.TRANSFER_OUT_QTY ?? 0) - (x.WAST_QTY ?? 0)) < 0).Count();

            Value = string.Format("{0:0}", Qty);
            return Value;
        }

        public string BillVoidQty(string BranchId)
        {
            string Value = "";
            decimal Qty = 0;
            if (string.IsNullOrEmpty(BranchId))
            {
                var data = db.TRANS_MN.Include(x => x.TRANS_DT).Where(x => x.TRANS_DATE >= today && x.TRANS_DATE <= tomorow
                && x.STATUS == Constants.DocumentStatus.AuthorizedDocument
                && x.CANCEL == "T"
                && x.TILL_NO == null).ToList();

                foreach (var main in data)
                {
                    Qty += main.TRANS_DT.Where(x => x.VOID == "T").Sum(x => x.UNITS_SOLD);
                }
            }
            else
            {
                var data = db.TRANS_MN.Include(x => x.TRANS_DT).Where(x => x.TRANS_DATE >= today && x.TRANS_DATE <= tomorow
               && x.STATUS == Constants.DocumentStatus.AuthorizedDocument
               && x.CANCEL == "T"
               && x.TILL_NO == BranchId).ToList();

                foreach (var main in data)
                {
                    Qty += main.TRANS_DT.Where(x => x.VOID == "T").Sum(x => x.UNITS_SOLD);
                }
            }
            Value = string.Format("{0:0}", Qty);
            return Value;
        }
        public string BillVoidValue(string BranchId)
        {
            string Value = "";
            decimal Val = 0;
            if (string.IsNullOrEmpty(BranchId))
            {
                var data = db.TRANS_MN.Include(x => x.TRANS_DT).Where(x => x.TRANS_DATE >= today && x.TRANS_DATE <= tomorow
                && x.STATUS == Constants.DocumentStatus.AuthorizedDocument
                && x.CANCEL == "T"
                && x.TILL_NO == null).ToList();

                foreach (var main in data)
                {
                    Val += main.TRANS_DT.Where(x => x.VOID == "T").Sum(x => x.UNITS_SOLD * x.UNIT_RETAIL);
                }
            }
            else
            {
                var data = db.TRANS_MN.Include(x => x.TRANS_DT).Where(x => x.TRANS_DATE >= today && x.TRANS_DATE <= tomorow
               && x.STATUS == Constants.DocumentStatus.AuthorizedDocument
               && x.CANCEL == "T"
               && x.TILL_NO == BranchId).ToList();

                foreach (var main in data)
                {
                    Val += main.TRANS_DT.Where(x => x.VOID == "T").Sum(x => x.UNITS_SOLD * x.UNIT_RETAIL);
                }
            }
            Value = string.Format("{0:0}", Val);
            return Value;
        }

        public string ItemVoidQty(string BranchId)
        {
            string Value = "";
            decimal Qty = 0;
            if (string.IsNullOrEmpty(BranchId))
            {
                var data = db.TRANS_MN.Include(x => x.TRANS_DT).Where(x => x.TRANS_DATE >= today && x.TRANS_DATE <= tomorow
                && x.STATUS == Constants.DocumentStatus.AuthorizedDocument
                && x.CANCEL == "F"
                && x.TILL_NO == null).ToList();

                foreach (var main in data)
                {
                    Qty += main.TRANS_DT.Where(x => x.VOID == "T").Sum(x => x.UNITS_SOLD);
                }
            }
            else
            {
                var data = db.TRANS_MN.Include(x => x.TRANS_DT).Where(x => x.TRANS_DATE >= today && x.TRANS_DATE <= tomorow
               && x.STATUS == Constants.DocumentStatus.AuthorizedDocument
               && x.CANCEL == "F"
               && x.TILL_NO == BranchId).ToList();

                foreach (var main in data)
                {
                    Qty += main.TRANS_DT.Where(x => x.VOID == "T").Sum(x => x.UNITS_SOLD);
                }
            }
            Value = string.Format("{0:0}", Qty);
            return Value;
        }
        public string ItemVoidValue(string BranchId)
        {
            string Value = "";
            decimal Val = 0;
            if (string.IsNullOrEmpty(BranchId))
            {
                var data = db.TRANS_MN.Include(x => x.TRANS_DT).Where(x => x.TRANS_DATE >= today && x.TRANS_DATE <= tomorow
                && x.STATUS == Constants.DocumentStatus.AuthorizedDocument
                && x.CANCEL == "F"
                && x.TILL_NO == null).ToList();

                foreach (var main in data)
                {
                    Val += main.TRANS_DT.Where(x => x.VOID == "T").Sum(x => x.UNITS_SOLD * x.UNIT_RETAIL);
                }
            }
            else
            {
                var data = db.TRANS_MN.Include(x => x.TRANS_DT).Where(x => x.TRANS_DATE >= today && x.TRANS_DATE <= tomorow
               && x.STATUS == Constants.DocumentStatus.AuthorizedDocument
               && x.CANCEL == "F"
               && x.TILL_NO == BranchId).ToList();

                foreach (var main in data)
                {
                    Val += main.TRANS_DT.Where(x => x.VOID == "T").Sum(x => x.UNITS_SOLD * x.UNIT_RETAIL);
                }
            }
            Value = string.Format("{0:0}", Val);
            return Value;
        }
        public List<JsChart> WeeklySales()
        {
            JsChart js = new JsChart();
            List<JsChart> WeeklySalesList = new List<JsChart>();
            var QueryDate = DateTime.Now.Date.AddDays(-6);

            var amount = db.TRANS_MN.Where(x => x.TRANS_DATE.Day == QueryDate.Day && x.TRANS_DATE.Month == QueryDate.Month & x.TRANS_DATE.Year == QueryDate.Year && x.STATUS == Constants.DocumentStatus.AuthorizedDocument && x.CANCEL == "F" && x.TRANS_TYPE == Constants.TransType.Sales).Sum(x => x.CASH_AMT);
            js.Day = DateTime.Now.Date.AddDays(-6).DayOfWeek.ToString();
            js.DayNumber = 1;
            js.Amount = amount ?? 0;
            WeeklySalesList.Add(js);

            js = new JsChart();
            QueryDate = DateTime.Now.Date.AddDays(-5);
            amount = db.TRANS_MN.Where(x => x.TRANS_DATE.Day == QueryDate.Day && x.TRANS_DATE.Month == QueryDate.Month & x.TRANS_DATE.Year == QueryDate.Year && x.STATUS == Constants.DocumentStatus.AuthorizedDocument && x.CANCEL == "F" && x.TRANS_TYPE == Constants.TransType.Sales).Sum(x => x.CASH_AMT);
            js.Day = DateTime.Now.Date.AddDays(-5).DayOfWeek.ToString();
            js.DayNumber = 2;
            js.Amount = amount ?? 0;
            WeeklySalesList.Add(js);

            js = new JsChart();
            QueryDate = DateTime.Now.Date.AddDays(-4);
            amount = db.TRANS_MN.Where(x => x.TRANS_DATE.Day == QueryDate.Day && x.TRANS_DATE.Month == QueryDate.Month & x.TRANS_DATE.Year == QueryDate.Year && x.STATUS == Constants.DocumentStatus.AuthorizedDocument && x.CANCEL == "F" && x.TRANS_TYPE == Constants.TransType.Sales).Sum(x => x.CASH_AMT);
            js.Day = DateTime.Now.Date.AddDays(-4).DayOfWeek.ToString();
            js.DayNumber = 3;
            js.Amount = amount ?? 0;
            WeeklySalesList.Add(js);

            js = new JsChart();
            QueryDate = DateTime.Now.Date.AddDays(-3);
            amount = db.TRANS_MN.Where(x => x.TRANS_DATE.Day == QueryDate.Day && x.TRANS_DATE.Month == QueryDate.Month & x.TRANS_DATE.Year == QueryDate.Year && x.STATUS == Constants.DocumentStatus.AuthorizedDocument && x.CANCEL == "F" && x.TRANS_TYPE == Constants.TransType.Sales).Sum(x => x.CASH_AMT);
            js.Day = DateTime.Now.Date.AddDays(-3).DayOfWeek.ToString();
            js.DayNumber = 4;
            js.Amount = amount ?? 0;
            WeeklySalesList.Add(js);

            js = new JsChart();
            QueryDate = DateTime.Now.Date.AddDays(-2);
            amount = db.TRANS_MN.Where(x => x.TRANS_DATE.Day == QueryDate.Day && x.TRANS_DATE.Month == QueryDate.Month & x.TRANS_DATE.Year == QueryDate.Year && x.STATUS == Constants.DocumentStatus.AuthorizedDocument && x.CANCEL == "F" && x.TRANS_TYPE == Constants.TransType.Sales).Sum(x => x.CASH_AMT);
            js.Day = DateTime.Now.Date.AddDays(-2).DayOfWeek.ToString();
            js.DayNumber = 5;
            js.Amount = amount ?? 0;
            WeeklySalesList.Add(js);

            js = new JsChart();
            QueryDate = DateTime.Now.Date.AddDays(-1);
            amount = db.TRANS_MN.Where(x => x.TRANS_DATE.Day == QueryDate.Day && x.TRANS_DATE.Month == QueryDate.Month & x.TRANS_DATE.Year == QueryDate.Year && x.STATUS == Constants.DocumentStatus.AuthorizedDocument && x.CANCEL == "F" && x.TRANS_TYPE == Constants.TransType.Sales).Sum(x => x.CASH_AMT);
            js.Day = DateTime.Now.Date.AddDays(-1).DayOfWeek.ToString();
            js.DayNumber = 6;
            js.Amount = amount ?? 0;
            WeeklySalesList.Add(js);

            js = new JsChart();
            QueryDate = DateTime.Now.Date;
            amount = db.TRANS_MN.Where(x => x.TRANS_DATE.Day == QueryDate.Day && x.TRANS_DATE.Month == QueryDate.Month & x.TRANS_DATE.Year == QueryDate.Year && x.STATUS == Constants.DocumentStatus.AuthorizedDocument && x.CANCEL == "F" && x.TRANS_TYPE == Constants.TransType.Sales).Sum(x => x.CASH_AMT);
            js.Day = DateTime.Now.Date.DayOfWeek.ToString();
            js.DayNumber = 7;
            js.Amount = amount ?? 0;
            WeeklySalesList.Add(js);



            //var Data = db.TRANS_MN.OrderBy(x=>x.TRANS_DATE).Take(7).Select(x => new { Text = x.TRANS_DATE, amount = x.CASH_AMT }).ToList();


            //if (i < Data.Count)
            //{
            //    js.DayNumber = i;
            //    js.Amount = Data[i].Sum(x => x.amount);
            //}

            //WeeklySalesList.Add(js);

            return WeeklySalesList;
        }
        public List<JsChart> MonthlySales()
        {
            var QueryMonth = DateTime.Now.AddYears(-1);
            QueryMonth = QueryMonth.AddMonths(1);
            QueryMonth = QueryMonth.AddDays(-(QueryMonth.Day - 1));
            QueryMonth = QueryMonth.AddTicks(-(QueryMonth.TimeOfDay.Ticks));


            var Data = (from transMain in db.TRANS_MN.Where(x => x.STATUS == Constants.DocumentStatus.AuthorizedDocument
                        && x.CANCEL == "F" && x.TRANS_TYPE == Constants.TransType.Sales)
                        join Trans in db.TRANS_DT.Where(x => (x.VOID == "F" && x.EXCH_FLAG == "T") || (x.VOID == "F" && x.EXCH_FLAG == "F")) on transMain.TRANS_NO equals Trans.TRANS_NO into g

                        where  transMain.TRANS_DATE.Year >= QueryMonth.Year
                        select new JsChart
                        {
                            MonthNumber = transMain.TRANS_DATE.Month,
                            //Amount = g.Sum(s => s.AMOUNT) 
                            Amount = transMain.CASH_AMT
                        }).GroupBy(x => x.MonthNumber).OrderByDescending(x => x.Key).ToList();


            List<JsChart> WeeklySalesList = new List<JsChart>();

            var j = 0;
            for (int i = QueryMonth.Month; i <= (QueryMonth.Month + 11); i++)
            {
                JsChart js = new JsChart();
                var monthNumber = i > 12 ? i - 12 : i;
                var single = Data.Where(x => x.Key == monthNumber).SingleOrDefault();
                if (single != null)
                {
                    js.MonthNumber = single.Key;
                    js.Month = GetMonthName(single.Key);
                    js.Amount = single.Sum(x => x.Amount);
                    j++;
                }
                else
                {
                    js.MonthNumber = monthNumber;
                    js.Month = GetMonthName(monthNumber);
                    js.Amount = 0;
                }
                WeeklySalesList.Add(js);
            }
            return WeeklySalesList;




































        }
        public List<JsChart> StockByDepartment()
        {
            var data = (
                from department in db.DEPARTMENTs
                from products in db.PRODUCTS.Where(x => x.DEPT_CODE == department.DEPT_CODE).DefaultIfEmpty()
                from prodBalance in db.PROD_BALANCE.Where(x => x.BARCODE == products.BARCODE).DefaultIfEmpty()
                select new JsChart
                {
                    DeptCode = department.DEPT_CODE,
                    DeptName = department.DEPT_NAME,
                    Amount = (prodBalance.GRN_QTY ?? 0) + (prodBalance.TRANSFER_IN_QTY ?? 0) - (prodBalance.GRF_QTY ?? 0) - (prodBalance.SALE_QTY ?? 0) - (prodBalance.TRANSFER_OUT_QTY ?? 0) - (prodBalance.WAST_QTY ?? 0)
                }).GroupBy(x => x.DeptCode).ToList();

            List<JsChart> List = new List<JsChart>();
            foreach (var item in data)
            {
                JsChart js = new JsChart();
                js.DeptCode = item.Key;
                js.DeptName = item.Where(x => x.DeptCode == item.Key).Select(x => x.DeptName).FirstOrDefault();
                js.Amount = item.Sum(x => x.Amount);
                List.Add(js);
            }

            return List;
        }
        public List<JsChart> SaleByDepartment()
        {
            var data = (
                 from department in db.DEPARTMENTs
                 from products in db.PRODUCTS.Where(x => x.DEPT_CODE == department.DEPT_CODE).DefaultIfEmpty()
                 from prodBalance in db.PROD_BALANCE.Where(x => x.BARCODE == products.BARCODE).DefaultIfEmpty()
                 select new JsChart
                 {
                     DeptCode = department.DEPT_CODE,
                     DeptName = department.DEPT_NAME,
                     Amount = (prodBalance.SALE_QTY ?? 0)
                 }).GroupBy(x => x.DeptCode).ToList();

            List<JsChart> List = new List<JsChart>();
            foreach (var item in data)
            {
                JsChart js = new JsChart();
                js.DeptCode = item.Key;
                js.DeptName = item.Where(x => x.DeptCode == item.Key).Select(x => x.DeptName).FirstOrDefault();
                js.Amount = item.Sum(x => x.Amount);
                List.Add(js);
            }

            return List;
        }
        public string GetMonthName(int month)
        {
            var MonthName = "";
            switch (month)
            {
                case 1:
                    MonthName = "January";
                    break;
                case 2:
                    MonthName = "February";
                    break;
                case 3:
                    MonthName = "March";
                    break;
                case 4:
                    MonthName = "April";
                    break;
                case 5:
                    MonthName = "May";
                    break;
                case 6:
                    MonthName = "June";
                    break;
                case 7:
                    MonthName = "July";
                    break;
                case 8:
                    MonthName = "August";
                    break;
                case 9:
                    MonthName = "September";
                    break;
                case 10:
                    MonthName = "October";
                    break;
                case 11:
                    MonthName = "November";
                    break;
                case 12:
                    MonthName = "December";
                    break;
                default:
                    MonthName = "Invalid Month";
                    break;
            }
            return MonthName;
        }
        public string GetDayName(int day)
        {
            var MonthName = "";
            switch (day)
            {
                case 1:
                    MonthName = "Monday";
                    break;
                case 2:
                    MonthName = "Tuesday";
                    break;
                case 3:
                    MonthName = "Wednesday";
                    break;
                case 4:
                    MonthName = "Thursday";
                    break;
                case 5:
                    MonthName = "Friday";
                    break;
                case 6:
                    MonthName = "Satudray";
                    break;
                case 0:
                    MonthName = "Sunday";
                    break;
                default:
                    MonthName = "Invalid Day";
                    break;
            }
            return MonthName;
        }
        public string GetPreviousDayName(string day)
        {
            var MonthName = "";
            switch (day)
            {
                case "Monday":
                    MonthName = "Sunday";
                    break;
                case "Tuesday":
                    MonthName = "Monday";
                    break;
                case "Wednesday":
                    MonthName = "Tuesday";
                    break;
                case "Thursday":
                    MonthName = "Wednesday";
                    break;
                case "Friday":
                    MonthName = "Thursday";
                    break;
                case "Satudray":
                    MonthName = "Friday";
                    break;
                case "Sunday":
                    MonthName = "Satudray";
                    break;
                default:
                    MonthName = "Invalid Day";
                    break;
            }
            return MonthName;
        }
    }
}