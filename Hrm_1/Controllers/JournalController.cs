using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Inventory.ViewModels.Journal;
using Newtonsoft.Json;
using Inventory.Filters;
using Inventory.Helper;

namespace Inventory.Controllers
{
    [Authorize]
    public class JournalController : Controller
    {
        DataContext db;
        public JournalController()
        {
            db = new DataContext();
        }
        // GET: Journal
        [Permission("Reservation Invoice")]
        public ActionResult ReservationInvoice()
        {
            JournalViewModel VM = new JournalViewModel();
            VM.VoucherDate = DateTime.Now.Date;
            VM.Dated = DateTime.Now.Date;
            ViewBag.Reservations = db.RESERVATION_DETAIL.Include(x => x.ROOM).Select(x => new { x.RES_ID, x.ROOM_CODE, x.ROOM.ROOM_NAME }).ToList();
            VM.docTypeList = VoucherDocType();
            VM.TransactionMode = TransactionModes();
            return View(VM);
        }
        [Permission("Recipt & Payment")]
        public ActionResult PaymentReceipt()
        {
            JournalViewModel VM = new JournalViewModel();
            VM.VoucherDate = DateTime.Now.Date;
            VM.Dated = DateTime.Now.Date;
            VM.docTypeList = VoucherDocType();
            VM.TransactionMode = TransactionModes();
            VM.CustomerList = db.SUPPLIERs.Where(x => x.PARTY_TYPE == Constants.CustomerSupplier.Customer).Select(x=> new SelectListItem{ Value = x.SUPL_CODE, Text = x.SUPL_NAME}).OrderBy(x=>x.Text).ToList();
            VM.SupplierList = db.SUPPLIERs.Where(x => x.PARTY_TYPE == Constants.CustomerSupplier.Supplier).Select(x=> new SelectListItem{ Value = x.SUPL_CODE, Text = x.SUPL_NAME} ).OrderBy(x => x.Text).ToList();
            VM.StaffList = db.STAFF_MEMBER.Select(x=> new SelectListItem{ Value = x.SUPL_CODE, Text = x.SUPL_NAME} ).ToList();
            VM.AccountListCB = db.GL_CHART.Where(x=>x.MAIN_ACCOUNT == "2010200000").Select(x=> new SelectListItem{ Value = x.ACCOUNT_CODE, Text = x.ACCOUNT_TITLE} ).ToList();
            return View(VM);
        }
        [Permission("Income & Expense")]
        public ActionResult IncomeExpense()
        {
            JournalViewModel VM = new JournalViewModel();
            VM.VoucherDate = DateTime.Now.Date;
            VM.Dated = DateTime.Now.Date;
            VM.docTypeList = IncomeExpenseDocType();
            VM.TransactionMode = TransactionModes();
            VM.AccountListIncome = db.GL_CHART.Where(x => x.ACCOUNT_CODE.StartsWith("4") && x.LEVEL_NO == "4").Select(x => new SelectListItem { Value = x.ACCOUNT_CODE, Text = x.ACCOUNT_TITLE }).ToList();
            VM.AccountListExpense = db.GL_CHART.Where(x => x.ACCOUNT_CODE.StartsWith("3") && x.LEVEL_NO == "4").Select(x => new SelectListItem { Value = x.ACCOUNT_CODE, Text = x.ACCOUNT_TITLE }).ToList();
            VM.AccountListCB = db.GL_CHART.Where(x => x.MAIN_ACCOUNT == "2010200000").Select(x => new SelectListItem { Value = x.ACCOUNT_CODE, Text = x.ACCOUNT_TITLE }).ToList();
            VM.StaffList = db.STAFF_MEMBER.Select(x => new SelectListItem { Value = x.SUPL_CODE, Text = x.SUPL_NAME }).ToList();

            return View(VM);
        }
        public ActionResult List(string viewName)
        {
            List<JournalViewModel> list = new List<JournalViewModel>(); 
            if (viewName == "PaymentReceiptList")
            {
                list = (from journal in db.JOURNALs.Where(x=>x.DOC_TYPE == "P" || x.DOC_TYPE == "R")
                        join customer in db.SUPPLIERs on journal.PARTY_CODE equals customer.SUPL_CODE
                        select new JournalViewModel
                        {
                            VoucherDate = journal.TRANS_DTE,
                            Doc_Type = journal.DOC_TYPE,
                            BookNo = journal.BOOK_NO,
                            BillNo = journal.BILL_NO,
                            Doc_Seq = journal.DOC_SEQ,
                            Status = journal.STATUS == "3" ? "Authorized" : "Unauthorizze",
                            ReceivedFrom = customer.SUPL_NAME,
                            Amount = journal.AMOUNT,
                            StaffCode = journal.STAFF_MEMBER.SUPL_NAME
                        }).OrderByDescending(x=>x.VoucherDate).ToList();
            }
            else if (viewName == "InvoiceList")
            {

            }
            else if (viewName == "IncomeExpenseList")
            {
                list = (from journal in db.JOURNALs.Where(x => x.DOC_TYPE == "I" || x.DOC_TYPE == "E")
                        join account in db.GL_CHART on journal.ACCOUNT_CODE_IE equals account.ACCOUNT_CODE
                        select new JournalViewModel
                        {
                            Doc_Seq = journal.DOC_SEQ,
                            ReceivedFrom = account.ACCOUNT_TITLE,
                            Amount = journal.AMOUNT
                 }).ToList();
            }

            

            return View(viewName,list);
        }
        public ActionResult Save(JournalViewModel VM)
        {
            DbContextTransaction transaction = db.Database.BeginTransaction();
            try
            {
                JOURNAL journal = new JOURNAL();
                if (string.IsNullOrEmpty(VM.Doc_Seq))
                {
                    var docType = VM.Doc_Type;
                    journal.DOC_TYPE = docType;
                    journal.DOC_SEQ = Doc_TypeCode(docType);
                    journal.INSERTED_BY = CommonFunctions.GetUserName();
                    journal.INSERTED_DTE = CommonFunctions.GetDateTimeNow();
                }
                else
                {
                    journal = db.JOURNALs.Where(x => x.DOC_SEQ == VM.Doc_Seq).SingleOrDefault();
                    
                }

                if (VM.Doc_Type == "R" || VM.Doc_Type == "I")
                {
                    journal.CREDIT_AMOUNT = VM.Amount;
                    journal.AMOUNT = VM.Amount;
                }
                else if (VM.Doc_Type == "P" || VM.Doc_Type == "E")
                {
                    journal.DEBIT_AMOUNT = VM.Amount;
                    journal.AMOUNT = VM.Amount;
                }

                journal.UPDATED_BY = CommonFunctions.GetUserName();
                journal.UPDATED_DTE = CommonFunctions.GetDateTimeNow();
                journal.ACCOUNT_CODE = VM.AccountCode;
                journal.ACCOUNT_CODE_IE = VM.AccountCodeIE;
                journal.ACCOUNT_TITTLE = VM.AccountTitle;
                journal.ACCOUNT_TITTLE_IE = VM.AccountTitleIE;
                journal.TRANS_DTE = VM.VoucherDate.Value.Date + DateTime.Now.TimeOfDay;
                journal.REFERENCE_DTE = VM.Dated;
                //journal.BANK_CODE = VM
                journal.CHQ_NBR = VM.ChequeNo == null ? VM.ChequeNo : VM.ChequeNo.Trim();
                journal.NARRATION = VM.Narration;
                journal.RES_ID = db.RESERVATION_DETAIL.Where(x=>x.ROOM_CODE == VM.RoomCode).Select(x=>x.RES_ID).SingleOrDefault();
                journal.ROOM_CODE = VM.RoomCode;
                journal.PARTY_TYPE = VM.Party_Type;
                //journal.PAY_TYPE = VM
                journal.REFERENCE = VM.ReceivedFrom == null ? VM.ReceivedFrom : VM.ReceivedFrom.Trim();
                //journal.REFERENCE_DTE = VM
                //journal.REFERENCE_PERSON = VM
                journal.STATUS = VM.Status;

                if (VM.Status == Constants.DocumentStatus.AuthorizedDocument && (VM.Doc_Type == "R" || VM.Doc_Type == "P"))
                {
                    var response = ChangeInSupplier(VM.CustomerCode, VM.Amount);
                    if (!response)
                        throw new Exception();
                }

                journal.V_TYPE = VM.PaymentMode;
                //journal.TOTAL_AMT = VM;
                journal.TRANSFER_STATUS = "0";

                journal.PARTY_CODE = VM.CustomerCode;
                journal.STAFF_CODE = VM.StaffCode;
                journal.BILL_NO = VM.BillNo;
                journal.BOOK_NO = VM.BookNo;

                journal.BANK_NAME = VM.BankName;
                journal.PAYMENT_PURPOSE = VM.PaymentPurpose;
                journal.COMPANY = VM.Company;
                journal.AMOUNT_IN_WORDS = VM.AmountInWords;
                //journal.V_TYPE = VM

                if (string.IsNullOrEmpty(VM.Doc_Seq))
                {
                    db.JOURNALs.Add(journal);
                }
                db.SaveChanges();
                transaction.Commit();
                return Content(journal.DOC_SEQ);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return Content("Ex");
            }
        }
        [Permission("Edit Document")]
        public ActionResult Edit(string id)
        {
            JournalViewModel JVM = new JournalViewModel();
            ViewBag.Reservations = db.RESERVATION_DETAIL.Include(x=>x.ROOM).Select(x => new { x.RES_ID, x.ROOM_CODE,x.ROOM.ROOM_NAME }).ToList();
            JVM.TransactionMode = TransactionModes();
            try
            {
                var journal = db.JOURNALs.Where(x => x.DOC_SEQ == id).SingleOrDefault();
                if (journal != null)
                {
                    ViewBag.Update = true;

                    JVM.Doc_Type = journal.DOC_TYPE;
                    JVM.VoucherDate = journal.TRANS_DTE;
                    JVM.Doc_Seq = journal.DOC_SEQ;
                    JVM.Amount = journal.AMOUNT;
                    JVM.Dated = journal.TRANS_DTE;
                    if (journal.RES_ID != null)
                        JVM.RES_ID = journal.RES_ID.Value;
                    JVM.RoomCode = journal.ROOM_CODE;
                    JVM.Dated = journal.REFERENCE_DTE;
                    JVM.ReceivedFrom = journal.REFERENCE;
                    JVM.BookNo = journal.BOOK_NO;
                    JVM.BillNo = journal.BILL_NO;
                    JVM.StaffCode = journal.STAFF_CODE;
                    JVM.CustomerCode = journal.PARTY_CODE;
                    JVM.Narration = journal.NARRATION;
                    JVM.Status = journal.STATUS;
                    JVM.BankName = journal.BANK_NAME;
                    JVM.PaymentPurpose = journal.PAYMENT_PURPOSE;
                    JVM.ChequeNo = journal.CHQ_NBR;
                    JVM.AmountInWords = journal.AMOUNT_IN_WORDS;
                    JVM.Company = journal.COMPANY;
                    JVM.AccountCode = journal.ACCOUNT_CODE;
                    JVM.AccountCodeIE = journal.ACCOUNT_CODE_IE;
                    JVM.PaymentMode = journal.V_TYPE;

                    JVM.StaffList = db.STAFF_MEMBER.Select(x => new SelectListItem { Value = x.SUPL_CODE, Text = x.SUPL_NAME }).ToList();

                    if (!string.IsNullOrEmpty(journal.ACCOUNT_CODE))
                    {
                        JVM.AccountListCB = db.GL_CHART.Where(x => x.ACCOUNT_CODE == journal.ACCOUNT_CODE).Select(x => new SelectListItem { Value = x.ACCOUNT_CODE, Text = x.ACCOUNT_TITLE }).ToList();
                    }
                    else
                    {
                        if (journal.V_TYPE == "CR" || journal.V_TYPE == "CP")
                        {
                            JVM.AccountListCB = db.GL_CHART.Where(x => x.MAIN_ACCOUNT == "2010200000").Select(x => new SelectListItem { Value = x.ACCOUNT_CODE, Text = x.ACCOUNT_TITLE }).ToList();
                        }
                        else if (journal.V_TYPE == "BR" || journal.V_TYPE == "BP")
                        {
                            JVM.AccountListCB = db.GL_CHART.Where(x => x.MAIN_ACCOUNT == "2010300000").Select(x => new SelectListItem { Value = x.ACCOUNT_CODE, Text = x.ACCOUNT_TITLE }).ToList();
                        }
                    }


                    string view ="";
                    if (journal.DOC_TYPE == "R" || journal.DOC_TYPE == "P")
                    {
                        view = "PaymentReceipt";
                        JVM.CustomerList = db.SUPPLIERs.Where(x => x.PARTY_TYPE == Constants.CustomerSupplier.Customer).Select(x => new SelectListItem { Value = x.SUPL_CODE, Text = x.SUPL_NAME }).ToList();
                        JVM.SupplierList = db.SUPPLIERs.Where(x => x.PARTY_TYPE == Constants.CustomerSupplier.Supplier).Select(x => new SelectListItem { Value = x.SUPL_CODE, Text = x.SUPL_NAME }).ToList();
                        JVM.docTypeList = VoucherDocType();
                    }
                    else if (journal.DOC_TYPE == "I" || journal.DOC_TYPE == "E")
                    {
                        view = "IncomeExpense";
                        JVM.AccountListIncome = db.GL_CHART.Where(x => x.ACCOUNT_CODE.StartsWith("4") && x.LEVEL_NO == "4").Select(x => new SelectListItem { Value = x.ACCOUNT_CODE, Text = x.ACCOUNT_TITLE }).ToList();
                        JVM.AccountListExpense = db.GL_CHART.Where(x => x.ACCOUNT_CODE.StartsWith("3") && x.LEVEL_NO == "4").Select(x => new SelectListItem { Value = x.ACCOUNT_CODE, Text = x.ACCOUNT_TITLE }).ToList();
                        JVM.docTypeList = IncomeExpenseDocType();
                    }
                    else
                    {
                        view = "ReservationInvoice";
                        JVM.docTypeList = VoucherDocType();
                    }

                    return View(view, JVM);
                }
                else
                {
                    return Content("Invalid Request");
                }
            }
            catch (Exception ex)
            {
                return Content("Ex");
            }
        }
        public static IEnumerable<SelectListItem> VoucherDocType()
        {
            IList<SelectListItem> items = new List<SelectListItem>
            {
                new SelectListItem{Text = "Receipt", Value = "R"},
                new SelectListItem{Text = "Payment", Value = "P"},
            };
            return items;
        }
        public static IEnumerable<SelectListItem> IncomeExpenseDocType()
        {
            IList<SelectListItem> items = new List<SelectListItem>
            {
                new SelectListItem{Text = "Income", Value = "I"},
                new SelectListItem{Text = "Expense", Value = "E"},
            };
            return items;
        }
        public static IEnumerable<SelectListItem> TransactionModes()
        {
            IList<SelectListItem> items = new List<SelectListItem>
            {
                new SelectListItem{Text = "CR-Cash Receipt Voucher", Value = "CR"},
                new SelectListItem{Text = "BR-Bank Receipt Voucher", Value = "BR"},
                //new SelectListItem{Text = "CP-Cash Payment Voucher", Value = "CP"},
                //new SelectListItem{Text = "BP-Bank Payment Voucher", Value = "BP"},
            };
            return items;
        }
        //public static IEnumerable<SelectListItem> IncomeExpenseTransactionModes()
        //{
        //    IList<SelectListItem> items = new List<SelectListItem>
        //    {
        //        new SelectListItem{Text = "CR-Cash Voucher", Value = "CR"},
        //        new SelectListItem{Text = "BR-Bank Voucher", Value = "BR"},
        //    };
        //    return items;
        //}
        public JsonResult AmountInWords(double amount)
        {
            try
            {
                var amountInWords = Helper.CommonFunctions.ConvertAmount(amount);
                return Json(amountInWords, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json("Ex", JsonRequestBehavior.AllowGet);
            }
            
        }
        private string Doc_TypeCode(string prefix)
        {
            var Doc_Code = "0";
            string year = DateTime.Now.Year.ToString();
            year = year.Substring(2, 2);
            string month = DateTime.Now.Month.ToString().PadLeft(2, '0');
            Doc_Code = prefix + year + month;

            string LastCode = db.JOURNALs.Where(a => a.DOC_TYPE == prefix).Max(a => a.DOC_SEQ);

            string checkIfMonthChange = "";
            if (LastCode != null)
            {
                checkIfMonthChange = LastCode.Substring(3, 2);
            }

            if (checkIfMonthChange != month)
            {
                LastCode = null;
            }

            if (string.IsNullOrEmpty(LastCode))
                return Doc_Code += "00001";
            else
            {
                var code = LastCode.Substring(5);
                return Doc_Code += (Convert.ToInt32(code) + 1).ToString().PadLeft(5, '0');
            }
        }
        public bool ChangeInSupplier(string SuplCode, decimal? Amount)
        {
            var supplierCustomer = db.SUPPLIERs.Where(x => x.SUPL_CODE == SuplCode).SingleOrDefault();
            if ((supplierCustomer.BALANCE ?? 0) >= Amount)
            {
                supplierCustomer.BALANCE = (supplierCustomer.BALANCE ?? 0) - Amount;
            }
            else
            {
                var remmainingAmount = (supplierCustomer.BALANCE ?? 0) - Amount; //Return Amount In Negitive
                supplierCustomer.OPENING = (supplierCustomer.OPENING ?? 0) + (remmainingAmount);
            }

            supplierCustomer.UPDATED_BY = CommonFunctions.GetUserID();
            supplierCustomer.UPDATE_DATE = CommonFunctions.GetDateTimeNow();

            db.SaveChanges();
            return true;
        }
        public ActionResult GetAccount(string id)
        {
            if (string.IsNullOrEmpty(id))
                return Content("Parameter Can't Be Null or Empty");

            var mainAccountCode = "";
            if (id == "BR" || id == "BP")
                mainAccountCode = "2010300000";
            else if (id == "CR" || id == "CP")
                mainAccountCode = "2010200000";

            var accountList = db.GL_CHART.Where(x => x.MAIN_ACCOUNT == mainAccountCode).Select(x => new { Value = x.ACCOUNT_CODE , Text = x.ACCOUNT_TITLE }).ToList();
            if (accountList.Count > 0)
                return Json(accountList, JsonRequestBehavior.AllowGet);
            else
                return Json("", JsonRequestBehavior.AllowGet);
        }
        [Permission("Authorize")]
        public ActionResult Authorize(JournalViewModel VM)
        {
            try
            {
                VM.Status = Constants.DocumentStatus.AuthorizedDocument;
                var response = Save(VM);
                return response;
            }
            catch (Exception)
            {
                return Content("Ex");
            }
        }
    }
}