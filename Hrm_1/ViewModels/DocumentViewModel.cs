using Inventory.ViewModels.Item;
using System.Collections.Generic;

namespace Inventory.ViewModels
{
    public class DocumentViewModel
    {
        public DocumentMainViewModel DocumentMain { get; set; }

        public List<DEPARTMENT> deptlit { get; set; }
        public List<DocumentMainViewModel> Onlinebooking { get; set; }
        public List<DocumentDetailViewModel> DocumentDetailList { get; set; }
        public List<SupplierViewModel> SupplierList { get; set; }
        public List<LOCATION> LocationList { get; set; }
        public List<ItemViewModel> ItemList { get; set; }
        public List<COLOUR> ColourList { get; set; }
        public List<STAFF_MEMBER> Stafflist { get; set; }
        public List<CUSTOMER_DISCOUNT_CATEGORY> DisountCategorylist {get;set;}

        public string Print { get; set; }
        public string Bcode { get; set; }
        public string Warehouse { get; set; }
        public int? Colour { get; set; }
        public string BranchId { get; set; }


    }


}