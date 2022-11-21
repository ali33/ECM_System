using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace TwainActiveX
{
    [Guid("2278DD40-E53F-427C-94FE-BD01E1A855D7")]
    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface ITwainActiveXEvent
    {
        [DispId(0x00000001)]
        void ScanPageEvent(string args);
    }
}
