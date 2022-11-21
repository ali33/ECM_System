using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ecm.LookupService
{
    /// <summary>
    /// The purpose of this class is to ....
    /// </summary>
    public enum LookupType
    {
        StoredProcedure,
        CommandText
    }

    public enum DataSourceType
    {
        ALL,
        VIEW,
        TABLE,
        STORED_PROCEDURE
    }
}
