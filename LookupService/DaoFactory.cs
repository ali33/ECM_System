using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ecm.LookupService
{
    /// <summary>
    /// Abstract factory class that creates data access objects.
    /// </summary>
    /// <remarks>
    /// GoF Design Pattern: Factory.
    /// </remarks>
    public abstract class DaoFactory
    {
        /// <summary>
        /// Gets a customer data access object.
        /// </summary>
        public abstract ILookupDao LookupDao { get; }
    }
}
