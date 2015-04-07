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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.epl;

using NUnit.Framework;

namespace com.espertech.esper.regression.db
{
    [TestFixture]
    public class TestDatabaseJoinOptions 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;

        [TearDown]
        public void TearDown()
        {
            _listener = null;
        }

        [Test]
        public void TestHasMetaSQLStringParam()
        {
            ConfigurationDBRef dbconfig = GetDBConfig();
            dbconfig.ColumnChangeCase = ConfigurationDBRef.ColumnChangeCaseEnum.UPPERCASE;
            Configuration configuration = GetConfig(dbconfig);
    
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            const string sql = "select myint from mytesttable where ${TheString} = myvarchar'" +
                               "metadatasql 'select myint from mytesttable'";
            String stmtText = "select MYINT from " +
                    " sql:MyDB ['" + sql + "] as s0," +
                    typeof(SupportBean).FullName + ".win:length(100) as s1";
    
            EPStatement statement = _epService.EPAdministrator.CreateEPL(stmtText);
            _listener = new SupportUpdateListener();
            statement.Events += _listener.Update;
    
            Assert.AreEqual(typeof(int?), statement.EventType.GetPropertyType("MYINT"));
    
            SendSupportBeanEvent("A");
            Assert.AreEqual(10, _listener.AssertOneGetNewAndReset().Get("MYINT"));
    
            SendSupportBeanEvent("H");
            Assert.AreEqual(80, _listener.AssertOneGetNewAndReset().Get("MYINT"));

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
#if TYPE_MAPPED_NOT_SUPPORTED
        [Test]
        public void TestTypeMapped()
        {
            ConfigurationDBRef dbconfig = GetDBConfig();
            dbconfig.ColumnChangeCase = ConfigurationDBRef.ColumnChangeCaseEnum.LOWERCASE;
            dbconfig.AddSqlTypesBinding(java.sql.Types.INTEGER, "string");
            Configuration configuration = GetConfig(dbconfig);
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
    
            const string sql = "select myint from mytesttable where ${IntPrimitive} = myint'" +
                               "metadatasql 'select myint from mytesttable'";
            string stmtText = "select myint from " +
                    " sql:MyDB ['" + sql + "] as s0," +
                    typeof(SupportBean).FullName + ".win:length(100) as s1";
    
            EPStatement statement = _epService.EPAdministrator.CreateEPL(stmtText);
            _listener = new SupportUpdateListener();
            statement.Events += _listener.Update;
    
            Assert.AreEqual(typeof(string), statement.EventType.GetPropertyType("myint"));
    
            SendSupportBeanEvent(10);
            Assert.AreEqual("10", _listener.AssertOneGetNewAndReset().Get("myint"));
    
            SendSupportBeanEvent(80);
            Assert.AreEqual("80", _listener.AssertOneGetNewAndReset().Get("myint"));

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
#endif

        [Test]
        public void TestNoMetaLexAnalysis()
        {
            ConfigurationDBRef dbconfig = GetDBConfig();
            Configuration configuration = GetConfig(dbconfig);
    
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
    
            const string sql = "select mydouble from mytesttable where ${IntPrimitive} = myint";
            Run(sql);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestNoMetaLexAnalysisGroup()
        {
            ConfigurationDBRef dbconfig = GetDBConfig();
            Configuration configuration = GetConfig(dbconfig);
    
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
    
            const string sql = "select mydouble, sum(myint) from mytesttable where ${IntPrimitive} = myint group by mydouble";
            Run(sql);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestPlaceholderWhere()
        {
            ConfigurationDBRef dbconfig = GetDBConfig();
            Configuration configuration = GetConfig(dbconfig);
    
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
    
            const string sql = "select mydouble from mytesttable ${$ESPER-SAMPLE-WHERE} where ${IntPrimitive} = myint";
            Run(sql);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        private void Run(String sql)
        {
            String stmtText = "select mydouble from " +
                    " sql:MyDB ['" + sql + "'] as s0," +
                    typeof(SupportBean).FullName + ".win:length(100) as s1";
    
            EPStatement statement = _epService.EPAdministrator.CreateEPL(stmtText);
            _listener = new SupportUpdateListener();
            statement.Events += _listener.Update;
    
            Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("mydouble"));
    
            SendSupportBeanEvent(10);
            Assert.AreEqual(1.2, _listener.AssertOneGetNewAndReset().Get("mydouble"));
    
            SendSupportBeanEvent(80);
            Assert.AreEqual(8.2, _listener.AssertOneGetNewAndReset().Get("mydouble"));
        }
    
        private static ConfigurationDBRef GetDBConfig()
        {
            var configDB = new ConfigurationDBRef();
            configDB.SetDatabaseDriver(SupportDatabaseService.DbDriverFactoryNative);
            configDB.ConnectionLifecycle = ConnectionLifecycleEnum.RETAIN;
            configDB.ConnectionCatalog = "test";
            //configDB.ConnectionReadOnly(true);
            configDB.ConnectionAutoCommit = true;
            return configDB;
        }
    
        private static Configuration GetConfig(ConfigurationDBRef configOracle)
        {
            var configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddDatabaseReference("MyDB", configOracle);
            configuration.EngineDefaults.LoggingConfig.IsEnableExecutionDebug = true;
    
            return configuration;
        }
    
        private void SendSupportBeanEvent(String stringValue)
        {
            var bean = new SupportBean {TheString = stringValue};
            _epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendSupportBeanEvent(int intPrimitive)
        {
            var bean = new SupportBean {IntPrimitive = intPrimitive};
            _epService.EPRuntime.SendEvent(bean);
        }
    }
}
