using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ecm.ContentExtractor
{
    public interface IContentExtractor
    {
        string ExtractText(byte[] fileByte);

        string ExtractText(string file);
    }
}
