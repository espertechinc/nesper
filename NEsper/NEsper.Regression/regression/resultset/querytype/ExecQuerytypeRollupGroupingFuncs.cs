///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;
using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset.querytype
{
    public class ExecQuerytypeRollupGroupingFuncs : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionFAFCarEventAndGroupingFunc(epService);
            RunAssertionDocSampleCarEventAndGroupingFunc(epService);
            RunAssertionGroupingFuncExpressionUse(epService);
            RunAssertionInvalid(epService);
        }
    
        private void RunAssertionFAFCarEventAndGroupingFunc(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType(typeof(CarEvent));
            epService.EPAdministrator.CreateEPL("create window CarWindow#keepall as CarEvent");
            epService.EPAdministrator.CreateEPL("insert into CarWindow select * from CarEvent");
    
            epService.EPRuntime.SendEvent(new CarEvent("skoda", "france", 10000));
            epService.EPRuntime.SendEvent(new CarEvent("skoda", "germany", 5000));
            epService.EPRuntime.SendEvent(new CarEvent("bmw", "france", 100));
            epService.EPRuntime.SendEvent(new CarEvent("bmw", "germany", 1000));
            epService.EPRuntime.SendEvent(new CarEvent("opel", "france", 7000));
            epService.EPRuntime.SendEvent(new CarEvent("opel", "germany", 7000));
    
            string epl = "select name, place, sum(count), grouping(name), grouping(place), grouping_id(name, place) as gid " +
                    "from CarWindow group by grouping sets((name, place),name, place,())";
            EPOnDemandQueryResult result = epService.EPRuntime.ExecuteQuery(epl);
    
            Assert.AreEqual(typeof(int?), result.EventType.GetPropertyType("grouping(name)").GetBoxedType());
            Assert.AreEqual(typeof(int?), result.EventType.GetPropertyType("gid").GetBoxedType());
    
            var fields = new string[]{"name", "place", "sum(count)", "grouping(name)", "grouping(place)", "gid"};
            EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new object[][]{
                    new object[] {"skoda", "france", 10000, 0, 0, 0},
                    new object[] {"skoda", "germany", 5000, 0, 0, 0},
                    new object[] {"bmw", "france", 100, 0, 0, 0},
                    new object[] {"bmw", "germany", 1000, 0, 0, 0},
                    new object[] {"opel", "france", 7000, 0, 0, 0},
                    new object[] {"opel", "germany", 7000, 0, 0, 0},
                    new object[] {"skoda", null, 15000, 0, 1, 1},
                    new object[] {"bmw", null, 1100, 0, 1, 1},
                    new object[] {"opel", null, 14000, 0, 1, 1},
                    new object[] {null, "france", 17100, 1, 0, 2},
                    new object[] {null, "germany", 13000, 1, 0, 2},
                    new object[] {null, null, 30100, 1, 1, 3}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionDocSampleCarEventAndGroupingFunc(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType(typeof(CarEvent));
    
            // try simple
            string epl = "select name, place, sum(count), grouping(name), grouping(place), grouping_id(name,place) as gid " +
                    "from CarEvent group by grouping sets((name, place), name, place, ())";
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(epl).Events += listener.Update;
            TryAssertionDocSampleCarEvent(epService, listener);
            epService.EPAdministrator.DestroyAllStatements();
    
            // try audit
            epService.EPAdministrator.CreateEPL("@Audit " + epl).Events += listener.Update;
            TryAssertionDocSampleCarEvent(epService, listener);
            epService.EPAdministrator.DestroyAllStatements();
    
            // try model
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(epl);
            Assert.AreEqual(epl, model.ToEPL());
            EPStatement stmt = epService.EPAdministrator.Create(model);
            Assert.AreEqual(epl, stmt.Text);
            stmt.Events += listener.Update;
            TryAssertionDocSampleCarEvent(epService, listener);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryAssertionDocSampleCarEvent(EPServiceProvider epService, SupportUpdateListener listener) {
            var fields = new string[]{"name", "place", "sum(count)", "grouping(name)", "grouping(place)", "gid"};
            epService.EPRuntime.SendEvent(new CarEvent("skoda", "france", 100));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{
                    new object[] {"skoda", "france", 100, 0, 0, 0},
                    new object[] {"skoda", null, 100, 0, 1, 1},
                    new object[] {null, "france", 100, 1, 0, 2},
                    new object[] {null, null, 100, 1, 1, 3}});
    
            epService.EPRuntime.SendEvent(new CarEvent("skoda", "germany", 75));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{
                    new object[] {"skoda", "germany", 75, 0, 0, 0},
                    new object[] {"skoda", null, 175, 0, 1, 1},
                    new object[] {null, "germany", 75, 1, 0, 2},
                    new object[] {null, null, 175, 1, 1, 3}});
        }
    
        private void RunAssertionGroupingFuncExpressionUse(EPServiceProvider epService) {
            GroupingSupportFunc.Parameters.Clear();
            epService.EPAdministrator.Configuration.AddEventType(typeof(CarEvent));
    
            // test uncorrelated subquery and expression-declaration and single-row func
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("myfunc", typeof(GroupingSupportFunc), "Myfunc");
            epService.EPAdministrator.CreateEPL("create expression myExpr {x=> '|' || x.name || '|'}");
            epService.EPAdministrator.Configuration.AddEventType(typeof(CarInfoEvent));
            string epl = "select myfunc(" +
                    "  name, place, sum(count), grouping(name), grouping(place), grouping_id(name, place)," +
                    "  (select refId from CarInfoEvent#lastevent), " +
                    "  myExpr(ce)" +
                    "  )" +
                    "from CarEvent ce group by grouping sets((name, place),name, place,())";
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(epl).Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new CarInfoEvent("a", "b", "c01"));
    
            epService.EPRuntime.SendEvent(new CarEvent("skoda", "france", 10000));
            EPAssertionUtil.AssertEqualsExactOrder(new object[][]{
                    new object[] {"skoda", "france", 10000, 0, 0, 0, "c01", "|skoda|"},
                    new object[] {"skoda", null, 10000, 0, 1, 1, "c01", "|skoda|"},
                    new object[] {null, "france", 10000, 1, 0, 2, "c01", "|skoda|"},
                    new object[] {null, null, 10000, 1, 1, 3, "c01", "|skoda|"}}, GroupingSupportFunc.AssertGetAndClear(4));
            epService.EPAdministrator.DestroyAllStatements();
    
            // test "prev" and "prior"
            string[] fields = "c0,c1,c2,c3".Split(',');
            string eplTwo = "select prev(1, name) as c0, prior(1, name) as c1, name as c2, sum(count) as c3 from CarEvent#keepall ce group by Rollup(name)";
            epService.EPAdministrator.CreateEPL(eplTwo).Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new CarEvent("skoda", "france", 10));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{
                    new object[] {null, null, "skoda", 10}, new object[]{null, null, null, 10}
            });
    
            epService.EPRuntime.SendEvent(new CarEvent("vw", "france", 15));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{
                    new object[] {"skoda", "skoda", "vw", 15}, new object[]{"skoda", "skoda", null, 25}
            });
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            // invalid use of function
            string expected = "Failed to validate select-clause expression 'grouping(TheString)': The grouping function requires the group-by clause to specify rollup, cube or grouping sets, and may only be used in the select-clause, having-clause or order-by-clause [select grouping(TheString) from SupportBean]";
            TryInvalid(epService, "select grouping(TheString) from SupportBean", "Error starting statement: " + expected);
            TryInvalid(epService, "select TheString, sum(IntPrimitive) from SupportBean(grouping(TheString) = 1) group by Rollup(TheString)",
                    "Failed to validate filter expression 'grouping(TheString)=1': The grouping function requires the group-by clause to specify rollup, cube or grouping sets, and may only be used in the select-clause, having-clause or order-by-clause [select TheString, sum(IntPrimitive) from SupportBean(grouping(TheString) = 1) group by Rollup(TheString)]");
            TryInvalid(epService, "select TheString, sum(IntPrimitive) from SupportBean where grouping(TheString) = 1 group by Rollup(TheString)",
                    "Failed to validate filter expression 'grouping(TheString)=1': The grouping function requires the group-by clause to specify rollup, cube or grouping sets, and may only be used in the select-clause, having-clause or order-by-clause [select TheString, sum(IntPrimitive) from SupportBean where grouping(TheString) = 1 group by Rollup(TheString)]");
            TryInvalid(epService, "select TheString, sum(IntPrimitive) from SupportBean group by Rollup(grouping(TheString))",
                    "Error starting statement: The grouping function requires the group-by clause to specify rollup, cube or grouping sets, and may only be used in the select-clause, having-clause or order-by-clause [select TheString, sum(IntPrimitive) from SupportBean group by Rollup(grouping(TheString))]");
    
            // invalid parameters
            TryInvalid(epService, "select TheString, sum(IntPrimitive), grouping(LongPrimitive) from SupportBean group by Rollup(TheString)",
                    "Error starting statement: Group-by with rollup requires a fully-aggregated query, the query is not full-aggregated because of property 'LongPrimitive' [select TheString, sum(IntPrimitive), grouping(LongPrimitive) from SupportBean group by Rollup(TheString)]");
            TryInvalid(epService, "select TheString, sum(IntPrimitive), grouping(TheString||'x') from SupportBean group by Rollup(TheString)",
                    "Error starting statement: Failed to find expression 'TheString||\"x\"' among group-by expressions [select TheString, sum(IntPrimitive), grouping(TheString||'x') from SupportBean group by Rollup(TheString)]");
    
            TryInvalid(epService, "select TheString, sum(IntPrimitive), grouping_id(TheString, TheString) from SupportBean group by Rollup(TheString)",
                    "Error starting statement: Duplicate expression 'TheString' among grouping function parameters [select TheString, sum(IntPrimitive), grouping_id(TheString, TheString) from SupportBean group by Rollup(TheString)]");
        }
    
        public class GroupingSupportFunc {
            private static readonly IList<object[]> parameters = new List<object[]>();
    
            public static void Myfunc(string name,
                                      string place,
                                      int? cnt,
                                      int? grpName,
                                      int? grpPlace,
                                      int? grpId,
                                      string refId,
                                      string namePlusDelim) {
                parameters.Add(new object[]{name, place, cnt, grpName, grpPlace, grpId, refId, namePlusDelim});
            }

            public static IList<object[]> Parameters => parameters;

            public static object[][] AssertGetAndClear(int numRows) {
                Assert.AreEqual(numRows, parameters.Count);
                object[][] result = parameters.ToArray();
                parameters.Clear();
                return result;
            }
        }
    
        public class CarInfoEvent {
            private readonly string name;
            private readonly string place;
            private readonly string refId;

            public CarInfoEvent(string name, string place, string refId) {
                this.name = name;
                this.place = place;
                this.refId = refId;
            }

            public string Name => name;

            public string Place => place;

            public string RefId => refId;
        }
    
        public class CarEvent {
            private readonly string name;
            private readonly string place;
            private readonly int count;
    
            public CarEvent(string name, string place, int count) {
                this.name = name;
                this.place = place;
                this.count = count;
            }

            public string Name => name;

            public string Place => place;

            public int Count => count;
        }
    }
} // end of namespace
