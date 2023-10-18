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
            var compiled = env.Compile("@public @buseventtype create schema MyItem(Id string)");
            env.Deploy(compiled);
            path.Add(compiled);

            env.CompileDeploy(
                "@name('s0') select " +
                methodName +
                "(TheString).where(v -> v.Id in ('id1', 'id3')) as c0 from SupportBean",
                path);
            env.AddListener("s0");

            env.SendEventBean(new SupportBean("id0,id1,id2,id3,id4", 0));
            env.AssertEventNew(
                "s0",
                @event => {
                    var real = @event.Get("c0");
                    var coll = real.Unwrap<IDictionary<string, object>>();
                    EPAssertionUtil.AssertPropsPerRow(
                        coll.ToArray(),
                        new[] { "Id" },
                        new[] {
                            new object[] { "id1" },
                            new object[] { "id3" }
                        });
                });

            env.UndeployAll();
        }

        private static void TryAssertionReturnTypeIsEventsInvalid(RegressionEnvironment env)
        {
            env.CompileDeploy("select myItemProducerInvalidNoType(TheString) as c0 from SupportBean");
            env.TryInvalidCompile(
                "select myItemProducerInvalidNoType(TheString).where(v =>  v.Id='Id1') as c0 from SupportBean",
                "Failed to validate select-clause expression 'myItemProducerInvalidNoType(TheStri...(68 chars)': Method 'MyItemProducerEventBeanArray' returns EventBean-array but does not provide the event type name [");

            // test invalid: event type name invalid
            env.TryInvalidCompile(
                "select myItemProducerInvalidWrongType(TheString).where(v =>  v.Id='Id1') as c0 from SupportBean",
                "Failed to validate select-clause expression 'myItemProducerInvalidWrongType(TheS...(74 chars)': Method 'MyItemProducerEventBeanArray' returns event type 'dummy' and the event type cannot be found [select myItemProducerInvalidWrongType(TheString).where(v -> v.Id='Id1') as c0 from SupportBean]");

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