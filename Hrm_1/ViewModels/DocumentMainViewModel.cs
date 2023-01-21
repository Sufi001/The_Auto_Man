using Inventory.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Inventory.ViewModels
{
    public class DocumentMainViewModel
    {
        public string DocNo { get; set; }
        public string DocNoDisplay { get; set; }
        [Required]
        public System.DateTime DocDate { get; set; }
        [Required]
        public string SuplCode { get; set; }
        public string Print { get; set; }
        public TimeSpan Time { get; set; }
        public string Location { get; set; }
        public string Phone  { get; set; }
        //[Required]
        public string Warehouse { get; set; }
        public string DocType { get; set; }
        public string Comments { get; set; }
        public string Userid { get; set; }
        public string Status { get; set; }
        public string DnNo { get; set; }
        public string staffcode { get; set; }
        public string CustomerName  { get; set; }
        public string TotalAmount { get; set; }
        public string DiscointAmount { get; set; }
        public string Advance { get; set; }
        public string Payment { get; set; }
        public decimal? Balance { get; set; }
        public decimal? FreeQty { get; set; }
        public string ReturnAmount { get; set; }
        public string RequestPage { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string BranchId { get; set; }
        public string GetPurchaseCode(string prefix, int i)
        {
            DataContext context = new DataContext();
            string d = DateTime.Now.ToString("yyyy");
            d = d.Substring(2, 2);
            d = prefix + d;
            string s = context.GRN_MAIN.Where(u => u.GRN_NO.StartsWith(d)).Max(u => u.GRN_NO);
            if (string.IsNullOrEmpty(s))
            {
                return d + "0001";
            }
            string code = CommonFunctions.GenerateCode(s.Substring(4), i);
            return d + code;
        }
        public string GetPurchaseReturnCode(string prefix, int i)
        {
            DataContext context = new DataContext();
            string d = DateTime.Now.ToString("yyyy");
            d = d.Substring(2, 2);
            d = prefix + d;
            string s = context.GRFS_MAIN.Where(u => u.GRF_NO.StartsWith(d)).Max(u => u.GRF_NO);
            if (string.IsNullOrEmpty(s))
            {
                return d + "0001";
            }
            string code = CommonFunctions.GenerateCode(s.Substring(4), i);
            return d + code;
        }
        public string GetSaleCode(string prefix, int i)
        {
            DataContext context = new DataContext();
            string d = DateTime.Now.ToString("yyyy");
            d = d.Substring(2, 2);
            d = prefix + d;
            string s = context.TRANS_MN.Where(u => u.TRANS_NO.StartsWith(d) 
            ).Max(u => u.TRANS_NO);
            if (string.IsNullOrEmpty(s))
            {
                return d + "000001";
            }
            string code = CommonFunctions.GenerateCode(s.Substring(4), i);
            return d + code;
        }
        public string GetDNCode()
        {
            DataContext db = new DataContext();
            string s = db.DN_MAIN.Max(u => u.DN_NO);
            if (string.IsNullOrEmpty(s))
            {
                return "000001";
            }
            return CommonFunctions.GenerateCode(s, 6);
        }

        //public string GetSaleCode(string prefix, int i, string doctype)
        //{
        //    DataContext context = new DataContext();
        //    string d = DateTime.Now.ToString("yyyy");
        //    d = d.Substring(2, 2);
        //    d = prefix + d;
        //    string s = context.TRANS_MN.Where(u => u.TRANS_NO.StartsWith(d)
        //    //&& u.TRANS_TYPE == doctype
        //    ).Max(u => u.TRANS_NO);
        //    if (string.IsNullOrEmpty(s))
        //    {
        //        return d + "000001";
        //    }
        //    CommonFunctions obj = new CommonFunctions();
        //    string code = obj.GenerateCode(s.Substring(4), i);
        //    return d + code;
        //}
        //public List<DocumentMainViewModel> getOnlineBooking()
        //{
        //    DataContext context = new DataContext();
        //  //  List<DocumentMainViewModel> List = new List<DocumentMainViewModel>();
        //    var date = DateTime.Now.Date;
        //    var staffdata = context.STAFF_MEMBER.ToList();
        //    var data = context.TRANS_OnMn.Where(x => x.TRANS_DATE >= date&&x.TRANS_TYPE=="2").Select(x => new DocumentMainViewModel
        //    {
        //        DocNo = x.TRANS_NO,
        //        DocDate = x.TRANS_DATE,
        //        CustomerName = x.reference_person,
        //        //  staffcode=staffdata.Where(u=>u.SUPL_CODE==x.s)
        //    }).ToList();
        //    return data;
        //} 
        public string GetAdjustmentCode(string prefix, int i)
        {
            DataContext context = new DataContext();
            string d = DateTime.Now.ToString("yyyy");
            d = d.Substring(2, 2);
            d = prefix + d;
            string s = context.STOCK_MAIN.Where(u => u.STOCK_NO.StartsWith(d)).Max(u => u.STOCK_NO);
            if (string.IsNullOrEmpty(s))
            {
                return d + "0001";
            }
            string code = CommonFunctions.GenerateCode(s.Substring(4), i);
            return d + code;
        }
    }
}