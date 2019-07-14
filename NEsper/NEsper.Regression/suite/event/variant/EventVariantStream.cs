///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.@event.variant
{
    public class EventVariantStream
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new EventVariantDynamicMapType());
            execs.Add(new EventVariantNamedWin());
            execs.Add(new EventVariantSingleColumnConversion());
            execs.Add(new EventVariantCoercionBoxedTypeMatch());
            execs.Add(new EventVariantSuperTypesInterfaces());
            execs.Add(new EventVariantPatternSubquery());
            execs.Add(new EventVariantInvalidInsertInto());
            execs.Add(new EventVariantSimple("VarStreamABPredefined"));
            execs.Add(new EventVariantSimple("VarStreamAny"));
            execs.Add(new EventVariantInsertInto());
            execs.Add(new EventVariantMetadata());
            execs.Add(new EventVariantAnyType());
            execs.Add(new EventVariantAnyTypeStaggered());
            execs.Add(new EventVariantInsertWrap());
            execs.Add(new EventVariantSingleStreamWrap());
            execs.Add(new EventVariantWildcardJoin());
            return execs;
        }

        private static void AssertEventTypeDefault(EventType eventType)
        {
            var expected = "theString,boolBoxed,intPrimitive,LongPrimitive,doublePrimitive,enumValue".SplitCsv();
            var propertyNames = eventType.PropertyNames;
            EPAssertionUtil.AssertEqualsAnyOrder(expected, propertyNames);
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("TheString"));
            Assert.AreEqual(typeof(bool?), eventType.GetPropertyType("BoolBoxed"));
            Assert.AreEqual(typeof(int?), eventType.GetPropertyType("IntPrimitive"));
            Assert.AreEqual(typeof(long?), eventType.GetPropertyType("LongPrimitive"));
            Assert.AreEqual(typeof(double?), eventType.GetPropertyType("DoublePrimitive"));
            Assert.AreEqual(typeof(SupportEnum), eventType.GetPropertyType("EnumValue"));
            foreach (var expectedProp in expected) {
                Assert.IsNotNull(eventType.GetGetter(expectedProp));
                Assert.IsTrue(eventType.IsProperty(expectedProp));
            }

            EPAssertionUtil.AssertEqualsAnyOrder(
                new object[] {
                    new EventPropertyDescriptor(
                        "TheString", 
                        typeof(string), 
                        null, 
                        false, 
                        false, 
                        false, 
                        false, 
                        false),
                    new EventPropertyDescriptor(
                        "BoolBoxed", 
                        typeof(bool?),
                        null, 
                        false, 
                        false, 
                        false, 
                        false, 
                        false),
                    new EventPropertyDescriptor(
                        "IntPrimitive", 
                        typeof(int?),
                        null, 
                        false, 
                        false, 
                        false, 
                        false, 
                        false),
                    new EventPropertyDescriptor(
                        "LongPrimitive",
                        typeof(long?),
                        null,
                        false,
                        false,
                        false,
                        false,
                        false),
                    new EventPropertyDescriptor(
                        "DoublePrimitive",
                        typeof(double?),
                        null,
                        false,
                        false,
                        false,
                        false,
                        false),
                    new EventPropertyDescriptor(
                        "EnumValue",
                        typeof(SupportEnum),
                        null,
                        false,
                        false,
                        false,
                        false,
                        false)
                },
                eventType.PropertyDescriptors);
        }

        public static object PreProcessEvent(object o)
        {
            return new SupportBean("E2", 0);
        }

        internal class EventVariantWildcardJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create variant schema MyVariantWJ as *;\n" +
                          "insert into MyVariantWJ select * from SupportBean sb unidirectional, SupportBean_S0#keepall as s0;\n" +
                          "@Name('s0') select * from MyVariantWJ";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(10));
                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new[] {"sb.TheString", "s0.id"},
                    new object[] {"E1", 10});

                env.UndeployAll();
            }
        }

        internal class EventVariantSingleStreamWrap : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create variant schema MyVariantSSW as *;\n" +
                          "insert into OneStream select *, 'a' as field from SupportBean;\n" +
                          "insert into MyVariantSSW select * from OneStream;\n" +
                          "@Name('s0') select * from MyVariantSSW";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new[] {"TheString", "field"},
                    new object[] {"E1", "a"});

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
                string[] fields = {"id"};

                env.Milestone(0);

                var epl = "@Name('s0') select irstream id? as id from " + variantStreamName + "#length(2)";
                env.CompileDeploy(epl).AddListener("s0");

                env.CompileDeploy("insert into " + variantStreamName + " select * from SupportBean_A");
                env.CompileDeploy("insert into " + variantStreamName + " select * from SupportBean_B");

                env.Milestone(1);

                env.SendEventBean(new SupportBean_A("E1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1"});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1"}});

                env.Milestone(2);

                env.SendEventBean(new SupportBean_B("E2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2"});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}});

                env.Milestone(3);

                env.SendEventBean(new SupportBean_B("E3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {"E3"});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {"E1"});
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E2"}, new object[] {"E3"}});

                env.UndeployAll();
            }
        }

        internal class EventVariantDynamicMapType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("insert into MyVariantTwoTyped select * from MyEvent");
                env.CompileDeploy("insert into MyVariantTwoTyped select * from MySecondEvent");
                env.CompileDeploy("@Name('s0') select * from MyVariantTwoTyped").AddListener("s0");

                env.SendEventMap(new Dictionary<string, object>(), "MyEvent");
                Assert.IsNotNull(env.Listener("s0").AssertOneGetNewAndReset());

                env.SendEventMap(new Dictionary<string, object>(), "MySecondEvent");
                Assert.IsNotNull(env.Listener("s0").AssertOneGetNewAndReset());

                env.UndeployAll();
            }
        }

        internal class EventVariantSingleColumnConversion : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("insert into MyVariantTwoTypedSBVariant select * from SupportBean");
                env.CompileDeploy("create window MainEventWindow#length(10000) as MyVariantTwoTypedSBVariant", path);
                env.CompileDeploy(
                    "insert into MainEventWindow select " +
                    typeof(EventVariantStream).Name +
                    ".preProcessEvent(event) from MyVariantTwoTypedSBVariant as event",
                    path);
                env.CompileDeploy("@Name('s0') select * from MainEventWindow where theString = 'E'", path);
                env.Statement("s0").AddListenerWithReplay(env.ListenerNew());

                env.SendEventBean(new SupportBean("E1", 1));

                env.UndeployAll();
            }
        }

        internal class EventVariantCoercionBoxedTypeMatch : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string fields;
                SupportBean bean;

                env.CompileDeploy("@Name('s0') select * from MyVariantTwoTypedSB").AddListener("s0");
                var typeSelectAll = env.Statement("s0").EventType;
                AssertEventTypeDefault(typeSelectAll);
                Assert.AreEqual(typeof(object), env.Statement("s0").EventType.UnderlyingType);

                env.CompileDeploy("insert into MyVariantTwoTypedSB select * from SupportBean");
                env.CompileDeploy("insert into MyVariantTwoTypedSB select * from SupportBeanVariantStream");

                // try wildcard
                object eventOne = new SupportBean("E0", -1);
                env.SendEventBean(eventOne);
                Assert.AreSame(eventOne, env.Listener("s0").AssertOneGetNewAndReset().Underlying);

                object eventTwo = new SupportBeanVariantStream("E1");
                env.SendEventBean(eventTwo);
                Assert.AreSame(eventTwo, env.Listener("s0").AssertOneGetNewAndReset().Underlying);

                env.UndeployModuleContaining("s0");

                fields = "theString,boolBoxed,intPrimitive,LongPrimitive,doublePrimitive,enumValue";
                env.CompileDeploy("@Name('s0') select " + fields + " from MyVariantTwoTypedSB").AddListener("s0");
                AssertEventTypeDefault(env.Statement("s0").EventType);

                // coerces to the higher resolution type, accepts boxed versus not boxed
                env.SendEventBean(new SupportBeanVariantStream("s1", true, 1, 20, 30, SupportEnum.ENUM_VALUE_1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields.SplitCsv(),
                    new object[] {"s1", true, 1, 20L, 30d, SupportEnum.ENUM_VALUE_1});

                bean = new SupportBean("s2", 99);
                bean.LongPrimitive = 33;
                bean.DoublePrimitive = 50;
                bean.EnumValue = SupportEnum.ENUM_VALUE_3;
                env.SendEventBean(bean);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields.SplitCsv(),
                    new object[] {"s2", null, 99, 33L, 50d, SupportEnum.ENUM_VALUE_3});
                env.UndeployModuleContaining("s0");

                // make sure a property is not known since the property is not found on SupportBeanVariantStream
                TryInvalidCompile(
                    env,
                    "select charBoxed from MyVariantTwoTypedSB",
                    "Failed to validate select-clause expression 'charBoxed': Property named 'charBoxed' is not valid in any stream");

                // try dynamic property: should exists but not show up as a declared property
                fields = "v1,v2,v3";
                env.CompileDeploy(
                        "@Name('s0') select longBoxed? as v1,charBoxed? as v2,doubleBoxed? as v3 from MyVariantTwoTypedSB")
                    .AddListener("s0");

                bean = new SupportBean();
                bean.LongBoxed = 33L;
                bean.CharBoxed = 'a';
                bean.DoubleBoxed = double.NaN;
                env.SendEventBean(bean);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields.SplitCsv(),
                    new object[] {33L, 'a', double.NaN});

                env.SendEventBean(new SupportBeanVariantStream("s2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields.SplitCsv(),
                    new object[] {null, null, null});

                env.UndeployAll();
            }
        }

        internal class EventVariantSuperTypesInterfaces : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("insert into MyVariantStreamTwo select * from SupportBeanVariantOne");
                env.CompileDeploy("insert into MyVariantStreamTwo select * from SupportBeanVariantTwo");

                env.CompileDeploy("@Name('s0') select * from MyVariantStreamTwo").AddListener("s0");
                var eventType = env.Statement("s0").EventType;

                var expected = "p0,p1,p2,p3,p4,p5,indexed,mapped,inneritem".SplitCsv();
                var propertyNames = eventType.PropertyNames;
                EPAssertionUtil.AssertEqualsAnyOrder(expected, propertyNames);
                Assert.AreEqual(typeof(ISupportBaseAB), eventType.GetPropertyType("p0"));
                Assert.AreEqual(typeof(ISupportAImplSuperG), eventType.GetPropertyType("p1"));
                Assert.AreEqual(typeof(LinkedList<object>), eventType.GetPropertyType("p2"));
                Assert.AreEqual(typeof(IList<object>), eventType.GetPropertyType("p3"));
                Assert.AreEqual(typeof(ICollection<object>), eventType.GetPropertyType("p4"));
                Assert.AreEqual(typeof(ICollection<object>), eventType.GetPropertyType("p5"));
                Assert.AreEqual(typeof(int[]), eventType.GetPropertyType("indexed"));
                Assert.AreEqual(typeof(IDictionary<string, object>), eventType.GetPropertyType("mapped"));
                Assert.AreEqual(
                    typeof(SupportBeanVariantOne.SupportBeanVariantOneInner),
                    eventType.GetPropertyType("inneritem"));

                env.UndeployModuleContaining("s0");

                env.CompileDeploy(
                    "@Name('s0') select p0,p1,p2,p3,p4,p5,indexed[0] as p6,indexArr[1] as p7,mappedKey('a') as p8,inneritem as p9,inneritem.val as p10 from MyVariantStreamTwo");
                env.AddListener("s0");
                eventType = env.Statement("s0").EventType;
                Assert.AreEqual(typeof(int?), eventType.GetPropertyType("p6"));
                Assert.AreEqual(typeof(int?), eventType.GetPropertyType("p7"));
                Assert.AreEqual(typeof(string), eventType.GetPropertyType("p8"));
                Assert.AreEqual(
                    typeof(SupportBeanVariantOne.SupportBeanVariantOneInner),
                    eventType.GetPropertyType("p9"));
                Assert.AreEqual(typeof(string), eventType.GetPropertyType("p10"));

                var ev1 = new SupportBeanVariantOne();
                env.SendEventBean(ev1);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "p6,p7,p8,p9,p10".SplitCsv(),
                    new object[] {1, 2, "val1", ev1.Inneritem, ev1.Inneritem.Val});

                var ev2 = new SupportBeanVariantTwo();
                env.SendEventBean(ev2);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "p6,p7,p8,p9,p10".SplitCsv(),
                    new object[] {10, 20, "val2", ev2.Inneritem, ev2.Inneritem.Val});

                env.UndeployAll();
            }
        }

        internal class EventVariantNamedWin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test named window
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@Name('window') create window MyVariantWindow#unique(TheString) as select * from MyVariantTwoTypedSB",
                    path);
                env.AddListener("window");
                env.CompileDeploy("insert into MyVariantWindow select * from MyVariantTwoTypedSB", path);
                env.CompileDeploy("insert into MyVariantTwoTypedSB select * from SupportBeanVariantStream", path);
                env.CompileDeploy("insert into MyVariantTwoTypedSB select * from SupportBean", path);

                object eventOne = new SupportBean("E1", -1);
                env.SendEventBean(eventOne);
                Assert.AreSame(eventOne, env.Listener("window").AssertOneGetNewAndReset().Underlying);

                env.Milestone(0);

                object eventTwo = new SupportBeanVariantStream("E2");
                env.SendEventBean(eventTwo);
                Assert.AreSame(eventTwo, env.Listener("window").AssertOneGetNewAndReset().Underlying);

                env.Milestone(1);

                object eventThree = new SupportBean("E2", -1);
                env.SendEventBean(eventThree);
                Assert.AreEqual(eventThree, env.Listener("window").LastNewData[0].Underlying);
                Assert.AreEqual(eventTwo, env.Listener("window").LastOldData[0].Underlying);
                env.Listener("window").Reset();

                env.Milestone(2);

                object eventFour = new SupportBeanVariantStream("E1");
                env.SendEventBean(eventFour);
                Assert.AreEqual(eventFour, env.Listener("window").LastNewData[0].Underlying);
                Assert.AreEqual(eventOne, env.Listener("window").LastOldData[0].Underlying);
                env.Listener("window").Reset();

                env.UndeployAll();
            }
        }

        internal class EventVariantPatternSubquery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("insert into MyVariantStreamFour select * from SupportBeanVariantStream");
                env.CompileDeploy("insert into MyVariantStreamFour select * from SupportBean");

                // test pattern
                env.CompileDeploy("@Name('s0') select * from pattern [a=MyVariantStreamFour => b=MyVariantStreamFour]");
                env.AddListener("s0");
                object[] events = {new SupportBean("E1", -1), new SupportBeanVariantStream("E2")};
                env.SendEventBean(events[0]);
                env.SendEventBean(events[1]);
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), "a,b".SplitCsv(), events);
                env.UndeployModuleContaining("s0");

                // test subquery
                env.CompileDeploy(
                    "@Name('s0') select * from SupportBean_A as a where exists(select * from MyVariantStreamFour#lastevent as b where b.TheString=a.id)");
                env.AddListener("s0");
                events = new object[]
                    {new SupportBean("E1", -1), new SupportBeanVariantStream("E2"), new SupportBean_A("E2")};

                env.SendEventBean(events[0]);
                env.SendEventBean(events[2]);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(events[1]);
                env.SendEventBean(events[2]);
                Assert.IsTrue(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class EventVariantInvalidInsertInto : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryInvalidCompile(
                    env,
                    "insert into MyVariantStreamFive select * from SupportBean_A",
                    "Selected event type is not a valid event type of the variant stream 'MyVariantStreamFive'");

                TryInvalidCompile(
                    env,
                    "insert into MyVariantStreamFive select IntPrimitive as k0 from SupportBean",
                    "Selected event type is not a valid event type of the variant stream 'MyVariantStreamFive' ");
            }
        }

        internal class EventVariantInsertInto : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("insert into MyStream select TheString, IntPrimitive from SupportBean", path);
                env.CompileDeploy("insert into VarStreamAny select TheString as abc from MyStream", path);
                env.CompileDeploy("@Name('Target') select * from VarStreamAny#keepall()", path);

                env.SendEventBean(new SupportBean("E1", 1));

                env.Milestone(0);

                var arr = EPAssertionUtil.EnumeratorToArray(env.GetEnumerator("Target"));
                EPAssertionUtil.AssertPropsPerRow(
                    arr,
                    new[] {"abc"},
                    new[] {new object[] {"E1"}});

                env.UndeployAll();
            }
        }

        internal class EventVariantMetadata : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@Name('s0') select * from VarStreamAny");
                var type = env.Statement("s0").EventType;
                env.UndeployAll();

                // assert type metadata
                Assert.AreEqual(EventTypeApplicationType.VARIANT, type.Metadata.ApplicationType);
                Assert.AreEqual("VarStreamAny", type.Metadata.Name);
                Assert.AreEqual(EventTypeTypeClass.VARIANT, type.Metadata.TypeClass);
            }
        }

        internal class EventVariantAnyType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("insert into VarStreamAny select * from SupportBean");
                env.CompileDeploy("insert into VarStreamAny select * from SupportBeanVariantStream");
                env.CompileDeploy("insert into VarStreamAny select * from SupportBean_A");
                env.CompileDeploy(
                    "insert into VarStreamAny select symbol as theString, volume as IntPrimitive, feed as id from SupportMarketDataBean");
                env.CompileDeploy("@Name('s0') select * from VarStreamAny").AddListener("s0");
                Assert.AreEqual(0, env.Statement("s0").EventType.PropertyNames.Length);

                object eventOne = new SupportBean("E0", -1);
                env.SendEventBean(eventOne);
                Assert.AreSame(eventOne, env.Listener("s0").AssertOneGetNewAndReset().Underlying);

                object eventTwo = new SupportBean_A("E1");
                env.SendEventBean(eventTwo);
                Assert.AreSame(eventTwo, env.Listener("s0").AssertOneGetNewAndReset().Underlying);

                env.UndeployModuleContaining("s0");

                env.CompileDeploy("@Name('s0') select TheString,id,intPrimitive from VarStreamAny").AddListener("s0");

                var eventType = env.Statement("s0").EventType;
                Assert.AreEqual(typeof(object), eventType.GetPropertyType("TheString"));
                Assert.AreEqual(typeof(object), eventType.GetPropertyType("id"));
                Assert.AreEqual(typeof(object), eventType.GetPropertyType("IntPrimitive"));

                var fields = "theString,id,intPrimitive".SplitCsv();
                env.SendEventBean(new SupportBeanVariantStream("E1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", null, null});

                env.SendEventBean(new SupportBean("E2", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", null, 10});

                env.SendEventBean(new SupportBean_A("E3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, "E3", null});

                env.SendEventBean(new SupportMarketDataBean("s1", 100, 1000L, "f1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"s1", "f1", 1000L});

                env.UndeployAll();
            }
        }

        internal class EventVariantAnyTypeStaggered : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("insert into MyStream select TheString, IntPrimitive from SupportBean", path);
                env.CompileDeploy("insert into VarStreamMD select TheString as abc from MyStream", path);
                env.CompileDeploy("@Name('Target') select * from VarStreamMD#keepall", path);

                env.SendEventBean(new SupportBean("E1", 1));

                var arr = EPAssertionUtil.EnumeratorToArray(env.GetEnumerator("Target"));
                EPAssertionUtil.AssertPropsPerRow(
                    arr,
                    new[] {"abc"},
                    new[] {new object[] {"E1"}});

                env.CompileDeploy("insert into MyStream2 select feed from SupportMarketDataBean", path);
                env.CompileDeploy("insert into VarStreamMD select feed as abc from MyStream2", path);

                env.SendEventBean(new SupportMarketDataBean("IBM", 1, 1L, "E2"));

                arr = EPAssertionUtil.EnumeratorToArray(env.GetEnumerator("Target"));
                EPAssertionUtil.AssertPropsPerRow(
                    arr,
                    new[] {"abc"},
                    new[] {new object[] {"E1"}, new object[] {"E2"}});

                env.UndeployAll();
            }
        }

        internal class EventVariantInsertWrap : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Test inserting a wrapper of underlying plus properties
                env.CompileDeploy("insert into VarStreamAny select 'test' as eventConfigId, * from SupportBean");
                env.CompileDeploy("@Name('s0') select * from VarStreamAny").AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                var @event = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual("test", @event.Get("eventConfigId"));
                Assert.AreEqual(1, @event.Get("IntPrimitive"));

                env.UndeployAll();
            }
        }
    }
} // end of namespace