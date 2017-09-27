///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.support.epl;
using com.espertech.esper.supportunit.epl;

using NUnit.Framework;

namespace com.espertech.esper.epl.db
{
    [TestFixture]
	public class TestDatabaseDSConnFactory
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        private DatabaseDSConnFactory databaseDSConnFactory;

        [SetUp]
	    public void SetUp() {
	        MysqlDataSource mySQLDataSource = new MysqlDataSource();
	        mySQLDataSource.User = SupportDatabaseService.DBUSER;
	        mySQLDataSource.Password = SupportDatabaseService.DBPWD;
	        mySQLDataSource.URL = "jdbc:mysql://localhost/test";

	        string envName = "java:comp/env/jdbc/MySQLDB";
	        SupportInitialContextFactory.AddContextEntry(envName, mySQLDataSource);

	        ConfigurationDBRef config = new ConfigurationDBRef();
	        Properties properties = new Properties();
	        properties.Put("java.naming.factory.initial", typeof(SupportInitialContextFactory).Name);
	        config.SetDataSourceConnection(envName, properties);

	        databaseDSConnFactory = new DatabaseDSConnFactory((ConfigurationDBRef.DataSourceConnection) config.ConnectionFactoryDesc, config.ConnectionSettings);
	    }

        [Test]
	    public void TestGetConnection() {
	        Connection connection = databaseDSConnFactory.Connection;
	        TryAndCloseConnection(connection);
	    }

	    private void TryAndCloseConnection(Connection connection) {
	        Statement stmt = connection.CreateStatement();
	        stmt.Execute("select 1 from dual");
	        ResultSet result = stmt.ResultSet;
	        result.Next();
	        Assert.AreEqual(1, result.GetInt(1));
	        result.Close();
	        stmt.Close();
	        connection.Close();
	    }
	}
} // end of namespace
