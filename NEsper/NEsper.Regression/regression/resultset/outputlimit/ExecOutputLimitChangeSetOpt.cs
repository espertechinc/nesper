///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.core.service;
using com.espertech.esper.core.service.resource;
using com.espertech.esper.epl.view;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset.outputlimit
{
    public class ExecOutputLimitChangeSetOpt : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
    
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            var currentTime = new AtomicLong(0);
            SendTime(epService, currentTime.Get());
    
            // unaggregated and ungrouped
            //
            TryAssertion(epService, currentTime, 0, false, "intPrimitive", null, null, "last", null);
            TryAssertion(epService, currentTime, 0, false, "intPrimitive", null, null, "last", "order by intPrimitive");
    
            TryAssertion(epService, currentTime, 5, false, "intPrimitive", null, null, "all", null);
            TryAssertion(epService, currentTime, 0, true, "intPrimitive", null, null, "all", null);
    
            TryAssertion(epService, currentTime, 0, false, "intPrimitive", null, null, "first", null);
    
            // fully-aggregated and ungrouped
            TryAssertion(epService, currentTime, 5, false, "count(*)", null, null, "last", null);
            TryAssertion(epService, currentTime, 0, true, "count(*)", null, null, "last", null);
    
            TryAssertion(epService, currentTime, 5, false, "count(*)", null, null, "all", null);
            TryAssertion(epService, currentTime, 0, true, "count(*)", null, null, "all", null);
    
            TryAssertion(epService, currentTime, 0, false, "count(*)", null, null, "first", null);
            TryAssertion(epService, currentTime, 0, false, "count(*)", null, "having count(*) > 0", "first", null);
    
            // aggregated and ungrouped
            TryAssertion(epService, currentTime, 5, false, "theString, count(*)", null, null, "last", null);
            TryAssertion(epService, currentTime, 0, true, "theString, count(*)", null, null, "last", null);
    
            TryAssertion(epService, currentTime, 5, false, "theString, count(*)", null, null, "all", null);
            TryAssertion(epService, currentTime, 0, true, "theString, count(*)", null, null, "all", null);
    
            TryAssertion(epService, currentTime, 0, true, "theString, count(*)", null, null, "first", null);
            TryAssertion(epService, currentTime, 0, true, "theString, count(*)", null, "having count(*) > 0", "first", null);
    
            // fully-aggregated and grouped
            TryAssertion(epService, currentTime, 5, false, "theString, count(*)", "group by theString", null, "last", null);
            TryAssertion(epService, currentTime, 0, true, "theString, count(*)", "group by theString", null, "last", null);
    
            TryAssertion(epService, currentTime, 5, false, "theString, count(*)", "group by theString", null, "all", null);
            TryAssertion(epService, currentTime, 0, true, "theString, count(*)", "group by theString", null, "all", null);
    
            TryAssertion(epService, currentTime, 0, false, "theString, count(*)", "group by theString", null, "first", null);
    
            // aggregated and grouped
            TryAssertion(epService, currentTime, 5, false, "theString, intPrimitive, count(*)", "group by theString", null, "last", null);
            TryAssertion(epService, currentTime, 0, true, "theString, intPrimitive, count(*)", "group by theString", null, "last", null);
    
            TryAssertion(epService, currentTime, 5, false, "theString, intPrimitive, count(*)", "group by theString", null, "all", null);
    
            TryAssertion(epService, currentTime, 0, false, "theString, intPrimitive, count(*)", "group by theString", null, "first", null);
    
            SupportMessageAssertUtil.TryInvalid(epService,
                    "@Hint('enable_outputlimit_opt') select sum(intPrimitive) " +
                            "from SupportBean output last every 4 events order by theString",
                    "Error starting statement: Error in the output rate limiting clause: The ENABLE_OUTPUTLIMIT_OPT hint is not supported with order-by");
        }
    
        private void TryAssertion(EPServiceProvider epService, AtomicLong currentTime,
                                  int expected,
                                  bool withHint,
                                  string selectClause,
                                  string groupBy,
                                  string having,
                                  string outputKeyword,
                                  string orderBy) {
            string epl = (withHint ? "@Hint('enable_outputlimit_opt') " : "") +
                    "select irstream " + selectClause + " " +
                    "from SupportBean#length(2) " +
                    (groupBy == null ? "" : groupBy + " ") +
                    (having == null ? "" : having + " ") +
                    "output " + outputKeyword + " every 1 seconds " +
                    (orderBy == null ? "" : orderBy);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            for (int i = 0; i < 5; i++) {
                epService.EPRuntime.SendEvent(new SupportBean("E" + i, i));
            }
    
            AssertResourcesOutputRate(stmt, expected);
    
            SendTime(epService, currentTime.AddAndGet(1000));
    
            AssertResourcesOutputRate(stmt, 0);
            stmt.Dispose();
            listener.Reset();
        }
    
        private void AssertResourcesOutputRate(EPStatement stmt, int numExpectedChangeset) {
            EPStatementSPI spi = (EPStatementSPI) stmt;
            StatementResourceHolder resources = spi.StatementContext.StatementExtensionServicesContext.StmtResources.ResourcesUnpartitioned;
            OutputProcessViewBase outputProcessViewBase = (OutputProcessViewBase) resources.EventStreamViewables[0].Views[0].Views[0];
            try {
                Assert.AreEqual(numExpectedChangeset, outputProcessViewBase.NumChangesetRows);
            } catch (UnsupportedOperationException ex) {
                // allowed
            }
        }
    
        private void SendTime(EPServiceProvider epService, long currentTime) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(currentTime));
        }
    }
} // end of namespace
