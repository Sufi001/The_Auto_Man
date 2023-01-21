using System.Collections.Generic;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Inventory.ViewModels
{
    public class GlChartViewModel
    {
        public string ACCOUNT_CATEGORY{ get; set; }
        public string ACCOUNT_CODE{ get; set; }
        [Required]
        public string ACCOUNT_TITLE{ get; set; }
        public string ACCOUNT_TYPE{ get; set; }
        public string CITY_CODE{ get; set; }
        public string DOC{ get; set; }
        public string INSERTED_BY{ get; set; }
        public string LEVEL_NO{ get; set; }
        public string MAIN_ACCOUNT{ get; set; }
        public decimal? OPEN_BAL{ get; set; }
        public decimal? OPEN_BAL_CR { get; set; }
        public string PARTY_TYPE{ get; set; }
        public string PHONE { get; set; }
        public string TITLE_URDU { get; set; }
        public string TRANSFER_STATUS{ get; set; }
        public string MAIN_ACCOUNT_CODE{ get; set; }
        public string CONTROL_CODE{ get; set; }
        public string SUB_CONTROL_CODE{ get; set; }
        public string SUBSIDIARY_CONTROL_CODE{ get; set; }

        public IEnumerable<SelectListItem> MainAccountList { get; set; }
        public IEnumerable<SelectListItem> ControlAccountList { get; set; }
        public IEnumerable<SelectListItem> SubControlAccountList { get; set; }
        public IEnumerable<SelectListItem> SubsidiaryControlAccountList { get; set; }

        //Generate account types to create new account head or sub types
        public List<SelectListItem> AccountTypeList = new List<SelectListItem>()
        {
            new SelectListItem() {Text="ASSET", Value="A"},
            new SelectListItem() { Text="LIABILITY", Value="L"},
            new SelectListItem() { Text="EXPENSE", Value="E"},
            new SelectListItem() { Text="REVENUE", Value="R"}
        };

        public List<SelectListItem> AccountCategoryList = new List<SelectListItem>()
        {
            new SelectListItem() {Text="CA-Cash Account", Value="CA"},
            new SelectListItem() { Text="BA-Bank Account", Value="BA"},
        };
    }
}