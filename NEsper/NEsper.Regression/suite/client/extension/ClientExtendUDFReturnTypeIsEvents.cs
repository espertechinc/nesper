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
using com.espertech.esper.common.client.hook.expr;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.client.extension
{
    public class ClientExtendUDFReturnTypeIsEvents : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var milestone = new AtomicLong();

            TryAssertionReturnTypeIsEvents(env, "myItemProducerEventBeanArray", milestone);
            TryAssertionReturnTypeIsEvents(env, "myItemProducerEventBeanCollection", milestone);
            TryAssertionReturnTypeIsEventsInvalid(env);
        }

        private static void TryAssertionReturnTypeIsEvents(
            RegressionEnvironment env,
            string methodName,
            AtomicLong milestone)
        {
            var path = new RegressionPath();
            var compiled = env.CompileWBusPublicType("create schema MyItem(Id string)");
            env.Deploy(compiled);
            path.Add(compiled);

            env.CompileDeploy(
                "@Name('s0') select " +
                methodName +
                "(TheString).where(v -> v.Id in ('Id1', 'Id3')) as c0 from SupportBean",
                path);
            env.AddListener("s0");

            env.SendEventBean(new SupportBean("id0,Id1,Id2,Id3,Id4", 0));
            var real = env.Listener("s0").AssertOneGetNewAndReset().Get("c0");
            var coll = real.Unwrap<IDictionary<string, object>>();
            //var coll = env.Listener("s0").AssertOneGetNewAndReset().Get("c0").Unwrap<IDictionary<string, object>>();
            EPAssertionUtil.AssertPropsPerRow(
                coll.ToArray(),
                new [] { "Id" },
                new[] {new object[] {"id1"}, new object[] {"id3"}});

            env.UndeployAll();
        }

        private static void TryAssertionReturnTypeIsEventsInvalid(RegressionEnvironment env)
        {
            env.CompileDeploy("select MyItemProducerInvalidNoType(TheString) as c0 from SupportBean");
            SupportMessageAssertUtil.TryInvalidCompile(
                env,
                "select MyItemProducerInvalidNoType(TheString).where(v -> v.Id='Id1') as c0 from SupportBean",
                "Failed to validate select-clause expression 'MyItemProducerInvalidNoType(TheStri...(68 chars)': Method 'MyItemProducerEventBeanArray' returns EventBean-array but does not provide the event type name [");

            // test invalid: event type name invalid
            SupportMessageAssertUtil.TryInvalidCompile(
                env,
                "select myItemProducerInvalidWrongType(TheString).where(v -> v.Id='Id1') as c0 from SupportBean",
                "Failed to validate select-clause expression 'MyItemProducerInvalidWrongType(TheS...(74 chars)': Method 'MyItemProducerEventBeanArray' returns event type 'dummy' and the event type cannot be found [select MyItemProducerInvalidWrongType(TheString).where(v -> v.Id='Id1') as c0 from SupportBean]");

            env.UndeployAll();
        }

        public static EventBean[] MyItemProducerEventBeanArray(
            string @string,
            EPLMethodInvocationContext context)
        {
            var split = @string.SplitCsv();
            var events = new EventBean[split.Length];
            for (var i = 0; i < split.Length; i++) {
                events[i] = context.EventBeanService.AdapterForMap(
                    Collections.SingletonDataMap("Id", split[i]),
                    "MyItem");
            }

            return events;
        }

        public static ICollection<EventBean> MyItemProducerEventBeanCollection(
            string @string,
            EPLMethodInvocationContext context)
        {
            return Arrays.AsList(MyItemProducerEventBeanArray(@string, context));
        }
    }
} // end of namespace