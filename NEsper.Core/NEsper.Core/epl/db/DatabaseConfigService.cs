///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;

namespace com.espertech.esper.epl.db
{
	/// <summary> Service providing database connection factory and configuration information
	/// for use with historical data polling.
	/// </summary>
	public interface DatabaseConfigService
	{
		/// <summary> Returns a connection factory for a configured database.</summary>
		/// <param name="databaseName">is the name of the database
		/// </param>
		/// <returns> is a connection factory to use to get connections to the database
		/// </returns>
		/// <throws>  DatabaseConfigException is thrown to indicate database configuration errors </throws>
		DatabaseConnectionFactory GetConnectionFactory(String databaseName);

	    /// <summary>
	    /// Returns the column metadata settings for the database.
	    /// </summary>
        /// <param name="databaseName">the database name</param>
        /// <returns>indicators for change case, metadata retrieval strategy and others</returns>
	    ColumnSettings GetQuerySetting(String databaseName);

	    /// <summary>
	    /// Returns true to indicate a setting to retain connections between lookups.
	    /// </summary>
	    /// <param name="databaseName">is the name of the database</param>
	    /// <param name="preparedStatementText">is the sql text</param>
	    /// <param name="contextAttributes">The context attributes.</param>
	    /// <returns>
	    /// a cache implementation to cache connection and prepared statements
	    /// </returns>
	    /// <throws>DatabaseConfigException is thrown to indicate database configuration errors</throws>
	    ConnectionCache GetConnectionCache(String databaseName, String preparedStatementText, IEnumerable<Attribute> contextAttributes);

        /// <summary>
        /// Returns a new cache implementation for this database.
        /// </summary>
        /// <param name="databaseName">is the name of the database to return a new cache implementation for for</param>
        /// <param name="statementContext">The statement context.</param>
        /// <param name="epStatementAgentInstanceHandle">is the statements-own handle for use in registering callbacks with services</param>
        /// <param name="dataCacheFactory">The data cache factory.</param>
        /// <param name="streamNumber">The stream number.</param>
        /// <returns>
        /// cache implementation
        /// </returns>
        /// <throws>DatabaseConfigException is thrown to indicate database configuration errors</throws>
        DataCache GetDataCache(String databaseName, StatementContext statementContext, EPStatementAgentInstanceHandle epStatementAgentInstanceHandle, DataCacheFactory dataCacheFactory, int streamNumber);
	}
}
