using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Inventory.ViewModels.Item
{
    public class ItemRegistration
    {
        private DataContext db;
        public ItemRegistration()
        {
            db = new DataContext();
        }
        [Required]
        [Display(Name = "Department")]
        public string DeptCode { get; set; }
        [Required]
        [Display(Name = "Group")]
        public string GroupCode { get; set; }
        [Required]
        [Display(Name = "Sub Group")]
        public string SubGroupCode { get; set; }
        [Required]
        [Display(Name = "Name")]
        [StringLength(70)]
        public string Description { get; set; }
        [Required]
        public string Type { get; set; }
        [Range(0, double.MaxValue)]
        [Display(Name = "Unit Cost")]
        public decimal? UnitCost { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        [Display(Name = "Unit Retail")]
        public decimal? UnitRetail { get; set; }
        public string Barcode { get; set; }
        public string Comments { get; set; }
        public string UanNo { get; set; }
        public List<string> UanNoList { get; set; }
        [Required]
        public string SupplierCode { get; set; }
        [Range(0, double.MaxValue)]
        public decimal? PackRetail { get; set; }
        [Range(0, double.MaxValue)]
        public decimal? MarketRetail { get; set; }
        [Range(0, double.MaxValue)]
        public decimal? PackQuantity { get; set; }
        [Range(0, double.MaxValue)]
        public decimal? MinRetail { get; set; }
        public HttpPostedFileBase Picture { get; set; }
        public HttpPostedFileBase Picture2 { get; set; }
        public HttpPostedFileBase Picture3 { get; set; }
        public HttpPostedFileBase Picture4 { get; set; }
        [Range(0, double.MaxValue)]
        public decimal? MinQuantity { get; set; }
        public decimal? OnHandQty { get; set; }
        [Range(0, double.MaxValue)]
        public decimal? ReorderLevel { get; set; }
        [Range(0, double.MaxValue)]
        public decimal? MaxQuantity { get; set; }
        public decimal? OnHandAmt { get; set; }
        public decimal? AvgCost { get; set; }
        [Range(0, double.MaxValue)]
        public decimal? CtnPcs { get; set; }
        public decimal? BulkQty { get; set; }
        public decimal? BulkRetail { get; set; }
        public bool Offer { get; set; }
        public decimal? PromoPercentage { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? StartOn { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? EndOn { get; set; }
        public bool? Active { get; set; }
        public string UrduName { get; set; }
        public string ReferenceCode { get; set; }
        public string ImgDesc { get; set; }
        public string ImgDesc2 { get; set; }

        //[Required]
        [Display(Name = "GST Percentage")]
        public decimal? GST_Percentage { get; set; }

        //[Required]
        public string GSTType { get; set; }

        public string Image { get; set; }
        public string Image1 { get; set; }
        public string Image2 { get; set; }
        public string Image3 { get; set; }
        public string Image4 { get; set; }
        
        public string Status { get; set; }
        public string SearchingKeywords { get; set; }

        public string IsRfCodeRequired = ConfigurationManager.AppSettings["RfCode"];


        public IEnumerable<SelectListItem> StatusDDL = new List<SelectListItem>
            {
                new SelectListItem{Text = "Active", Value = "a"},
                new SelectListItem{Text = "Deactive", Value = "i"},
            };
        public IEnumerable<SelectListItem> BrandList
        {
            get
            {
                return db.BRANDs.ToList().Select(x => new SelectListItem
                {
                    Value = x.ID.ToString(),
                    Text = x.NAME
                });
            }
        }
        public string Brand { get; set; }


    }


}