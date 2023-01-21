using System.Web;
using System.Web.Optimization;

namespace Inventory
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            //bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
            //            "~/Scripts/jquery-{version}.js"));

            //bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
            //            "~/Scripts/jquery.validate*"));

            //// Use the development version of Modernizr to develop with and learn from. Then, when you're
            //// ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            //bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
            //            "~/Scripts/modernizr-*"));

            //bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
            //          //"~/Scripts/bootstrap.js",
            //          //"~/Scripts/respond.js",
            //          ));

            //bundles.Add(new ScriptBundle("~/bundles/FullCalendar").Include(
            //          "~/Scripts/bootstrap.js",
            //          "~/Scripts/respond.js",
            //          ));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      //"~/Content/bootstrap.css",
                      "~/AdminLte/plugins/fontawesome-free/css/all.min.css",
                      "~/AdminLte/plugins/tempusdominus-bootstrap-4/css/tempusdominus-bootstrap-4.min.css",
                      "~/AdminLte/plugins/icheck-bootstrap/icheck-bootstrap.min.css",
                      "~/AdminLte/plugins/jqvmap/jqvmap.min.css",
                      "~/AdminLte/plugins/overlayScrollbars/css/OverlayScrollbars.min.css",
                      "~/AdminLte/plugins/pace-progress/themes/black/pace-theme-flat-top.css",
                      "~/AdminLte/plugins/ekko-lightbox/ekko-lightbox.css",
                      "~/AdminLte/plugins/ion-rangeslider/css/ion.rangeSlider.min.css",
                      "~/AdminLte/plugins/bootstrap-slider/css/bootstrap-slider.min.css",
                      "~/AdminLte/plugins/sweetalert2/sweetalert2.min.css",
                      "~/AdminLte/plugins/toastr/toastr.min.css",
                      "~/AdminLte/plugins/jsgrid/jsgrid.min.css",
                      "~/AdminLte/plugins/jsgrid/jsgrid-theme.min.css",
                      "~/AdminLte/plugins/datatables-bs4/css/dataTables.bootstrap4.css",
                      "~/AdminLte/plugins/bootstrap-colorpicker/css/bootstrap-colorpicker.min.css",
                      "~/AdminLte/plugins/daterangepicker/daterangepicker.css",
                      "~/AdminLte/plugins/bootstrap4-duallistbox/bootstrap-duallistbox.min.css",
                      "~/AdminLte/plugins/summernote/summernote-bs4.css",
                      "~/AdminLte/dist/css/adminlte.min.css"
                      ));

            bundles.Add(new StyleBundle("~/Content/Calendar").Include(
                      "~/AdminLte/plugins/fullcalendar/main.min.css",
                      "~/AdminLte/plugins/fullcalendar-interaction/main.min.css",
                      "~/AdminLte/plugins/fullcalendar-daygrid/main.min.css",
                      "~/AdminLte/plugins/fullcalendar-timegrid/main.min.css",
                      "~/AdminLte/plugins/fullcalendar-bootstrap/main.min.css"
                      ));
            bundles.Add(new StyleBundle("~/Content/Select2").Include(
                      "~/AdminLte/plugins/select2/css/select2.min.css",
                      "~/AdminLte/plugins/select2-bootstrap4-theme/select2-bootstrap4.min.css"
                      ));
        }
    }
}
