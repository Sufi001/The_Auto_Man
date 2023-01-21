using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace Inventory.ViewModel
{
    public class CustomerSupplierViewModel
    {
        public string SUPL_NAME { get; set; }
        public string SUPL_CODE { get; set; }
        public string CUST_CODE { get; set; }
        public string Terms { get; set; }
        public string TransNo { get; set; }
        public decimal? TransPoint { get; set; }
        public decimal? RedemPoint { get; set; }
        public DateTime TransDate { get; set; }
        public decimal TransAmount { get; set; }
        [Required]
        [MaxLength(70)]
        //[RegularExpression("^([a-zA-Z]+ +)*[a-zA-Z]+$", ErrorMessage = "Use Alphabets only please")]
        public string NAME { get; set; }
        [Required]

        //[RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{5})$", ErrorMessage = "Not a valid Phone number")]
        public string Mobile { get; set; }
        //  [Required]
        //[RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{5})$", ErrorMessage = "Not a valid Phone number")]
        public string Phone { get; set; }

        //[RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{5})$", ErrorMessage = "Not a valid Phone number")]
        public string Phone2 { get; set; }
        // [Required]
        public string ADDRESS { get; set; }
        //[Required]
        [EmailAddress]
        public string EMAIL { get; set; }
        public string NTN { get; set; }
        public string STRN { get; set; }
        public string CNIC { get; set; }

        //[Required]
        public string Contact_Person { get; set; }
        public string STATUS { get; set; }
        // [Required]
        public string LOCATION { get; set; }
        public string CITY { get; set; }
        public string BusinessName { get; set; }
        public string BusinessUrl { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public HttpPostedFileBase CnicPic { get; set; }
        public HttpPostedFileBase Picture { get; set; }
        public string Comments { get; set; }
        public string PayTerms { get; set; }
        public decimal? PayDays { get; set; }
        public string AREA { get; set; }
        public string AREA_NAME { get; set; }
        public string UrduName { get; set; }

        public string DiscountCategory { get; set; }

        //  [Required]
        [Range(-99999999, 99999999)]
        public Nullable<decimal> Balance { get; set; }

        //[Required]
        //[RegularExpression("^[0-9]*$", ErrorMessage = "Use Numbers only please")]
        public string account_code { get; set; }

        public Nullable<System.DateTime> Proof_DT { get; set; }
        public string Proof_By { get; set; }

        public List<CUSTOMER_DISCOUNT_CATEGORY> Discountcategorylist { get; set; }
        public IEnumerable<LOCATION> LocationList { get; set; }
        public IEnumerable<CITY> CityList { get; set; }
        public IEnumerable<AREA> AreaList { get; set; }
        public IEnumerable<STAFF_STATUS> StaffStatusRoles { get; set; }
        public IEnumerable<CustomerSupplierViewModel> CustomerSupplierList { get; set; }
        public string StaffRole { get; set; }
        [Range(-99999999, 99999999)]
        public Nullable<decimal> OpeningBalance { get; set; }

        public IEnumerable<SelectListItem> PayTermsList { get { return getPayTerms(); } }

        public IEnumerable<SelectListItem> getPayTerms()
        {
            IList<SelectListItem> items = new List<SelectListItem>
            {
                new SelectListItem{Text = "Cash", Value = "1"},
                new SelectListItem{Text = "Credit", Value = "2"},
                new SelectListItem{Text = "Sales Base", Value = "3"},
            };
            return items;
        }


    }

}