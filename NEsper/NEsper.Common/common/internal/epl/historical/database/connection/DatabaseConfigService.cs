///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.historical.database.connection
{
    /// <summary>
    /// Service providing database connection factory and configuration information
    /// for use with historical data polling.
    /// </summary>
    public interface DatabaseConfigService
    {
        /// <summary>
        /// Returns a connection factory for a configured database.
        /// </summary>
        /// <param name="databaseName">is the name of the database</param>
        /// <returns>is a connection factory to use to get connections to the database</returns>
        /// <throws>DatabaseConfigException is thrown to indicate database configuration errors</throws>
        DatabaseConnectionFactory GetConnectionFactory(string databaseName);
    }
} // end of namespace