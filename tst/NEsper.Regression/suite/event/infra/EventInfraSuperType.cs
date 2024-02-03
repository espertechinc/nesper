///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Avro.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.scopetest;

using NEsper.Avro.Extensions;

using static com.espertech.esper.regressionlib.support.@event.SupportEventInfra;
using static com.espertech.esper.runtime.client.scopetest.SupportUpdateListener;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
    public class EventInfraSuperType : RegressionExecution
    {
        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.OBSERVEROPS);
        }

        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();

            // Bean
            RunAssertion(
                env,
                path,
                "Bean",
                FBEANWTYPE,
                new Bean_Type_Root(),
                new Bean_Type_1(),
                new Bean_Type_2(),
                new Bean_Type_2_1());

            // Map
            RunAssertion(
                env,
                path,
                "Map",
                FMAPWTYPE,
                new Dictionary<string, object>(),
                new Dictionary<string, object>(),
                new Dictionary<string, object>(),
                new Dictionary<string, object>());

            // OA
            RunAssertion(
                env,
                path,
                "OA",
                FOAWTYPE,
                Array.Empty<object>(),
                Array.Empty<object>(),
                Array.Empty<object>(),
                Array.Empty<object>());

            // Avro
            var fake = SchemaBuilder.Record("fake");
            RunAssertion(
                env,
                path,
                "Avro",
                FAVROWTYPE,
                new GenericRecord(fake),
                new GenericRecord(fake),
                new GenericRecord(fake),
                new GenericRecord(fake));

            // Json
            var schemas = "@public @buseventtype @name('schema') create json schema Json_Type_Root();\n" +
                          "@public @buseventtype create json schema Json_Type_1() inherits Json_Type_Root;\n" +
                          "@public @buseventtype create json schema Json_Type_2() inherits Json_Type_Root;\n" +
                          "@public @buseventtype create json schema Json_Type_2_1() inherits Json_Type_2;\n";
            env.CompileDeploy(schemas, path);
            RunAssertion(
                env,
                path,
                "Json",
                FJSONWTYPE,
                "{}",
                "{}",
                "{}",
                "{}");
        }

        private void RunAssertion(
            RegressionEnvironment env,
            RegressionPath path,
            string typePrefix,
            FunctionSendEventWType sender,
            object root,
            object type1,
            object type2,
            object type21)
        {
            var typeNames = new[] { "Type_Root", "Type_1", "Type_2", "Type_2_1" };
            var statements = new EPStatement[4];
            var listeners = new SupportUpdateListener[4];
            for (var i = 0; i < typeNames.Length; i++) {
                env.CompileDeploy("@name('s" + i + "') select * from " + typePrefix + "_" + typeNames[i], path);
                statements[i] = env.Statement("s" + i);
                listeners[i] = new SupportUpdateListener();
                statements[i].AddListener(listeners[i]);
            }

            sender.Invoke(env, root, typePrefix + "_" + typeNames[0]);
            EPAssertionUtil.AssertEqualsExactOrder(
                new[] { true, false, false, false },
                GetInvokedFlagsAndReset(listeners));

            sender.Invoke(env, type1, typePrefix + "_" + typeNames[1]);
            EPAssertionUtil.AssertEqualsExactOrder(
                new[] { true, true, false, false },
                GetInvokedFlagsAndReset(listeners));

            sender.Invoke(env, type2, typePrefix + "_" + typeNames[2]);
            EPAssertionUtil.AssertEqualsExactOrder(
                new[] { true, false, true, false },
                GetInvokedFlagsAndReset(listeners));

            sender.Invoke(env, type21, typePrefix + "_" + typeNames[3]);
            EPAssertionUtil.AssertEqualsExactOrder(
                new[] { true, false, true, true },
                GetInvokedFlagsAndReset(listeners));

            env.UndeployAll();
        }

        public class Bean_Type_Root
        {
        }

        public class Bean_Type_1 : Bean_Type_Root
        {
        }

        public class Bean_Type_2 : Bean_Type_Root
        {
        }

        public class Bean_Type_2_1 : Bean_Type_2
        {
        }
    }
} // end of namespace