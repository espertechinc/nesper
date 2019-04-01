///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Data;
using System.Data.Common;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.epl.db;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.db
{
    public class ExecDatabaseDMConnFactory : RegressionExecution
    {
        public override void Configure(Configuration configuration)
        {
            var configDB = SupportDatabaseService.CreateDefaultConfig();
            configDB.ConnectionLifecycle = ConnectionLifecycleEnum.RETAIN;
            configDB.ConnectionCatalog = "test";
            configDB.ConnectionTransactionIsolation = IsolationLevel.Serializable;

            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
            configuration.EngineDefaults.Logging.IsEnableADO = true;
            configuration.AddDatabaseReference("MyDB", configDB);
        }

        public override void Run(EPServiceProvider epService)
        {
            RunAssertionGetConnection();
        }

        public void RunAssertionGetConnection()
        {
            TryAndCloseConnectionWithFactory(
                () =>
                {
                    var properties = SupportDatabaseService.DefaultProperties;
                    var config = SupportDatabaseService.CreateDefaultConfig(properties);
                    config.ConnectionCatalog = "test";
                    config.ConnectionAutoCommit = false; // not supported yet
                    config.ConnectionTransactionIsolation = IsolationLevel.Unspecified;
                    return new DatabaseDriverConnFactory((DbDriverFactoryConnection)config.ConnectionFactoryDesc, config.ConnectionSettings);
                });

            TryAndCloseConnection(SupportDatabaseService.DbDriverFactoryDefault);

#if X64
            // ODBC drivers are sensitive to which platform they are installed on; we only test them when performing
            // tests with X64 since we usually do not install the 32-bit drivers.
            TryAndCloseConnection(SupportDatabaseService.DbDriverFactoryODBC);
#endif
        }

        private static void TryAndCloseConnectionWithFactory(Func<DatabaseDriverConnFactory> connectionFactory)
        {
            TryAndCloseConnection(connectionFactory.Invoke().Driver.CreateConnection());
        }

        private static void TryAndCloseConnection(ConnectionFactoryDesc connectionFactoryDesc)
        {
            var config = new ConfigurationDBRef { ConnectionFactoryDesc = connectionFactoryDesc };
            var connectionFactory = new DatabaseDriverConnFactory(
                    (DbDriverFactoryConnection)config.ConnectionFactoryDesc, config.ConnectionSettings);
            var connection = connectionFactory.Driver.CreateConnection();
            TryAndCloseConnection(connection);
        }

        private static void TryAndCloseConnection(DbConnection connection)
        {
            DbCommand stmt;

            stmt = connection.CreateCommand();
            stmt.CommandType = CommandType.Text;
            switch (connection.GetType().Name) {
                case "MySqlConnection":
                    stmt.CommandText = "select 1 from dual";
                    break;
                case "NpgsqlConnection":
                    stmt.CommandText = "select 1";
                    break;
                default:
                    throw new IllegalStateException("unrecognized driver");
            }

            using (DbDataReader result = stmt.ExecuteReader())
            {
                Assert.IsTrue(result.Read());
                Assert.AreEqual(1, result.GetInt32(0));
                result.Close();
            }

            stmt.Dispose();

            connection.Close();
            connection.Dispose();
        }
    }
}
