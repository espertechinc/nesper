///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Data.Common;
using com.espertech.esper.compat;
using com.espertech.esper.compat.container;

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
        /// Initializes the <see cref="DbDriverGeneric"/> class.
        /// </summary>
        public DbDriverGeneric(DbProviderFactoryManager dbProviderFactoryManager)
        {
            _dbProviderFactory = dbProviderFactoryManager.GetFactory("MySql.Data.MySqlClient");
            _isPositional = false;
            _paramPrefix = "@";
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
        protected override bool UsePositionalParameters
        {
            get { return _isPositional; }
        }

        /// <summary>
        /// Gets the parameter prefix.
        /// </summary>
        /// <value>The param prefix.</value>
        protected override string ParamPrefix
        {
            get { return _paramPrefix; }
        }

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
