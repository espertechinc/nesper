///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Data.Common;
using System.Runtime.Serialization;
using System.Security.Permissions;

using com.espertech.esper.compat;
using com.espertech.esper.compat.container;

namespace com.espertech.esper.epl.db.drivers
{
    /// <summary>
    /// A generic database driver.
    /// </summary>
    [Serializable]
    public class DbDriverGeneric : BaseDbDriver, ISerializable
    {
        private readonly String _dbProviderFactoryName;
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
            _dbProviderFactoryName = factoryName;
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
            _dbProviderFactoryName = dbProviderFactory.GetType().FullName; // half-baked
            _isPositional = isPositional;
            _paramPrefix = paramPrefix;
        }

        protected DbDriverGeneric(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            var container = (IContainer)context.Context;
            if (container == null)
            {
                throw new IllegalStateException("context is not set to container");
            }

            var dbProviderFactoryManager = container.Resolve<DbProviderFactoryManager>();
            var dbProviderFactoryName = info.GetString("_dbProviderFactoryName");

            _dbProviderFactory = dbProviderFactoryManager.GetFactory(dbProviderFactoryName);
            _isPositional = info.GetBoolean("_isPositional");
            _paramPrefix = info.GetString("_paramPrefix");
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("_isPositional", _isPositional);
            info.AddValue("_paramPrefix", _paramPrefix);
            info.AddValue("_dbProviderFactoryName", _dbProviderFactoryName);
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
