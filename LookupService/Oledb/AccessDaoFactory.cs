using System;
using System.Collections.Generic;
using System.Text;

namespace Ecm.LookupService.Oledb
{
    /// <summary>
    /// Microsoft Access specific factory that creates Microsoft Access 
    /// specific data access objects.
    /// </summary>
    public class AccessDaoFactory : DaoFactory
    {
        /// <summary>
        /// Gets a Microsoft Access specific customer data access object.
        /// </summary>
        public override ILookupDao LookupDao
        {
            get { return new AccessLookupDao(); }
        }


    }
}
