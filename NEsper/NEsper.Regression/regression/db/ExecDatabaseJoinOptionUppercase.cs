///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Data;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.db
{
    public class ExecDatabaseJoinOptionUppercase : RegressionExecution {
        public override void Configure(Configuration configuration) {
            ConfigurationDBRef dbconfig = GetDBConfig();
            dbconfig.ColumnChangeCase = ConfigurationDBRef.ColumnChangeCaseEnum.UPPERCASE;
            configuration.AddDatabaseReference("MyDB", dbconfig);
            configuration.EngineDefaults.Logging.IsEnableExecutionDebug = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            string sql = "select myint from mytesttable where ${TheString} = myvarchar'" +
                    "metadatasql 'select myint from mytesttable'";
            string stmtText = "select MYINT from " +
                    " sql:MyDB ['" + sql + "] as s0," +
                    typeof(SupportBean).FullName + "#length(100) as s1";
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            Assert.AreEqual(typeof(int?), statement.EventType.GetPropertyType("MYINT"));
    
            SendSupportBeanEvent(epService, "A");
            Assert.AreEqual(10, listener.AssertOneGetNewAndReset().Get("MYINT"));
    
            SendSupportBeanEvent(epService, "H");
            Assert.AreEqual(80, listener.AssertOneGetNewAndReset().Get("MYINT"));
        }
    
        internal static ConfigurationDBRef GetDBConfig() {
            var configDB = SupportDatabaseService.CreateDefaultConfig();
            configDB.ConnectionLifecycle = ConnectionLifecycleEnum.RETAIN;
            configDB.ConnectionCatalog = "test";
            configDB.ConnectionTransactionIsolation = IsolationLevel.Serializable;
            configDB.ConnectionAutoCommit = true;
            return configDB;
        }
    
        private void SendSupportBeanEvent(EPServiceProvider epService, string theString) {
            var bean = new SupportBean();
            bean.TheString = theString;
            epService.EPRuntime.SendEvent(bean);
        }
    }
} // end of namespace
