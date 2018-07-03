///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Data.Common;
using System.Diagnostics;
using System.Runtime.Serialization;

using com.espertech.esper.compat;
using com.espertech.esper.compat.container;

using MySql.Data.MySqlClient;

namespace com.espertech.esper.epl.db.drivers
{
    /// <summary>
    /// A database driver specific to the MySQL driver.  The MySQL driver
    /// is a named positional driver.
    /// </summary>
    [Serializable]
    public class DbDriverMySQL : BaseDbDriver
    {
        [NonSerialized]
        private readonly DbProviderFactory _dbProviderFactory;
        
        /// <summary>
        /// Initializes the <see cref="DbDriverMySQL"/> class.
        /// </summary>
        public DbDriverMySQL(DbProviderFactoryManager dbProviderFactoryManager)
        {
            _dbProviderFactory = new MySqlClientFactory();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbDriverMySQL"/> class.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="context">The context.</param>
        /// <exception cref="IllegalStateException">context is not set to container</exception>
        protected DbDriverMySQL(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            var container = (IContainer) context.Context;
            if (container == null) {
                throw new IllegalStateException("context is not set to container");
            }

            _dbProviderFactory = new MySqlClientFactory();
        }

        /// <summary>
        /// Factory method that is used to create instance of a connection.
        /// </summary>
        /// <returns></returns>
        public override DbConnection CreateConnection()
        {
            var dbConnection = new MySqlConnection();
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
        protected override bool UsePositionalParameters => false;

        /// <summary>
        /// Gets the parameter prefix.
        /// </summary>
        /// <value>The param prefix.</value>
        protected override string ParamPrefix => "?";

        /// <summary>
        /// Creates a connection string builder.
        /// </summary>
        /// <returns></returns>
        protected override DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            return _dbProviderFactory.CreateConnectionStringBuilder();
        }
    }
}
