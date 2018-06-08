///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using static com.espertech.esper.regression.db.ExecDatabaseJoinOptionUppercase;

using NUnit.Framework;

namespace com.espertech.esper.regression.db
{
    public class ExecDatabaseJoinOptionLowercase : RegressionExecution {
        public override void Configure(Configuration configuration) {
            ConfigurationDBRef dbconfig = GetDBConfig();
            dbconfig.ColumnChangeCase = ConfigurationDBRef.ColumnChangeCaseEnum.LOWERCASE;
            //dbconfig.AddSqlTypesBinding(java.sql.Types.INTEGER, "string");
            configuration.AddDatabaseReference("MyDB", dbconfig);
        }
    
        public override void Run(EPServiceProvider epService) {
            string sql = "select myint from mytesttable where ${IntPrimitive} = myint'" +
                    "metadatasql 'select myint from mytesttable'";
            string stmtText = "select myint from " +
                    " sql:MyDB ['" + sql + "] as s0," +
                    typeof(SupportBean).FullName + "#length(100) as s1";
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            Assert.AreEqual(typeof(int?), statement.EventType.GetPropertyType("myint"));
    
            SendSupportBeanEvent(epService, 10);
            Assert.AreEqual(10, listener.AssertOneGetNewAndReset().Get("myint"));
    
            SendSupportBeanEvent(epService, 80);
            Assert.AreEqual(80, listener.AssertOneGetNewAndReset().Get("myint"));
        }
    
        private void SendSupportBeanEvent(EPServiceProvider epService, int intPrimitive) {
            var bean = new SupportBean();
            bean.IntPrimitive = intPrimitive;
            epService.EPRuntime.SendEvent(bean);
        }
    }
} // end of namespace
