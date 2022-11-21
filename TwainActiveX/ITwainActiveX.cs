using Ecm.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace TwainActiveX
{
    [InterfaceType(ComInterfaceType.InterfaceIsDual), Guid("A26B826D-07A3-468F-B80A-F71B23B9966A")]
    [ComVisible(true)]
    public interface ITwainActiveX
    {
        [ComVisible(true)]
        List<string> Scan(bool isShowDialog, string uploadURL);

        [ComVisible(true)]
        string ScanCapture(bool isShowDialog, string uploadURL, string scanToken);

        [ComVisible(true)]
        void ClearScanFile();
    }
}
