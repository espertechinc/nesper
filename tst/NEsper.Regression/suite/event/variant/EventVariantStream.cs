///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;


namespace com.espertech.esper.regressionlib.suite.@event.variant
{
    public class EventVariantStream
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithDynamicMapType(execs);
            WithNamedWin(execs);
            WithSingleColumnConversion(execs);
            WithCoercionBoxedTypeMatch(execs);
            WithSuperTypesInterfaces(execs);
            WithPatternSubquery(execs);
            WithInvalidInsertInto(execs);
            WithSimple(execs);
            WithInsertInto(execs);
            WithMetadata(execs);
            WithAnyType(execs);
            WithAnyTypeStaggered(execs);
            WithInsertWrap(execs);
            WithSingleStreamWrap(execs);
            WithWildcardJoin(execs);
            WithWithLateCreateSchema(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithWithLateCreateSchema(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventVariantWithLateCreateSchema());
            return execs;
        }

        public static IList<RegressionExecution> WithWildcardJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventVariantWildcardJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithSingleStreamWrap(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventVariantSingleStreamWrap());
            return execs;
        }

        public static IList<RegressionExecution> WithInsertWrap(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventVariantInsertWrap());
            return execs;
        }

        public static IList<RegressionExecution> WithAnyTypeStaggered(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventVariantAnyTypeStaggered());
            return execs;
        }

        public static IList<RegressionExecution> WithAnyType(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventVariantAnyType());
            return execs;
        }

        public static IList<RegressionExecution> WithMetadata(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventVariantMetadata());
            return execs;
        }

        public static IList<RegressionExecution> WithInsertInto(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventVariantInsertInto());
            return execs;
        }

        public static IList<RegressionExecution> WithSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventVariantSimple("VarStreamABPredefined"));
            execs.Add(new EventVariantSimple("VarStreamAny"));
            return execs;
        }

        public static IList<RegressionExecution> WithInvalidInsertInto(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventVariantInvalidInsertInto());
            return execs;
        }

        public static IList<RegressionExecution> WithPatternSubquery(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventVariantPatternSubquery());
            return execs;
        }

        public static IList<RegressionExecution> WithSuperTypesInterfaces(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventVariantSuperTypesInterfaces());
            return execs;
        }

        public static IList<RegressionExecution> WithCoercionBoxedTypeMatch(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventVariantCoercionBoxedTypeMatch());
            return execs;
        }

        public static IList<RegressionExecution> WithSingleColumnConversion(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventVariantSingleColumnConversion());
            return execs;
        }

        public static IList<RegressionExecution> WithNamedWin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventVariantNamedWin());
            return execs;
        }

        public static IList<RegressionExecution> WithDynamicMapType(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventVariantDynamicMapType());
            return execs;
        }

        private class EventVariantWithLateCreateSchema : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create variant schema MyVariants as *", path);
                env.CompileDeploy("@name('out') select * from MyVariants#length(10)", path);
                env.CompileDeploy("@public @buseventtype create map schema SomeEventOne as (id string)", path);
                env.CompileDeploy("@public @buseventtype create objectarray schema SomeEventTwo as (id string)", path);
                env.CompileDeploy("insert into MyVariants select * from SomeEventOne", path);
                env.CompileDeploy("insert into MyVariants select * from SomeEventTwo", path);

                env.SendEventMap(Collections.SingletonDataMap("id", "E1"), "SomeEventOne");
                env.SendEventObjectArray(new object[] { "E2" }, "SomeEventTwo");
                env.SendEventMap(Collections.SingletonDataMap("id", "E3"), "SomeEventOne");
                env.SendEventObjectArray(new object[] { "E4" }, "SomeEventTwo");

                env.Milestone(0);

                env.AssertPropsPerRowIterator(
                    "out",
                    "id".SplitCsv(),
                    new object[][]
                        { new object[] { "E1" }, new object[] { "E2" }, new object[] { "E3" }, new object[] { "E4" } });

                env.UndeployAll();
            }
        }

        private class EventVariantWildcardJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create variant schema MyVariantWJ as *;\n" +
                          "insert into MyVariantWJ select * from SupportBean sb unidirectional, SupportBean_S0#keepall as s0;\n" +
                          "@name('s0') select * from MyVariantWJ";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(10));
                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertPropsNew("s0", new string[] { "sb.theString", "s0.id" }, new object[] { "E1", 10 });

                env.UndeployAll();
            }
        }

        private class EventVariantSingleStreamWrap : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create variant schema MyVariantSSW as *;\n" +
                          "insert into OneStream select *, 'a' as field from SupportBean;\n" +
                          "insert into MyVariantSSW select * from OneStream;\n" +
                          "@name('s0') select * from MyVariantSSW";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertPropsNew("s0", new string[] { "theString", "field" }, new object[] { "E1", "a" });

                env.UndeployAll();
            }
        }

        public class EventVariantSimple : RegressionExecution
        {
            private readonly string variantStreamName;

            internal EventVariantSimple(string variantStreamName)
            {
                this.variantStreamName = variantStreamName;
            }

            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "id" };

                env.Milestone(0);

                var epl = "@name('s0') select irstream id? as id from " + variantStreamName + "#length(2)";
                env.CompileDeploy(epl).AddListener("s0");

                env.CompileDeploy("insert into " + variantStreamName + " select * from SupportBean_A");
                env.CompileDeploy("insert into " + variantStreamName + " select * from SupportBean_B");

                env.Milestone(1);

                env.SendEventBean(new SupportBean_A("E1"));
                env.AssertPropsNew("s0", fields, new object[] { "E1" });
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "E1" } });

                env.Milestone(2);

                env.SendEventBean(new SupportBean_B("E2"));
                env.AssertPropsNew("s0", fields, new object[] { "E2" });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1" }, new object[] { "E2" } });

                env.Milestone(3);

                env.SendEventBean(new SupportBean_B("E3"));
                env.AssertPropsIRPair("s0", fields, new object[] { "E3" }, new object[] { "E1" });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2" }, new object[] { "E3" } });

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "variantStreamName='" +
                       variantStreamName +
                       '\'' +
                       '}';
            }
        }

        private class EventVariantDynamicMapType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("insert into MyVariantTwoTyped select * from MyEvent");
                env.CompileDeploy("insert into MyVariantTwoTyped select * from MySecondEvent");
                env.CompileDeploy("@name('s0') select * from MyVariantTwoTyped").AddListener("s0");

                env.SendEventMap(new Dictionary<string, object>(), "MyEvent");
                env.AssertListenerInvoked("s0");

                env.SendEventMap(new Dictionary<string, object>(), "MySecondEvent");
                env.AssertListenerInvoked("s0");

                env.UndeployAll();
            }
        }

        private class EventVariantSingleColumnConversion : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("insert into MyVariantTwoTypedSBVariant select * from SupportBean");
                env.CompileDeploy(
                    "@public create window MainEventWindow#length(10000) as MyVariantTwoTypedSBVariant",
                    path);
                env.CompileDeploy(
                    "insert into MainEventWindow select " +
                    nameof(EventVariantStream) +
                    ".preProcessEvent(event) from MyVariantTwoTypedSBVariant as event",
                    path);
                env.CompileDeploy("@name('s0') select * from MainEventWindow where theString = 'E'", path);
                env.AssertThat(() => env.Statement("s0").AddListenerWithReplay(env.ListenerNew()));

                env.SendEventBean(new SupportBean("E1", 1));

                env.UndeployAll();
            }
        }

        private class EventVariantCoercionBoxedTypeMatch : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string fields;
                SupportBean bean;

                env.CompileDeploy("@name('s0') select * from MyVariantTwoTypedSB").AddListener("s0");
                env.AssertStatement(
                    "s0",
                    statement => {
                        var typeSelectAll = statement.EventType;
                        AssertEventTypeDefault(typeSelectAll);
                        Assert.AreEqual(typeof(object), statement.EventType.UnderlyingType);
                    });

                env.CompileDeploy("insert into MyVariantTwoTypedSB select * from SupportBean");
                env.CompileDeploy("insert into MyVariantTwoTypedSB select * from SupportBeanVariantStream");

                // try wildcard
                object eventOne = new SupportBean("E0", -1);
                env.SendEventBean(eventOne);
                env.AssertEventNew("s0", @event => Assert.AreSame(eventOne, @event.Underlying));

                object eventTwo = new SupportBeanVariantStream("E1");
                env.SendEventBean(eventTwo);
                env.AssertEventNew("s0", @event => Assert.AreSame(eventTwo, @event.Underlying));

                env.UndeployModuleContaining("s0");

                fields = "theString,boolBoxed,intPrimitive,longPrimitive,doublePrimitive,enumValue";
                env.CompileDeploy("@name('s0') select " + fields + " from MyVariantTwoTypedSB").AddListener("s0");
                env.AssertStatement("s0", statement => AssertEventTypeDefault(statement.EventType));

                // coerces to the higher resolution type, accepts boxed versus not boxed
                env.SendEventBean(new SupportBeanVariantStream("s1", true, 1, 20, 30, SupportEnum.ENUM_VALUE_1));
                env.AssertPropsNew(
                    "s0",
                    fields.SplitCsv(),
                    new object[] { "s1", true, 1, 20L, 30d, SupportEnum.ENUM_VALUE_1 });

                bean = new SupportBean("s2", 99);
                bean.LongPrimitive = 33;
                bean.DoublePrimitive = 50;
                bean.EnumValue = SupportEnum.ENUM_VALUE_3;
                env.SendEventBean(bean);
                env.AssertPropsNew(
                    "s0",
                    fields.SplitCsv(),
                    new object[] { "s2", null, 99, 33L, 50d, SupportEnum.ENUM_VALUE_3 });
                env.UndeployModuleContaining("s0");

                // make sure a property is not known since the property is not found on SupportBeanVariantStream
                env.TryInvalidCompile(
                    "select charBoxed from MyVariantTwoTypedSB",
                    "Failed to validate select-clause expression 'charBoxed': Property named 'charBoxed' is not valid in any stream");

                // try dynamic property: should exists but not show up as a declared property
                fields = "v1,v2,v3";
                env.CompileDeploy(
                        "@name('s0') select longBoxed? as v1,charBoxed? as v2,doubleBoxed? as v3 from MyVariantTwoTypedSB")
                    .AddListener("s0");

                bean = new SupportBean();
                bean.LongBoxed = 33L;
                bean.CharBoxed = 'a';
                bean.DoubleBoxed = double.NaN;
                env.SendEventBean(bean);
                env.AssertPropsNew("s0", fields.SplitCsv(), new object[] { 33L, 'a', double.NaN });

                env.SendEventBean(new SupportBeanVariantStream("s2"));
                env.AssertPropsNew("s0", fields.SplitCsv(), new object[] { null, null, null });

                env.UndeployAll();
            }
        }

        private class EventVariantSuperTypesInterfaces : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("insert into MyVariantStreamTwo select * from SupportBeanVariantOne");
                env.CompileDeploy("insert into MyVariantStreamTwo select * from SupportBeanVariantTwo");

                env.CompileDeploy("@name('s0') select * from MyVariantStreamTwo").AddListener("s0");
                env.AssertStatement(
                    "s0",
                    statement => {
                        var eventType = statement.EventType;

                        var expected = new[] { "P0", "P1", "P2", "P3", "P4", "P5", "Indexed", "Mapped", "Inneritem" };
                        var propertyNames = eventType.PropertyNames;
                        EPAssertionUtil.AssertEqualsAnyOrder(expected, propertyNames);
                        Assert.AreEqual(typeof(ISupportBaseAB), eventType.GetPropertyType("P0"));
                        Assert.AreEqual(typeof(ISupportAImplSuperG), eventType.GetPropertyType("P1"));
                        Assert.AreEqual(typeof(LinkedList<object>), eventType.GetPropertyType("P2"));
                        Assert.AreEqual(typeof(IList<object>), eventType.GetPropertyType("P3"));
                        Assert.AreEqual(typeof(ICollection<object>), eventType.GetPropertyType("P4"));
                        Assert.AreEqual(typeof(ICollection<object>), eventType.GetPropertyType("P5"));
                        Assert.AreEqual(typeof(int[]), eventType.GetPropertyType("Indexed"));
                        Assert.AreEqual(typeof(IDictionary<string, string>), eventType.GetPropertyType("Mapped"));
                        Assert.AreEqual(
                            typeof(SupportBeanVariantOne.SupportBeanVariantOneInner),
                            eventType.GetPropertyType("Inneritem"));
                    });

                env.UndeployModuleContaining("s0");

                env.CompileDeploy(
                    "@name('s0') select P0,P1,P2,P3,P4,P5,Indexed[0] as P6,IndexArr[1] as P7,MappedKey('a') as P8,Inneritem as P9,Inneritem.Val as P10 from MyVariantStreamTwo");
                env.AddListener("s0");
                env.AssertStatement(
                    "s0",
                    statement => {
                        var eventType = statement.EventType;
                        Assert.AreEqual(typeof(int?), eventType.GetPropertyType("P6"));
                        Assert.AreEqual(typeof(int?), eventType.GetPropertyType("P7"));
                        Assert.AreEqual(typeof(string), eventType.GetPropertyType("P8"));
                        Assert.AreEqual(
                            typeof(SupportBeanVariantOne.SupportBeanVariantOneInner),
                            eventType.GetPropertyType("P9"));
                        Assert.AreEqual(typeof(string), eventType.GetPropertyType("P10"));
                    });

                var ev1 = new SupportBeanVariantOne();
                env.SendEventBean(ev1);
                env.AssertPropsNew(
                    "s0",
                    new[] { "P6", "P7", "P8", "P9", "P10" },
                    new object[] { 1, 2, "val1", ev1.Inneritem, ev1.Inneritem.Val });

                var ev2 = new SupportBeanVariantTwo();
                env.SendEventBean(ev2);
                env.AssertPropsNew(
                    "s0",
                    new[] { "P6", "P7", "P8", "P9", "P10" },
                    new object[] { 10, 20, "val2", ev2.Inneritem, ev2.Inneritem.Val });

                env.UndeployAll();
            }
        }

        private class EventVariantNamedWin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test named window
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('window') @public create window MyVariantWindow#unique(theString) as select * from MyVariantTwoTypedSB",
                    path);
                env.AddListener("window");
                env.CompileDeploy("insert into MyVariantWindow select * from MyVariantTwoTypedSB", path);
                env.CompileDeploy("insert into MyVariantTwoTypedSB select * from SupportBeanVariantStream", path);
                env.CompileDeploy("insert into MyVariantTwoTypedSB select * from SupportBean", path);

                object eventOne = new SupportBean("E1", -1);
                env.SendEventBean(eventOne);
                env.AssertEventNew("window", @event => Assert.AreSame(eventOne, @event.Underlying));

                env.Milestone(0);

                object eventTwo = new SupportBeanVariantStream("E2");
                env.SendEventBean(eventTwo);
                env.AssertEventNew("window", @event => Assert.AreSame(eventTwo, @event.Underlying));

                env.Milestone(1);

                object eventThree = new SupportBean("E2", -1);
                env.SendEventBean(eventThree);
                env.AssertListener(
                    "window",
                    listener => {
                        Assert.AreEqual(eventThree, listener.LastNewData[0].Underlying);
                        Assert.AreEqual(eventTwo, listener.LastOldData[0].Underlying);
                        listener.Reset();
                    });

                env.Milestone(2);

                object eventFour = new SupportBeanVariantStream("E1");
                env.SendEventBean(eventFour);
                env.AssertListener(
                    "window",
                    listener => {
                        Assert.AreEqual(eventFour, listener.LastNewData[0].Underlying);
                        Assert.AreEqual(eventOne, listener.LastOldData[0].Underlying);
                        listener.Reset();
                    });

                env.UndeployAll();
            }
        }

        private class EventVariantPatternSubquery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("insert into MyVariantStreamFour select * from SupportBeanVariantStream");
                env.CompileDeploy("insert into MyVariantStreamFour select * from SupportBean");

                // test pattern
                env.CompileDeploy("@name('s0') select * from pattern [a=MyVariantStreamFour -> b=MyVariantStreamFour]");
                env.AddListener("s0");
                object[] events = { new SupportBean("E1", -1), new SupportBeanVariantStream("E2") };
                env.SendEventBean(events[0]);
                env.SendEventBean(events[1]);
                env.AssertPropsNew("s0", "a,b".SplitCsv(), events);
                env.UndeployModuleContaining("s0");

                // test subquery
                env.CompileDeploy(
                    "@name('s0') select * from SupportBean_A as a where exists(select * from MyVariantStreamFour#lastevent as b where b.theString=a.id)");
                env.AddListener("s0");
                events = new object[]
                    { new SupportBean("E1", -1), new SupportBeanVariantStream("E2"), new SupportBean_A("E2") };

                env.SendEventBean(events[0]);
                env.SendEventBean(events[2]);
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(events[1]);
                env.SendEventBean(events[2]);
                env.AssertListenerInvoked("s0");

                env.UndeployAll();
            }
        }

        private class EventVariantInvalidInsertInto : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.TryInvalidCompile(
                    "insert into MyVariantStreamFive select * from SupportBean_A",
                    "Selected event type is not a valid event type of the variant stream 'MyVariantStreamFive'");

                env.TryInvalidCompile(
                    "insert into MyVariantStreamFive select intPrimitive as k0 from SupportBean",
                    "Selected event type is not a valid event type of the variant stream 'MyVariantStreamFive' ");
            }
        }

        private class EventVariantInsertInto : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public insert into MyStream select theString, intPrimitive from SupportBean", path);
                env.CompileDeploy("@public insert into VarStreamAny select theString as abc from MyStream", path);
                env.CompileDeploy("@name('Target') select * from VarStreamAny#keepall()", path);

                env.SendEventBean(new SupportBean("E1", 1));

                env.Milestone(0);

                env.AssertPropsPerRowIterator(
                    "Target",
                    new string[] { "abc" },
                    new object[][] { new object[] { "E1" } });

                env.UndeployAll();
            }
        }

        private class EventVariantMetadata : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@name('s0') select * from VarStreamAny");
                env.AssertStatement(
                    "s0",
                    statement => {
                        // assert type metadata
                        var type = statement.EventType;
                        Assert.AreEqual(EventTypeApplicationType.VARIANT, type.Metadata.ApplicationType);
                        Assert.AreEqual("VarStreamAny", type.Metadata.Name);
                        Assert.AreEqual(EventTypeTypeClass.VARIANT, type.Metadata.TypeClass);
                    });

                env.UndeployAll();
            }
        }

        private class EventVariantAnyType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("insert into VarStreamAny select * from SupportBean");
                env.CompileDeploy("insert into VarStreamAny select * from SupportBeanVariantStream");
                env.CompileDeploy("insert into VarStreamAny select * from SupportBean_A");
                env.CompileDeploy(
                    "insert into VarStreamAny select symbol as theString, volume as intPrimitive, feed as id from SupportMarketDataBean");
                env.CompileDeploy("@name('s0') select * from VarStreamAny").AddListener("s0");
                env.AssertStatement("s0", statement => Assert.AreEqual(0, statement.EventType.PropertyNames.Length));

                object eventOne = new SupportBean("E0", -1);
                env.SendEventBean(eventOne);
                env.AssertEventNew("s0", @event => Assert.AreSame(eventOne, @event.Underlying));

                object eventTwo = new SupportBean_A("E1");
                env.SendEventBean(eventTwo);
                env.AssertEventNew("s0", @event => Assert.AreSame(eventTwo, @event.Underlying));

                env.UndeployModuleContaining("s0");

                env.CompileDeploy("@name('s0') select theString,id,intPrimitive from VarStreamAny").AddListener("s0");

                env.AssertStatement(
                    "s0",
                    statement => {
                        var eventType = statement.EventType;
                        Assert.AreEqual(typeof(object), eventType.GetPropertyType("theString"));
                        Assert.AreEqual(typeof(object), eventType.GetPropertyType("id"));
                        Assert.AreEqual(typeof(object), eventType.GetPropertyType("intPrimitive"));
                    });

                var fields = "theString,id,intPrimitive".SplitCsv();
                env.SendEventBean(new SupportBeanVariantStream("E1"));
                env.AssertPropsNew("s0", fields, new object[] { "E1", null, null });

                env.SendEventBean(new SupportBean("E2", 10));
                env.AssertPropsNew("s0", fields, new object[] { "E2", null, 10 });

                env.SendEventBean(new SupportBean_A("E3"));
                env.AssertPropsNew("s0", fields, new object[] { null, "E3", null });

                env.SendEventBean(new SupportMarketDataBean("s1", 100, 1000L, "f1"));
                env.AssertPropsNew("s0", fields, new object[] { "s1", "f1", 1000L });

                env.UndeployAll();
            }
        }

        private class EventVariantAnyTypeStaggered : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public insert into MyStream select theString, intPrimitive from SupportBean", path);
                env.CompileDeploy("insert into VarStreamMD select theString as abc from MyStream", path);
                env.CompileDeploy("@name('Target') select * from VarStreamMD#keepall", path);

                env.SendEventBean(new SupportBean("E1", 1));

                env.AssertPropsPerRowIterator(
                    "Target",
                    new string[] { "abc" },
                    new object[][] { new object[] { "E1" } });

                env.CompileDeploy("@public insert into MyStream2 select feed from SupportMarketDataBean", path);
                env.CompileDeploy("insert into VarStreamMD select feed as abc from MyStream2", path);

                env.SendEventBean(new SupportMarketDataBean("IBM", 1, 1L, "E2"));

                env.AssertPropsPerRowIterator(
                    "Target",
                    new string[] { "abc" },
                    new object[][] { new object[] { "E1" }, new object[] { "E2" } });

                env.UndeployAll();
            }
        }

        private class EventVariantInsertWrap : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Test inserting a wrapper of underlying plus properties
                env.CompileDeploy("insert into VarStreamAny select 'test' as eventConfigId, * from SupportBean");
                env.CompileDeploy("@name('s0') select * from VarStreamAny").AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertEventNew(
                    "s0",
                    @event => {
                        Assert.AreEqual("test", @event.Get("eventConfigId"));
                        Assert.AreEqual(1, @event.Get("intPrimitive"));
                    });

                env.UndeployAll();
            }
        }

        private static void AssertEventTypeDefault(EventType eventType)
        {
            var expected = "theString,boolBoxed,intPrimitive,longPrimitive,doublePrimitive,enumValue".SplitCsv();
            var propertyNames = eventType.PropertyNames;
            EPAssertionUtil.AssertEqualsAnyOrder(expected, propertyNames);
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("theString"));
            Assert.AreEqual(typeof(bool?), eventType.GetPropertyType("boolBoxed"));
            Assert.AreEqual(typeof(int?), eventType.GetPropertyType("intPrimitive"));
            Assert.AreEqual(typeof(long?), eventType.GetPropertyType("longPrimitive"));
            Assert.AreEqual(typeof(double?), eventType.GetPropertyType("doublePrimitive"));
            Assert.AreEqual(typeof(SupportEnum), eventType.GetPropertyType("enumValue"));
            foreach (var expectedProp in expected) {
                Assert.IsNotNull(eventType.GetGetter(expectedProp));
                Assert.IsTrue(eventType.IsProperty(expectedProp));
            }

            SupportEventPropUtil.AssertPropsEquals(
                eventType.PropertyDescriptors.ToArray(),
                new SupportEventPropDesc("theString", typeof(string)),
                new SupportEventPropDesc("boolBoxed", typeof(bool?)),
                new SupportEventPropDesc("intPrimitive", typeof(int?)),
                new SupportEventPropDesc("longPrimitive", typeof(long?)),
                new SupportEventPropDesc("doublePrimitive", typeof(double?)),
                new SupportEventPropDesc("enumValue", typeof(SupportEnum)));
        }

        public static object PreProcessEvent(object o)
        {
            return new SupportBean("E2", 0);
        }
    }
} // end of namespace