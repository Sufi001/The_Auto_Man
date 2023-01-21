using Inventory.ViewModels.Item;
using System.Collections.Generic;

namespace Inventory.ViewModels
{

    public class TransferPageViewModel
    {
        public TransferMainViewModel TransferMain { get; set; }
        public List<TransferDetailViewModel> TransferDetailList { get; set; }
        public List<LOCATION> LocationList { get; set; }
        public List<WAREHOUSE> WarehouseList { get; set; }
        public List<ItemViewModel> ItemList { get; set; }
        public string Bcode { get; set; }
        public List<BranchViewModel> Branches { get; set; }
    }
}