///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Data.Common;

namespace com.espertech.esper.epl.db.drivers
{
    public class DbProviderFactoryManagerDefault : DbProviderFactoryManager
    {
        /// <summary>
        /// Gets the factory associated with the name.
        /// </summary>
        /// <param name="factoryName">Name of the factory.</param>
        public DbProviderFactory GetFactory(string factoryName)
        {
#if NETSTANDARD2_0
            throw new NotSupportedException("use DbProviderFactoryManagerCustom");
#else
            return DbProviderFactories.GetFactory(factoryName);
#endif
        }
    }
}
