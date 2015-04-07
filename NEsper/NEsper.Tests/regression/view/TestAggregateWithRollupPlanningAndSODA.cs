///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.IO;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.epl.agg.rollup;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.client;
using com.espertech.esper.support.epl;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestAggregateWithRollupPlanningAndSODA 
    {
        public static readonly string PLAN_CALLBACK_HOOK = string.Format(
            "@Hook(Type={0}.INTERNAL_GROUPROLLUP_PLAN,Hook='{1}')", 
            typeof(HookType).FullName, 
            typeof(SupportGroupRollupPlanHook).FullName);
    
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp()
        {
            var config = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName);}
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
        }
    
        [Test]
        public void TestGroupByPlanning() {
            _epService.EPAdministrator.Configuration.AddEventType(typeof(ABCProp));
    
            // plain rollup
            Validate("a", "rollup(a)", new string[]{"a", ""});
            Validate("a, b", "rollup(a, b)", new string[]{"a,b", "a", ""});
            Validate("a, b, c", "rollup(a, b, c)", new string[]{"a,b,c", "a,b", "a", ""});
            Validate("a, b, c, d", "rollup(a, b, c, d)", new string[]{"a,b,c,d", "a,b,c", "a,b", "a", ""});
    
            // rollup with unenclosed
            Validate("a, b", "a, rollup(b)", new string[]{"a,b", "a"});
            Validate("a, b, c", "a, b, rollup(c)", new string[]{"a,b,c", "a,b"});
            Validate("a, b, c", "a, rollup(b, c)", new string[]{"a,b,c", "a,b", "a"});
            Validate("a, b, c, d", "a, b, rollup(c, d)", new string[]{"a,b,c,d", "a,b,c", "a,b"});
            Validate("a, b, c, d, e", "a, b, rollup(c, d, e)", new string[]{"a,b,c,d,e", "a,b,c,d", "a,b,c", "a,b"});
    
            // plain cube
            Validate("a", "cube(a)", new string[]{"a", ""});
            Validate("a, b", "cube(a, b)", new string[]{"a,b", "a", "b", ""});
            Validate("a, b, c", "cube(a, b, c)", new string[]{"a,b,c", "a,b", "a,c", "a", "b,c", "b", "c", ""});
            Validate("a, b, c, d", "cube(a, b, c, d)", new string[]{"a,b,c,d", "a,b,c", "a,b,d",
                    "a,b", "a,c,d", "a,c", "a,d", "a",
                    "b,c,d", "b,c", "b,d", "b",
                    "c,d", "c", "d", ""});
    
            // cube with unenclosed
            Validate("a, b", "a, cube(b)", new string[]{"a,b", "a"});
            Validate("a, b, c", "a, cube(b, c)", new string[]{"a,b,c", "a,b", "a,c", "a"});
            Validate("a, b, c, d", "a, cube(b, c, d)", new string[]{"a,b,c,d", "a,b,c", "a,b,d", "a,b", "a,c,d", "a,c", "a,d", "a"});
            Validate("a, b, c, d", "a, b, cube(c, d)", new string[]{"a,b,c,d", "a,b,c", "a,b,d", "a,b"});
    
            // plain grouping set
            Validate("a", "grouping sets(a)", new string[]{"a"});
            Validate("a", "grouping sets(a)", new string[]{"a"});
            Validate("a, b", "grouping sets(a, b)", new string[]{"a", "b"});
            Validate("a, b", "grouping sets(a, b, (a, b), ())", new string[]{"a", "b", "a,b", ""});
            Validate("a, b", "grouping sets(a, (a, b), (), b)", new string[]{"a", "a,b", "", "b"});
            Validate("a, b, c", "grouping sets((a, b), (a, c), (), (b, c))", new string[]{"a,b", "a,c", "", "b,c"});
            Validate("a, b", "grouping sets((a, b))", new string[]{"a,b"});
            Validate("a, b, c", "grouping sets((a, b, c), ())", new string[]{"a,b,c", ""});
            Validate("a, b, c", "grouping sets((), (a, b, c), (b, c))", new string[]{"", "a,b,c", "b,c"});
    
            // grouping sets with unenclosed
            Validate("a, b", "a, grouping sets(b)", new string[]{"a,b"});
            Validate("a, b, c", "a, grouping sets(b, c)", new string[]{"a,b", "a,c"});
            Validate("a, b, c", "a, grouping sets((b, c))", new string[]{"a,b,c"});
            Validate("a, b, c, d", "a, b, grouping sets((), c, d, (c, d))", new string[]{"a,b", "a,b,c", "a,b,d", "a,b,c,d"});
    
            // multiple grouping sets
            Validate("a, b", "grouping sets(a), grouping sets(b)", new string[]{"a,b"});
            Validate("a, b, c", "grouping sets(a), grouping sets(b, c)", new string[]{"a,b", "a,c"});
            Validate("a, b, c, d", "grouping sets(a, b), grouping sets(c, d)", new string[]{"a,c", "a,d", "b,c", "b,d"});
            Validate("a, b, c", "grouping sets((), a), grouping sets(b, c)", new string[]{"b", "c", "a,b", "a,c"});
            Validate("a, b, c, d", "grouping sets(a, b, c), grouping sets(d)", new string[]{"a,d", "b,d", "c,d"});
            Validate("a, b, c, d, e", "grouping sets(a, b, c), grouping sets(d, e)", new string[]{"a,d", "a,e", "b,d", "b,e", "c,d", "c,e"});
    
            // multiple rollups
            Validate("a, b, c", "rollup(a, b), rollup(c)", new string[]{"a,b,c", "a,b", "a,c", "a", "c", ""});
            Validate("a, b, c, d", "rollup(a, b), rollup(c, d)", new string[]{"a,b,c,d", "a,b,c", "a,b", "a,c,d", "a,c", "a", "c,d", "c", ""});
    
            // grouping sets with rollup or cube inside
            Validate("a, b, c", "grouping sets(a, rollup(b, c))", new string[]{"a", "b,c", "b", ""});
            Validate("a, b, c", "grouping sets(a, cube(b, c))", new string[]{"a", "b,c", "b", "c", ""});
            Validate("a, b", "grouping sets(rollup(a, b))", new string[]{"a,b", "a", ""});
            Validate("a, b", "grouping sets(cube(a, b))", new string[]{"a,b", "a", "b", ""});
            Validate("a, b, c, d", "grouping sets((a, b), rollup(c, d))", new string[]{"a,b", "c,d", "c", ""});
            Validate("a, b, c, d", "grouping sets(a, b, rollup(c, d))", new string[]{"a", "b", "c,d", "c", ""});
    
            // cube and rollup with combined expression
            Validate("a, b, c", "cube((a, b), c)", new string[]{"a,b,c", "a,b", "c", ""});
            Validate("a, b, c", "rollup((a, b), c)", new string[]{"a,b,c", "a,b", ""});
            Validate("a, b, c, d", "cube((a, b), (c, d))", new string[]{"a,b,c,d", "a,b", "c,d", ""});
            Validate("a, b, c, d", "rollup((a, b), (c, d))", new string[]{"a,b,c,d", "a,b", ""});
            Validate("a, b, c", "cube(a, (b, c))", new string[]{"a,b,c", "a", "b,c", ""});
            Validate("a, b, c", "rollup(a, (b, c))", new string[]{"a,b,c", "a", ""});
            Validate("a, b, c", "grouping sets(rollup((a, b), c))", new string[]{"a,b,c", "a,b", ""});
    
            // multiple cubes and rollups
            Validate("a, b, c, d", "rollup(a, b), rollup(c, d)", new string[]{"a,b,c,d", "a,b,c", "a,b",
                    "a,c,d", "a,c", "a", "c,d", "c", ""});
            Validate("a, b", "cube(a), cube(b)", new string[]{"a,b", "a", "b", ""});
            Validate("a, b, c", "cube(a, b), cube(c)", new string[]{"a,b,c", "a,b", "a,c", "a", "b,c", "b", "c", ""});
        }
    
        private void Validate(string selectClause, string groupByClause, string[] expectedCSV)
        {
    
            var epl = PLAN_CALLBACK_HOOK + " select " + selectClause + ", count(*) from ABCProp group by " + groupByClause;
            SupportGroupRollupPlanHook.Reset();
            var stmt = _epService.EPAdministrator.CreateEPL(epl);
            ComparePlan(expectedCSV);
            stmt.Dispose();
    
            var model = _epService.EPAdministrator.CompileEPL(epl);
            Assert.AreEqual(epl, model.ToEPL());
            SupportGroupRollupPlanHook.Reset();
            stmt = _epService.EPAdministrator.Create(model);
            ComparePlan(expectedCSV);
            Assert.AreEqual(epl, stmt.Text);
            stmt.Dispose();
        }
    
        private void ComparePlan(string[] expectedCSV)
        {
            GroupByRollupPlanDesc plan = SupportGroupRollupPlanHook.Plan;
            var levels = plan.RollupDesc.Levels;
            var received = new string[levels.Length][];
            for (var i = 0; i < levels.Length; i++) {
                var level = levels[i];
                if (level.IsAggregationTop) {
                    received[i] = new string[0];
                }
                else {
                    received[i] = new string[level.RollupKeys.Length];
                    for (var j = 0; j < received[i].Length; j++) {
                        var key = level.RollupKeys[j];
                        received[i][j] = plan.Expressions[key].ToExpressionStringMinPrecedenceSafe();
                    }
                }
            }

            Assert.AreEqual(expectedCSV.Length, received.Length, "Received: " + ToCSV(received));
            for (var i = 0; i < expectedCSV.Length; i++) {
                var receivedCSV = ToCSV(received[i]);
                Assert.AreEqual(expectedCSV[i], receivedCSV, "Failed at row " + i);
            }
        }
    
        private string ToCSV(string[][] received) {
            var writer = new StringWriter();
            var delimiter = "";
            foreach (var item in received) {
                writer.Write(delimiter);
                writer.Write(ToCSV(item));
                delimiter = "  ";
            }
            return writer.ToString();
        }
    
        private string ToCSV(string[] received) {
            var writer = new StringWriter();
            var delimiter = "";
            foreach (var item in received) {
                writer.Write(delimiter);
                writer.Write(item);
                delimiter = ",";
            }
            return writer.ToString();
        }

        public class ABCProp
        {
            public ABCProp(string a, string b, string c, string d, string e, string f, string g, string h)
            {
                A = a;
                B = b;
                C = c;
                D = d;
                E = e;
                F = f;
                G = g;
                H = h;
            }

            public string A { get; private set; }

            public string B { get; private set; }

            public string C { get; private set; }

            public string D { get; private set; }

            public string E { get; private set; }

            public string F { get; private set; }

            public string G { get; private set; }

            public string H { get; private set; }
        }
    }
}
