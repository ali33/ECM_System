using System.Web;
using System.Web.Optimization;

namespace ArchiveMVC5
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                       "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                       "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));
            
            bundles.Add(new ScriptBundle("~/bundles/scripts").Include(
                        "~/Scripts/ECM.js"));

            bundles.Add(new ScriptBundle("~/bundles/plugins").Include(
                   "~/plugins/bootstrap/js/bootstrap.js",/*Bootstrap 3.3.2 JS*/
                   "~/plugins/bootstrap-dialog/js/bootstrap-dialog.js",/*Bootstrap-dialog JS*/
                   "~/plugins/bootstrap-toggle/js/bootstrap-toggle.js",/*Bootstrap-dialog JS*/
                   "~/plugins/sparkline/jquery.sparkline.min.js",/*Sparkline*/
                   "~/plugins/jvectormap/jquery-jvectormap-1.2.2.min.js",/*jvectormap*/
                   "~/plugins/jvectormap/jquery-jvectormap-world-mill-en.js",
                   "~/plugins/knob/jquery.knob.js",/*jQuery Knob Chart*/
                                                   /*"~/plugins/morris/raphael-min.js",Morris.js charts
                                                   "~/plugins/morris/morris.min.js",*/
                   "~/plugins/daterangepicker/daterangepicker.js",/*daterangepicker*/
                   "~/plugins/datepicker/bootstrap-datepicker.js",/*datepicker*/
                   "~/plugins/datepicker/locales/bootstrap-datepicker.vi.js",/*datepicker locales VI*/
                   "~/plugins/bootstrap-wysihtml5/bootstrap3-wysihtml5.all.min.js",/*Bootstrap WYSIHTML5*/
                   "~/plugins/iCheck/icheck.min.js",/*iCheck*/
                   "~/plugins/slimScroll/jquery.slimscroll.min.js",/*Slimscroll*/
                   //"~/plugins/fastclick/fastclick.min.js",/*FastClick*/
                    "~/Content/dist/js/app.js",/*AdminLTE App*/
                   // "~/Content/dist/js/pages/dashboard.js",//AdminLTE dashboard demo (This is only for demo purposes)
                    //"~/Content/dist/js/pages/dashboard2.js",
                    "~/Content/dist/js/demo.js"/*AdminLTE for demo purposes*/
                   ));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                 "~/Content/dist/css/AdminLTE.css",/*Theme style*/
                 "~/Content/Style.css",/*Customer style*/
                 "~/Content/paging.css",/*Paging style*/
                 //"~/Content/datepicker.css",
                 "~/Content/load.css",
                 "~/Content/dist/css/skins/_all-skins.min.css",/* AdminLTE Skins. Choose a skin from the css/skins
                                                                    folder instead of downloading all of them to reduce the load.*/
                 "~/Content/font-awesome-4.3.0/css/font-awesome.min.css",/*FontAwesome 4.3.0*/
                  "~/Content/ionicons-2.0.1/css/ionicons.min.css"/*Ionicons 2.0.0*/
                 ));

            bundles.Add(new StyleBundle("~/plugins/css").Include(
               "~/plugins/bootstrap/css/bootstrap.css",/*Bootstrap 3.3.2*/
               "~/plugins/bootstrap-dialog/css/bootstrap-dialog.css",/*Bootstrap-dialog*/
               "~/plugins/bootstrap-dialog/css/bootstrap-toggle.css",/*Bootstrap-dialog*/
               "~/plugins/bootstrap/css/bootstrap-responsive.css",/*Bootstrap Responsive 3.3.2*/
               "~/plugins/iCheck/flat/blue.css",/*iCheck*/
               "~/plugins/iCheck/square/blue.css",/*iCheck*/
                                                  /*"~/plugins/morris/morris.css",Morris chart*/
               "~/plugins/jvectormap/jquery-jvectormap-1.2.2.css",/*jvectormap*/
               "~/plugins/datepicker/datepicker3.css",/*Date Picker*/
               "~/plugins/daterangepicker/daterangepicker-bs3.css",/*Daterange picker*/
               "~/plugins/bootstrap-wysihtml5/bootstrap3-wysihtml5.min.css"/*bootstrap wysihtml5 - text editor*/
           ));
        }
    }
}
