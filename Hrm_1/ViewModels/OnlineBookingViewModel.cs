using Inventory.Helper;
using Inventory.ViewModels.Item;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Inventory.ViewModels
{
    public class OnlineBookingViewModel
    {
        public string DocNo { get; set; }
        //[Required]
        public System.DateTime DocDate { get; set; }
        //[Required]
        public string SuplCode { get; set; }
        public TimeSpan Time { get; set; }
      //  [Required]
        [StringLength(11)]
        public string Phone { get; set; }
      //  [Required]
        //[RegularExpression("^([a-zA-Z]+ +)*[a-zA-Z]+$", ErrorMessage = "Use Alphabets only please")]
        [StringLength(40)]
        public string FirstName { get; set;}
        public string LastName  { get; set; }
        public string ItemName { get; set; }
        public string Email { get; set; }
        public string ItemCode  { get; set; }
        public string ItemPrice { get; set; }
        public string providerName { get; set; }
        public string providercode { get; set; }
        public string TotalPrice { get; set; }
        public List<OnlineBookingViewModel> DetailBooking { get; set; }

        public List<OnlineBookingViewModel> Providerlists { get; set; }

        public List<ItemViewModel> ItemList { get; set; }
       // public List<StaffMember> Providerlist { get; set; }
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
            string code = CommonFunctions.GenerateCode(s.Substring(3), i);
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
            string code = CommonFunctions.GenerateCode(s.Substring(3), i);
            return d + code;
        }
        //public string GetSaleCode(string prefix, int i)
        //{
        //    DataContext context = new DataContext();
        //    string d = DateTime.Now.ToString("yyyy");
        //    d = d.Substring(2, 2);
        //    d = prefix + d;
        //    string s = context.TRANS_OnDt.Where(u => u.TRANS_NO.StartsWith(d)
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

    }
}