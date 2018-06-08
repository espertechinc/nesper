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
    public class ExecDatabaseJoinOptions : RegressionExecution {
        public override void Configure(Configuration configuration) {
            ConfigurationDBRef dbconfig = GetDBConfig();
            configuration.AddDatabaseReference("MyDB", dbconfig);
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionNoMetaLexAnalysis(epService);
            RunAssertionNoMetaLexAnalysisGroup(epService);
            RunAssertionPlaceholderWhere(epService);
        }
    
        private void RunAssertionNoMetaLexAnalysis(EPServiceProvider epService) {
            string sql = "select mydouble from mytesttable where ${IntPrimitive} = myint";
            Run(epService, sql);
        }
    
        private void RunAssertionNoMetaLexAnalysisGroup(EPServiceProvider epService) {
            string sql = "select mydouble, sum(myint) from mytesttable where ${IntPrimitive} = myint group by mydouble";
            Run(epService, sql);
        }
    
        private void RunAssertionPlaceholderWhere(EPServiceProvider epService) {
            string sql = "select mydouble from mytesttable ${$ESPER-SAMPLE-WHERE} where ${IntPrimitive} = myint";
            Run(epService, sql);
        }
    
        private void Run(EPServiceProvider epService, string sql) {
            string stmtText = "select mydouble from " +
                    " sql:MyDB ['" + sql + "'] as s0," +
                    typeof(SupportBean).FullName + "#length(100) as s1";
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("mydouble"));
    
            SendSupportBeanEvent(epService, 10);
            Assert.AreEqual(1.2, listener.AssertOneGetNewAndReset().Get("mydouble"));
    
            SendSupportBeanEvent(epService, 80);
            Assert.AreEqual(8.2, listener.AssertOneGetNewAndReset().Get("mydouble"));
    
            statement.Dispose();
        }
    
        private void SendSupportBeanEvent(EPServiceProvider epService, int intPrimitive) {
            var bean = new SupportBean();
            bean.IntPrimitive = intPrimitive;
            epService.EPRuntime.SendEvent(bean);
        }
    }
} // end of namespace
