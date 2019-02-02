using System.Web;
using System.Web.Mvc;

namespace Veb_portal_za_aukcijsku_prodaju
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
