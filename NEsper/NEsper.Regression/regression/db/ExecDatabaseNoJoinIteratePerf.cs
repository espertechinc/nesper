///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.db
{
    public class ExecDatabaseNoJoinIteratePerf : RegressionExecution
    {
        public override void Configure(Configuration configuration)
        {
            var configDB = SupportDatabaseService.CreateDefaultConfig();
            configDB.ConnectionLifecycle = ConnectionLifecycleEnum.RETAIN;
            configDB.LRUCache = 100000;
            configDB.ConnectionCatalog = "test";
            configuration.AddDatabaseReference("MyDB", configDB);
        }
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.CreateEPL("create variable bool queryvar_bool");
            epService.EPAdministrator.CreateEPL("create variable int lower");
            epService.EPAdministrator.CreateEPL("create variable int upper");
            epService.EPAdministrator.CreateEPL("on SupportBean set queryvar_bool=BoolPrimitive, lower=IntPrimitive,upper=IntBoxed");
    
            string stmtText = "select * from sql:MyDB ['select mybigint, mybool from mytesttable where ${queryvar_bool} = mytesttable.mybool and myint between ${lower} and ${upper} order by mybigint']";
            var fields = new string[]{"mybigint", "mybool"};
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            SendSupportBeanEvent(epService, true, 20, 60);
    
            long start = PerformanceObserver.MilliTime;
            for (int i = 0; i < 10000; i++) {
                EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {4L, true}});
            }
            long end = PerformanceObserver.MilliTime;
            long delta = end - start;
            Assert.IsTrue(delta < 1000, "delta=" + delta);
    
            stmt.Dispose();
        }
    
        private void SendSupportBeanEvent(EPServiceProvider epService, bool boolPrimitive, int intPrimitive, int intBoxed) {
            var bean = new SupportBean();
            bean.BoolPrimitive = boolPrimitive;
            bean.IntPrimitive = intPrimitive;
            bean.IntBoxed = intBoxed;
            epService.EPRuntime.SendEvent(bean);
        }
    }
} // end of namespace
