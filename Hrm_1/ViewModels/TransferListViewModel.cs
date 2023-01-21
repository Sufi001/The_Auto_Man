using System;
using System.Collections.Generic;
using System.Linq;

namespace Inventory.ViewModels
{
    public class TransferListViewModel
    {
        public string DocNo { get; set; }
        public DateTime Doc { get; set; }
        public string Location { get; set; }
        public string Warehouse { get; set; }
        public decimal Amount { get; set; }
        public string DocType { get; set; }
        public string Status { get; set; }
        public string BranchFrom { get; set; }
        public string BranchTo { get; set; }
        public List<TransferListViewModel> GetStockTransferList(string doctype)
        {
            DataContext context = new DataContext();
            var list = (from transferMain in context.TRANSFER_MAIN
                        from loc in context.LOCATIONs.Where(x=>x.LOC_ID == transferMain.LOCATION).DefaultIfEmpty()
                        from branchFrom in context.BRANCHes.Where(x=>x.BRANCH_CODE == transferMain.BRANCH_ID_FROM).DefaultIfEmpty()
                        from branchTo in context.BRANCHes.Where(x=>x.BRANCH_CODE == transferMain.BRANCH_ID_TO).DefaultIfEmpty()
                        where transferMain.DOC_TYPE == doctype
                        let Amount = (decimal?)((from a in context.TRANSFER_DETAIL
                                                 where transferMain.DOC_NO == a.DOC_NO
                                                 select (a.COST * a.QTY)).Sum()) ?? 0
                        select new TransferListViewModel
                        {
                            Doc = transferMain.DOC,
                            DocNo = transferMain.DOC_NO,
                            DocType = transferMain.DOC_TYPE,
                            Location = loc.NAME,
                            Amount = Amount,
                            Status = transferMain.STATUS == "0" ? "Unauthorized" : "Authorized",
                            BranchFrom = branchFrom.BRANCH_NAME ?? "MAIN",
                            BranchTo = branchTo.BRANCH_NAME,
                        }).OrderByDescending(x=>x.DocNo).ToList();


            //return list.Select(item => new TransferListViewModel
            //{
            //    DocType = item.DocType,
            //    DocNo = item.DocNo,
            //    Doc = item.Doc,
            //    Location = item.LocationType,
            //    Amount = item.Amount,
            //    Status = (item.Status == "0") ? "Unauthorized" : "Authorized"
            //})
            //    .ToList();


            return list;
        }
    }
}