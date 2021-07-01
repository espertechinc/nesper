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

using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.db;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.epl.historical.database.connection
{
    [TestFixture]
    [Category("DatabaseTest")]
    [Category("IntegrationTest")]
    public class TestDatabaseDmConnFactory : AbstractCommonTest
    {
#if false
        private readonly DriverConnectionFactoryDesc _databaseDmConnConnectionFactoryOne;
        private readonly DriverConnectionFactoryDesc _databaseDmConnConnectionFactoryTwo;
        private readonly DriverConnectionFactoryDesc _databaseDmConnConnectionFactoryThree;
#endif

        [SetUp]
        public void SetUp()
        {
            ImportService engineImportService = SupportClasspathImport.GetInstance(container);

            // driver-manager config 1
            var config = new ConfigurationCommonDBRef();
            config.SetDatabaseDriver(SupportDatabaseService.GetInstance(container).DriverConnectionFactoryNative);
            config.ConnectionAutoCommit = true;
            config.ConnectionCatalog = "test";
            config.ConnectionTransactionIsolation = IsolationLevel.Serializable;
            config.ConnectionReadOnly = true;

#if false
            _databaseDmConnConnectionFactoryOne = new DriverConnectionFactoryDesc(
                (ConfigurationCommonDBRef.DriverManagerConnection) config.ConnectionFactoryDesc,
                config.ConnectionSettings, engineImportService);

            // driver-manager config 2
            config = new ConfigurationCommonDBRef();
            config.SetDriverManagerConnection(SupportDatabaseURL.DRIVER, SupportDatabaseURL.PARTURL, SupportDatabaseURL.DBUSER, SupportDatabaseURL.DBPWD);
            _databaseDmConnConnectionFactoryTwo = new DriverConnectionFactoryDesc(
                (ConfigurationCommonDBRef.DriverManagerConnection) config.ConnectionFactoryDesc,
                config.ConnectionSettings, engineImportService);

            // driver-manager config 3
            config = new ConfigurationCommonDBRef();
            Properties properties = new Properties();
            properties["user"] = SupportDatabaseURL.DBUSER;
            properties["password"] = SupportDatabaseURL.DBPWD;
            config.SetDriverManagerConnection(SupportDatabaseURL.DRIVER, SupportDatabaseURL.PARTURL, properties);
            _databaseDmConnConnectionFactoryThree = new DatabaseDMConnFactory(
                (ConfigurationCommonDBRef.DriverManagerConnection) config.ConnectionFactoryDesc,
                config.ConnectionSettings, engineImportService);
#endif
        }

        [Test]
        public void TestGetConnection()
        {
            TryAndCloseConnectionWithFactory(
                () => {
                    var properties = supportDatabaseService.DefaultProperties;
                    var config = supportDatabaseService.CreateDefaultConfig(properties);
                    config.ConnectionCatalog = "test";
                    config.ConnectionAutoCommit = false; // not supported yet
                    config.ConnectionTransactionIsolation = IsolationLevel.Unspecified;
                    return new DatabaseDriverConnFactory(
                        base.container,
                        (DriverConnectionFactoryDesc) config.ConnectionFactoryDesc,
                        config.ConnectionSettings);
                });

            TryAndCloseConnection(supportDatabaseService.DriverConnectionFactoryDefault);

#if X64
            // ODBC drivers are sensitive to which platform they are installed on; we only test them when performing
            // tests with X64 since we usually do not install the 32-bit drivers.
            TryAndCloseConnection(SupportDatabaseService.DbDriverFactoryODBC);
#endif
        }

        private void TryAndCloseConnectionWithFactory(Func<DatabaseDriverConnFactory> connectionFactory)
        {
            TryAndCloseConnection(connectionFactory.Invoke().Driver.CreateConnection());
        }

        private void TryAndCloseConnection(ConnectionFactoryDesc connectionFactoryDesc)
        {
            var config = new ConfigurationCommonDBRef { ConnectionFactoryDesc = connectionFactoryDesc };
            var connectionFactory = new DatabaseDriverConnFactory(
                container,
                (DriverConnectionFactoryDesc) config.ConnectionFactoryDesc,
                config.ConnectionSettings);
            var connection = connectionFactory.Driver.CreateConnection();
            TryAndCloseConnection(connection);
        }

        private void TryAndCloseConnection(DbConnection connection)
        {
            using (var stmt = connection.CreateCommand())
            {
                stmt.CommandType = CommandType.Text;
                switch (connection.GetType().Name)
                {
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
                }
            }

            connection.Close();
            connection.Dispose();
        }

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
