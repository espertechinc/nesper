///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.bookexample;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.runtime.client;

using NUnit.Framework;
using NUnit.Framework.Legacy;
using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
    public class ExprEnumDataSources
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithProperty(execs);
            WithSubstitutionParameter(execs);
            WithEnumObject(execs);
            WithSortedMaxMinBy(execs);
            WithJoin(execs);
            WithPrevWindowSorted(execs);
            WithNamedWindow(execs);
            WithSubselect(execs);
            WithAccessAggregation(execs);
            WithPrevFuncs(execs);
            WithUDFStaticMethod(execs);
            WithPropertySchema(execs);
            WithPropertyInsertIntoAtEventBean(execs);
            WithPatternInsertIntoAtEventBean(execs);
            WithPatternFilter(execs);
            WithVariable(execs);
            WithTableRow(execs);
            WithMatchRecognizeDefine(execs);
            WithMatchRecognizeMeasures(execs);
            WithCast(execs);
            WithPropertyGenericComponentType(execs);
            WithUDFStaticMethodGeneric(execs);
            WithSubqueryGenericComponentType(execs);
            WithBeanWithMap(execs);
            WithContextPropUnnested(execs);
            WithContextPropNested(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithContextPropNested(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumContextPropNested());
            return execs;
        }

        public static IList<RegressionExecution> WithContextPropUnnested(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumContextPropUnnested());
            return execs;
        }

        public static IList<RegressionExecution> WithBeanWithMap(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumBeanWithMap());
            return execs;
        }

        public static IList<RegressionExecution> WithSubqueryGenericComponentType(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumSubqueryGenericComponentType());
            return execs;
        }

        public static IList<RegressionExecution> WithUDFStaticMethodGeneric(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumUDFStaticMethodGeneric());
            return execs;
        }

        public static IList<RegressionExecution> WithPropertyGenericComponentType(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumPropertyGenericComponentType());
            return execs;
        }

        public static IList<RegressionExecution> WithCast(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumCast());
            return execs;
        }

        public static IList<RegressionExecution> WithMatchRecognizeMeasures(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumMatchRecognizeMeasures(false));
            execs.Add(new ExprEnumMatchRecognizeMeasures(true));
            return execs;
        }

        public static IList<RegressionExecution> WithMatchRecognizeDefine(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumMatchRecognizeDefine());
            return execs;
        }

        public static IList<RegressionExecution> WithTableRow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumTableRow());
            return execs;
        }

        public static IList<RegressionExecution> WithVariable(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumVariable());
            return execs;
        }

        public static IList<RegressionExecution> WithPatternFilter(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumPatternFilter());
            return execs;
        }

        public static IList<RegressionExecution> WithPatternInsertIntoAtEventBean(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumPatternInsertIntoAtEventBean());
            return execs;
        }

        public static IList<RegressionExecution> WithPropertyInsertIntoAtEventBean(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumPropertyInsertIntoAtEventBean());
            return execs;
        }

        public static IList<RegressionExecution> WithPropertySchema(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumPropertySchema());
            return execs;
        }

        public static IList<RegressionExecution> WithUDFStaticMethod(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumUDFStaticMethod());
            return execs;
        }

        public static IList<RegressionExecution> WithPrevFuncs(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumPrevFuncs());
            return execs;
        }

        public static IList<RegressionExecution> WithAccessAggregation(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumAccessAggregation());
            return execs;
        }

        public static IList<RegressionExecution> WithSubselect(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumSubselect());
            return execs;
        }

        public static IList<RegressionExecution> WithNamedWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumNamedWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithPrevWindowSorted(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumPrevWindowSorted());
            return execs;
        }

        public static IList<RegressionExecution> WithJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithSortedMaxMinBy(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumSortedMaxMinBy());
            return execs;
        }

        public static IList<RegressionExecution> WithEnumObject(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumEnumObject());
            return execs;
        }

        public static IList<RegressionExecution> WithSubstitutionParameter(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumSubstitutionParameter());
            return execs;
        }

        public static IList<RegressionExecution> WithProperty(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumProperty());
            return execs;
        }

        internal class ExprEnumContextPropUnnested : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype create schema MyLocalEventWithInts as " +
                          typeof(MyLocalEventWithInts).Name +
                          ";\n" +
                          "@public create context MyContext start MyLocalEventWithInts as mle end SupportBean_S0;\n" +
                          "@name('s0') context MyContext select \n" +
                          "  context.mle.myFunc() as c0,\n" +
                          "  context.mle.intValues.anyOf(i => i.intValue() > 0) as c1,\n" +
                          "  context.mle.intValues.where(i => i.intValue() > 0).countOf() > 0 as c2\n" +
                          "  from SupportBean\n";
                env.CompileDeploy(epl).AddListener("s0");

                SendAssert(env, true, 0, 1);
                SendAssert(env, false, 0, 0);

                env.UndeployAll();
            }

            private void SendAssert(
                RegressionEnvironment env,
                bool expected,
                params int[] intValues)
            {
                env.SendEventBean(new MyLocalEventWithInts(new HashSet<int>(intValues)));
                env.SendEventBean(new SupportBean());
                env.AssertPropsNew(
                    "s0",
                    new string[] { "c0", "c1", "c2" },
                    new object[] { expected, expected, expected });
                env.SendEventBean(new SupportBean_S0(0));
            }
        }

        internal class ExprEnumContextPropNested : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype create schema MyLocalEventWithInts as " +
                          typeof(MyLocalEventWithInts).MaskTypeName() +
                          ";\n" +
                          "@public create context MyContext " +
                          "  context ACtx start MyLocalEventWithInts as mle end SupportBean_S0,\n" +
                          "  context BCtx start SupportBean\n" +
                          ";\n" +
                          "@name('s0') context MyContext select \n" +
                          "  context.ACtx.mle.myFunc() as c0,\n" +
                          "  context.ACtx.mle.intValues.anyOf(i => i.intValue() > 0) as c1,\n" +
                          "  context.ACtx.mle.intValues.where(i => i.intValue() > 0).countOf() > 0 as c2\n" +
                          "  from SupportBean\n";
                env.CompileDeploy(epl).AddListener("s0");

                SendAssert(env, true, 0, 1);
                SendAssert(env, false, 0, 0);

                env.UndeployAll();
            }

            private void SendAssert(
                RegressionEnvironment env,
                bool expected,
                params int[] intValues)
            {
                env.SendEventBean(new MyLocalEventWithInts(new HashSet<int>(intValues)));
                env.SendEventBean(new SupportBean());
                env.AssertPropsNew(
                    "s0",
                    new string[] { "c0", "c1", "c2" },
                    new object[] { expected, expected, expected });
                env.SendEventBean(new SupportBean_S0(0));
            }
        }

        internal class ExprEnumBeanWithMap : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype create schema MyEvent as " +
                          typeof(SupportEventWithMapOfCollOfString).MaskTypeName() +
                          ";\n" +
                          "@name('s0') select * from MyEvent(mymap('a').anyOf(x -> x = 'x'));\n";
                env.CompileDeploy(epl).AddListener("s0");

                SendAssert(env, "a", EmptyList<string>.Instance, false);
                SendAssert(env, "a", Arrays.AsList("a", "b"), false);
                SendAssert(env, "a", Arrays.AsList("a", "x"), true);
                SendAssert(env, "b", Arrays.AsList("a", "x"), false);

                env.UndeployAll();
            }

            private void SendAssert(
                RegressionEnvironment env,
                string mapKey,
                IList<string> values,
                bool received)
            {
                env.SendEventBean(new SupportEventWithMapOfCollOfString(mapKey, values), "MyEvent");
                env.AssertListenerInvokedFlag("s0", received);
            }
        }

        internal class ExprEnumSubqueryGenericComponentType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@public @buseventType create schema MyEvent as (Item Nullable<Integer>);\n" +
                    "@name('s0') select (select Item from MyEvent#keepall).sumOf(v => v.get()) as c0 from SupportBean;\n";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, 10);
                SendEvent(env, -2);
                env.SendEventBean(new SupportBean());
                env.AssertPropsNew("s0", "c0".Split(","), new object[] { 8 });

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.SERDEREQUIRED);
            }

            private void SendEvent(
                RegressionEnvironment env,
                int i)
            {
                env.SendEventMap(Collections.SingletonDataMap("Item", i), "MyEvent");
            }
        }

        internal class ExprEnumUDFStaticMethodGeneric : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select " +
                          this.GetType().MaskTypeName() +
                          ".Doit().sumOf(v => v.get()) as c0 from SupportBean;";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean());
                env.AssertEqualsNew("s0", "c0", 30);

                env.UndeployAll();
            }

            public static ICollection<int?> Doit()
            {
                return Collections.List<int?>(10, 20);
            }
        }

        internal class ExprEnumPropertyGenericComponentType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@public @buseventType create schema MyEvent as (arrayOfOptionalInt Nullable<Integer>[], listOfOptionalInt List<Nullable<Integer>>);\n" +
                    "@name('s0') select arrayOfOptionalInt.sumOf(v => v.get()) as c0, arrayOfOptionalInt.where(v => v.get() > 0).sumOf(v => v.get()) as c1," +
                    "listOfOptionalInt.sumOf(v => v.get()) as c2, listOfOptionalInt.where(v => v.get() > 0).sumOf(v => v.get()) as c3 from MyEvent;\n";
                env.CompileDeploy(epl).AddListener("s0");

                IDictionary<string, object> @event = new Dictionary<string, object>();
                @event.Put("arrayOfOptionalInt", MakeOptional(10, -1));
                @event.Put("listOfOptionalInt", Arrays.AsList(MakeOptional(5, -2)));
                env.SendEventMap(@event, "MyEvent");
                env.AssertPropsNew("s0", "c0,c1,c2,c3".Split(","), new object[] { 9, 10, 3, 5 });

                env.UndeployAll();
            }

            private int?[] MakeOptional(
                int first,
                int second)
            {
                return new[] {
                    first,
                    new int?(second)
                };
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.SERDEREQUIRED);
            }
        }

        internal class ExprEnumCast : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@public @buseventtype create schema MyLocalEvent as " +
                    typeof(MyLocalEvent).MaskTypeName() +
                    ";\n" +
                    "@name('s0') select cast(Value.SomeCollection?, `System.Collections.Generic.ICollection<object>`).countOf() as cnt from MyLocalEvent";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new MyLocalEvent(new MyLocalWithCollection(Collections.List<object>("a", "b"))));
                env.AssertEqualsNew("s0", "cnt", 2);

                env.UndeployAll();
            }
        }

        internal class ExprEnumPatternInsertIntoAtEventBean : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@public @buseventtype create schema MyEvent(id string, value int);\n" +
                    "insert into StreamWithAll select * from pattern[[4] me=MyEvent];\n" +
                    "insert into StreamGreaterZero select me.where(v => v.value>0) @eventbean as megt from StreamWithAll;\n" +
                    "insert into StreamLessThenTen select megt.where(v => v.value<10) @eventbean as melt from StreamGreaterZero;\n" +
                    "@name('s0') select * from StreamLessThenTen;\n";
                env.CompileDeploy(epl).AddListener("s0");

                var e1 = SendEvent(env, "E1", 1);
                SendEvent(env, "E2", -1);
                SendEvent(env, "E3", 11);
                var e4 = SendEvent(env, "E4", 4);

                env.AssertEventNew(
                    "s0",
                    @event => {
                        var result = (IDictionary<string, object>)@event.Underlying;
                        var events = (EventBean[])result.Get("melt");
                        ClassicAssert.AreSame(e1, events[0].Underlying);
                        ClassicAssert.AreSame(e4, events[1].Underlying);
                    });

                env.UndeployAll();
            }

            private IDictionary<string, object> SendEvent(
                RegressionEnvironment env,
                string id,
                int value)
            {
                var @event = CollectionUtil.BuildMap("id", id, "value", value);
                env.SendEventMap(@event, "MyEvent");
                return @event;
            }
        }

        internal class ExprEnumPropertySchema : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@public @buseventtype create schema OrderDetail(itemId string);\n" +
                    "@public @buseventtype create schema OrderEvent(details OrderDetail[]);\n" +
                    "@name('s0') select details.where(i => i.itemId = '001') as c0 from OrderEvent;\n";
                env.CompileDeploy(epl, new RegressionPath()).AddListener("s0");

                var detailOne = CollectionUtil.PopulateNameValueMap("itemId", "002");
                var detailTwo = CollectionUtil.PopulateNameValueMap("itemId", "001");
                env.SendEventMap(
                    CollectionUtil.PopulateNameValueMap(
                        "details",
                        new IDictionary<string, object>[] { detailOne, detailTwo }),
                    "OrderEvent");

                env.AssertEventNew(
                    "s0",
                    @event => {
                        var c = @event.Get("c0").Unwrap<IDictionary<string, object>>();
                        EPAssertionUtil.AssertEqualsExactOrder(c.ToArray(), new[] { detailTwo });
                    });

                env.UndeployAll();
            }
        }
        
        internal class ExprEnumPropertyInsertIntoAtEventBean : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@public @buseventtype create objectarray schema StockTick(Id string, Price int);\n" +
                    "insert into TicksLarge select window(*).where(e => e.Price > 100) @eventbean as ticksLargePrice\n" +
                    "from StockTick#time(10) having count(*) > 2;\n" +
                    "@name('s0') select ticksLargePrice.where(e => e.Price < 200) as ticksLargeLess200 from TicksLarge;\n";
                env.CompileDeploy(epl, new RegressionPath()).AddListener("s0");

                env.SendEventObjectArray(new object[] { "E1", 90 }, "StockTick");
                env.SendEventObjectArray(new object[] { "E2", 120 }, "StockTick");
                env.SendEventObjectArray(new object[] { "E3", 95 }, "StockTick");

                env.AssertEventNew(
                    "s0",
                    @event => ClassicAssert.AreEqual(1, @event.Get("ticksLargeLess200").Unwrap<object>().Count));

                env.UndeployAll();
            }
        }

        internal class ExprEnumMatchRecognizeMeasures : RegressionExecution
        {
            private readonly bool select;

            public ExprEnumMatchRecognizeMeasures(bool select)
            {
                this.select = select;
            }

            public void Run(RegressionEnvironment env)
            {
                string epl;
                if (!select) {
                    epl = "select Ids from SupportBean match_recognize ( " +
                          "  measures A.selectFrom(o -> o.TheString) as Ids ";
                }
                else {
                    epl =
                        "select a.selectFrom(o -> o.TheString) as Ids from SupportBean match_recognize (measures A as a ";
                }

                epl = "@name('s0') " + epl + " pattern (A{3}) define A as A.IntPrimitive = 1)";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 1));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean("E3", 1));
                env.AssertEventNew("s0", @event => AssertColl("E1,E2,E3", @event.Get("Ids")));

                env.SendEventBean(new SupportBean("E4", 1));
                env.SendEventBean(new SupportBean("E5", 1));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean("E6", 1));
                env.AssertEventNew("s0", @event => AssertColl("E4,E5,E6", @event.Get("Ids")));

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "select=" +
                       select +
                       '}';
            }
        }

        internal class ExprEnumSubstitutionParameter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TrySubstitutionParameter(env, "?::int[primitive]", new int[] { 1, 10, 100 });
                TrySubstitutionParameter(env, "?::System.Object[]", new object[] { 1, 10, 100 });
                TrySubstitutionParameter(env, "?::Integer[]", new int?[] { 1, 10, 100 });
            }
        }

        internal class ExprEnumTableRow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test table access expression
                var path = new RegressionPath();
                env.CompileDeploy("@public create table MyTableUnkeyed(theWindow window(*) @type(SupportBean))", path);
                env.CompileDeploy(
                    "into table MyTableUnkeyed select window(*) as theWindow from SupportBean#time(30)",
                    path);
                env.SendEventBean(new SupportBean("E1", 10));
                env.SendEventBean(new SupportBean("E2", 20));

                env.CompileDeploy(
                    "@name('s0')select MyTableUnkeyed.theWindow.anyOf(v=>IntPrimitive=10) as c0 from SupportBean_A",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean_A("A0"));
                env.AssertEqualsNew("s0", "c0", true);

                env.UndeployAll();
            }
        }

        internal class ExprEnumPatternFilter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select * from pattern [ ([2]a=SupportBean_ST0) -> b=SupportBean(IntPrimitive > a.max(i -> P00))]";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean_ST0("E1", 10));
                env.SendEventBean(new SupportBean_ST0("E2", 15));
                env.SendEventBean(new SupportBean("E3", 15));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean("E4", 16));
                env.AssertPropsNew("s0", "a[0].Id,a[1].Id,b.TheString".Split(","), new object[] { "E1", "E2", "E4" });
                env.UndeployAll();

                env.CompileDeploy(
                    "@name('s0') select * from pattern [ a=SupportBean_ST0 until b=SupportBean -> c=SupportBean(IntPrimitive > a.sumOf(i => P00))]");
                env.AddListener("s0");

                env.SendEventBean(new SupportBean_ST0("E10", 10));
                env.SendEventBean(new SupportBean_ST0("E11", 15));
                env.SendEventBean(new SupportBean("E12", -1));
                env.SendEventBean(new SupportBean("E13", 25));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean("E14", 26));
                env.AssertPropsNew(
                    "s0",
                    "a[0].Id,a[1].Id,b.TheString,c.TheString".Split(","),
                    new object[] { "E10", "E11", "E12", "E14" });

                env.UndeployAll();
            }
        }

        internal class ExprEnumMatchRecognizeDefine : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // try define-clause
                var fieldsOne = new[] {"a_array[0].TheString", "a_array[1].TheString", "b.TheString"};
                var textOne = "@name('s0') select * from SupportBean " +
                              "match_recognize (" +
                              " measures A as a_array, B as b " +
                              " pattern (A* B)" +
                              " define" +
                              " B as A.anyOf(v=> v.IntPrimitive = B.IntPrimitive)" +
                              ")";
                env.CompileDeploy(textOne).AddListener("s0");
                env.SendEventBean(new SupportBean("A1", 10));
                env.SendEventBean(new SupportBean("A2", 20));
                env.SendEventBean(new SupportBean("A3", 20));
                env.AssertPropsNew("s0", fieldsOne, new object[] { "A1", "A2", "A3" });

                env.SendEventBean(new SupportBean("A4", 1));
                env.SendEventBean(new SupportBean("A5", 2));
                env.SendEventBean(new SupportBean("A6", 3));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean("A7", 2));
                env.AssertPropsNew("s0", fieldsOne, new object[] { "A4", "A5", "A7" });
                env.UndeployAll();

                // try measures-clause
                var fieldsTwo = "c0".Split(",");
                var textTwo = "@name('s0') select * from SupportBean " +
                              "match_recognize (" +
                              " measures A.anyOf(v=> v.IntPrimitive = B.IntPrimitive) as c0 " +
                              " pattern (A* B)" +
                              " define" +
                              " A as A.TheString like 'A%'," +
                              " B as B.TheString like 'B%'" +
                              ")";
                env.CompileDeploy(textTwo).AddListener("s0");

                env.SendEventBean(new SupportBean("A1", 10));
                env.SendEventBean(new SupportBean("A2", 20));
                env.AssertListenerNotInvoked("s0");
                env.SendEventBean(new SupportBean("B1", 20));
                env.AssertPropsNew("s0", fieldsTwo, new object[] { true });

                env.SendEventBean(new SupportBean("A1", 10));
                env.SendEventBean(new SupportBean("A2", 20));
                env.SendEventBean(new SupportBean("B1", 15));
                env.AssertPropsNew("s0", fieldsTwo, new object[] { false });

                env.UndeployAll();
            }
        }

        internal class ExprEnumEnumObject : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1".Split(",");
                var epl = "@name('s0') select " +
                          "SupportEnumTwo.ENUM_VALUE_1.GetMystrings().anyOf(v => v = Id) as c0, " +
                          "Value.GetMystrings().anyOf(v => v = '2') as c1 " +
                          "from SupportEnumTwoEvent";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportEnumTwoEvent("0", SupportEnumTwo.ENUM_VALUE_1));
                env.AssertPropsNew("s0", fields, new object[] { true, false });

                env.SendEventBean(new SupportEnumTwoEvent("2", SupportEnumTwo.ENUM_VALUE_2));
                env.AssertPropsNew("s0", fields, new object[] { false, true });

                env.UndeployAll();
            }
        }

        internal class ExprEnumSortedMaxMinBy : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3,c4".Split(",");

                var eplWindowAgg = "@name('s0') select " +
                                   "sorted(TheString).allOf(x => x.IntPrimitive < 5) as c0," +
                                   "maxby(TheString).allOf(x => x.IntPrimitive < 5) as c1," +
                                   "minby(TheString).allOf(x => x.IntPrimitive < 5) as c2," +
                                   "maxbyever(TheString).allOf(x => x.IntPrimitive < 5) as c3," +
                                   "minbyever(TheString).allOf(x => x.IntPrimitive < 5) as c4" +
                                   " from SupportBean#length(5)";
                env.CompileDeploy(eplWindowAgg).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertPropsNew("s0", fields, new object[] { true, true, true, true, true });

                env.UndeployAll();
            }
        }

        internal class ExprEnumJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select * from SupportSelectorEvent#keepall as sel, SupportContainerEvent#keepall as cont " +
                    "where cont.Items.anyOf(i => sel.Selector = i.Selected)";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportSelectorEvent("S1", "sel1"));
                env.SendEventBean(new SupportContainerEvent("C1", new SupportContainedItem("I1", "sel1")));
                env.AssertListenerInvoked("s0");

                env.UndeployAll();
            }
        }

        internal class ExprEnumPrevWindowSorted : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select prevwindow(st0) as val0, prevwindow(st0).EsperInternalNoop() as val1 " +
                          "from SupportBean_ST0#sort(3, P00 asc) as st0";
                env.CompileDeploy(epl).AddListener("s0");

                env.AssertStmtTypes(
                    "s0",
                    "val0,val1".Split(","),
                    new Type[] { typeof(SupportBean_ST0[]), typeof(ICollection<SupportBean_ST0>) });

                env.SendEventBean(new SupportBean_ST0("E1", 5));
                LambdaAssertionUtil.AssertST0IdWReset(env, "val1", "E1");

                env.SendEventBean(new SupportBean_ST0("E2", 6));
                LambdaAssertionUtil.AssertST0IdWReset(env, "val1", "E1,E2");

                env.SendEventBean(new SupportBean_ST0("E3", 4));
                LambdaAssertionUtil.AssertST0IdWReset(env, "val1", "E3,E1,E2");

                env.SendEventBean(new SupportBean_ST0("E5", 3));
                LambdaAssertionUtil.AssertST0IdWReset(env, "val1", "E5,E3,E1");
                env.UndeployAll();

                // Scalar version
                var fields = new string[] { "val0" };
                var stmtScalar = "@name('s0') select prevwindow(Id).where(x => x not like '%ignore%') as val0 " +
                                 "from SupportBean_ST0#keepall as st0";
                env.CompileDeploy(stmtScalar).AddListener("s0");
                env.AssertStmtTypes(
                    "s0",
                    fields,
                    new Type[] {
                        typeof(ICollection<string>)
                    });

                env.SendEventBean(new SupportBean_ST0("E1", 5));
                LambdaAssertionUtil.AssertValuesArrayScalarWReset(env, "val0", "E1");

                env.SendEventBean(new SupportBean_ST0("E2ignore", 6));
                LambdaAssertionUtil.AssertValuesArrayScalarWReset(env, "val0", "E1");

                env.SendEventBean(new SupportBean_ST0("E3", 4));
                LambdaAssertionUtil.AssertValuesArrayScalarWReset(env, "val0", "E3", "E1");

                env.SendEventBean(new SupportBean_ST0("ignoreE5", 3));
                LambdaAssertionUtil.AssertValuesArrayScalarWReset(env, "val0", "E3", "E1");

                env.UndeployAll();
            }
        }

        internal class ExprEnumNamedWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "@public create window MyWindow#keepall as SupportBean_ST0;\n" +
                          "on SupportBean_A delete from MyWindow;\n" +
                          "insert into MyWindow select * from SupportBean_ST0;\n";
                env.CompileDeploy(epl, path);

                env.CompileDeploy(
                    "@name('s0') select MyWindow.allOf(x => x.P00 < 5) as allOfX from SupportBean#keepall",
                    path);
                env.AddListener("s0");
                env.AssertStmtTypes("s0", "allOfX".Split(","), new Type[] { typeof(bool?) });

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertEqualsNew("s0", "allOfX", null);

                env.SendEventBean(new SupportBean_ST0("ST0", "1", 10));
                env.SendEventBean(new SupportBean("E2", 10));
                env.AssertEqualsNew("s0", "allOfX", false);

                env.UndeployModuleContaining("s0");
                env.SendEventBean(new SupportBean_A("A1"));

                // test named window correlated
                var eplNamedWindowCorrelated =
                    "@name('s0') select MyWindow(Key0 = sb.TheString).allOf(x => x.P00 < 5) as allOfX from SupportBean#keepall sb";
                env.CompileDeploy(eplNamedWindowCorrelated, path).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertEqualsNew("s0", "allOfX", null);

                env.SendEventBean(new SupportBean_ST0("E2", "KEY1", 1));
                env.SendEventBean(new SupportBean("E2", 0));
                env.AssertEqualsNew("s0", "allOfX", null);

                env.SendEventBean(new SupportBean("KEY1", 0));
                env.AssertEqualsNew("s0", "allOfX", true);

                env.UndeployAll();
            }
        }

        internal class ExprEnumSubselect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test subselect-wildcard
                var eplSubselect =
                    "@name('s0') select (select * from SupportBean_ST0#keepall).allOf(x => x.P00 < 5) as allOfX from SupportBean#keepall";
                env.CompileDeploy(eplSubselect).AddListener("s0");

                env.SendEventBean(new SupportBean_ST0("ST0", "1", 0));
                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertEqualsNew("s0", "allOfX", true);

                env.SendEventBean(new SupportBean_ST0("ST0", "1", 10));
                env.SendEventBean(new SupportBean("E2", 2));
                env.AssertEqualsNew("s0", "allOfX", false);
                env.UndeployAll();

                // test subselect scalar return
                var eplSubselectScalar =
                    "@name('s0') select (select Id from SupportBean_ST0#keepall).allOf(x => x  like '%B%') as allOfX from SupportBean#keepall";
                env.CompileDeploy(eplSubselectScalar).AddListener("s0");

                env.SendEventBean(new SupportBean_ST0("B1", 0));
                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertEqualsNew("s0", "allOfX", true);

                env.SendEventBean(new SupportBean_ST0("A1", 0));
                env.SendEventBean(new SupportBean("E2", 2));
                env.AssertEqualsNew("s0", "allOfX", false);
                env.UndeployAll();

                // test subselect-correlated scalar return
                var eplSubselectScalarCorrelated =
                    "@name('s0') select (select Key0 from SupportBean_ST0#keepall st0 where st0.Id = sb.TheString).allOf(x => x  like '%hello%') as allOfX from SupportBean#keepall sb";
                env.CompileDeploy(eplSubselectScalarCorrelated).AddListener("s0");

                env.SendEventBean(new SupportBean_ST0("A1", "hello", 0));
                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertEqualsNew("s0", "allOfX", null);

                env.SendEventBean(new SupportBean_ST0("A2", "hello", 0));
                env.SendEventBean(new SupportBean("A2", 1));
                env.AssertEqualsNew("s0", "allOfX", true);

                env.SendEventBean(new SupportBean_ST0("A3", "test", 0));
                env.SendEventBean(new SupportBean("A3", 1));
                env.AssertEqualsNew("s0", "allOfX", false);
                env.UndeployAll();

                // test subselect multivalue return
                var fields = new string[] { "Id", "P00" };
                var eplSubselectMultivalue =
                    "@name('s0') select (select Id, P00 from SupportBean_ST0#keepall).take(10) as c0 from SupportBean";
                env.CompileDeploy(eplSubselectMultivalue).AddListener("s0");

                env.SendEventBean(new SupportBean_ST0("B1", 10));
                env.SendEventBean(new SupportBean("E1", 0));
                env.AssertEventNew(
                    "s0",
                    @event => AssertPropsMapRows(
                        @event.Get("c0").Unwrap<object>(),
                        fields,
                        new object[][] { new object[] { "B1", 10 } }));

                env.SendEventBean(new SupportBean_ST0("B2", 20));
                env.SendEventBean(new SupportBean("E2", 0));
                env.AssertEventNew(
                    "s0",
                    @event => AssertPropsMapRows(
                        @event.Get("c0").Unwrap<object>(),
                        fields,
                        new object[][] { new object[] { "B1", 10 }, new object[] { "B2", 20 } }));
                env.UndeployAll();

                // test subselect that delivers events
                var epl =
                    "@public @buseventtype create schema AEvent (Symbol string);\n" +
                    "@public @buseventtype create schema BEvent (a AEvent);\n" +
                    "@name('s0') select (select a from BEvent#keepall).anyOf(v => Symbol = 'GE') as flag from SupportBean;\n";
                env.CompileDeploy(epl, new RegressionPath()).AddListener("s0");

                env.SendEventMap(MakeBEvent("XX"), "BEvent");
                env.SendEventBean(new SupportBean());
                env.AssertEqualsNew("s0", "flag", false);

                Console.WriteLine("Critical Event");
                env.SendEventMap(MakeBEvent("GE"), "BEvent");
                env.SendEventBean(new SupportBean());
                env.AssertEqualsNew("s0", "flag", true);

                env.UndeployAll();
            }
        }

        internal class ExprEnumVariable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create variable string[] myvar = { 'E1', 'E3' };\n" +
                          "@name('s0') select * from SupportBean(myvar.anyOf(v => v = TheString));\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertListenerInvoked("s0");
                env.SendEventBean(new SupportBean("E2", 1));
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        internal class ExprEnumAccessAggregation : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "val0", "val1", "val2", "val3", "val4" };

                // test window(*) and first(*)
                var eplWindowAgg = "@name('s0') select " +
                                   "window(*).allOf(x => x.IntPrimitive < 5) as val0," +
                                   "first(*).allOf(x => x.IntPrimitive < 5) as val1," +
                                   "first(*, 1).allOf(x => x.IntPrimitive < 5) as val2," +
                                   "last(*).allOf(x => x.IntPrimitive < 5) as val3," +
                                   "last(*, 1).allOf(x => x.IntPrimitive < 5) as val4" +
                                   " from SupportBean#length(2)";
                env.CompileDeploy(eplWindowAgg).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertPropsNew("s0", fields, new object[] { true, true, null, true, null });

                env.SendEventBean(new SupportBean("E2", 10));
                env.AssertPropsNew("s0", fields, new object[] { false, true, false, false, true });

                env.SendEventBean(new SupportBean("E3", 2));
                env.AssertPropsNew("s0", fields, new object[] { false, false, true, true, false });

                env.UndeployAll();

                // test scalar: window(*) and first(*)
                var eplWindowAggScalar = "@name('s0') select " +
                                         "window(IntPrimitive).allOf(x => x < 5) as val0," +
                                         "first(IntPrimitive).allOf(x => x < 5) as val1," +
                                         "first(IntPrimitive, 1).allOf(x => x < 5) as val2," +
                                         "last(IntPrimitive).allOf(x => x < 5) as val3," +
                                         "last(IntPrimitive, 1).allOf(x => x < 5) as val4" +
                                         " from SupportBean#length(2)";
                env.CompileDeploy(eplWindowAggScalar).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertPropsNew("s0", fields, new object[] { true, true, null, true, null });

                env.SendEventBean(new SupportBean("E2", 10));
                env.AssertPropsNew("s0", fields, new object[] { false, true, false, false, true });

                env.SendEventBean(new SupportBean("E3", 2));
                env.AssertPropsNew("s0", fields, new object[] { false, false, true, true, false });

                env.UndeployAll();
            }
        }

        internal class ExprEnumProperty : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test fragment type - collection inside
                var eplFragment =
                    "@name('s0') select Contained.allOf(x => x.P00 < 5) as allOfX from SupportBean_ST0_Container#keepall";
                env.CompileDeploy(eplFragment).AddListener("s0");

                env.SendEventBean(SupportBean_ST0_Container.Make3Value("ID1,KEY1,1"));
                env.AssertEqualsNew("s0", "allOfX", true);

                env.SendEventBean(SupportBean_ST0_Container.Make3Value("ID1,KEY1,10"));
                env.AssertEqualsNew("s0", "allOfX", false);
                env.UndeployAll();

                // test array and iterable
                var fields = "val0,val1".Split(",");
                eplFragment = "@name('s0') select Intarray.sumof() as val0, " +
                              "Intiterable.sumOf() as val1 " +
                              " from SupportCollection#keepall";
                env.CompileDeploy(eplFragment).AddListener("s0");

                env.SendEventBean(SupportCollection.MakeNumeric("5,6,7"));
                env.AssertPropsNew("s0", fields, new object[] { 5 + 6 + 7, 5 + 6 + 7 });

                env.UndeployAll();

                // test map event type with object-array prop
                var path = new RegressionPath();
                env.CompileDeploy("@buseventtype @public create schema MySchema (Books BookDesc[])", path);

                env.CompileDeploy("@name('s0') select Books.max(i => i.Price) as mymax from MySchema", path);
                env.AddListener("s0");

                var @event = Collections.SingletonDataMap(
                    "Books",
                    new BookDesc[] { new BookDesc("1", "book1", "dave", 1.00, null) });
                env.SendEventMap(@event, "MySchema");
                env.AssertPropsNew("s0", "mymax".Split(","), new object[] { 1.0 });

                env.UndeployAll();

                // test method invocation variations returning list/array of string and test UDF +property as well
                RunAssertionMethodInvoke(
                    env,
                    "select e.GetTheList().anyOf(v => v = Selector) as flag from SupportSelectorWithListEvent e");
                RunAssertionMethodInvoke(
                    env,
                    "select convertToArray(TheList).anyOf(v => v = Selector) as flag from SupportSelectorWithListEvent e");
                RunAssertionMethodInvoke(
                    env,
                    "select TheArray.anyOf(v => v = Selector) as flag from SupportSelectorWithListEvent e");
                RunAssertionMethodInvoke(
                    env,
                    "select e.GetTheArray().anyOf(v => v = Selector) as flag from SupportSelectorWithListEvent e");
                RunAssertionMethodInvoke(
                    env,
                    "select e.TheList.anyOf(v => v = e.Selector) as flag from pattern[every e=SupportSelectorWithListEvent]");
                RunAssertionMethodInvoke(
                    env,
                    "select e.NestedMyEvent.MyNestedList.anyOf(v => v = e.Selector) as flag from pattern[every e=SupportSelectorWithListEvent]");
                RunAssertionMethodInvoke(
                    env,
                    "select " +
                    typeof(SupportSelectorWithListEvent).FullName +
                    ".ConvertToArray(TheList).anyOf(v => v = Selector) as flag from SupportSelectorWithListEvent e");

                env.UndeployAll();
            }
        }

        public static void RunAssertionMethodInvoke(
            RegressionEnvironment env,
            string epl)
        {
            var fields = "flag".Split(",");
            env.CompileDeploy("@name('s0') " + epl).AddListener("s0");

            env.SendEventBean(new SupportSelectorWithListEvent("1"));
            env.AssertPropsNew("s0", fields, new object[] { true });

            env.SendEventBean(new SupportSelectorWithListEvent("4"));
            env.AssertPropsNew("s0", fields, new object[] { false });

            env.UndeployAll();
        }

        internal class ExprEnumPrevFuncs : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = { "val0", "val1", "val2" };
                // test prevwindow(*) etc
                var epl = "@Name('s0') select " +
                          "prevwindow(sb).allOf(x -> x.IntPrimitive < 5) as val0," +
                          "prev(sb,1).allOf(x -> x.IntPrimitive < 5) as val1," +
                          "prevtail(sb,1).allOf(x -> x.IntPrimitive < 5) as val2" +
                          " from SupportBean#length(2) as sb";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertPropsNew("s0", fields, new object[] { true, null, null });

                env.SendEventBean(new SupportBean("E2", 10));
                env.AssertPropsNew("s0", fields, new object[] { false, true, false });

                env.SendEventBean(new SupportBean("E3", 2));
                env.AssertPropsNew("s0", fields, new object[] { false, false, true });

                env.SendEventBean(new SupportBean("E4", 3));
                env.AssertPropsNew("s0", fields, new object[] { true, true, true });
                env.UndeployAll();

                // test scalar prevwindow(property) etc
                var eplScalar = "@name('s0') select " +
                                "prevwindow(IntPrimitive).allOf(x => x < 5) as val0," +
                                "prev(IntPrimitive,1).allOf(x => x < 5) as val1," +
                                "prevtail(IntPrimitive,1).allOf(x => x < 5) as val2" +
                                " from SupportBean#length(2) as sb";
                env.CompileDeploy(eplScalar).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertPropsNew("s0", fields, new object[] { true, null, null });

                env.SendEventBean(new SupportBean("E2", 10));
                env.AssertPropsNew("s0", fields, new object[] { false, true, false });

                env.SendEventBean(new SupportBean("E3", 2));
                env.AssertPropsNew("s0", fields, new object[] { false, false, true });

                env.SendEventBean(new SupportBean("E4", 3));
                env.AssertPropsNew("s0", fields, new object[] { true, true, true });

                env.UndeployAll();
            }
        }

        internal class ExprEnumUDFStaticMethod : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] { "val1", "val2", "val3", "val4" };
                var epl = "@Name('s0') select " +
                          "SupportBean_ST0_Container.MakeSampleList().where(x -> x.P00 < 5) as val1, " +
                          "SupportBean_ST0_Container.MakeSampleArray().where(x -> x.P00 < 5) as val2, " +
                          "makeSampleList().where(x -> x.P00 < 5) as val3, " +
                          "makeSampleArray().where(x -> x.P00 < 5) as val4 " +
                          "from SupportBean#length(2) as sb";
                env.CompileDeploy(epl).AddListener("s0");

                SupportBean_ST0_Container.Samples = new string[] { "E1,1", "E2,20", "E3,3" };
                env.SendEventBean(new SupportBean());
                env.AssertListener(
                    "s0",
                    listener => {
                        foreach (var field in fields) {
                            var result = listener
                                .AssertOneGetNew()
                                .Get(field)
                                .UnwrapIntoArray<SupportBean_ST0>();
                            ClassicAssert.AreEqual(2, result.Length, "Failed for field " + field);
                        }

                        listener.Reset();
                    });

                SupportBean_ST0_Container.Samples = null;
                env.SendEventBean(new SupportBean());
                env.AssertListener(
                    "s0",
                    listener => {
                        foreach (var field in fields) {
                            ClassicAssert.IsNull(listener.AssertOneGetNew().Get(field));
                        }

                        listener.Reset();
                    });

                SupportBean_ST0_Container.Samples = Array.Empty<string>();
                env.SendEventBean(new SupportBean());
                env.AssertListener(
                    "s0",
                    listener => {
                        foreach (var field in fields) {
                            var result = listener
                                .AssertOneGetNew()
                                .Get(field)
                                .UnwrapIntoArray<SupportBean_ST0>();
                            ClassicAssert.AreEqual(0, result.Length);
                        }

                        listener.Reset();
                    });
                env.UndeployAll();

                // test UDF returning scalar values collection
                fields = new[] {"val0", "val1", "val2", "val3"};
                var eplScalar = "@name('s0') select " +
                                "SupportCollection.MakeSampleListString().where(x => x != 'E1') as val0, " +
                                "SupportCollection.MakeSampleArrayString().where(x => x != 'E1') as val1, " +
                                "makeSampleListString().where(x => x != 'E1') as val2, " +
                                "makeSampleArrayString().where(x => x != 'E1') as val3 " +
                                "from SupportBean#length(2) as sb";
                env.CompileDeploy(eplScalar).AddListener("s0");
                env.AssertStatement(
                    "s0",
                    statement => env.AssertStmtTypesAllSame("s0", fields, typeof(ICollection<string>)));

                SupportCollection.SampleCSV = "E1,E2,E3";
                env.SendEventBean(new SupportBean());
                env.AssertListener(
                    "s0",
                    listener => {
                        var @event = listener.AssertOneGetNewAndReset();
                        foreach (var field in fields) {
                            LambdaAssertionUtil.AssertValuesArrayScalar(@event, field, "E2", "E3");
                        }
                    });

                SupportCollection.SampleCSV = null;
                env.SendEventBean(new SupportBean());
                env.AssertListener(
                    "s0",
                    listener => {
                        var @event = listener.AssertOneGetNewAndReset();
                        foreach (var field in fields) {
                            LambdaAssertionUtil.AssertValuesArrayScalar(@event, field, null);
                        }
                    });

                SupportCollection.SampleCSV = "";
                env.SendEventBean(new SupportBean());
                env.AssertListener(
                    "s0",
                    listener => {
                        var @event = listener.AssertOneGetNewAndReset();
                        foreach (var field in fields) {
                            LambdaAssertionUtil.AssertValuesArrayScalar(@event, field);
                        }
                    });

                env.UndeployAll();
            }
        }

        private static void TrySubstitutionParameter(
            RegressionEnvironment env,
            string substitution,
            object parameter)
        {
            var compiled = env.Compile(
                "@name('s0') select * from SupportBean(" + substitution + ".sequenceEqual({1, IntPrimitive, 100}))");
            env.Deploy(
                compiled,
                new DeploymentOptions().WithStatementSubstitutionParameter(
                    new SupportPortableDeploySubstitutionParams(1, parameter).SetStatementParameters));
            env.AddListener("s0");

            env.SendEventBean(new SupportBean("E1", 10));
            env.AssertListenerInvoked("s0");

            env.SendEventBean(new SupportBean("E2", 20));
            env.AssertListenerNotInvoked("s0");

            env.UndeployAll();
        }

        private static SupportBean_ST0[] ToArray(ICollection<SupportBean_ST0> collection)
        {
            if (!collection.IsEmpty()) { // not applicable in .NET | collection.GetEnumerator().Next() is EventBean
                Assert.Fail("Iterator provides EventBean instances");
            }

            return collection.ToArray();
        }

        private static IDictionary<string, object> MakeBEvent(string symbol)
        {
            IDictionary<string, object> map = new Dictionary<string, object>();
            map.Put("a", Collections.SingletonMap("Symbol", symbol));
            return map;
        }

        private static void AssertPropsMapRows(
            ICollection<object> rows,
            string[] fields,
            object[][] objects)
        {
            var mapsColl = rows.Unwrap<IDictionary<string, object>>();
            var maps = mapsColl.ToArray();
            EPAssertionUtil.AssertPropsPerRow(maps, fields, objects);
        }

        private static void AssertColl(
            string expected,
            object value)
        {
            EPAssertionUtil.AssertEqualsExactOrder(expected.Split(","), value.UnwrapIntoArray<object>());
        }

        public class MyLocalEvent
        {
            public MyLocalEvent(object value)
            {
                this.Value = value;
            }

            public object Value { get; }
        }

        public class MyLocalEventWithInts
        {
            public MyLocalEventWithInts(ISet<int> intValues)
            {
                this.IntValues = intValues;
            }

            public ISet<int> IntValues { get; }

            public bool MyFunc()
            {
                foreach (var val in IntValues) {
                    if (val > 0) {
                        return true;
                    }
                }

                return false;
            }
        }

        public class MyLocalWithCollection
        {
            public MyLocalWithCollection(ICollection<object> someCollection)
            {
                this.SomeCollection = someCollection;
            }

            public ICollection<object> SomeCollection { get; }
        }

        public class SupportEventWithMapOfCollOfString
        {
            private readonly IDictionary<string, ICollection<string>> mymap;

            public SupportEventWithMapOfCollOfString(
                string mapkey,
                ICollection<string> mymap)
            {
                this.mymap = Collections.SingletonMap(mapkey, mymap);
            }

            public IDictionary<string, ICollection<string>> GetMymap()
            {
                return mymap;
            }
        }
    }
} // end of namespace