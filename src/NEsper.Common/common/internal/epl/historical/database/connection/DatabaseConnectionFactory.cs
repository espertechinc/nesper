///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.db;

namespace com.espertech.esper.common.@internal.epl.historical.database.connection
{
    /// <summary>
    /// Factory for new database connections.
    /// </summary>
    public interface DatabaseConnectionFactory
    {
        /// <summary>
        /// Gets the driver.
        /// </summary>
        DbDriver Driver { get; }
    }
} // end of namespace