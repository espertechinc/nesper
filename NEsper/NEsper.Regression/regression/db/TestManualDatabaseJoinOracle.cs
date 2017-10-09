///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.db
{
	/// <summary>
	/// Manual test for testing Oracle database access
	/// <para />Peculiarities for Oracle:
	/// - all column names are uppercase
	/// - integer and other number type maps to BigDecmial (getObject)
	/// - Oracle driver does not support obtaining metadata from prepared statement
	/// </summary>
    [TestFixture]
	public class TestManualDatabaseJoinOracle  {
	    // Oracle access
	    private const string DBUSER = "USER";
	    private const string DBPWD = "PWD";
	    private const string DRIVER = "oracle.jdbc.driver.OracleDriver";
	    private const string FULLURL = "jdbc:oracle:thin:@host:port:sid";
	    private const string CATALOG = "CATALOG";
	    private const string TABLE = "mytesttable";
	    private const string TABLE_NAME = CATALOG + ".\"" + TABLE + "\"";

	    private EPServiceProvider epService;
	    private SupportUpdateListener listener;

        [Test]
	    public void TestDummy() {
	        // otherwise this test fails with no-runnable-method
	    }

	    public void ManualHasMetaSQLStringParam() {
	        ConfigurationDBRef dbconfig = ConfigOracle;
	        Configuration configuration = GetConfig(dbconfig);

	        epService = EPServiceProviderManager.GetDefaultProvider(configuration);
	        epService.Initialize();

	        string table = CATALOG + ".\"" + TABLE + "\"";
	        string sql = "select myint from " + table + " where ${string} = myvarchar'" +
	                     "metadatasql 'select myint from " + table + "'";
	        string stmtText = "select MYINT from " +
	                          " sql:MyDB ['" + sql + "] as s0," +
	                          typeof(SupportBean).Name + "#length(100) as s1";

	        EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
	        listener = new SupportUpdateListener();
	        statement.AddListener(listener);

	        Assert.AreEqual(typeof(BigDecimal), statement.EventType.GetPropertyType("MYINT"));

	        SendSupportBeanEvent("A");
	        EventBean theEvent = listener.AssertOneGetNewAndReset();
	        Assert.AreEqual(new BigDecimal(10), theEvent.Get("MYINT"));

	        SendSupportBeanEvent("H");
	        theEvent = listener.AssertOneGetNewAndReset();
	        Assert.AreEqual(new BigDecimal(80), theEvent.Get("MYINT"));
	    }

	    public void ManualHasMetaSQLIntParamLowercase() {
	        ConfigurationDBRef dbconfig = ConfigOracle;
	        dbconfig.ColumnChangeCase = ConfigurationDBRef.ColumnChangeCaseEnum.LOWERCASE;
	        Configuration configuration = GetConfig(dbconfig);
	        epService = EPServiceProviderManager.GetDefaultProvider(configuration);
	        epService.Initialize();

	        string sql = "select mydouble from " + TABLE_NAME + " where ${intPrimitive} = myint'" +
	                     "metadatasql 'select mydouble from " + TABLE_NAME + "'";
	        string stmtText = "select mydouble from " +
	                          " sql:MyDB ['" + sql + "] as s0," +
	                          typeof(SupportBean).Name + "#length(100) as s1";

	        EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
	        listener = new SupportUpdateListener();
	        statement.AddListener(listener);

	        Assert.AreEqual(typeof(BigDecimal), statement.EventType.GetPropertyType("mydouble"));

	        SendSupportBeanEvent(10);
	        BigDecimal result = (BigDecimal) listener.AssertOneGetNewAndReset().Get("mydouble");
	        Assert.AreEqual(12, Math.Round(result.DoubleValue() * 10d));

	        SendSupportBeanEvent(80);
	        result = (BigDecimal) listener.AssertOneGetNewAndReset().Get("mydouble");
	        Assert.AreEqual(82, Math.Round(result.DoubleValue() * 10d));
	    }

	    public void ManualTypeMapped() {
	        ConfigurationDBRef dbconfig = ConfigOracle;
	        dbconfig.ColumnChangeCase = ConfigurationDBRef.ColumnChangeCaseEnum.LOWERCASE;
	        dbconfig.AddSqlTypesBinding(2, "int");
	        Configuration configuration = GetConfig(dbconfig);
	        epService = EPServiceProviderManager.GetDefaultProvider(configuration);
	        epService.Initialize();

	        string sql = "select myint from " + TABLE_NAME + " where ${intPrimitive} = myint'" +
	                     "metadatasql 'select myint from " + TABLE_NAME + "'";
	        string stmtText = "select myint from " +
	                          " sql:MyDB ['" + sql + "] as s0," +
	                          typeof(SupportBean).Name + "#length(100) as s1";

	        EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
	        listener = new SupportUpdateListener();
	        statement.AddListener(listener);

	        Assert.AreEqual(typeof(int?), statement.EventType.GetPropertyType("myint"));

	        SendSupportBeanEvent(10);
	        Assert.AreEqual(10, listener.AssertOneGetNewAndReset().Get("myint"));

	        SendSupportBeanEvent(80);
	        Assert.AreEqual(80, listener.AssertOneGetNewAndReset().Get("myint"));
	    }

	    public void ManualNoMetaLexAnalysis() {
	        ConfigurationDBRef dbconfig = ConfigOracle;
	        dbconfig.ColumnChangeCase = ConfigurationDBRef.ColumnChangeCaseEnum.LOWERCASE;
	        Configuration configuration = GetConfig(dbconfig);

	        epService = EPServiceProviderManager.GetDefaultProvider(configuration);
	        epService.Initialize();

	        string sql = "select mydouble from " + TABLE_NAME + " where ${intPrimitive} = myint";
	        Run(sql);
	    }

	    public void ManualNoMetaLexAnalysisGroup() {
	        ConfigurationDBRef dbconfig = ConfigOracle;
	        dbconfig.ColumnChangeCase = ConfigurationDBRef.ColumnChangeCaseEnum.LOWERCASE;
	        Configuration configuration = GetConfig(dbconfig);

	        epService = EPServiceProviderManager.GetDefaultProvider(configuration);
	        epService.Initialize();

	        string sql = "select mydouble, sum(myint) from " + TABLE_NAME + " where ${intPrimitive} = myint group by mydouble";
	        Run(sql);
	    }

	    public void ManualPlaceholderWhere() {
	        ConfigurationDBRef dbconfig = ConfigOracle;
	        dbconfig.ColumnChangeCase = ConfigurationDBRef.ColumnChangeCaseEnum.LOWERCASE;
	        Configuration configuration = GetConfig(dbconfig);

	        epService = EPServiceProviderManager.GetDefaultProvider(configuration);
	        epService.Initialize();

	        string sql = "select mydouble from " + TABLE_NAME + " ${$ESPER-SAMPLE-WHERE} where ${intPrimitive} = myint";
	        Run(sql);
	    }

	    private void Run(string sql) {
	        string stmtText = "select mydouble from " +
	                          " sql:MyDB ['" + sql + "'] as s0," +
	                          typeof(SupportBean).Name + "#length(100) as s1";

	        EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
	        listener = new SupportUpdateListener();
	        statement.AddListener(listener);

	        Assert.AreEqual(typeof(BigDecimal), statement.EventType.GetPropertyType("mydouble"));

	        SendSupportBeanEvent(10);
	        BigDecimal result = (BigDecimal) listener.AssertOneGetNewAndReset().Get("mydouble");
	        Assert.AreEqual(12, Math.Round(result.DoubleValue() * 10d));

	        SendSupportBeanEvent(80);
	        result = (BigDecimal) listener.AssertOneGetNewAndReset().Get("mydouble");
	        Assert.AreEqual(82, Math.Round(result.DoubleValue() * 10d));
	    }

	    private ConfigurationDBRef GetConfigOracle() {
	        ConfigurationDBRef configDB = new ConfigurationDBRef();
	        configDB.SetDriverManagerConnection(DRIVER, FULLURL, DBUSER, DBPWD);
	        configDB.ConnectionLifecycleEnum = ConfigurationDBRef.ConnectionLifecycleEnum.RETAIN;
	        configDB.ConnectionCatalog = CATALOG;
	        configDB.ConnectionReadOnly = true;
	        configDB.ConnectionAutoCommit = true;
	        return configDB;
	    }

	    private Configuration GetConfig(ConfigurationDBRef configOracle) {
	        Configuration configuration = SupportConfigFactory.Configuration;
	        configuration.AddDatabaseReference("MyDB", configOracle);
	        configuration.EngineDefaults.Logging.EnableExecutionDebug = true;

	        return configuration;
	    }

	    private void SendSupportBeanEvent(string theString) {
	        SupportBean bean = new SupportBean();
	        bean.TheString = theString;
	        epService.EPRuntime.SendEvent(bean);
	    }

	    private void SendSupportBeanEvent(int intPrimitive) {
	        SupportBean bean = new SupportBean();
	        bean.IntPrimitive = intPrimitive;
	        epService.EPRuntime.SendEvent(bean);
	    }
	}
} // end of namespace
