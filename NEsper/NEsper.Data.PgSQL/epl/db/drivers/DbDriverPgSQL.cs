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
using com.espertech.esper.compat.container;
using Npgsql;

namespace com.espertech.esper.epl.db.drivers
{
    /// <summary>
    /// A database driver specific to the NPGSQL driver.
    /// </summary>
    [Serializable]
    public class DbDriverPgSQL : BaseDbDriver
    {
        /// <summary>
        /// Initializes the <see cref="DbDriverPgSQL"/> class.
        /// </summary>
        public DbDriverPgSQL(DbProviderFactoryManager dbProviderFactoryManager)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbDriverPgSQL"/> class.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="context">The context.</param>
        protected DbDriverPgSQL(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Factory method that is used to create instance of a connection.
        /// </summary>
        /// <returns></returns>
        public override DbConnection CreateConnection()
        {
            var dbConnection = new NpgsqlConnection();
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
        protected override string ParamPrefix => ":";

        /// <summary>
        /// Creates a connection string builder.
        /// </summary>
        /// <returns></returns>
        protected override DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            return new NpgsqlConnectionStringBuilder();
        }

        /// <summary>
        /// Registers the specified container.
        /// </summary>
        /// <param name="container">The container.</param>
        public static void Register(IContainer container)
        {
            container.Register<DbProviderFactory>(
                NpgsqlFactory.Instance,
                Lifespan.Singleton,
                typeof(NpgsqlFactory).FullName);
        }
    }
}
