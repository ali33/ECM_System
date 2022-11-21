using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArchiveMVC5.Utility
{
    public class ProcessCache
    {
        public static string Cache(object obj)
        {
            var cTime = DateTime.Now.AddMinutes(24 * 3600);
            var cExp = System.Web.Caching.Cache.NoSlidingExpiration;
            var cPri = System.Web.Caching.CacheItemPriority.Normal;


            string id = Guid.NewGuid().ToString();
            System.Web.HttpContext.Current.Cache.Add(id, obj, null, cTime, cExp, cPri, null);
            return id;
        }
    }
}