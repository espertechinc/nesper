///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;
using SupportBeanSimple = com.espertech.esper.regressionlib.support.bean.SupportBeanSimple;

//using SupportBean_A = com.espertech.esper.common.@internal.support.SupportBean_A;
//using SupportBeanSimple = com.espertech.esper.common.@internal.support.SupportBeanSimple;

namespace com.espertech.esper.regressionlib.suite.epl.other
{
    public class EPLOtherSelectWildcardWAdditional
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
#if REGRESSION_EXECUTIONS
            WithSingleOM(execs);
            WithSingle(execs);
            WithSingleInsertInto(execs);
            WithJoinInsertInto(execs);
            WithJoinNoCommonProperties(execs);
            WithJoinCommonProperties(execs);
            WithCombinedProperties(execs);
            WithWildcardMapEvent(execs);
            With(InvalidRepeatedProperties)(execs);
#endif
            return execs;
        }

        public static IList<RegressionExecution> WithInvalidRepeatedProperties(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherInvalidRepeatedProperties());
            return execs;
        }

        public static IList<RegressionExecution> WithWildcardMapEvent(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherWildcardMapEvent());
            return execs;
        }

        public static IList<RegressionExecution> WithCombinedProperties(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherCombinedProperties());
            return execs;
        }

        public static IList<RegressionExecution> WithJoinCommonProperties(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherJoinCommonProperties());
            return execs;
        }

        public static IList<RegressionExecution> WithJoinNoCommonProperties(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherJoinNoCommonProperties());
            return execs;
        }

        public static IList<RegressionExecution> WithJoinInsertInto(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherJoinInsertInto());
            return execs;
        }

        public static IList<RegressionExecution> WithSingleInsertInto(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherSingleInsertInto());
            return execs;
        }

        public static IList<RegressionExecution> WithSingle(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherSingle());
            return execs;
        }

        public static IList<RegressionExecution> WithSingleOM(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherSingleOM());
            return execs;
        }

        private class EPLOtherSingleOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.CreateWildcard()
                    .Add(Expressions.Concat("MyString", "MyString"), "concat");
                model.FromClause = FromClause.Create(
                    FilterStream.Create("SupportBeanSimple").AddView(View.Create("length", Expressions.Constant(5))));
                model = env.CopyMayFail(model);

                var text = "select *, MyString||MyString as concat from SupportBeanSimple#length(5)";
                Assert.AreEqual(text, model.ToEPL());
                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0");

                AssertSimple(env);

                env.AssertStatement(
                    "s0",
                    statement => SupportEventPropUtil.AssertPropsEquals(
                        statement.EventType.PropertyDescriptors.ToArray(),
                        new SupportEventPropDesc("MyString", typeof(string)),
                        new SupportEventPropDesc("MyInt", typeof(int)),
                        new SupportEventPropDesc("concat", typeof(string))));

                env.UndeployAll();
            }
        }

        private class EPLOtherSingle : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@name('s0') select *, MyString||MyString as concat from SupportBeanSimple#length(5)";
                env.CompileDeploy(text).AddListener("s0");
                AssertSimple(env);
                env.UndeployAll();
            }
        }

        private class EPLOtherSingleInsertInto : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var text =
                    "@name('insert') @public insert into SomeEvent select *, MyString||MyString as concat from SupportBeanSimple#length(5)";
                env.CompileDeploy(text, path).AddListener("insert");

                var textTwo = "@name('s0') select * from SomeEvent#length(5)";
                env.CompileDeploy(textTwo, path).AddListener("s0");
                AssertSimple(env);
                AssertProperties(env, "insert", Collections.EmptyDataMap);

                env.UndeployAll();
            }
        }

        private class EPLOtherJoinInsertInto : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var text = "@name('insert') @public insert into SomeJoinEvent select *, MyString||MyString as concat " +
                           "from SupportBeanSimple#length(5) as eventOne, SupportMarketDataBean#length(5) as eventTwo";
                env.CompileDeploy(text, path).AddListener("insert");

                var textTwo = "@name('s0') select * from SomeJoinEvent#length(5)";
                env.CompileDeploy(textTwo, path).AddListener("s0");

                AssertNoCommonProperties(env);
                AssertProperties(env, "insert", Collections.EmptyDataMap);

                env.UndeployAll();
            }
        }

        private class EPLOtherJoinNoCommonProperties : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eventNameOne = nameof(SupportBeanSimple);
                var eventNameTwo = nameof(SupportMarketDataBean);
                var text = "@name('s0') select *, MyString||MyString as concat " +
                           "from " +
                           eventNameOne +
                           "#length(5) as eventOne, " +
                           eventNameTwo +
                           "#length(5) as eventTwo";
                env.CompileDeploy(text).AddListener("s0");

                AssertNoCommonProperties(env);

                env.UndeployAll();

                text = "@name('s0') select *, MyString||MyString as concat " +
                       "from " +
                       eventNameOne +
                       "#length(5) as eventOne, " +
                       eventNameTwo +
                       "#length(5) as eventTwo " +
                       "where eventOne.MyString = eventTwo.Symbol";
                env.CompileDeploy(text).AddListener("s0");

                AssertNoCommonProperties(env);

                env.UndeployAll();
            }
        }

        private class EPLOtherJoinCommonProperties : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eventNameOne = nameof(SupportBean_A);
                var eventNameTwo = nameof(SupportBean_B);
                var text = "@name('s0') select *, eventOne.Id||eventTwo.Id as concat " +
                           "from " +
                           eventNameOne +
                           "#length(5) as eventOne, " +
                           eventNameTwo +
                           "#length(5) as eventTwo ";
                env.CompileDeploy(text).AddListener("s0");

                AssertCommonProperties(env);

                env.UndeployAll();

                text = "@name('s0') select *, eventOne.Id||eventTwo.Id as concat " +
                       "from " +
                       eventNameOne +
                       "#length(5) as eventOne, " +
                       eventNameTwo +
                       "#length(5) as eventTwo " +
                       "where eventOne.Id = eventTwo.Id";
                env.CompileDeploy(text).AddListener("s0");

                AssertCommonProperties(env);

                env.UndeployAll();
            }
        }

        private class EPLOtherCombinedProperties : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text =
                    "@name('s0') select *, indexed[0].mapped('0ma').Value||indexed[0].mapped('0mb').Value as concat from SupportBeanCombinedProps#length(5)";
                env.CompileDeploy(text).AddListener("s0");
                AssertCombinedProps(env);
                env.UndeployAll();
            }
        }

        private class EPLOtherWildcardMapEvent : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@name('s0') select *, TheString||TheString as concat from MyMapEventIntString#length(5)";
                env.CompileDeploy(text).AddListener("s0");

                // The map to send into the eventService
                IDictionary<string, object> props = new Dictionary<string, object>();
                props.Put("int", 1);
                props.Put("TheString", "xx");
                env.SendEventMap(props, "MyMapEventIntString");

                // The map of expected results
                IDictionary<string, object> properties = new Dictionary<string, object>();
                properties.Put("int", 1);
                properties.Put("TheString", "xx");
                properties.Put("concat", "xxxx");

                AssertProperties(env, "s0", properties);

                env.UndeployAll();
            }
        }

        private class EPLOtherInvalidRepeatedProperties : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "select *, MyString||MyString as MyString from SupportBeanSimple#length(5)";
                env.TryInvalidCompile(text, "skip");
            }
        }

        private static void AssertNoCommonProperties(RegressionEnvironment env)
        {
            var eventSimple = SendSimpleEvent(env, "string");
            var eventMarket = SendMarketEvent(env, "string");

            env.AssertListener(
                "s0",
                listener => {
                    var theEvent = listener.LastNewData[0];
                    IDictionary<string, object> properties = new Dictionary<string, object>();
                    properties.Put("concat", "stringstring");
                    AssertProperties(env, "s0", properties);
                    Assert.AreSame(eventSimple, theEvent.Get("eventOne"));
                    Assert.AreSame(eventMarket, theEvent.Get("eventTwo"));
                });
        }

        private static void AssertSimple(RegressionEnvironment env)
        {
            var theEvent = SendSimpleEvent(env, "string");

            env.AssertListener(
                "s0",
                listener => {
                    Assert.AreEqual("stringstring", listener.LastNewData[0].Get("concat"));
                    IDictionary<string, object> properties = new Dictionary<string, object>();
                    properties.Put("concat", "stringstring");
                    properties.Put("MyString", "string");
                    properties.Put("MyInt", 0);
                    AssertProperties(env, "s0", properties);

                    Assert.AreEqual(
                        typeof(Pair<object, IDictionary<string, object>>),
                        listener.LastNewData[0].EventType.UnderlyingType);
                    Assert.IsTrue(listener.LastNewData[0].Underlying is Pair<object, IDictionary<string, object>>);
                    var pair = (Pair<object, IDictionary<string, object>>)listener.LastNewData[0].Underlying;
                    Assert.AreEqual(theEvent, pair.First);
                    Assert.AreEqual("stringstring", pair.Second.Get("concat"));
                });
        }

        private static void AssertCommonProperties(RegressionEnvironment env)
        {
            SendABEvents(env, "string");
            env.AssertListener(
                "s0",
                listener => {
                    var theEvent = listener.LastNewData[0];
                    IDictionary<string, object> properties = new Dictionary<string, object>();
                    properties.Put("concat", "stringstring");
                    AssertProperties(env, "s0", properties);
                    Assert.IsNotNull(theEvent.Get("eventOne"));
                    Assert.IsNotNull(theEvent.Get("eventTwo"));
                });
        }

        private static void AssertCombinedProps(RegressionEnvironment env)
        {
            SendCombinedProps(env);
            env.AssertListener(
                "s0",
                listener => {
                    var eventBean = listener.LastNewData[0];

                    Assert.AreEqual("0ma0", eventBean.Get("indexed[0].mapped('0ma').Value"));
                    Assert.AreEqual("0ma1", eventBean.Get("indexed[0].mapped('0mb').Value"));
                    Assert.AreEqual("1ma0", eventBean.Get("indexed[1].mapped('1ma').Value"));
                    Assert.AreEqual("1ma1", eventBean.Get("indexed[1].mapped('1mb').Value"));

                    Assert.AreEqual("0ma0", eventBean.Get("array[0].mapped('0ma').Value"));
                    Assert.AreEqual("1ma1", eventBean.Get("array[1].mapped('1mb').Value"));

                    Assert.AreEqual("0ma00ma1", eventBean.Get("concat"));
                });
        }

        private static void AssertProperties(
            RegressionEnvironment env,
            string statementName,
            IDictionary<string, object> properties)
        {
            env.AssertListener(
                statementName,
                listener => {
                    var theEvent = listener.LastNewData[0];
                    foreach (var property in properties.Keys) {
                        Assert.AreEqual(properties.Get(property), theEvent.Get(property));
                    }
                });
        }

        private static SupportBeanSimple SendSimpleEvent(
            RegressionEnvironment env,
            string s)
        {
            var bean = new SupportBeanSimple(s, 0);
            env.SendEventBean(bean);
            return bean;
        }

        private static SupportMarketDataBean SendMarketEvent(
            RegressionEnvironment env,
            string symbol)
        {
            var bean = new SupportMarketDataBean(symbol, 0.0, 0L, null);
            env.SendEventBean(bean);
            return bean;
        }

        private static void SendABEvents(
            RegressionEnvironment env,
            string id)
        {
            var beanOne = new SupportBean_A(id);
            var beanTwo = new SupportBean_B(id);
            env.SendEventBean(beanOne);
            env.SendEventBean(beanTwo);
        }

        private static void SendCombinedProps(RegressionEnvironment env)
        {
            env.SendEventBean(SupportBeanCombinedProps.MakeDefaultBean());
        }
    }
} // end of namespace