///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.support.epl;

using NUnit.Framework;

namespace com.espertech.esper.epl.db
{
    [TestFixture]
    public class TestDatabaseDMConnFactory 
    {
        [SetUp]
        public void SetUp()
        {
        }
    
        [Test]
        public void TestGetConnection()
        {
            TryAndCloseConnectionWithFactory(
                () =>
                {
                    var properties = new Properties();
                    var config = new ConfigurationDBRef();
                    properties["Server"] = System.Net.Dns.GetHostName();
                    properties["Uid"] = "esper";
                    config.SetDatabaseDriver(SupportDatabaseService.DbDriverFactoryNative, properties);
                    config.ConnectionCatalog = "test";
                    config.ConnectionAutoCommit = false; // not supported yet
                    config.ConnectionTransactionIsolation = IsolationLevel.Unspecified;
                    return new DatabaseDriverConnFactory((DbDriverFactoryConnection) config.ConnectionFactoryDesc, config.ConnectionSettings);
                });

            TryAndCloseConnection(SupportDatabaseService.DbDriverFactoryNative);

#if TEST_ODBC
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
            stmt.CommandText = "select 1 from dual";
            stmt.CommandType = CommandType.Text;

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
