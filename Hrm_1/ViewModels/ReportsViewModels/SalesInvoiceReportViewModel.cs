using System.Collections.Generic;

namespace Inventory.ViewModels.ReportsViewModels
{
    public class SalesInvoiceReportViewModel
    {
        public CompanyViewModel Company { get; set; }
        public ReportMainViewModel ReportMain { get; set; }
        public List<ReportDetailViewModel> ReportDetailList { get; set; }

    }
}