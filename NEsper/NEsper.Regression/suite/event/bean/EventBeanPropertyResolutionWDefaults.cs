///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.@event.bean
{
    public class EventBeanPropertyResolutionWDefaults
    {
        public enum GROUP
        {
            FOO,
            BAR
        }

        public enum LocalEventEnum
        {
            NEW
        }

        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLBeanReservedKeywordEscape());
            execs.Add(new EPLBeanWriteOnly());
            execs.Add(new EPLBeanCaseSensitive());
            return execs;
        }

        private static void TryEnumWithKeyword(RegressionEnvironment env)
        {
            env.CompileDeploy("select * from LocalEventWithEnum(LocalEventEnum=LocalEventEnum.`NEW`)");
        }

        private static void TryInvalidControlCharacter(EventBean eventBean)
        {
            try {
                eventBean.Get("a\u008F");
                Assert.Fail();
            }
            catch (PropertyAccessException ex) {
                AssertMessage(ex, "Failed to parse property 'a\u008F': Unexpected token '\u008F'");
            }
        }

        private static void TryEnumItselfReserved(RegressionEnvironment env)
        {
            env.CompileDeploy("select * from LocalEventWithGroup(`GROUP`=`GROUP`.FOO)");
        }

        internal class EPLBeanReservedKeywordEscape : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@Name('s0') select `Seconds`, `Order` from SomeKeywords").AddListener("s0");

                object theEvent = new SupportBeanReservedKeyword(1, 2);
                env.SendEventBean(theEvent, "SomeKeywords");
                var eventBean = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(1, eventBean.Get("Seconds"));
                Assert.AreEqual(2, eventBean.Get("Order"));

                env.UndeployAll();
                env.CompileDeploy("@Name('s0') select * from `Order`").AddListener("s0");

                env.SendEventBean(theEvent, "Order");
                eventBean = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(1, eventBean.Get("Seconds"));
                Assert.AreEqual(2, eventBean.Get("Order"));

                env.UndeployAll();
                env.CompileDeploy("@Name('s0') select Timestamp.`Hour` as val from SomeKeywords").AddListener("s0");

                var bean = new SupportBeanReservedKeyword(1, 2);
                bean.Timestamp = new SupportBeanReservedKeyword.Inner();
                bean.Timestamp.Hour = 10;
                env.SendEventBean(bean, "" + "SomeKeywords" + "");
                eventBean = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(10, eventBean.Get("val"));
                env.UndeployAll();

                // test back-tick with spaces etc
                env.CompileDeploy(
                        "@Name('s0') select `candidate book` as c0, `XML Message Type` as c1, `select` as c2, `children's books`[0] as c3, `my <> map`('xx') as c4 from MyType")
                    .AddListener("s0");

                IDictionary<string, object> defValues = new Dictionary<string, object>();
                defValues.Put("candidate book", "Enders Game");
                defValues.Put("XML Message Type", "book");
                defValues.Put("select", 100);
                defValues.Put("children's books", new[] {50, 51});
                defValues.Put("my <> map", Collections.SingletonDataMap("xx", "abc"));
                env.SendEventMap(defValues, "MyType");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "c0", "c1", "c2", "c3", "c4" },
                    new object[] {"Enders Game", "book", 100, 50, "abc"});
                env.UndeployAll();

                TryInvalidCompile(
                    env,
                    "select `select` from SupportBean",
                    "Failed to validate select-clause expression 'select': Property named 'select' is not valid in any stream [");
                TryInvalidCompile(
                    env,
                    "select `ab cd` from SupportBean",
                    "Failed to validate select-clause expression 'ab cd': Property named 'ab cd' is not valid in any stream [");

                // test resolution as nested property
                var path = new RegressionPath();
                env.CompileDeploy("create schema MyEvent as (customer string, `from` string)", path);
                env.CompileDeploy("insert into DerivedStream select customer,`from` from MyEvent", path);
                env.CompileDeploy("create window TheWindow#firstunique(customer,`from`) as DerivedStream", path);
                env.CompileDeploy(
                    "on pattern [a=TheWindow -> timer:interval(12 hours)] as S0 delete from TheWindow as S1 where S0.a.`from`=S1.`from`",
                    path);

                // test escape in column name
                env.CompileDeploy("@Name('s0') select TheString as `order`, TheString as `price.for.goods` from SupportBean")
                    .AddListener("s0");
                var eventTypeS0 = env.Statement("s0").EventType;
                Assert.AreEqual(typeof(string), eventTypeS0.GetPropertyType("order"));
                Assert.AreEqual("price.for.goods", eventTypeS0.PropertyDescriptors[1].PropertyName);

                env.SendEventBean(new SupportBean("E1", 1));
                var @out = (IDictionary<string, object>) env.Listener("s0").AssertOneGetNew().Underlying;
                Assert.AreEqual("E1", @out.Get("order"));
                Assert.AreEqual("E1", @out.Get("price.for.goods"));

                // try control character
                TryInvalidControlCharacter(env.Listener("s0").AssertOneGetNew());
                // try enum with keyword
                TryEnumWithKeyword(env);

                TryEnumItselfReserved(env);

                env.UndeployAll();
            }
        }

        internal class EPLBeanWriteOnly : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@Name('s0') select * from SupportBeanWriteOnly").AddListener("s0");

                object theEvent = new SupportBeanWriteOnly();
                env.SendEventBean(theEvent);
                var eventBean = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreSame(theEvent, eventBean.Underlying);

                var type = env.Statement("s0").EventType;
                Assert.AreEqual(0, type.PropertyNames.Length);

                env.UndeployAll();
            }
        }

        internal class EPLBeanCaseSensitive : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@Name('s0') select MYPROPERTY, myproperty, myProperty from SupportBeanDupProperty")
                    .AddListener("s0");

                env.SendEventBean(new SupportBeanDupProperty("lowercamel", "uppercamel", "upper", "lower"));
                var result = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual("upper", result.Get("MYPROPERTY"));
                Assert.AreEqual("lower", result.Get("myproperty"));
                Assert.AreEqual("lowercamel", result.Get("myProperty"));

                env.UndeployAll();
                TryInvalidCompile(
                    env,
                    "select MYProperty from SupportBeanDupProperty",
                    "Failed to validate select-clause expression 'MYProperty': Property named 'MYProperty' is not valid in any stream (did you mean 'myproperty'?)");
            }
        }

        [Serializable]
        public class LocalEventWithEnum
        {
            public LocalEventWithEnum(LocalEventEnum localEventEnum)
            {
                LocalEventEnum = localEventEnum;
            }

            public LocalEventEnum LocalEventEnum { get; }
        }

        [Serializable]
        public class LocalEventWithGroup
        {
            public LocalEventWithGroup(GROUP group)
            {
                GROUP = group;
            }

            public GROUP GROUP { get; }
        }
    }
} // end of namespace