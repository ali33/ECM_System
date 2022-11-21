using System;
using System.Collections.Generic;
using System.Text;

namespace Ecm.LookupService.SqlServer
{
    /// <summary>
    /// Sql Server specific factory that creates Sql Server specific data access objects.
    /// </summary>
    public class SqlServerDaoFactory : DaoFactory
    {
        /// <summary>
        /// Gets a Sql server specific customer data access object.
        /// </summary>
        public override ILookupDao LookupDao
        {
            get { return new SqlServerLookupDao(); }
        }
    }
}
