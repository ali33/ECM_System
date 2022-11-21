using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using ArchiveMVC5.Models;

namespace ArchiveMVC5.Utility
{
    public class Checking
    {
        public static bool CheckUserLogin()
        {
            if (String.IsNullOrEmpty((string)ArchiveMVC5.Utility.Utilities.GetSession(ArchiveMVC5.Models.Constant.UserName)))
                return true;
            return false;
        }
    }
}