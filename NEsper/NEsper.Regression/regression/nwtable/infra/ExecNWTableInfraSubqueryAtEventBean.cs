///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.infra
{
    public class ExecNWTableInfraSubqueryAtEventBean : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("ABean", typeof(SupportBean_S0));
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>();
    
            RunAssertionSubSelStar(epService, true);
            RunAssertionSubSelStar(epService, false);
        }
    
        private void RunAssertionSubSelStar(EPServiceProvider epService, bool namedWindow) {
            string eplCreate = namedWindow ?
                    "create window MyInfra#keepall as (c0 string, c1 int)" :
                    "create table MyInfra(c0 string primary key, c1 int)";
            epService.EPAdministrator.CreateEPL(eplCreate);
    
            // create insert into
            string eplInsert = "insert into MyInfra select TheString as c0, IntPrimitive as c1 from SupportBean";
            epService.EPAdministrator.CreateEPL(eplInsert);
    
            // create subquery
            string eplSubquery = "select p00, (select * from MyInfra) @eventbean as detail from SupportBean_S0";
            EPStatement stmtSubquery = epService.EPAdministrator.CreateEPL(eplSubquery);
            var listener = new SupportUpdateListener();
            stmtSubquery.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            AssertReceived(listener, null);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            AssertReceived(listener, new object[][]{new object[] {"E1", 1}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            AssertReceived(listener, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 2}});
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void AssertReceived(SupportUpdateListener listener, object[][] values) {
            EventBean @event = listener.AssertOneGetNewAndReset();
            EventBean[] events = (EventBean[]) @event.GetFragment("detail");
            if (values == null) {
                Assert.IsNull(events);
                return;
            }
            EPAssertionUtil.AssertPropsPerRowAnyOrder(events, "c0,c1".Split(','), values);
        }
    }
} // end of namespace
