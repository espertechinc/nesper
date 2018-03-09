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
    /// <summary>
    /// A generic database driver.
    /// </summary>
    [Serializable]
    public class DbDriverGeneric : BaseDbDriver
    {
        private readonly DbProviderFactory _dbProviderFactory;
        private readonly bool _isPositional;
        private readonly String _paramPrefix;

        /// <summary>
        /// Initializes the <see cref="DbDriverGeneric" /> class.
        /// </summary>
        /// <param name="dbProviderFactoryManager">The database provider factory manager.</param>
        /// <param name="factoryName">Name of the factory.</param>
        public DbDriverGeneric(DbProviderFactoryManager dbProviderFactoryManager, string factoryName)
            : this(dbProviderFactoryManager, factoryName, false, "@")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbDriverGeneric"/> class.
        /// </summary>
        /// <param name="dbProviderFactoryManager">The database provider factory manager.</param>
        /// <param name="factoryName">Name of the factory.</param>
        /// <param name="isPositional">if set to <c>true</c> [is positional].</param>
        /// <param name="paramPrefix">The parameter prefix.</param>
        public DbDriverGeneric(
            DbProviderFactoryManager dbProviderFactoryManager,
            string factoryName,
            bool isPositional,
            string paramPrefix)
            : this(dbProviderFactoryManager.GetFactory(factoryName), isPositional, paramPrefix)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbDriverGeneric"/> class.
        /// </summary>
        /// <param name="dbProviderFactory">The database provider factory.</param>
        /// <param name="isPositional">if set to <c>true</c> [is positional].</param>
        /// <param name="paramPrefix">The parameter prefix.</param>
        public DbDriverGeneric(DbProviderFactory dbProviderFactory, bool isPositional, string paramPrefix)
        {
            _dbProviderFactory = dbProviderFactory;
            _isPositional = isPositional;
            _paramPrefix = paramPrefix;
        }
        
        /// <summary>
        /// Factory method that is used to create instance of a connection.
        /// </summary>
        /// <returns></returns>
        public override DbConnection CreateConnection()
        {
            try
            {
                DbConnection dbConnection = _dbProviderFactory.CreateConnection();
                dbConnection.ConnectionString = ConnectionString;
                dbConnection.Open();
                return dbConnection;
            }
            catch (DbException ex)
            {
                String detail = "DbException: " + ex.Message + " VendorError: " + ex.ErrorCode;
                throw new DatabaseConfigException(
                    "Error obtaining database connection using connection-string '" + ConnectionString +
                    "' with detail " + detail, ex);
            }
        }

        /// <summary>
        /// Gets a value indicating whether [use position parameters].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [use position parameters]; otherwise, <c>false</c>.
        /// </value>
        protected override bool UsePositionalParameters => _isPositional;

        /// <summary>
        /// Gets the parameter prefix.
        /// </summary>
        /// <value>The param prefix.</value>
        protected override string ParamPrefix => _paramPrefix;

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
