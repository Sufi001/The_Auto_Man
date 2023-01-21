//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.IO;
//using System.Data;
//using System.Text;
//using System.Drawing.Imaging;
//using System.Drawing.Printing;
//using Microsoft.Reporting.WebForms;
//using System.Drawing;
//using CustomerRec;

//namespace Inventory.Models
//{
//    public class Printerclass
//    {
//        private int m_currentPageIndex;
//        private IList<Stream> m_streams;

//        //private DataTable LoadSalesData()
//        //{
//        //    // Create a new DataSet and read sales data file 
//        //    //    data.xml into the first DataTable.
//        //    DataSet dataSet = new DataSet();
//        //    dataSet.ReadXml(@"..\..\data.xml");
//        //    return dataSet.Tables[0];
//        //}
//        // Routine to provide to the report renderer, in order to
//        //    save an image for each page of the report.
//        private Stream CreateStream(string name,
//          string fileNameExtension, Encoding encoding,
//          string mimeType, bool willSeek)
//        {
//            Stream stream = new MemoryStream();
//            m_streams.Add(stream);
//            return stream;
//        }
//        // Export the given report as an EMF (Enhanced Metafile) file.
//        private void Export(LocalReport report)
//        {
//            string deviceInfo =
//              @"<DeviceInfo>
//                <OutputFormat>EMF</OutputFormat>
//                <PageWidth>12in</PageWidth>
//                <PageHeight>11in</PageHeight>
//                <Orientation>Landscape</Orientation> 
//                <MarginTop>0.1in</MarginTop>
//                <MarginLeft>0.1in</MarginLeft>
//                <MarginRight>0.1in</MarginRight>
//                <MarginBottom>0.1in</MarginBottom>
//            </DeviceInfo>";
//            Warning[] warnings;
//            m_streams = new List<Stream>();
//            report.Render("Image", deviceInfo, CreateStream,
//               out warnings);
//            foreach (Stream stream in m_streams)
//                stream.Position = 0;
//        }
//        // Handler for PrintPageEvents
//        private void PrintPage(object sender, PrintPageEventArgs ev)
//        {
//            Metafile pageImage = new
//               Metafile(m_streams[m_currentPageIndex]);

//            // Adjust rectangular area with printer margins.
//            Rectangle adjustedRect = new Rectangle(
//                ev.PageBounds.Left - (int)ev.PageSettings.HardMarginX,
//                ev.PageBounds.Top - (int)ev.PageSettings.HardMarginY,
//                ev.PageBounds.Width,
//                ev.PageBounds.Height);

//            // Draw a white background for the report
//            ev.Graphics.FillRectangle(Brushes.White, adjustedRect);

//            // Draw the report content
//            ev.Graphics.DrawImage(pageImage, adjustedRect);

//            // Prepare for the next page. Make sure we haven't hit the end.
//            m_currentPageIndex++;
//            ev.HasMorePages = (m_currentPageIndex < m_streams.Count);
//        }

//        private void Print()
//        {
//            if (m_streams == null || m_streams.Count == 0)
//                throw new Exception("Error: no stream to print.");
//            PrintDocument printDoc = new PrintDocument();
//            if (!printDoc.PrinterSettings.IsValid)
//            {
//                throw new Exception("Error: cannot find the default printer.");
//            }
//            else
//            {
//                printDoc.PrintPage += new PrintPageEventHandler(PrintPage);
//                m_currentPageIndex = 0;
//                printDoc.Print();
//            }
//        }
//        // Create a local report for Report.rdlc, load the data,
//        //    export the report to an .emf file, and print it.
//        public string Run(string path,string Doc_Number)
//        {
//            // bool flag = false;
//            try
//            {
//                LocalReport report = new LocalReport();
//                report.ReportPath = path;//Path.Combine(@"~\Report\Stock Position.rdlc");
//                    //  report.ReportPath = @"D:\AFZAAL AHMAD\working apps\WorkingApps\Inventory for Saloon\Inventory\Hrm_1\Report\Stock Position.rdlc";
//                MyConnection conn = new MyConnection();
//                var modelList = conn.Select(@"select pb.BARCODE,p.UNIT_RETAIL,p.DESCRIPTION as ItemName,pb.GRN_AMOUNT,pb.GRN_QTY,pb.GRF_AMOUNY,pb.GRF_QTY,
//                                            pb.SALE_QTY,pb.SALE_AMOUNT,pb.TRANSFER_IN_AMOUNT,pb.TRANSFER_IN_QTY,
//                                            pb.TRANSFER_OUT_AMOUNT,pb.TRANSFER_OUT_QTY  from PROD_BALANCE pb 
//                                            left join  PRODUCTS p on pb.BARCODE =p.BARCODE ").Tables[0];
//                var  db = new DataContext();
//            //var modelList = (from salesMain in db.TRANS_MN
//            //                 join salesDetail in db.TRANS_DT
//            //                 on salesMain.TRANS_NO equals salesDetail.TRANS_NO
//            //                 join supplier in db.SUPPLIERs
//            //                 on salesMain.party_code equals supplier.SUPL_CODE
//            //                 join product in db.PRODUCTS
//            //                 on salesDetail.BARCODE equals product.BARCODE
//            //                 //  join location in db.Login_Location
//            //                 //  on salesMain.LOC_ID equals location.Log_Loc_Id
//            //                 where salesMain.TRANS_NO == Doc_Number && supplier.party_type == Constants.Constants.Customer //&& salesMain.status == "3"
//            //                 select new
//            //                 {
//            //                     SupplierName = supplier.SUPL_NAME,
//            //                     SupplierContactNo = supplier.MOBILE,
//            //                     DocumentDate = salesMain.START_TIME,
//            //                     SupplierAddress = supplier.ADDRESS,
//            //                     SupplierPhoneNo = supplier.PHONE,
//            //                     SupplierCode = supplier.SUPL_CODE,
//            //                     DocumentNo = salesMain.TRANS_NO,
//            //                     ItemCode = salesDetail.BARCODE,
//            //                     ItemName = product.DESCRIPTION,
//            //                     Qty = salesDetail.UNITS_SOLD,
//            //                     Rate = salesDetail.UNIT_RETAIL,
//            //                     Amount = (salesDetail.UNIT_RETAIL * salesDetail.UNITS_SOLD) - salesDetail.dis_amount ?? 0,
//            //                     PartyType = supplier.party_type,
//            //                     //    Location = location.Log_Loc_Name,
//            //                     //    Username = username,
//            //                     Unit_Cost = salesDetail.UNIT_COST,
//            //                     BookingDate = salesMain.TRANS_DATE,
//            //                     UanNo = product.UAN_NO,
//            //                     Discount = salesDetail.dis_amount ?? 0
//            //                 }).ToList();
//            ReportDataSource rd = new ReportDataSource("DataSet1", modelList);
//            report.DataSources.Add(
//               rd);

//            Export(report);
//            Print();
//                return "True";
//            }
//            catch (Exception ex)
//            {
//                return ex.ToString();
//            }
//        }

//        public void Dispose()
//        {
//            if (m_streams != null)
//            {
//                foreach (Stream stream in m_streams)
//                    stream.Close();
//                m_streams = null;
//            }
//        }
//    }
//}