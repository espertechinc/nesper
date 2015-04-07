///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Data.Common;

namespace com.espertech.esper.epl.db.drivers
{
    /// <summary>
    /// A database driver specific to the MySQL driver.  The MySQL driver
    /// is a named positional driver.
    /// </summary>
    [Serializable]
    public class DbDriverMySQL : BaseDbDriver
    {
        private static readonly DbProviderFactory dbProviderFactory;
        
        /// <summary>
        /// Initializes the <see cref="DbDriverMySQL"/> class.
        /// </summary>
        static DbDriverMySQL()
        {
            // MySQL needs to be installed on the host box in order for us
            // to make use of it.  Additionally we don't want to bind anything
            // to the library that does not need to artifically be bound.

            dbProviderFactory = DbProviderFactories.GetFactory("MySql.Data.MySqlClient");
        }

        /// <summary>
        /// Factory method that is used to create instance of a connection.
        /// </summary>
        /// <returns></returns>
        public override DbConnection CreateConnection()
        {
            DbConnection dbConnection = dbProviderFactory.CreateConnection();
            dbConnection.ConnectionString = ConnectionString;
            dbConnection.Open();
            return dbConnection;
        }

        /// <summary>
        /// Gets a value indicating whether [use position parameters].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [use position parameters]; otherwise, <c>false</c>.
        /// </value>
        protected override bool UsePositionalParameters
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the parameter prefix.
        /// </summary>
        /// <value>The param prefix.</value>
        protected override string ParamPrefix
        {
            get { return "?"; }
        }

        /// <summary>
        /// Creates a connection string builder.
        /// </summary>
        /// <returns></returns>
        protected override DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            return dbProviderFactory.CreateConnectionStringBuilder();
        }
    }
}
