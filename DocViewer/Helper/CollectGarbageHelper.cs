using System;

namespace Ecm.DocViewer.Helper
{
    public class CollectGarbageHelper
    {
        public static void CollectGarbage()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}
