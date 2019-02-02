using System.Web;
using System.Web.Optimization;

namespace Veb_portal_za_aukcijsku_prodaju
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/otherjs").Include(
                      "~/Scripts/jquery.smartmenus.js",
                      "~/Scripts/jquery.smartmenus.bootstrap.js",
                      "~/Scripts/jquery.simpleGallery.js",
                      "~/Scripts/jquery.simpleLens.js",
                      "~/Scripts/sequence-theme.modern-slide-in.js",
                      "~/Scripts/custom.js",
                      "~/Scripts/nouislider.js",
                      "~/Scripts/sequence.js",
                      "~/Scripts/slick.js",
                      "~/Scripts/open-forms.js"));

            bundles.Add(new StyleBundle("~/Styles/css").Include(
                      "~/Content/css/custom.css",
                      "~/Content/css/bootstrap.css",
                      "~/Content/css/style.css",
                      "~/Content/css/font-awesome.css",
                      "~/Content/css/jquery.simpleLens.css",
                      "~/Content/css/jquery.smartmenus.bootstrap.css",
                      "~/Content/css/nouislider.css",
                      "~/Content/css/sequence-theme.modern-slide-in.css",
                      "~/Content/css/slick.css"));
            BundleTable.EnableOptimizations = false;
        }
    }
}
