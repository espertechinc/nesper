///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.insertinto
{
    /// <summary>
    ///     Test for populating an empty type:
    ///     - an empty insert-into property list is allowed, i.e. "insert into EmptySchema()"
    ///     - an empty select-clause is not allowed, i.e. "select from xxx" fails
    ///     - we require "select null from" (unnamed null column) for populating an empty type
    /// </summary>
    public class EPLInsertIntoEmptyPropType
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithNamedWindowModelAfter(execs);
            WithCreateSchemaInsertInto(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithCreateSchemaInsertInto(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoCreateSchemaInsertInto());
            return execs;
        }

        public static IList<RegressionExecution> WithNamedWindowModelAfter(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoNamedWindowModelAfter());
            return execs;
        }

        private static void TryAssertionInsertBean(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeploy("create schema MyBeanWithoutProps as " + typeof(SupportBeanWithoutProps).MaskTypeName(), path);
            env.CompileDeploy("insert into MyBeanWithoutProps select null from SupportBean", path);
            env.CompileDeploy("@Name('s0') select * from MyBeanWithoutProps", path).AddListener("s0");

            env.SendEventBean(new SupportBean());
            Assert.IsTrue(env.Listener("s0").AssertOneGetNewAndReset().Underlying is SupportBeanWithoutProps);

            env.UndeployAll();
        }

        private static void TryAssertionInsertMap(
            RegressionEnvironment env,
            bool soda)
        {
            var path = new RegressionPath();
            env.CompileDeploy(soda, "create map schema EmptyMapSchema as ()", path);
            env.CompileDeploy("insert into EmptyMapSchema() select null from SupportBean", path);
            env.CompileDeploy("@Name('s0') select * from EmptyMapSchema", path).AddListener("s0");

            env.SendEventBean(new SupportBean());
            var @event = env.Listener("s0").AssertOneGetNewAndReset();
            Assert.IsTrue(((IDictionary<string, object>) @event.Underlying).IsEmpty());
            Assert.AreEqual(0, @event.EventType.PropertyDescriptors.Count);

            env.UndeployAll();
        }

        private static void TryAssertionInsertOA(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeploy("create objectarray schema EmptyOASchema()", path);
            env.CompileDeploy("insert into EmptyOASchema select null from SupportBean", path);

            var supportSubscriber = new SupportSubscriber();
            env.CompileDeploy("@Name('s0') select * from EmptyOASchema", path).AddListener("s0");
            env.Statement("s0").Subscriber = supportSubscriber;

            env.SendEventBean(new SupportBean());
            Assert.AreEqual(0, ((object[]) env.Listener("s0").AssertOneGetNewAndReset().Underlying).Length);

            var lastNewSubscriberData = supportSubscriber.LastNewData;
            Assert.AreEqual(1, lastNewSubscriberData.Length);
            Assert.AreEqual(0, ((object[]) lastNewSubscriberData[0]).Length);

            env.UndeployAll();
        }

        internal class EPLInsertIntoNamedWindowModelAfter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create schema EmptyPropSchema()", path);
                env.CompileDeploy("@Name('window') create window EmptyPropWin#keepall as EmptyPropSchema", path);
                env.CompileDeploy("insert into EmptyPropWin() select null from SupportBean", path);

                env.SendEventBean(new SupportBean());

                var events = EPAssertionUtil.EnumeratorToArray(env.GetEnumerator("window"));
                Assert.AreEqual(1, events.Length);
                Assert.AreEqual("EmptyPropWin", events[0].EventType.Name);

                // try fire-and-forget query
                env.CompileExecuteFAF("insert into EmptyPropWin select null", path);
                Assert.AreEqual(2, EPAssertionUtil.EnumeratorToArray(env.GetEnumerator("window")).Length);
                env.CompileExecuteFAF("delete from EmptyPropWin", path); // empty window

                // try on-merge
                env.CompileDeploy(
                    "on SupportBean_S0 merge EmptyPropWin " +
                    "when not matched then insert select null",
                    path);
                env.SendEventBean(new SupportBean_S0(0));
                Assert.AreEqual(1, EPAssertionUtil.EnumeratorToArray(env.GetEnumerator("window")).Length);

                // try on-insert
                env.CompileDeploy("on SupportBean_S1 insert into EmptyPropWin select null", path);
                env.SendEventBean(new SupportBean_S1(0));
                Assert.AreEqual(2, EPAssertionUtil.EnumeratorToArray(env.GetEnumerator("window")).Length);

                env.UndeployAll();
            }
        }

        internal class EPLInsertIntoCreateSchemaInsertInto : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryAssertionInsertMap(env, true);
                TryAssertionInsertMap(env, false);
                TryAssertionInsertOA(env);
                TryAssertionInsertBean(env);
            }
        }
    }
} // end of namespace