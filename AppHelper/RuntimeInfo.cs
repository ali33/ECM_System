using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Ecm.AppHelper
{
    public class RuntimeInfo
    {
        public static CultureInfo Culture { get; set; }

    }

    public enum ShellAddToRecentDocsFlags
    {
        Pidl = 0x001,
        Path = 0x002,
    }
}
