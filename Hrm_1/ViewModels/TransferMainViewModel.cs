using Inventory.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Inventory.ViewModels
{
    public class TransferMainViewModel
    {
        public string LocationType { get; set; }
        public string doc_no { get; set; }
        public string DocNoDisplay { get; set; }
        public System.DateTime doc_date { get; set; }
        public string doc_type { get; set; }
        public List<LocationType> LocationTypeList { get; set; }
       // [Required]
        public string location { get; set; }
       // [Required]
        public string warehouse { get; set; }
        public string userid { get; set; }
        public string Status { get; set; }
        public string GetTransferCode(string prefix, int i)
        {
            DataContext db = new DataContext();
            var LoginLovation = Convert.ToInt16(CommonFunctions.GetLocation());
            string d = DateTime.Now.ToString("yyyy");
            d = d.Substring(2, 2);

            d = prefix + LoginLovation + d;
            string s = db.TRANSFER_MAIN.Where(u => u.DOC_NO.StartsWith(d)).Max(u => u.DOC_NO);
            if (string.IsNullOrEmpty(s))
                return d + "0000001";

            string code = CommonFunctions.GenerateCode(s.Substring(4), i);
            return d + code;
        }
        public List<LocationType> getLocationTypeList ()
        {
            List<LocationType> list = new List<LocationType>();
            LocationType obj = new LocationType();
            obj.Location_Id = "1";
            obj.Location_Name = "Store";
            list.Add(obj);
            LocationType obj1 = new LocationType();
            obj1.Location_Id = "2";
            obj1.Location_Name = "Parler";
            list.Add(obj1);
            return list;
        }
        public string BranchIdFrom { get; set; }
        public string BranchIdTo { get; set; }
    }
    public class LocationType
    {
        public string Location_Id { get; set; }
        public string Location_Name { get; set; }
    }
}