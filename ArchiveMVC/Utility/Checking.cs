using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using ArchiveMVC.Models;

namespace ArchiveMVC.Utility
{
    public class Checking
    {
        public static bool CheckUserLogin()
        {
            if (String.IsNullOrEmpty((string)ArchiveMVC.Utility.Utilities.GetSession(ArchiveMVC.Models.Constant.UserName)))
                return true;
            return false;
        }
    }
}