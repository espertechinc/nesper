///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.@event.bean
{
    public class EventBeanPropertyResolutionWDefaults
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithReservedKeywordEscape(execs);
            WithWriteOnly(execs);
            WithCaseSensitive(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithCaseSensitive(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLBeanCaseSensitive());
            return execs;
        }

        public static IList<RegressionExecution> WithWriteOnly(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLBeanWriteOnly());
            return execs;
        }

        public static IList<RegressionExecution> WithReservedKeywordEscape(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLBeanReservedKeywordEscape());
            return execs;
        }

        private class EPLBeanReservedKeywordEscape : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@name('s0') select `Seconds`, `Order` from SomeKeywords").AddListener("s0");

                object theEvent = new SupportBeanReservedKeyword(1, 2);
                env.SendEventBean(theEvent, "SomeKeywords");
                env.AssertEventNew(
                    "s0",
                    eventBean => {
                        ClassicAssert.AreEqual(1, eventBean.Get("Seconds"));
                        ClassicAssert.AreEqual(2, eventBean.Get("Order"));
                    });

                env.UndeployAll();
                env.CompileDeploy("@name('s0') select * from `Order`").AddListener("s0");

                env.SendEventBean(theEvent, "Order");
                env.AssertEventNew(
                    "s0",
                    eventBean => {
                        ClassicAssert.AreEqual(1, eventBean.Get("Seconds"));
                        ClassicAssert.AreEqual(2, eventBean.Get("Order"));
                    });

                env.UndeployAll();
                env.CompileDeploy("@name('s0') select Timestamp.`Hour` as val from SomeKeywords").AddListener("s0");

                var bean = new SupportBeanReservedKeyword(1, 2);
                bean.Timestamp = new SupportBeanReservedKeyword.Inner();
                bean.Timestamp.Hour = 10;
                env.SendEventBean(bean, "SomeKeywords");
                env.AssertEqualsNew("s0", "val", 10);
                env.UndeployAll();

                // test back-tick with spaces etc
                env.CompileDeploy(
                        "@name('s0') select `candidate book` as c0, `XML Message Type` as c1, `select` as c2, `children's books`[0] as c3, `my <> map`('xx') as c4 from MyType")
                    .AddListener("s0");

                IDictionary<string, object> defValues = new Dictionary<string, object>();
                defValues.Put("candidate book", "Enders Game");
                defValues.Put("XML Message Type", "book");
                defValues.Put("select", 100);
                defValues.Put("children's books", new int[] { 50, 51 });
                defValues.Put("my <> map", Collections.SingletonDataMap("xx", "abc"));
                env.SendEventMap(defValues, "MyType");
                env.AssertPropsNew(
                    "s0",
                    "c0,c1,c2,c3,c4".SplitCsv(),
                    new object[] { "Enders Game", "book", 100, 50, "abc" });
                env.UndeployAll();

                env.TryInvalidCompile(
                    "select `select` from SupportBean",
                    "Failed to validate select-clause expression 'select': Property named 'select' is not valid in any stream [");
                env.TryInvalidCompile(
                    "select `ab cd` from SupportBean",
                    "Failed to validate select-clause expression 'ab cd': Property named 'ab cd' is not valid in any stream [");

                // test resolution as nested property
                var path = new RegressionPath();
                env.CompileDeploy("@public create schema MyEvent as (customer string, `from` string)", path);
                env.CompileDeploy("@public insert into DerivedStream select customer,`from` from MyEvent", path);
                env.CompileDeploy(
                    "@public create window TheWindow#firstunique(customer,`from`) as DerivedStream",
                    path);
                env.CompileDeploy(
                    "on pattern [a=TheWindow -> timer:interval(12 hours)] as s0 delete from TheWindow as s1 where s0.a.`from`=s1.`from`",
                    path);

                // test escape in column name
                env.CompileDeploy(
                        "@name('s0') select TheString as `order`, TheString as `price.for.goods` from SupportBean")
                    .AddListener("s0");
                env.AssertStatement(
                    "s0",
                    statement => {
                        ClassicAssert.AreEqual(typeof(string), statement.EventType.GetPropertyType("order"));
                        ClassicAssert.AreEqual("price.for.goods", statement.EventType.PropertyDescriptors[1].PropertyName);
                    });

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertEventNew(
                    "s0",
                    eventBean => {
                        var @out = (IDictionary<string, object>)eventBean.Underlying;
                        ClassicAssert.AreEqual("E1", @out.Get("order"));
                        ClassicAssert.AreEqual("E1", @out.Get("price.for.goods"));

                        // try control character
                        TryInvalidControlCharacter(eventBean);
                    });

                // try enum with keyword
                TryEnumWithKeyword(env);

                TryEnumItselfReserved(env);

                env.UndeployAll();
            }
        }

        private class EPLBeanWriteOnly : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@name('s0') select * from SupportBeanWriteOnly").AddListener("s0");

                object theEvent = new SupportBeanWriteOnly();
                env.SendEventBean(theEvent);
                env.AssertEventNew("s0", eventBean => ClassicAssert.AreSame(theEvent, eventBean.Underlying));

                env.AssertStatement(
                    "s0",
                    statement => {
                        var type = statement.EventType;
                        ClassicAssert.AreEqual(0, type.PropertyNames.Length);
                    });

                env.UndeployAll();
            }
        }

        private class EPLBeanCaseSensitive : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@name('s0') select MYPROPERTY, myproperty, myProperty from SupportBeanDupProperty")
                    .AddListener("s0");

                env.SendEventBean(new SupportBeanDupProperty("lowercamel", "uppercamel", "upper", "lower"));
                env.AssertEventNew(
                    "s0",
                    result => {
                        ClassicAssert.AreEqual("upper", result.Get("MYPROPERTY"));
                        ClassicAssert.AreEqual("lower", result.Get("myproperty"));
                        ClassicAssert.AreEqual("lowercamel", result.Get("myProperty"));
                    });

                env.UndeployAll();
                env.TryInvalidCompile(
                    "select MYProperty from SupportBeanDupProperty",
                    "Failed to validate select-clause expression 'MYProperty': Property named 'MYProperty' is not valid in any stream (did you mean 'myproperty'?)");
            }
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
                SupportMessageAssertUtil.AssertMessage(
                    ex,
                    "Property named 'a\u008F' is not a valid property name for this type");
            }
        }

        private static void TryEnumItselfReserved(RegressionEnvironment env)
        {
            env.CompileDeploy("select * from LocalEventWithGroup(`GROUP`=`GROUP`.FOO)");
        }

        public class LocalEventWithEnum
        {
            public LocalEventWithEnum(LocalEventEnum localEventEnum)
            {
                this.LocalEventEnum = localEventEnum;
            }

            public LocalEventEnum LocalEventEnum { get; }
        }

        public enum LocalEventEnum
        {
            NEW
        }

        public class LocalEventWithGroup
        {
            public LocalEventWithGroup(GROUP group)
            {
                this.GROUP = group;
            }

            public GROUP GROUP { get; }
        }

        public enum GROUP
        {
            FOO,
            BAR
        }
    }
} // end of namespace