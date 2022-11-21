using System;

namespace Ecm.ContentViewer.Helper
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
