using System;
using System.Collections.Generic;
using System.Text;

namespace Ecm.LookupService.Oracle
{
    /// <summary>
    /// Oracle specific factory that creates Oracle specific data access objects.
    /// </summary>
    public class OracleDaoFactory : DaoFactory
    {
        /// <summary>
        /// Gets an Oracle specific customer data access object.
        /// </summary>
        public override ILookupDao LookupDao
        {
            get { return new OracleLookupDao(); }
        }

    }
}
