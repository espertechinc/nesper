///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;
// using static org.junit.Assert.assertEquals;

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
    
            string epl = "select name, place, sum(count), Grouping(name), Grouping(place), Grouping_id(name, place) as gid " +
                    "from CarWindow group by grouping Sets((name, place),name, place,())";
            EPOnDemandQueryResult result = epService.EPRuntime.ExecuteQuery(epl);
    
            Assert.AreEqual(typeof(int?), result.EventType.GetPropertyType("Grouping(name)"));
            Assert.AreEqual(typeof(int?), result.EventType.GetPropertyType("gid"));
    
            var fields = new string[]{"name", "place", "sum(count)", "Grouping(name)", "Grouping(place)", "gid"};
            EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new Object[][]{
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
            string epl = "select name, place, sum(count), Grouping(name), Grouping(place), Grouping_id(name,place) as gid " +
                    "from CarEvent group by grouping Sets((name, place), name, place, ())";
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(epl).AddListener(listener);
            TryAssertionDocSampleCarEvent(epService, listener);
            epService.EPAdministrator.DestroyAllStatements();
    
            // try audit
            epService.EPAdministrator.CreateEPL("@Audit " + epl).AddListener(listener);
            TryAssertionDocSampleCarEvent(epService, listener);
            epService.EPAdministrator.DestroyAllStatements();
    
            // try model
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(epl);
            Assert.AreEqual(epl, model.ToEPL());
            EPStatement stmt = epService.EPAdministrator.Create(model);
            Assert.AreEqual(epl, stmt.Text);
            stmt.AddListener(listener);
            TryAssertionDocSampleCarEvent(epService, listener);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryAssertionDocSampleCarEvent(EPServiceProvider epService, SupportUpdateListener listener) {
            var fields = new string[]{"name", "place", "sum(count)", "Grouping(name)", "Grouping(place)", "gid"};
            epService.EPRuntime.SendEvent(new CarEvent("skoda", "france", 100));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{
                    new object[] {"skoda", "france", 100, 0, 0, 0},
                    new object[] {"skoda", null, 100, 0, 1, 1},
                    new object[] {null, "france", 100, 1, 0, 2},
                    new object[] {null, null, 100, 1, 1, 3}});
    
            epService.EPRuntime.SendEvent(new CarEvent("skoda", "germany", 75));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{
                    new object[] {"skoda", "germany", 75, 0, 0, 0},
                    new object[] {"skoda", null, 175, 0, 1, 1},
                    new object[] {null, "germany", 75, 1, 0, 2},
                    new object[] {null, null, 175, 1, 1, 3}});
        }
    
        private void RunAssertionGroupingFuncExpressionUse(EPServiceProvider epService) {
            GroupingSupportFunc.Parameters.Clear();
            epService.EPAdministrator.Configuration.AddEventType(typeof(CarEvent));
    
            // test uncorrelated subquery and expression-declaration and single-row func
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("myfunc", typeof(GroupingSupportFunc).Name, "myfunc");
            epService.EPAdministrator.CreateEPL("create expression myExpr {x=> '|' || x.name || '|'}");
            epService.EPAdministrator.Configuration.AddEventType(typeof(CarInfoEvent));
            string epl = "select Myfunc(" +
                    "  name, place, sum(count), Grouping(name), Grouping(place), Grouping_id(name, place)," +
                    "  (select refId from CarInfoEvent#lastevent), " +
                    "  MyExpr(ce)" +
                    "  )" +
                    "from CarEvent ce group by grouping Sets((name, place),name, place,())";
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(epl).AddListener(listener);
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new CarInfoEvent("a", "b", "c01"));
    
            epService.EPRuntime.SendEvent(new CarEvent("skoda", "france", 10000));
            EPAssertionUtil.AssertEqualsExactOrder(new Object[][]{
                    new object[] {"skoda", "france", 10000, 0, 0, 0, "c01", "|skoda|"},
                    new object[] {"skoda", null, 10000, 0, 1, 1, "c01", "|skoda|"},
                    new object[] {null, "france", 10000, 1, 0, 2, "c01", "|skoda|"},
                    new object[] {null, null, 10000, 1, 1, 3, "c01", "|skoda|"}}, GroupingSupportFunc.AssertGetAndClear(4));
            epService.EPAdministrator.DestroyAllStatements();
    
            // test "prev" and "prior"
            string[] fields = "c0,c1,c2,c3".Split(',');
            string eplTwo = "select Prev(1, name) as c0, Prior(1, name) as c1, name as c2, sum(count) as c3 from CarEvent#keepall ce group by Rollup(name)";
            epService.EPAdministrator.CreateEPL(eplTwo).AddListener(listener);
    
            epService.EPRuntime.SendEvent(new CarEvent("skoda", "france", 10));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{
                    new object[] {null, null, "skoda", 10}, {null, null, null, 10}
            });
    
            epService.EPRuntime.SendEvent(new CarEvent("vw", "france", 15));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{
                    new object[] {"skoda", "skoda", "vw", 15}, {"skoda", "skoda", null, 25}
            });
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            // invalid use of function
            string expected = "Failed to validate select-clause expression 'Grouping(theString)': The grouping function requires the group-by clause to specify rollup, cube or grouping sets, and may only be used in the select-clause, having-clause or order-by-clause [select Grouping(theString) from SupportBean]";
            TryInvalid(epService, "select Grouping(theString) from SupportBean", "Error starting statement: " + expected);
            TryInvalid(epService, "select theString, sum(intPrimitive) from SupportBean(Grouping(theString) = 1) group by Rollup(theString)",
                    "Failed to validate filter expression 'Grouping(theString)=1': The grouping function requires the group-by clause to specify rollup, cube or grouping sets, and may only be used in the select-clause, having-clause or order-by-clause [select theString, sum(intPrimitive) from SupportBean(Grouping(theString) = 1) group by Rollup(theString)]");
            TryInvalid(epService, "select theString, sum(intPrimitive) from SupportBean where Grouping(theString) = 1 group by Rollup(theString)",
                    "Failed to validate filter expression 'Grouping(theString)=1': The grouping function requires the group-by clause to specify rollup, cube or grouping sets, and may only be used in the select-clause, having-clause or order-by-clause [select theString, sum(intPrimitive) from SupportBean where Grouping(theString) = 1 group by Rollup(theString)]");
            TryInvalid(epService, "select theString, sum(intPrimitive) from SupportBean group by Rollup(Grouping(theString))",
                    "Error starting statement: The grouping function requires the group-by clause to specify rollup, cube or grouping sets, and may only be used in the select-clause, having-clause or order-by-clause [select theString, sum(intPrimitive) from SupportBean group by Rollup(Grouping(theString))]");
    
            // invalid parameters
            TryInvalid(epService, "select theString, sum(intPrimitive), Grouping(longPrimitive) from SupportBean group by Rollup(theString)",
                    "Error starting statement: Group-by with rollup requires a fully-aggregated query, the query is not full-aggregated because of property 'longPrimitive' [select theString, sum(intPrimitive), Grouping(longPrimitive) from SupportBean group by Rollup(theString)]");
            TryInvalid(epService, "select theString, sum(intPrimitive), Grouping(theString||'x') from SupportBean group by Rollup(theString)",
                    "Error starting statement: Failed to find expression 'theString||\"x\"' among group-by expressions [select theString, sum(intPrimitive), Grouping(theString||'x') from SupportBean group by Rollup(theString)]");
    
            TryInvalid(epService, "select theString, sum(intPrimitive), Grouping_id(theString, theString) from SupportBean group by Rollup(theString)",
                    "Error starting statement: Duplicate expression 'theString' among grouping function parameters [select theString, sum(intPrimitive), Grouping_id(theString, theString) from SupportBean group by Rollup(theString)]");
        }
    
        public class GroupingSupportFunc {
            private static List<Object[]> parameters = new List<>();
    
            public static void Myfunc(string name,
                                      string place,
                                      int? cnt,
                                      int? grpName,
                                      int? grpPlace,
                                      int? grpId,
                                      string refId,
                                      string namePlusDelim) {
                parameters.Add(new Object[]{name, place, cnt, grpName, grpPlace, grpId, refId, namePlusDelim});
            }
    
            public static List<Object[]> GetParameters() {
                return parameters;
            }
    
            static Object[][] AssertGetAndClear(int numRows) {
                Assert.AreEqual(numRows, parameters.Count);
                Object[][] result = parameters.ToArray(new Object[numRows][]);
                parameters.Clear();
                return result;
            }
        }
    
        public class CarInfoEvent {
            private readonly string name;
            private readonly string place;
            private readonly string refId;
    
            private CarInfoEvent(string name, string place, string refId) {
                this.name = name;
                this.place = place;
                this.refId = refId;
            }
    
            public string GetName() {
                return name;
            }
    
            public string GetPlace() {
                return place;
            }
    
            public string GetRefId() {
                return refId;
            }
        }
    
        public class CarEvent {
            private readonly string name;
            private readonly string place;
            private readonly int count;
    
            private CarEvent(string name, string place, int count) {
                this.name = name;
                this.place = place;
                this.count = count;
            }
    
            public string GetName() {
                return name;
            }
    
            public string GetPlace() {
                return place;
            }
    
            public int GetCount() {
                return count;
            }
        }
    }
} // end of namespace
