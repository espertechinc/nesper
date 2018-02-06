///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Data.Common;

namespace com.espertech.esper.epl.db.drivers
{
    public interface DbProviderFactoryManager
    {
        /// <summary>
        /// Gets the factory.
        /// </summary>
        /// <param name="factoryName">Name of the factory.</param>
        /// <returns></returns>
        DbProviderFactory GetFactory(string factoryName);
    }
}
