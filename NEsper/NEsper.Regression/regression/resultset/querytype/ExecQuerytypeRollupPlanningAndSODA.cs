///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.agg.rollup;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;

// using static org.junit.Assert.assertEquals;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset.querytype
{
    public class ExecQuerytypeRollupPlanningAndSODA : RegressionExecution {
        public static readonly string PLAN_CALLBACK_HOOK = "@Hook(type=" + typeof(HookType).Name + ".INTERNAL_GROUPROLLUP_PLAN,hook='" + typeof(SupportGroupRollupPlanHook).Name + "')";
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType(typeof(ABCProp));
    
            // plain rollup
            Validate(epService, "a", "Rollup(a)", new string[]{"a", ""});
            Validate(epService, "a, b", "Rollup(a, b)", new string[]{"a,b", "a", ""});
            Validate(epService, "a, b, c", "Rollup(a, b, c)", new string[]{"a,b,c", "a,b", "a", ""});
            Validate(epService, "a, b, c, d", "Rollup(a, b, c, d)", new string[]{"a,b,c,d", "a,b,c", "a,b", "a", ""});
    
            // rollup with unenclosed
            Validate(epService, "a, b", "a, Rollup(b)", new string[]{"a,b", "a"});
            Validate(epService, "a, b, c", "a, b, Rollup(c)", new string[]{"a,b,c", "a,b"});
            Validate(epService, "a, b, c", "a, Rollup(b, c)", new string[]{"a,b,c", "a,b", "a"});
            Validate(epService, "a, b, c, d", "a, b, Rollup(c, d)", new string[]{"a,b,c,d", "a,b,c", "a,b"});
            Validate(epService, "a, b, c, d, e", "a, b, Rollup(c, d, e)", new string[]{"a,b,c,d,e", "a,b,c,d", "a,b,c", "a,b"});
    
            // plain cube
            Validate(epService, "a", "Cube(a)", new string[]{"a", ""});
            Validate(epService, "a, b", "Cube(a, b)", new string[]{"a,b", "a", "b", ""});
            Validate(epService, "a, b, c", "Cube(a, b, c)", new string[]{"a,b,c", "a,b", "a,c", "a", "b,c", "b", "c", ""});
            Validate(epService, "a, b, c, d", "Cube(a, b, c, d)", new string[]{"a,b,c,d", "a,b,c", "a,b,d",
                    "a,b", "a,c,d", "a,c", "a,d", "a",
                    "b,c,d", "b,c", "b,d", "b",
                    "c,d", "c", "d", ""});
    
            // cube with unenclosed
            Validate(epService, "a, b", "a, Cube(b)", new string[]{"a,b", "a"});
            Validate(epService, "a, b, c", "a, Cube(b, c)", new string[]{"a,b,c", "a,b", "a,c", "a"});
            Validate(epService, "a, b, c, d", "a, Cube(b, c, d)", new string[]{"a,b,c,d", "a,b,c", "a,b,d", "a,b", "a,c,d", "a,c", "a,d", "a"});
            Validate(epService, "a, b, c, d", "a, b, Cube(c, d)", new string[]{"a,b,c,d", "a,b,c", "a,b,d", "a,b"});
    
            // plain grouping set
            Validate(epService, "a", "grouping Sets(a)", new string[]{"a"});
            Validate(epService, "a", "grouping Sets(a)", new string[]{"a"});
            Validate(epService, "a, b", "grouping Sets(a, b)", new string[]{"a", "b"});
            Validate(epService, "a, b", "grouping Sets(a, b, (a, b), ())", new string[]{"a", "b", "a,b", ""});
            Validate(epService, "a, b", "grouping Sets(a, (a, b), (), b)", new string[]{"a", "a,b", "", "b"});
            Validate(epService, "a, b, c", "grouping Sets((a, b), (a, c), (), (b, c))", new string[]{"a,b", "a,c", "", "b,c"});
            Validate(epService, "a, b", "grouping Sets((a, b))", new string[]{"a,b"});
            Validate(epService, "a, b, c", "grouping Sets((a, b, c), ())", new string[]{"a,b,c", ""});
            Validate(epService, "a, b, c", "grouping Sets((), (a, b, c), (b, c))", new string[]{"", "a,b,c", "b,c"});
    
            // grouping sets with unenclosed
            Validate(epService, "a, b", "a, grouping Sets(b)", new string[]{"a,b"});
            Validate(epService, "a, b, c", "a, grouping Sets(b, c)", new string[]{"a,b", "a,c"});
            Validate(epService, "a, b, c", "a, grouping Sets((b, c))", new string[]{"a,b,c"});
            Validate(epService, "a, b, c, d", "a, b, grouping Sets((), c, d, (c, d))", new string[]{"a,b", "a,b,c", "a,b,d", "a,b,c,d"});
    
            // multiple grouping sets
            Validate(epService, "a, b", "grouping Sets(a), grouping Sets(b)", new string[]{"a,b"});
            Validate(epService, "a, b, c", "grouping Sets(a), grouping Sets(b, c)", new string[]{"a,b", "a,c"});
            Validate(epService, "a, b, c, d", "grouping Sets(a, b), grouping Sets(c, d)", new string[]{"a,c", "a,d", "b,c", "b,d"});
            Validate(epService, "a, b, c", "grouping Sets((), a), grouping Sets(b, c)", new string[]{"b", "c", "a,b", "a,c"});
            Validate(epService, "a, b, c, d", "grouping Sets(a, b, c), grouping Sets(d)", new string[]{"a,d", "b,d", "c,d"});
            Validate(epService, "a, b, c, d, e", "grouping Sets(a, b, c), grouping Sets(d, e)", new string[]{"a,d", "a,e", "b,d", "b,e", "c,d", "c,e"});
    
            // multiple rollups
            Validate(epService, "a, b, c", "Rollup(a, b), Rollup(c)", new string[]{"a,b,c", "a,b", "a,c", "a", "c", ""});
            Validate(epService, "a, b, c, d", "Rollup(a, b), Rollup(c, d)", new string[]{"a,b,c,d", "a,b,c", "a,b", "a,c,d", "a,c", "a", "c,d", "c", ""});
    
            // grouping sets with rollup or cube inside
            Validate(epService, "a, b, c", "grouping Sets(a, Rollup(b, c))", new string[]{"a", "b,c", "b", ""});
            Validate(epService, "a, b, c", "grouping Sets(a, Cube(b, c))", new string[]{"a", "b,c", "b", "c", ""});
            Validate(epService, "a, b", "grouping Sets(Rollup(a, b))", new string[]{"a,b", "a", ""});
            Validate(epService, "a, b", "grouping Sets(Cube(a, b))", new string[]{"a,b", "a", "b", ""});
            Validate(epService, "a, b, c, d", "grouping Sets((a, b), Rollup(c, d))", new string[]{"a,b", "c,d", "c", ""});
            Validate(epService, "a, b, c, d", "grouping Sets(a, b, Rollup(c, d))", new string[]{"a", "b", "c,d", "c", ""});
    
            // cube and rollup with combined expression
            Validate(epService, "a, b, c", "Cube((a, b), c)", new string[]{"a,b,c", "a,b", "c", ""});
            Validate(epService, "a, b, c", "Rollup((a, b), c)", new string[]{"a,b,c", "a,b", ""});
            Validate(epService, "a, b, c, d", "Cube((a, b), (c, d))", new string[]{"a,b,c,d", "a,b", "c,d", ""});
            Validate(epService, "a, b, c, d", "Rollup((a, b), (c, d))", new string[]{"a,b,c,d", "a,b", ""});
            Validate(epService, "a, b, c", "Cube(a, (b, c))", new string[]{"a,b,c", "a", "b,c", ""});
            Validate(epService, "a, b, c", "Rollup(a, (b, c))", new string[]{"a,b,c", "a", ""});
            Validate(epService, "a, b, c", "grouping Sets(Rollup((a, b), c))", new string[]{"a,b,c", "a,b", ""});
    
            // multiple cubes and rollups
            Validate(epService, "a, b, c, d", "Rollup(a, b), Rollup(c, d)", new string[]{"a,b,c,d", "a,b,c", "a,b",
                    "a,c,d", "a,c", "a", "c,d", "c", ""});
            Validate(epService, "a, b", "Cube(a), Cube(b)", new string[]{"a,b", "a", "b", ""});
            Validate(epService, "a, b, c", "Cube(a, b), Cube(c)", new string[]{"a,b,c", "a,b", "a,c", "a", "b,c", "b", "c", ""});
        }
    
        private void Validate(EPServiceProvider epService, string selectClause, string groupByClause, string[] expectedCSV) {
    
            string epl = PLAN_CALLBACK_HOOK + " select " + selectClause + ", count(*) from ABCProp group by " + groupByClause;
            SupportGroupRollupPlanHook.Reset();
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            ComparePlan(expectedCSV);
            stmt.Dispose();
    
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(epl);
            Assert.AreEqual(epl, model.ToEPL());
            SupportGroupRollupPlanHook.Reset();
            stmt = epService.EPAdministrator.Create(model);
            ComparePlan(expectedCSV);
            Assert.AreEqual(epl, stmt.Text);
            stmt.Dispose();
        }
    
        private void ComparePlan(string[] expectedCSV) {
            GroupByRollupPlanDesc plan = SupportGroupRollupPlanHook.Plan;
            AggregationGroupByRollupLevel[] levels = plan.RollupDesc.Levels;
            var received = new string[levels.Length][];
            for (int i = 0; i < levels.Length; i++) {
                AggregationGroupByRollupLevel level = levels[i];
                if (level.IsAggregationTop) {
                    received[i] = new string[0];
                } else {
                    received[i] = new string[level.RollupKeys.Length];
                    for (int j = 0; j < received[i].Length; j++) {
                        int key = level.RollupKeys[j];
                        received[i][j] = ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(plan.Expressions[key]);
                    }
                }
            }
    
            Assert.AreEqual("Received: " + ToCSV(received), expectedCSV.Length, received.Length);
            for (int i = 0; i < expectedCSV.Length; i++) {
                string receivedCSV = ToCSV(received[i]);
                Assert.AreEqual("Failed at row " + i, expectedCSV[i], receivedCSV);
            }
        }
    
        private string ToCSV(string[][] received) {
            var writer = new StringWriter();
            string delimiter = "";
            foreach (string[] item in received) {
                writer.Write(delimiter);
                writer.Write(ToCSV(item));
                delimiter = "  ";
            }
            return writer.ToString();
        }
    
        private string ToCSV(string[] received) {
            var writer = new StringWriter();
            string delimiter = "";
            foreach (string item in received) {
                writer.Write(delimiter);
                writer.Write(item);
                delimiter = ",";
            }
            return writer.ToString();
        }
    
        public class ABCProp {
            private readonly string a;
            private readonly string b;
            private readonly string c;
            private readonly string d;
            private readonly string e;
            private readonly string f;
            private readonly string g;
            private readonly string h;
    
            private ABCProp(string a, string b, string c, string d, string e, string f, string g, string h) {
                this.a = a;
                this.b = b;
                this.c = c;
                this.d = d;
                this.e = e;
                this.f = f;
                this.g = g;
                this.h = h;
            }
    
            public string GetA() {
                return a;
            }
    
            public string GetB() {
                return b;
            }
    
            public string GetC() {
                return c;
            }
    
            public string GetD() {
                return d;
            }
    
            public string GetE() {
                return e;
            }
    
            public string GetF() {
                return f;
            }
    
            public string GetG() {
                return g;
            }
    
            public string GetH() {
                return h;
            }
        }
    }
} // end of namespace
