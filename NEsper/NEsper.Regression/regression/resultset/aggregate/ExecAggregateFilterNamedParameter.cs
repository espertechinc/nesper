///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.rettype;
using com.espertech.esper.plugin;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset.aggregate
{
    public class ExecAggregateFilterNamedParameter : RegressionExecution
    {
        public override void Configure(Configuration configuration)
        {
            configuration.EngineDefaults.Execution.IsAllowIsolatedService = true;
            configuration.EngineDefaults.ViewResources.IsShareViews = false;
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType<SupportBean_S0>();
            configuration.AddEventType(typeof(SupportBean_S1));
        }

        public override void Run(EPServiceProvider epService)
        {
            RunAssertionFirstAggSODA(epService, false);
            RunAssertionFirstAggSODA(epService, true);

            RunAssertionMethodAggSQLAll(epService);
            RunAssertionMethodAggSQLMixedFilter(epService);
            RunAssertionMethodAggLeaving(epService);
            RunAssertionMethodAggnth(epService);
            RunAssertionMethodAggRateUnbound(epService);
            RunAssertionMethodAggRateBound(epService);
            RunAssertionMethodPlugIn(epService);

            RunAssertionAccessAggLinearBound(epService, false);
            RunAssertionAccessAggLinearBound(epService, true);
            RunAssertionAccessAggLinearUnbound(epService, false);
            RunAssertionAccessAggLinearUnbound(epService, true);
            RunAssertionAccessAggLinearWIndex(epService);
            RunAssertionAccessAggLinearBoundMixedFilter(epService);
            RunAssertionAccessAggPlugIn(epService);

            RunAssertionAccessAggSortedBound(epService, false);
            RunAssertionAccessAggSortedBound(epService, true);
            RunAssertionAccessAggSortedUnbound(epService, false);
            RunAssertionAccessAggSortedUnbound(epService, true);
            RunAssertionAccessAggSortedMulticriteria(epService);

            RunAssertionIntoTable(epService, false);
            RunAssertionIntoTable(epService, true);
            RunAssertionIntoTableCountMinSketch(epService);

            RunAssertionAuditAndReuse(epService);

            RunAssertionInvalid(epService);
        }

        private void RunAssertionAccessAggPlugIn(EPServiceProvider epService)
        {
            var config = new ConfigurationPlugInAggregationMultiFunction();
            config.FunctionNames = "concatAccessAgg".Split(',');
            config.MultiFunctionFactoryClassName = typeof(MyAccessAggFactory).FullName;
            epService.EPAdministrator.Configuration.AddPlugInAggregationMultiFunction(config);

            var fields = "c0".Split(',');
            var epl = "select ConcatAccessAgg(TheString, filter:TheString like 'A%') as c0 from SupportBean";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            SendEventAssert(epService, listener, "X1", 0, fields, new object[] {""});
            SendEventAssert(epService, listener, "A1", 0, fields, new object[] {"A1"});
            SendEventAssert(epService, listener, "A2", 0, fields, new object[] {"A1A2"});
            SendEventAssert(epService, listener, "X2", 0, fields, new object[] {"A1A2"});

            stmt.Dispose();
        }

        private void RunAssertionMethodPlugIn(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddPlugInAggregationFunctionFactory(
                "concatMethodAgg", typeof(MyMethodAggFuncFactory));

            var fields = "c0".Split(',');
            var epl = "select concatMethodAgg(TheString, filter:TheString like 'A%') as c0 from SupportBean";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            SendEventAssert(epService, listener, "X1", 0, fields, new object[] {""});
            SendEventAssert(epService, listener, "A1", 0, fields, new object[] {"A1"});
            SendEventAssert(epService, listener, "A2", 0, fields, new object[] {"A1A2"});
            SendEventAssert(epService, listener, "X2", 0, fields, new object[] {"A1A2"});

            stmt.Dispose();
        }

        private void RunAssertionIntoTableCountMinSketch(EPServiceProvider epService)
        {
            var epl =
                "create table WordCountTable(wordcms countMinSketch());\n" +
                "into table WordCountTable select countMinSketchAdd(TheString, filter:IntPrimitive > 0) as wordcms from SupportBean;\n" +
                "@Name('stmt') select WordCountTable.wordcms.countMinSketchFrequency(p00) as c0 from SupportBean_S0;\n";

            var deploymentResult = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.GetStatement("stmt").Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean_S1(0));

            SendEvent(epService, "hello", 0);
            SendEventAssertCount(epService, listener, "hello", 0L);

            SendEvent(epService, "name", 1);
            SendEventAssertCount(epService, listener, "name", 1L);

            SendEvent(epService, "name", 0);
            SendEventAssertCount(epService, listener, "name", 1L);

            SendEvent(epService, "name", 1);
            SendEventAssertCount(epService, listener, "name", 2L);

            epService.EPAdministrator.DeploymentAdmin.Undeploy(deploymentResult.DeploymentId);
        }

        private void RunAssertionMethodAggRateBound(EPServiceProvider epService)
        {
            var fields = "myrate,myqtyrate".Split(',');
            var epl = "select " +
                      "rate(LongPrimitive, filter:TheString like 'A%') as myrate, " +
                      "rate(LongPrimitive, IntPrimitive, filter:TheString like 'A%') as myqtyrate " +
                      "from SupportBean#length(3)";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            SendEventWLong(epService, "X1", 1000, 10);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {null, null});

            SendEventWLong(epService, "X2", 1200, 0);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {null, null});

            SendEventWLong(epService, "X2", 1300, 0);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {null, null});

            SendEventWLong(epService, "A1", 1000, 10);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {null, null});

            SendEventWLong(epService, "A2", 1200, 0);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {null, null});

            SendEventWLong(epService, "A3", 1300, 0);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {null, null});

            SendEventWLong(epService, "A4", 1500, 14);
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fields, new object[] {3 * 1000 / 500d, 14 * 1000 / 500d});

            SendEventWLong(epService, "A5", 2000, 11);
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fields, new object[] {3 * 1000 / 800d, 25 * 1000 / 800d});

            stmt.Dispose();
        }

        private void RunAssertionMethodAggRateUnbound(EPServiceProvider epService)
        {
            var isolated = epService.GetEPServiceIsolated("I1");
            isolated.EPRuntime.SendEvent(new CurrentTimeEvent(0));

            var fields = "c0".Split(',');
            var epl = "select rate(1, filter:TheString like 'A%') as c0 from SupportBean";
            var stmt = isolated.EPAdministrator.CreateEPL(epl, "stmt1", null);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            SendEventAssert(isolated, listener, "X1", 0, fields, new object[] {null});
            SendEventAssert(isolated, listener, "A1", 1, fields, new object[] {null});

            isolated.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
            SendEventAssert(isolated, listener, "X2", 2, fields, new object[] {null});
            SendEventAssert(isolated, listener, "A2", 2, fields, new object[] {1.0});
            SendEventAssert(isolated, listener, "A3", 3, fields, new object[] {2.0});

            stmt.Dispose();
            isolated.Dispose();
        }

        private void RunAssertionMethodAggnth(EPServiceProvider epService)
        {
            var fields = "c0".Split(',');
            var epl = "select nth(IntPrimitive, 1, filter:TheString like 'A%') as c0 from SupportBean";

            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            SendEventAssert(epService, listener, "X1", 0, fields, new object[] {null});
            SendEventAssert(epService, listener, "X2", 0, fields, new object[] {null});
            SendEventAssert(epService, listener, "A3", 1, fields, new object[] {null});
            SendEventAssert(epService, listener, "A4", 2, fields, new object[] {1});
            SendEventAssert(epService, listener, "X3", 0, fields, new object[] {1});
            SendEventAssert(epService, listener, "A5", 3, fields, new object[] {2});
            SendEventAssert(epService, listener, "X4", 0, fields, new object[] {2});

            stmt.Dispose();
        }

        private void RunAssertionMethodAggLeaving(EPServiceProvider epService)
        {
            var fields = "c0,c1".Split(',');
            var epl = "select " +
                      "leaving(filter:IntPrimitive=1) as c0," +
                      "leaving(filter:IntPrimitive=2) as c1" +
                      " from SupportBean#length(2)";

            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            SendEventAssert(epService, listener, "E1", 2, fields, new object[] {false, false});
            SendEventAssert(epService, listener, "E2", 1, fields, new object[] {false, false});
            SendEventAssert(epService, listener, "E3", 3, fields, new object[] {false, true});
            SendEventAssert(epService, listener, "E4", 4, fields, new object[] {true, true});

            stmt.Dispose();
        }

        private void RunAssertionAuditAndReuse(EPServiceProvider epService)
        {
            var epl = "select " +
                      "sum(IntPrimitive, filter:IntPrimitive=1) as c0, sum(IntPrimitive, filter:IntPrimitive=1) as c1, " +
                      "window(*, filter:IntPrimitive=1) as c2, window(*, filter:IntPrimitive=1) as c3 " +
                      " from SupportBean#length(3)";

            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));

            stmt.Dispose();
        }

        public void RunAssertionInvalid(EPServiceProvider epService)
        {
            // invalid filter expression name parameter: multiple values
            SupportMessageAssertUtil.TryInvalid(
                epService, "select sum(IntPrimitive, filter:(IntPrimitive, DoublePrimitive)) from SupportBean",
                "Error starting statement: Failed to validate select-clause expression 'sum(IntPrimitive,filter:(IntPrimiti...(55 chars)': Filter named parameter requires a single expression returning a boolean-typed value");

            // multiple filter expressions
            SupportMessageAssertUtil.TryInvalid(
                epService, "select sum(IntPrimitive, IntPrimitive > 0, filter:IntPrimitive < 0) from SupportBean",
                "Error starting statement: Failed to validate select-clause expression 'sum(IntPrimitive,IntPrimitive>0,fil...(54 chars)': Only a single filter expression can be provided");

            // invalid filter expression name parameter: not returning boolean
            SupportMessageAssertUtil.TryInvalid(
                epService, "select sum(IntPrimitive, filter:IntPrimitive) from SupportBean",
                "Error starting statement: Failed to validate select-clause expression 'sum(IntPrimitive,filter:IntPrimitive)': Filter named parameter requires a single expression returning a boolean-typed value");

            // create-table does not allow filters
            SupportMessageAssertUtil.TryInvalid(
                epService, "create table MyTable(totals sum(int, filter:true))",
                "Error starting statement: Failed to validate table-column expression 'sum(int,filter:true)': The 'group_by' and 'filter' parameter is not allowed in create-table statements");

            // invalid correlated subquery
            SupportMessageAssertUtil.TryInvalid(
                epService,
                "select (select sum(IntPrimitive, filter:s0.p00='a') from SupportBean) from SupportBean_S0 as s0",
                "Error starting statement: Failed to plan subquery number 1 querying SupportBean: Subselect aggregation functions cannot aggregate across correlated properties");
        }

        private void RunAssertionIntoTable(EPServiceProvider epService, bool join)
        {
            var epl =
                "create table MyTable(\n" +
                "totalA sum(int, true),\n" +
                "totalB sum(int, true),\n" +
                "winA window(*) @Type(SupportBean),\n" +
                "winB window(*) @Type(SupportBean),\n" +
                "sortedA sorted(IntPrimitive) @Type(SupportBean),\n" +
                "sortedB sorted(IntPrimitive) @Type(SupportBean));\n" +
                "into table MyTable select\n" +
                "sum(IntPrimitive, filter: TheString like 'A%') as totalA,\n" +
                "sum(IntPrimitive, filter: TheString like 'B%') as totalB,\n" +
                "window(sb, filter: TheString like 'A%') as winA,\n" +
                "window(sb, filter: TheString like 'B%') as winB,\n" +
                "sorted(sb, filter: TheString like 'A%') as sortedA,\n" +
                "sorted(sb, filter: TheString like 'B%') as sortedB\n" +
                "from " + (join ? "SupportBean_S1#lastevent, SupportBean#keepall as sb;\n" : "SupportBean as sb;\n") +
                "@Name('stmt') select MyTable.totalA as ta, MyTable.totalB as tb, MyTable.winA as wa, MyTable.winB as wb, MyTable.sortedA as sa, MyTable.sortedB as sb from SupportBean_S0";
            var deploymentResult = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.GetStatement("stmt").Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean_S1(0));

            SendEvent(epService, "X1", 1);
            SendEventAssertInfoTable(epService, listener, null, null, null, null, null, null);

            var a1 = SendEvent(epService, "A1", 1);
            SendEventAssertInfoTable(epService, listener, 1, null, new[] {a1}, null, new[] {a1}, null);

            var b2 = SendEvent(epService, "B2", 20);
            SendEventAssertInfoTable(epService, listener, 1, 20, new[] {a1}, new[] {b2}, new[] {a1}, new[] {b2});

            var a3 = SendEvent(epService, "A3", 10);
            SendEventAssertInfoTable(
                epService, listener, 11, 20, new[] {a1, a3}, new[] {b2}, new[] {a1, a3}, new[] {b2});

            var b4 = SendEvent(epService, "B4", 2);
            SendEventAssertInfoTable(
                epService, listener, 11, 22, new[] {a1, a3}, new[] {b2, b4}, new[] {a1, a3}, new[] {b4, b2});

            epService.EPAdministrator.DeploymentAdmin.Undeploy(deploymentResult.DeploymentId);
        }

        private void RunAssertionAccessAggLinearWIndex(EPServiceProvider epService)
        {
            var fields = "c0,c1,c2,c3".Split(',');
            var epl = "select " +
                      "first(IntPrimitive, 0, filter:TheString like 'A%') as c0," +
                      "first(IntPrimitive, 1, filter:TheString like 'A%') as c1," +
                      "last(IntPrimitive, 0, filter:TheString like 'A%') as c2," +
                      "last(IntPrimitive, 1, filter:TheString like 'A%') as c3" +
                      " from SupportBean#length(3)";

            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            SendEventAssert(epService, listener, "B1", 1, fields, new object[] {null, null, null, null});
            SendEventAssert(epService, listener, "A2", 2, fields, new object[] {2, null, 2, null});
            SendEventAssert(epService, listener, "A3", 3, fields, new object[] {2, 3, 3, 2});
            SendEventAssert(epService, listener, "A4", 4, fields, new object[] {2, 3, 4, 3});
            SendEventAssert(epService, listener, "B2", 2, fields, new object[] {3, 4, 4, 3});
            SendEventAssert(epService, listener, "B3", 3, fields, new object[] {4, null, 4, null});
            SendEventAssert(epService, listener, "B4", 4, fields, new object[] {null, null, null, null});

            stmt.Dispose();
        }

        private void RunAssertionAccessAggSortedBound(EPServiceProvider epService, bool join)
        {
            var fields = "aMaxby,aMinby,aSorted,bMaxby,bMinby,bSorted".Split(',');
            var epl = "select " +
                      "maxby(IntPrimitive, filter:TheString like 'A%').TheString as aMaxby," +
                      "minby(IntPrimitive, filter:TheString like 'A%').TheString as aMinby," +
                      "sorted(IntPrimitive, filter:TheString like 'A%') as aSorted," +
                      "maxby(IntPrimitive, filter:TheString like 'B%').TheString as bMaxby," +
                      "minby(IntPrimitive, filter:TheString like 'B%').TheString as bMinby," +
                      "sorted(IntPrimitive, filter:TheString like 'B%') as bSorted" +
                      " from " + (join ? "SupportBean_S1#lastevent, SupportBean#length(4)" : "SupportBean#length(4)");

            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean_S1(0));

            var b1 = SendEvent(epService, "B1", 1);
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fields, new object[] {null, null, null, "B1", "B1", new[] {b1}});

            var a10 = SendEvent(epService, "A10", 10);
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fields,
                new object[] {"A10", "A10", new[] {a10}, "B1", "B1", new[] {b1}});

            var b2 = SendEvent(epService, "B2", 2);
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fields,
                new object[] {"A10", "A10", new[] {a10}, "B2", "B1", new[] {b1, b2}});

            var a5 = SendEvent(epService, "A5", 5);
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fields,
                new object[] {"A10", "A5", new[] {a5, a10}, "B2", "B1", new[] {b1, b2}});

            var a15 = SendEvent(epService, "A15", 15);
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fields,
                new object[] {"A15", "A5", new[] {a5, a10, a15}, "B2", "B2", new[] {b2}});

            SendEvent(epService, "X3", 3);
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fields,
                new object[] {"A15", "A5", new[] {a5, a15}, "B2", "B2", new[] {b2}});

            SendEvent(epService, "X4", 4);
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fields,
                new object[] {"A15", "A5", new[] {a5, a15}, null, null, null});

            SendEvent(epService, "X5", 5);
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fields, new object[] {"A15", "A15", new[] {a15}, null, null, null});

            SendEvent(epService, "X6", 6);
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fields, new object[] {null, null, null, null, null, null});

            stmt.Dispose();
        }

        private void RunAssertionAccessAggSortedMulticriteria(EPServiceProvider epService)
        {
            var fields = "aSorted,bSorted".Split(',');
            var epl = "select " +
                      "sorted(IntPrimitive, DoublePrimitive, filter:TheString like 'A%') as aSorted," +
                      "sorted(IntPrimitive, DoublePrimitive, filter:TheString like 'B%') as bSorted" +
                      " from SupportBean#keepall";

            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            var b1 = SendEvent(epService, "B1", 1, 10);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {null, new[] {b1}});

            var a1 = SendEvent(epService, "A1", 100, 2);
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fields, new object[] {new[] {a1}, new[] {b1}});

            var b2 = SendEvent(epService, "B2", 1, 4);
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fields, new object[] {new[] {a1}, new[] {b2, b1}});

            var a2 = SendEvent(epService, "A2", 100, 3);
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fields, new object[] {new[] {a1, a2}, new[] {b2, b1}});

            stmt.Dispose();
        }

        private void RunAssertionAccessAggSortedUnbound(EPServiceProvider epService, bool join)
        {
            var fields = "aMaxby,aMaxbyever,aMinby,aMinbyever".Split(',');
            var epl = "select " +
                      "maxby(IntPrimitive, filter:TheString like 'A%').TheString as aMaxby," +
                      "maxbyever(IntPrimitive, filter:TheString like 'A%').TheString as aMaxbyever," +
                      "minby(IntPrimitive, filter:TheString like 'A%').TheString as aMinby," +
                      "minbyever(IntPrimitive, filter:TheString like 'A%').TheString as aMinbyever" +
                      " from " + (join ? "SupportBean_S1#lastevent, SupportBean#keepall" : "SupportBean");

            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean_S1(0));

            SendEventAssert(epService, listener, "B1", 1, fields, new object[] {null, null, null, null});
            SendEventAssert(epService, listener, "A10", 10, fields, new object[] {"A10", "A10", "A10", "A10"});
            SendEventAssert(epService, listener, "A5", 5, fields, new object[] {"A10", "A10", "A5", "A5"});
            SendEventAssert(epService, listener, "A15", 15, fields, new object[] {"A15", "A15", "A5", "A5"});
            SendEventAssert(epService, listener, "B1000", 1000, fields, new object[] {"A15", "A15", "A5", "A5"});

            stmt.Dispose();
        }

        private void RunAssertionAccessAggLinearBound(EPServiceProvider epService, bool join)
        {
            var fields = "aFirst,aLast,aWindow,bFirst,bLast,bWindow".Split(',');
            var epl = "select " +
                      "first(IntPrimitive, filter:TheString like 'A%') as aFirst," +
                      "last(IntPrimitive, filter:TheString like 'A%') as aLast," +
                      "window(IntPrimitive, filter:TheString like 'A%') as aWindow," +
                      "first(IntPrimitive, filter:TheString like 'B%') as bFirst," +
                      "last(IntPrimitive, filter:TheString like 'B%') as bLast," +
                      "window(IntPrimitive, filter:TheString like 'B%') as bWindow" +
                      " from " + (join ? "SupportBean_S1#lastevent, SupportBean#length(5)" : "SupportBean#length(5)");

            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean_S1(0));

            SendEventAssert(epService, listener, "X1", 1, fields, new object[] {null, null, null, null, null, null});
            SendEventAssert(epService, listener, "B2", 2, fields, new object[] {null, null, null, 2, 2, new[] {2}});
            SendEventAssert(epService, listener, "B3", 3, fields, new object[] {null, null, null, 2, 3, new[] {2, 3}});
            SendEventAssert(epService, listener, "A4", 4, fields, new object[] {4, 4, new[] {4}, 2, 3, new[] {2, 3}});
            SendEventAssert(
                epService, listener, "B5", 5, fields, new object[] {4, 4, new[] {4}, 2, 5, new[] {2, 3, 5}});
            SendEventAssert(
                epService, listener, "A6", 6, fields, new object[] {4, 6, new[] {4, 6}, 2, 5, new[] {2, 3, 5}});
            SendEventAssert(
                epService, listener, "X2", 7, fields, new object[] {4, 6, new[] {4, 6}, 3, 5, new[] {3, 5}});
            SendEventAssert(epService, listener, "X3", 8, fields, new object[] {4, 6, new[] {4, 6}, 5, 5, new[] {5}});
            SendEventAssert(epService, listener, "X4", 9, fields, new object[] {6, 6, new[] {6}, 5, 5, new[] {5}});
            SendEventAssert(epService, listener, "X5", 10, fields, new object[] {6, 6, new[] {6}, null, null, null});
            SendEventAssert(epService, listener, "X6", 11, fields, new object[] {null, null, null, null, null, null});

            stmt.Dispose();
        }

        private void RunAssertionAccessAggLinearUnbound(EPServiceProvider epService, bool join)
        {
            var fields = "aFirst,aFirstever,aLast,aLastever,aCountever".Split(',');
            var epl = "select " +
                      "first(IntPrimitive, filter:TheString like 'A%') as aFirst," +
                      "firstever(IntPrimitive, filter:TheString like 'A%') as aFirstever," +
                      "last(IntPrimitive, filter:TheString like 'A%') as aLast," +
                      "lastever(IntPrimitive, filter:TheString like 'A%') as aLastever," +
                      "countever(IntPrimitive, filter:TheString like 'A%') as aCountever" +
                      " from " + (join ? "SupportBean_S1#lastevent, SupportBean#keepall" : "SupportBean");

            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean_S1(0));

            SendEventAssert(epService, listener, "X0", 0, fields, new object[] {null, null, null, null, 0L});
            SendEventAssert(epService, listener, "A1", 1, fields, new object[] {1, 1, 1, 1, 1L});
            SendEventAssert(epService, listener, "X2", 2, fields, new object[] {1, 1, 1, 1, 1L});
            SendEventAssert(epService, listener, "A3", 3, fields, new object[] {1, 1, 3, 3, 2L});
            SendEventAssert(epService, listener, "X4", 4, fields, new object[] {1, 1, 3, 3, 2L});

            stmt.Dispose();
        }

        private void RunAssertionAccessAggLinearBoundMixedFilter(EPServiceProvider epService)
        {
            var fields = "c0,c1,c2".Split(',');
            var epl = "select " +
                      "window(sb, filter:TheString like 'A%') as c0," +
                      "window(sb) as c1," +
                      "window(filter:TheString like 'B%', sb) as c2" +
                      " from SupportBean#keepall as sb";

            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            var x1 = SendEvent(epService, "X1", 1);
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fields, new object[] {null, new[] {x1}, null});

            var a2 = SendEvent(epService, "A2", 2);
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fields, new object[] {new[] {a2}, new[] {x1, a2}, null});

            var b3 = SendEvent(epService, "B3", 3);
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fields, new object[] {new[] {a2}, new[] {x1, a2, b3}, new[] {b3}});

            stmt.Dispose();
        }

        private void RunAssertionMethodAggSQLMixedFilter(EPServiceProvider epService)
        {
            var fields = "c0,c1,c2".Split(',');
            var epl = "select " +
                      "sum(IntPrimitive, filter:TheString like 'A%') as c0," +
                      "sum(IntPrimitive) as c1," +
                      "sum(filter:TheString like 'B%', IntPrimitive) as c2" +
                      " from SupportBean";

            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            SendEventAssert(epService, listener, "X1", 1, fields, new object[] {null, 1, null});
            SendEventAssert(epService, listener, "B2", 20, fields, new object[] {null, 1 + 20, 20});
            SendEventAssert(epService, listener, "A3", 300, fields, new object[] {300, 1 + 20 + 300, 20});
            SendEventAssert(epService, listener, "X1", 2, fields, new object[] {300, 1 + 20 + 300 + 2, 20});

            stmt.Dispose();
        }

        private void RunAssertionMethodAggSQLAll(EPServiceProvider epService)
        {
            var epl = "select " +
                      "avedev(DoublePrimitive, filter:IntPrimitive between 1 and 3) as cAvedev," +
                      "avg(DoublePrimitive, filter:IntPrimitive between 1 and 3) as cAvg, " +
                      "count(*, filter:IntPrimitive between 1 and 3) as cCount, " +
                      "max(DoublePrimitive, filter:IntPrimitive between 1 and 3) as cMax, " +
                      "fmax(DoublePrimitive, filter:IntPrimitive between 1 and 3) as cFmax, " +
                      "maxever(DoublePrimitive, filter:IntPrimitive between 1 and 3) as cMaxever, " +
                      "fmaxever(DoublePrimitive, filter:IntPrimitive between 1 and 3) as cFmaxever, " +
                      "median(DoublePrimitive, filter:IntPrimitive between 1 and 3) as cMedian, " +
                      "min(DoublePrimitive, filter:IntPrimitive between 1 and 3) as cMin, " +
                      "fmin(DoublePrimitive, filter:IntPrimitive between 1 and 3) as cFmin, " +
                      "minever(DoublePrimitive, filter:IntPrimitive between 1 and 3) as cMinever, " +
                      "fminever(DoublePrimitive, filter:IntPrimitive between 1 and 3) as cFminever, " +
                      "stddev(DoublePrimitive, filter:IntPrimitive between 1 and 3) as cStddev, " +
                      "sum(DoublePrimitive, filter:IntPrimitive between 1 and 3) as cSum " +
                      "from SupportBean";

            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            SendEventAssertSQLFuncs(
                epService, listener, "E1", 0, 50, null, null, 0L, null, null, null, null, null, null, null, null, null,
                null, null);
            SendEventAssertSQLFuncs(
                epService, listener, "E2", 2, 10, 0.0, 10d, 1L, 10d, 10d, 10d, 10d, 10.0, 10d, 10d, 10d, 10d, null,
                10d);
            SendEventAssertSQLFuncs(
                epService, listener, "E3", 100, 10, 0.0, 10d, 1L, 10d, 10d, 10d, 10d, 10.0, 10d, 10d, 10d, 10d, null,
                10d);
            SendEventAssertSQLFuncs(
                epService, listener, "E4", 1, 20, 5.0, 15d, 2L, 20d, 20d, 20d, 20d, 15.0, 10d, 10d, 10d, 10d,
                7.0710678118654755, 30d);

            stmt.Dispose();
        }

        private void RunAssertionFirstAggSODA(EPServiceProvider epService, bool soda)
        {
            var fields = "c0,c1".Split(',');
            var epl = "select " +
                      "first(*,filter:IntPrimitive=1).TheString as c0, " +
                      "first(*,filter:IntPrimitive=2).TheString as c1" +
                      " from SupportBean#length(3)";
            var stmt = SupportModelHelper.CreateByCompileOrParse(epService, soda, epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            SendEventAssert(epService, listener, "E1", 3, fields, new object[] {null, null});
            SendEventAssert(epService, listener, "E2", 2, fields, new object[] {null, "E2"});
            SendEventAssert(epService, listener, "E3", 1, fields, new object[] {"E3", "E2"});
            SendEventAssert(epService, listener, "E4", 2, fields, new object[] {"E3", "E2"});
            SendEventAssert(epService, listener, "E5", -1, fields, new object[] {"E3", "E4"});
            SendEventAssert(epService, listener, "E6", -1, fields, new object[] {null, "E4"});
            SendEventAssert(epService, listener, "E7", -1, fields, new object[] {null, null});

            stmt.Dispose();
        }

        private void SendEventAssertSQLFuncs(
            EPServiceProvider epService,
            SupportUpdateListener listener,
            string theString,
            int intPrimitive,
            double doublePrimitive,
            object cAvedev,
            object cAvg, 
            object cCount,
            object cMax,
            object cFmax, 
            object cMaxever, 
            object cFmaxever,
            object cMedian,
            object cMin,
            object cFmin, 
            object cMinever,
            object cFminever,
            object cStddev,
            object cSum)
        {
            var sb = new SupportBean(theString, intPrimitive);
            sb.DoublePrimitive = doublePrimitive;
            epService.EPRuntime.SendEvent(sb);
            var @event = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(cAvedev, @event.Get("cAvedev"));
            Assert.AreEqual(cAvg, @event.Get("cAvg"));
            Assert.AreEqual(cCount, @event.Get("cCount"));
            Assert.AreEqual(cMax, @event.Get("cMax"));
            Assert.AreEqual(cFmax, @event.Get("cFmax"));
            Assert.AreEqual(cMaxever, @event.Get("cMaxever"));
            Assert.AreEqual(cFmaxever, @event.Get("cFmaxever"));
            Assert.AreEqual(cMedian, @event.Get("cMedian"));
            Assert.AreEqual(cMin, @event.Get("cMin"));
            Assert.AreEqual(cFmin, @event.Get("cFmin"));
            Assert.AreEqual(cMinever, @event.Get("cMinever"));
            Assert.AreEqual(cFminever, @event.Get("cFminever"));
            Assert.AreEqual(cStddev, @event.Get("cStddev"));
            Assert.AreEqual(cSum, @event.Get("cSum"));
        }

        private void SendEventAssert(
            EPServiceProvider epService, SupportUpdateListener listener, string theString, int intPrimitive,
            string[] fields, object[] expected)
        {
            SendEvent(epService, theString, intPrimitive);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, expected);
        }

        private void SendEventAssert(
            EPServiceProviderIsolated isolated, SupportUpdateListener listener, string theString, int intPrimitive,
            string[] fields, object[] expected)
        {
            isolated.EPRuntime.SendEvent(new SupportBean(theString, intPrimitive));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, expected);
        }

        private SupportBean SendEvent(EPServiceProvider epService, string theString, int intPrimitive)
        {
            return SendEvent(epService, theString, intPrimitive, -1);
        }

        private SupportBean SendEvent(
            EPServiceProvider epService, string theString, int intPrimitive, double doublePrimitive)
        {
            var sb = new SupportBean(theString, intPrimitive);
            sb.DoublePrimitive = doublePrimitive;
            epService.EPRuntime.SendEvent(sb);
            return sb;
        }

        private void SendEventAssertInfoTable(
            EPServiceProvider epService, SupportUpdateListener listener, object ta, object tb, object wa, object wb,
            object sa, object sb)
        {
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), "ta,tb,wa,wb,sa,sb".Split(','), new[] {ta, tb, wa, wb, sa, sb});
        }

        private void SendEventAssertCount(
            EPServiceProvider epService, SupportUpdateListener listener, string p00, object expected)
        {
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, p00));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "c0".Split(','), new[] {expected});
        }

        private void SendEventWLong(EPServiceProvider epService, string theString, long longPrimitive, int intPrimitive)
        {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            epService.EPRuntime.SendEvent(bean);
        }

        public class MyMethodAggFuncFactory : AggregationFunctionFactory
        {
            public void Validate(AggregationValidationContext validationContext)
            {
                Assert.IsNotNull(validationContext.NamedParameters.Get("filter").First());
            }

            public AggregationMethod NewAggregator()
            {
                return new MyMethodAggMethod();
            }

            public string FunctionName
            {
                get => null;
                set { }
            }

            public Type ValueType => typeof(string);
        }

        public class MyMethodAggMethod : AggregationMethod
        {
            private StringWriter _buffer = new StringWriter();

            public void Enter(object value)
            {
                var arr = (object[]) value;
                var pass = arr[1];
                if (true.Equals(pass))
                {
                    _buffer.Write(arr[0].ToString());
                }
            }

            public void Leave(object value)
            {
                // not implemented
            }

            public void Clear()
            {
                _buffer = new StringWriter();
            }

            public object Value => _buffer.ToString();
        }

        public class MyAccessAggFactory : PlugInAggregationMultiFunctionFactory
        {
            public void AddAggregationFunction(PlugInAggregationMultiFunctionDeclarationContext declarationContext)
            {
            }

            public PlugInAggregationMultiFunctionHandler ValidateGetHandler(
                PlugInAggregationMultiFunctionValidationContext validationContext)
            {
                Assert.IsNotNull(validationContext.NamedParameters.Get("filter").First());
                var valueEval = validationContext.ParameterExpressions[0].ExprEvaluator;
                ExprEvaluator filterEval = validationContext.NamedParameters.Get("filter")[0].ExprEvaluator;
                return new MyAccessAggHandler(valueEval, filterEval);
            }
        }

        public class ProxyAggregationAccessor : AggregationAccessor
        {
            public Func<AggregationState, EvaluateParams, object> ProcGetValue;

            public object GetValue(AggregationState state, EvaluateParams evalParams)
            {
                return ProcGetValue(state, evalParams);
            }

            public ICollection<EventBean> GetEnumerableEvents(AggregationState state, EvaluateParams evalParams)
            {
                return null;
            }

            public EventBean GetEnumerableEvent(AggregationState state, EvaluateParams evalParams)
            {
                return null;
            }

            public ICollection<object> GetEnumerableScalar(AggregationState state, EvaluateParams evalParams)
            {
                return null;
            }
        }

        public class MyAccessAggHandler : PlugInAggregationMultiFunctionHandler
        {
            private readonly ExprEvaluator _filterEval;
            private readonly ExprEvaluator _valueEval;

            public MyAccessAggHandler(ExprEvaluator valueEval, ExprEvaluator filterEval)
            {
                _valueEval = valueEval;
                _filterEval = filterEval;
            }

            public AggregationAccessor Accessor
            {
                get
                {
                    return new ProxyAggregationAccessor
                    {
                        ProcGetValue = (state, evaluateParams) => ((MyAccessAggState) state).Buffer.ToString()
                    };
                }
            }

            public EPType ReturnType => EPTypeHelper.SingleValue(typeof(string));

            public AggregationStateKey AggregationStateUniqueKey => new ProxyAggregationStateKey();

            public PlugInAggregationMultiFunctionStateFactory StateFactory
            {
                get
                {
                    return new ProxyPlugInAggregationMultiFunctionStateFactory
                    {
                        ProcMakeAggregationState = stateContext => new MyAccessAggState(_valueEval, _filterEval)
                    };
                }
            }

            public AggregationAgent GetAggregationAgent(PlugInAggregationMultiFunctionAgentContext agentContext)
            {
                return null;
            }
        }

        public class MyAccessAggState : AggregationState
        {
            private readonly ExprEvaluator _filterEval;
            private readonly ExprEvaluator _valueEval;

            public MyAccessAggState(ExprEvaluator valueEval, ExprEvaluator filterEval)
            {
                _valueEval = valueEval;
                _filterEval = filterEval;
            }

            public StringWriter Buffer { get; private set; } = new StringWriter();

            public void ApplyEnter(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
            {
                var evaluateParams = new EvaluateParams(eventsPerStream, true, exprEvaluatorContext);
                var pass = _filterEval.Evaluate(evaluateParams);
                if (true.Equals(pass))
                {
                    var value = _valueEval.Evaluate(evaluateParams);
                    Buffer.Write(value);
                }
            }

            public void ApplyLeave(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
            {
                // no need
            }

            public void Clear()
            {
                Buffer = new StringWriter();
            }
        }
    }
} // end of namespace