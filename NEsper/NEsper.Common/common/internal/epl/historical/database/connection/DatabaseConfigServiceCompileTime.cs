///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.historical.database.core;

namespace com.espertech.esper.common.@internal.epl.historical.database.connection
{
    /// <summary>
    /// Service providing database connection factory and configuration information
    /// for use with historical data polling.
    /// </summary>
    public interface DatabaseConfigServiceCompileTime : DatabaseConfigService
    {
        /// <summary>
        /// Returns the column metadata settings for the database.
        /// </summary>
        /// <param name="databaseName">is the database name</param>
        /// <returns>indicators for change case, metadata retrieval strategy and others</returns>
        /// <throws>DatabaseConfigException if the name was not configured</throws>
        ColumnSettings GetQuerySetting(string databaseName);
    }
} // end of namespace