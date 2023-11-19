///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Avro.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.bookexample;

using NEsper.Avro.Extensions;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.other
{
    public class EPLOtherSplitStream
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
#if REGRESSION_EXECUTIONS
            With2SplitNoDefaultOutputFirst(execs);
            WithInvalid(execs);
            WithFromClause(execs);
            WithSplitPremptiveNamedWindow(execs);
            With1SplitDefault(execs);
            WithSubquery(execs);
            With2SplitNoDefaultOutputAll(execs);
            With3SplitOutputAll(execs);
            With3SplitDefaultOutputFirst(execs);
            With4Split(execs);
            WithSubqueryMultikeyWArray(execs);
            With(SingleInsert)(execs);
#endif
            return execs;
        }

        public static IList<RegressionExecution> WithSingleInsert(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherSplitStreamSingleInsert());
            return execs;
        }

        public static IList<RegressionExecution> WithSubqueryMultikeyWArray(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherSplitStreamSubqueryMultikeyWArray());
            return execs;
        }

        public static IList<RegressionExecution> With4Split(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherSplitStream4Split());
            return execs;
        }

        public static IList<RegressionExecution> With3SplitDefaultOutputFirst(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherSplitStream3SplitDefaultOutputFirst());
            return execs;
        }

        public static IList<RegressionExecution> With3SplitOutputAll(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherSplitStream3SplitOutputAll());
            return execs;
        }

        public static IList<RegressionExecution> With2SplitNoDefaultOutputAll(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherSplitStream2SplitNoDefaultOutputAll());
            return execs;
        }

        public static IList<RegressionExecution> WithSubquery(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherSplitStreamSubquery());
            return execs;
        }

        public static IList<RegressionExecution> With1SplitDefault(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherSplitStream1SplitDefault());
            return execs;
        }

        public static IList<RegressionExecution> WithSplitPremptiveNamedWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherSplitStreamSplitPremptiveNamedWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithFromClause(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherSplitStreamFromClause());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherSplitStreamInvalid());
            return execs;
        }

        public static IList<RegressionExecution> With2SplitNoDefaultOutputFirst(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherSplitStream2SplitNoDefaultOutputFirst());
            return execs;
        }

        public class EPLOtherSplitStreamSingleInsert : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create context mycontext initiated by SupportBean as criteria;" +
                          "context mycontext on SupportBean_S0 as event" +
                          "  insert into SomeOtherStream select context.id as cid, context.criteria as criteria, event as event;" +
                          "@name('s0') select * from SomeOtherStream;";
                env.CompileDeploy(epl).AddListener("s0");

                var criteria = new SupportBean("E1", 0);
                env.SendEventBean(criteria);
                var trigger = new SupportBean_S0(1);
                env.SendEventBean(trigger);

                env.AssertEventNew(
                    "s0",
                    bean => {
                        Assert.AreSame(criteria, bean.Get("criteria"));
                        Assert.AreSame(trigger, bean.Get("event"));
                    });

                env.UndeployAll();
            }
        }

        private class EPLOtherSplitStreamSubqueryMultikeyWArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create schema AValue(Value int);\n" +
                          "on SupportBean\n" +
                          "  insert into AValue select (select sum(Value) as c0 from SupportEventWithIntArray#keepall group by array) as Value where IntPrimitive > 0\n" +
                          "  insert into AValue select 0 as Value where IntPrimitive <= 0;\n" +
                          "@name('s0') select * from AValue;\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportEventWithIntArray("E1", new int[] { 1, 2 }, 10));
                env.SendEventBean(new SupportEventWithIntArray("E2", new int[] { 1, 2 }, 11));

                env.Milestone(0);
                AssertSplitResult(env, 21);

                env.SendEventBean(new SupportEventWithIntArray("E3", new int[] { 1, 2 }, 12));
                AssertSplitResult(env, 33);

                env.Milestone(1);

                env.SendEventBean(new SupportEventWithIntArray("E4", new int[] { 1 }, 13));
                AssertSplitResult(env, null);

                env.UndeployAll();
            }

            private void AssertSplitResult(
                RegressionEnvironment env,
                int? expected)
            {
                env.SendEventBean(new SupportBean("X", 0));
                env.AssertEqualsNew("s0", "value", 0);

                env.SendEventBean(new SupportBean("Y", 1));
                env.AssertEqualsNew("s0", "value", expected);
            }
        }

        private class EPLOtherSplitStreamInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.TryInvalidCompile(
                    "on SupportBean select * where IntPrimitive=1 insert into BStream select * where 1=2",
                    "Required insert-into clause is not provided, the clause is required for split-stream syntax");

                env.TryInvalidCompile(
                    "on SupportBean insert into AStream select * where IntPrimitive=1 group by string insert into BStream select * where 1=2",
                    "A group-by clause, having-clause or order-by clause is not allowed for the split stream syntax");

                env.TryInvalidCompile(
                    "on SupportBean insert into AStream select * where IntPrimitive=1 insert into BStream select avg(IntPrimitive) where 1=2",
                    "Aggregation functions are not allowed in this context");
            }
        }

        private class EPLOtherSplitStreamFromClause : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryAssertionFromClauseBeginBodyEnd(env);
                TryAssertionFromClauseAsMultiple(env);
                TryAssertionFromClauseOutputFirstWhere(env);
                TryAssertionFromClauseDocSample(env);
            }
        }

        private class EPLOtherSplitStreamSplitPremptiveNamedWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                    TryAssertionSplitPremptiveNamedWindow(env, rep);
                }
            }
        }

        private class EPLOtherSplitStream1SplitDefault : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();

                // test wildcard
                var stmtOrigText = "@name('insert') @public on SupportBean insert into AStream select *";
                env.CompileDeploy(stmtOrigText, path).AddListener("insert");

                env.CompileDeploy("@name('s0') select * from AStream", path).AddListener("s0");

                SendSupportBean(env, "E1", 1);
                env.AssertEqualsNew("s0", "TheString", "E1");
                env.AssertListenerNotInvoked("insert");

                // test select
                stmtOrigText =
                    "@name('s1') @public on SupportBean insert into BStreamABC select 3*IntPrimitive as value";
                env.CompileDeploy(stmtOrigText, path);

                env.CompileDeploy("@name('s2') select value from BStreamABC", path).AddListener("s2");

                SendSupportBean(env, "E1", 6);
                env.AssertEqualsNew("s2", "value", 18);

                // assert type is original type
                env.AssertStatement(
                    "insert",
                    statement => Assert.AreEqual(typeof(SupportBean), statement.EventType.UnderlyingType));
                env.AssertIterator("insert", iterator => Assert.IsFalse(iterator.MoveNext()));

                env.UndeployAll();
            }
        }

        private class EPLOtherSplitStream2SplitNoDefaultOutputFirst : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtOrigText = "@Name('split') @public on SupportBean " +
                                   "insert into AStream2SP select * where IntPrimitive=1 " +
                                   "insert into BStream2SP select * where IntPrimitive=1 or IntPrimitive=2";
                env.CompileDeploy(stmtOrigText, path).AddListener("split");
                TryAssertion(env, path);
                path.Clear();

                // statement object model
                var model = new EPStatementObjectModel();
                model.Annotations = Arrays.AsList(new AnnotationPart("Audit"), new AnnotationPart("public"));
                model.FromClause = FromClause.Create(FilterStream.Create("SupportBean"));
                model.InsertInto = InsertIntoClause.Create("AStream2SP");
                model.SelectClause = SelectClause.CreateWildcard();
                model.WhereClause = Expressions.Eq("IntPrimitive", 1);
                var clause = OnClause.CreateOnInsertSplitStream();
                model.OnExpr = clause;
                var item = OnInsertSplitStreamItem.Create(
                    InsertIntoClause.Create("BStream2SP"),
                    SelectClause.CreateWildcard(),
                    Expressions.Or(Expressions.Eq("IntPrimitive", 1), Expressions.Eq("IntPrimitive", 2)));
                clause.AddItem(item);
                model.Annotations = Arrays.AsList(AnnotationPart.NameAnnotation("split"), new AnnotationPart("public"));
                Assert.AreEqual(stmtOrigText, model.ToEPL());
                env.CompileDeploy(model, path).AddListener("split");
                TryAssertion(env, path);
                path.Clear();

                env.EplToModelCompileDeploy(stmtOrigText, path).AddListener("split");
                TryAssertion(env, path);
            }
        }

        private class EPLOtherSplitStreamSubquery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtOrigText = "@name('split') @public on SupportBean " +
                                   "insert into AStreamSub select (select P00 from SupportBean_S0#lastevent) as string where IntPrimitive=(select Id from SupportBean_S0#lastevent) " +
                                   "insert into BStreamSub select (select P01 from SupportBean_S0#lastevent) as string where IntPrimitive<>(select Id from SupportBean_S0#lastevent) or (select Id from SupportBean_S0#lastevent) is null";
                env.CompileDeploy(stmtOrigText, path).AddListener("split");

                env.CompileDeploy("@name('s0') select * from AStreamSub", path).AddListener("s0");
                env.CompileDeploy("@name('s1') select * from BStreamSub", path).AddListener("s1");

                SendSupportBean(env, "E1", 1);
                env.AssertListenerNotInvoked("s0");
                env.AssertEqualsNew("s1", "string", null);

                env.SendEventBean(new SupportBean_S0(10, "x", "y"));

                SendSupportBean(env, "E2", 10);
                env.AssertEqualsNew("s0", "string", "x");
                env.AssertListenerNotInvoked("s1");

                SendSupportBean(env, "E3", 9);
                env.AssertListenerNotInvoked("s0");
                env.AssertEqualsNew("s1", "string", "y");

                env.UndeployAll();
            }
        }

        private class EPLOtherSplitStream2SplitNoDefaultOutputAll : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtOrigText = "@name('split') @public on SupportBean " +
                                   "insert into AStream2S select TheString where IntPrimitive=1 " +
                                   "insert into BStream2S select TheString where IntPrimitive=1 or IntPrimitive=2 " +
                                   "output all";
                env.CompileDeploy(stmtOrigText, path).AddListener("split");

                env.CompileDeploy("@name('s0') select * from AStream2S", path).AddListener("s0");
                env.CompileDeploy("@name('s1') select * from BStream2S", path).AddListener("s1");

                env.AssertThat(
                    () => {
                        Assert.AreNotSame(env.Statement("s0").EventType, env.Statement("s1").EventType);
                        Assert.AreSame(
                            env.Statement("s0").EventType.UnderlyingType,
                            env.Statement("s1").EventType.UnderlyingType);
                    });

                SendSupportBean(env, "E1", 1);
                env.AssertEqualsNew("s0", "TheString", "E1");
                env.AssertEqualsNew("s1", "TheString", "E1");
                env.AssertListenerNotInvoked("split");

                SendSupportBean(env, "E2", 2);
                env.AssertListenerNotInvoked("s0");
                env.AssertEqualsNew("s1", "TheString", "E2");
                env.AssertListenerNotInvoked("split");

                SendSupportBean(env, "E3", 1);
                env.AssertEqualsNew("s0", "TheString", "E3");
                env.AssertEqualsNew("s1", "TheString", "E3");
                env.AssertListenerNotInvoked("split");

                SendSupportBean(env, "E4", -999);
                env.AssertListenerNotInvoked("s0");
                env.AssertListenerNotInvoked("s1");
                env.AssertEqualsNew("split", "TheString", "E4");

                env.UndeployAll();
            }
        }

        private class EPLOtherSplitStream3SplitOutputAll : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtOrigText = "@name('split') @public on SupportBean " +
                                   "insert into AStream2S select TheString || '_1' as TheString where IntPrimitive in (1, 2) " +
                                   "insert into BStream2S select TheString || '_2' as TheString where IntPrimitive in (2, 3) " +
                                   "insert into CStream2S select TheString || '_3' as TheString " +
                                   "output all";
                env.CompileDeploy(stmtOrigText, path).AddListener("split");

                env.CompileDeploy("@name('s0') select * from AStream2S", path).AddListener("s0");
                env.CompileDeploy("@name('s1') select * from BStream2S", path).AddListener("s1");
                env.CompileDeploy("@name('s2') select * from CStream2S", path).AddListener("s2");

                SendSupportBean(env, "E1", 2);
                env.AssertEqualsNew("s0", "TheString", "E1_1");
                env.AssertEqualsNew("s1", "TheString", "E1_2");
                env.AssertEqualsNew("s2", "TheString", "E1_3");
                env.AssertListenerNotInvoked("split");

                SendSupportBean(env, "E2", 1);
                env.AssertEqualsNew("s0", "TheString", "E2_1");
                env.AssertListenerNotInvoked("s1");
                env.AssertEqualsNew("s2", "TheString", "E2_3");
                env.AssertListenerNotInvoked("split");

                SendSupportBean(env, "E3", 3);
                env.AssertListenerNotInvoked("s0");
                env.AssertEqualsNew("s1", "TheString", "E3_2");
                env.AssertEqualsNew("s2", "TheString", "E3_3");
                env.AssertListenerNotInvoked("split");

                SendSupportBean(env, "E4", -999);
                env.AssertListenerNotInvoked("s0");
                env.AssertListenerNotInvoked("s1");
                env.AssertEqualsNew("s2", "TheString", "E4_3");
                env.AssertListenerNotInvoked("split");

                env.UndeployAll();
            }
        }

        private class EPLOtherSplitStream3SplitDefaultOutputFirst : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtOrigText = "@name('split') @public on SupportBean as mystream " +
                                   "insert into AStream34 select mystream.TheString||'_1' as TheString where IntPrimitive=1 " +
                                   "insert into BStream34 select mystream.TheString||'_2' as TheString where IntPrimitive=2 " +
                                   "insert into CStream34 select TheString||'_3' as TheString";
                env.CompileDeploy(stmtOrigText, path).AddListener("split");

                env.CompileDeploy("@name('s0') select * from AStream34", path).AddListener("s0");
                env.CompileDeploy("@name('s1') select * from BStream34", path).AddListener("s1");
                env.CompileDeploy("@name('s2') select * from CStream34", path).AddListener("s2");

                env.AssertThat(
                    () => {
                        Assert.AreNotSame(env.Statement("s0").EventType, env.Statement("s1").EventType);
                        Assert.AreSame(
                            env.Statement("s0").EventType.UnderlyingType,
                            env.Statement("s1").EventType.UnderlyingType);
                    });

                SendSupportBean(env, "E1", 1);
                env.AssertEqualsNew("s0", "TheString", "E1_1");
                env.AssertListenerNotInvoked("s1");
                env.AssertListenerNotInvoked("s2");
                env.AssertListenerNotInvoked("split");

                SendSupportBean(env, "E2", 2);
                env.AssertListenerNotInvoked("s0");
                env.AssertEqualsNew("s1", "TheString", "E2_2");
                env.AssertListenerNotInvoked("s2");
                env.AssertListenerNotInvoked("split");

                SendSupportBean(env, "E3", 1);
                env.AssertEqualsNew("s0", "TheString", "E3_1");
                env.AssertListenerNotInvoked("s1");
                env.AssertListenerNotInvoked("s2");
                env.AssertListenerNotInvoked("split");

                SendSupportBean(env, "E4", -999);
                env.AssertListenerNotInvoked("s0");
                env.AssertListenerNotInvoked("s1");
                env.AssertEqualsNew("s2", "TheString", "E4_3");
                env.AssertListenerNotInvoked("split");

                env.UndeployAll();
            }
        }

        private class EPLOtherSplitStream4Split : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtOrigText = "@name('split') @public on SupportBean " +
                                   "insert into AStream34 select TheString||'_1' as TheString where IntPrimitive=10 " +
                                   "insert into BStream34 select TheString||'_2' as TheString where IntPrimitive=20 " +
                                   "insert into CStream34 select TheString||'_3' as TheString where IntPrimitive<0 " +
                                   "insert into DStream34 select TheString||'_4' as TheString";
                env.CompileDeploy(stmtOrigText, path).AddListener("split");

                env.CompileDeploy("@name('s0') select * from AStream34", path).AddListener("s0");
                env.CompileDeploy("@name('s1') select * from BStream34", path).AddListener("s1");
                env.CompileDeploy("@name('s2') select * from CStream34", path).AddListener("s2");
                env.CompileDeploy("@name('s3') select * from DStream34", path).AddListener("s3");

                SendSupportBean(env, "E5", -999);
                env.AssertListenerNotInvoked("s0");
                env.AssertListenerNotInvoked("s1");
                env.AssertEqualsNew("s2", "TheString", "E5_3");
                env.AssertListenerNotInvoked("s3");
                env.AssertListenerNotInvoked("split");

                SendSupportBean(env, "E6", 9999);
                env.AssertListenerNotInvoked("s0");
                env.AssertListenerNotInvoked("s1");
                env.AssertListenerNotInvoked("s2");
                env.AssertEqualsNew("s3", "TheString", "E6_4");
                env.AssertListenerNotInvoked("split");

                SendSupportBean(env, "E7", 20);
                env.AssertListenerNotInvoked("s0");
                env.AssertEqualsNew("s1", "TheString", "E7_2");
                env.AssertListenerNotInvoked("s2");
                env.AssertListenerNotInvoked("s3");
                env.AssertListenerNotInvoked("split");

                SendSupportBean(env, "E8", 10);
                env.AssertEqualsNew("s0", "TheString", "E8_1");
                env.AssertListenerNotInvoked("s1");
                env.AssertListenerNotInvoked("s2");
                env.AssertListenerNotInvoked("s3");
                env.AssertListenerNotInvoked("split");

                env.UndeployAll();
            }
        }

        private static void SendSupportBean(
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            env.SendEventBean(bean);
        }

        private static void TryAssertion(
            RegressionEnvironment env,
            RegressionPath path)
        {
            env.CompileDeploy("@name('s0') select * from AStream2SP", path).AddListener("s0");
            env.CompileDeploy("@name('s1') select * from BStream2SP", path).AddListener("s1");

            env.AssertThat(
                () => {
                    Assert.AreNotSame(env.Statement("s0").EventType, env.Statement("s1").EventType);
                    Assert.AreSame(
                        env.Statement("s0").EventType.UnderlyingType,
                        env.Statement("s1").EventType.UnderlyingType);
                });

            SendSupportBean(env, "E1", 1);
            env.AssertEqualsNew("s0", "TheString", "E1");
            env.AssertListenerNotInvoked("s1");

            SendSupportBean(env, "E2", 2);
            env.AssertListenerNotInvoked("s0");
            env.AssertEqualsNew("s1", "TheString", "E2");

            SendSupportBean(env, "E3", 1);
            env.AssertEqualsNew("s0", "TheString", "E3");
            env.AssertListenerNotInvoked("s1");

            SendSupportBean(env, "E4", -999);
            env.AssertListenerNotInvoked("s0");
            env.AssertListenerNotInvoked("s1");
            env.AssertEqualsNew("split", "TheString", "E4");

            env.UndeployAll();
        }

        private static void TryAssertionSplitPremptiveNamedWindow(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum)
        {
            var path = new RegressionPath();
            env.CompileDeploy(
                eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedTrigger)) +
                " @public @buseventtype create schema TypeTrigger(trigger int)",
                path);

            env.CompileDeploy(
                eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedTypeTwo)) +
                " @public create schema TypeTwo(col2 int)",
                path);
            env.CompileDeploy(
                eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedTypeTwo)) +
                " @public create window WinTwo#keepall as TypeTwo",
                path);

            var stmtOrigText = "@public on TypeTrigger " +
                               "insert into OtherStream select 1 " +
                               "insert into WinTwo(col2) select 2 " +
                               "output all";
            env.CompileDeploy(stmtOrigText, path);

            env.CompileDeploy("@name('s0') on OtherStream select col2 from WinTwo", path).AddListener("s0");

            // populate WinOne
            env.SendEventBean(new SupportBean("E1", 2));

            // fire trigger
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                env.SendEventObjectArray(new object[] { null }, "TypeTrigger");
            }
            else if (eventRepresentationEnum.IsMapEvent()) {
                env.SendEventMap(new Dictionary<string, object>(), "TypeTrigger");
            }
            else if (eventRepresentationEnum.IsAvroEvent()) {
                var @event = new GenericRecord(SchemaBuilder.Record("name", TypeBuilder.OptionalInt("trigger")));
                env.SendEventAvro(@event, "TypeTrigger");
            }
            else if (eventRepresentationEnum.IsJsonEvent() || eventRepresentationEnum.IsJsonProvidedClassEvent()) {
                env.SendEventJson("{}", "TypeTrigger");
            }
            else {
                Assert.Fail();
            }

            env.AssertEqualsNew("s0", "col2", 2);

            env.UndeployAll();
        }

        private static void TryAssertionFromClauseBeginBodyEnd(RegressionEnvironment env)
        {
            TryAssertionFromClauseBeginBodyEnd(env, false);
            TryAssertionFromClauseBeginBodyEnd(env, true);
        }

        private static void TryAssertionFromClauseAsMultiple(RegressionEnvironment env)
        {
            TryAssertionFromClauseAsMultiple(env, false);
            TryAssertionFromClauseAsMultiple(env, true);
        }

        private static void TryAssertionFromClauseAsMultiple(
            RegressionEnvironment env,
            bool soda)
        {
            var path = new RegressionPath();
            var epl = "@public on OrderBean as oe " +
                      "insert into StartEvent select oe.Orderdetail.OrderId as oi " +
                      "insert into ThenEvent select * from [select oe.Orderdetail.OrderId as oi, ItemId from Orderdetail.Items] as Item " +
                      "insert into MoreEvent select oe.Orderdetail.OrderId as oi, Item.ItemId as ItemId from [select oe, * from Orderdetail.Items] as Item " +
                      "output all";
            env.CompileDeploy(soda, epl, path);

            env.CompileDeploy("@name('s0') select * from StartEvent", path).AddListener("s0");
            env.CompileDeploy("@name('s1') select * from ThenEvent", path).AddListener("s1");
            env.CompileDeploy("@name('s2') select * from MoreEvent", path).AddListener("s2");

            env.SendEventBean(OrderBeanFactory.MakeEventOne());
            var fieldsOrderId = "oi".SplitCsv();
            var fieldsItems = "oi,ItemId".SplitCsv();
            env.AssertPropsNew("s0", fieldsOrderId, new object[] { "PO200901" });
            var expected = new object[][] {
                new object[] { "PO200901", "A001" }, new object[] { "PO200901", "A002" },
                new object[] { "PO200901", "A003" }
            };
            env.AssertListener(
                "s1",
                listener => EPAssertionUtil.AssertPropsPerRow(
                    listener.GetAndResetDataListsFlattened().First,
                    fieldsItems,
                    expected));
            env.AssertListener(
                "s2",
                listener => EPAssertionUtil.AssertPropsPerRow(
                    listener.GetAndResetDataListsFlattened().First,
                    fieldsItems,
                    expected));

            env.UndeployAll();
        }

        private static void TryAssertionFromClauseBeginBodyEnd(
            RegressionEnvironment env,
            bool soda)
        {
            var path = new RegressionPath();
            var epl = "@name('split') @public on OrderBean " +
                      "insert into BeginEvent select Orderdetail.OrderId as OrderId " +
                      "insert into OrderItem select * from [select Orderdetail.OrderId as OrderId, * from Orderdetail.Items] " +
                      "insert into EndEvent select Orderdetail.OrderId as OrderId " +
                      "output all";
            env.CompileDeploy(soda, epl, path);
            env.AssertStatement(
                "split",
                statement => Assert.AreEqual(
                    StatementType.ON_SPLITSTREAM,
                    statement.GetProperty(StatementProperty.STATEMENTTYPE)));

            env.CompileDeploy("@name('s0') select * from BeginEvent", path).AddListener("s0");
            env.CompileDeploy("@name('s1') select * from OrderItem", path).AddListener("s1");
            env.CompileDeploy("@name('s2') select * from EndEvent", path).AddListener("s2");

            env.AssertThat(
                () => {
                    var orderItemType = env.Runtime.EventTypeService.GetEventType(
                        env.DeploymentId("split"),
                        "OrderItem");
                    Assert.AreEqual(
                        "[Amount, ItemId, Price, ProductId, OrderId]",
                        CompatExtensions.Render(orderItemType.PropertyNames));
                });

            env.SendEventBean(OrderBeanFactory.MakeEventOne());
            AssertFromClauseWContained(
                env,
                "PO200901",
                new object[][] {
                    new object[] { "PO200901", "A001" }, new object[] { "PO200901", "A002" },
                    new object[] { "PO200901", "A003" }
                });

            env.SendEventBean(OrderBeanFactory.MakeEventTwo());
            AssertFromClauseWContained(env, "PO200902", new object[][] { new object[] { "PO200902", "B001" } });

            env.SendEventBean(OrderBeanFactory.MakeEventFour());
            AssertFromClauseWContained(env, "PO200904", Array.Empty<object[]>());

            env.UndeployAll();
        }

        private static void TryAssertionFromClauseOutputFirstWhere(RegressionEnvironment env)
        {
            TryAssertionFromClauseOutputFirstWhere(env, false);
            TryAssertionFromClauseOutputFirstWhere(env, true);
        }

        private static void TryAssertionFromClauseOutputFirstWhere(
            RegressionEnvironment env,
            bool soda)
        {
            var path = new RegressionPath();
            var fieldsOrderId = "oe.Orderdetail.OrderId".SplitCsv();
            var epl = "@public on OrderBean as oe " +
                      "insert into HeaderEvent select Orderdetail.OrderId as OrderId where 1=2 " +
                      "insert into StreamOne select * from [select oe, * from Orderdetail.Items] where ProductId=\"10020\" " +
                      "insert into StreamTwo select * from [select oe, * from Orderdetail.Items] where ProductId=\"10022\" " +
                      "insert into StreamThree select * from [select oe, * from Orderdetail.Items] where ProductId in (\"10020\",\"10025\",\"10022\")";
            env.CompileDeploy(soda, epl, path);

            var listenerEPL = new string[]
                { "select * from StreamOne", "select * from StreamTwo", "select * from StreamThree" };
            for (var i = 0; i < listenerEPL.Length; i++) {
                env.CompileDeploy("@name('s" + i + "')" + listenerEPL[i], path).AddListener("s" + i);
            }

            env.SendEventBean(OrderBeanFactory.MakeEventOne());
            env.AssertPropsNew("s0", fieldsOrderId, new object[] { "PO200901" });
            env.AssertListenerNotInvoked("s1");
            env.AssertListenerNotInvoked("s2");

            env.SendEventBean(OrderBeanFactory.MakeEventTwo());
            env.AssertListenerNotInvoked("s0");
            env.AssertPropsNew("s1", fieldsOrderId, new object[] { "PO200902" });
            env.AssertListenerNotInvoked("s2");

            env.SendEventBean(OrderBeanFactory.MakeEventThree());
            env.AssertListenerNotInvoked("s0");
            env.AssertListenerNotInvoked("s1");
            env.AssertPropsNew("s2", fieldsOrderId, new object[] { "PO200903" });

            env.SendEventBean(OrderBeanFactory.MakeEventFour());
            env.AssertListenerNotInvoked("s0");
            env.AssertListenerNotInvoked("s1");
            env.AssertListenerNotInvoked("s2");

            env.UndeployAll();
        }

        private static void TryAssertionFromClauseDocSample(RegressionEnvironment env)
        {
            var epl =
                "create schema MyOrderItem(ItemId string);\n" +
                "@public @buseventtype create schema MyOrderEvent(OrderId string, Items MyOrderItem[]);\n" +
                "on MyOrderEvent\n" +
                "  insert into MyOrderBeginEvent select OrderId\n" +
                "  insert into MyOrderItemEvent select * from [select OrderId, * from Items]\n" +
                "  insert into MyOrderEndEvent select OrderId\n" +
                "  output all;\n" +
                "create context MyOrderContext \n" +
                "  initiated by MyOrderBeginEvent as obe\n" +
                "  terminated by MyOrderEndEvent(OrderId = obe.OrderId);\n" +
                "@name('count') context MyOrderContext select count(*) as orderItemCount from MyOrderItemEvent output when terminated;\n";
            env.CompileDeploy(epl, new RegressionPath()).AddListener("count");

            IDictionary<string, object> @event = new Dictionary<string, object>();
            @event.Put("OrderId", "1010");
            @event.Put(
                "Items",
                new IDictionary<string, object>[] {
                    Collections.SingletonDataMap("ItemId", "A0001")
                });
            env.SendEventMap(@event, "MyOrderEvent");

            env.AssertEqualsNew("count", "orderItemCount", 1L);

            env.UndeployAll();
        }

        private static void AssertFromClauseWContained(
            RegressionEnvironment env,
            string orderId,
            object[][] expected)
        {
            var fieldsOrderId = "OrderId".SplitCsv();
            var fieldsItems = "OrderId,ItemId".SplitCsv();
            env.AssertListener(
                "s0",
                listener => EPAssertionUtil.AssertProps(
                    listener.AssertOneGetNewAndReset(),
                    fieldsOrderId,
                    new object[] { orderId }));
            env.AssertListener(
                "s1",
                listener => EPAssertionUtil.AssertPropsPerRow(
                    listener.GetAndResetDataListsFlattened().First,
                    fieldsItems,
                    expected));
            env.AssertListener(
                "s2",
                listener => EPAssertionUtil.AssertProps(
                    listener.AssertOneGetNewAndReset(),
                    fieldsOrderId,
                    new object[] { orderId }));
        }

        public class MyLocalJsonProvidedTrigger
        {
            public int trigger;
        }

        public class MyLocalJsonProvidedTypeTwo
        {
            public int col2;
        }
    }
} // end of namespace