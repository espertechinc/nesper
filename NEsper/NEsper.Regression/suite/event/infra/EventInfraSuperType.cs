///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using Avro.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat;
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
        public void Run(RegressionEnvironment env)
        {
            // Bean
            RunAssertion(
                env,
                "Bean",
                FBEANWTYPE,
                new Bean_Type_Root(),
                new Bean_Type_1(),
                new Bean_Type_2(),
                new Bean_Type_2_1());

            // Map
            RunAssertion(
                env,
                "Map",
                FMAPWTYPE,
                new Dictionary<string, object>(),
                new Dictionary<string, object>(),
                new Dictionary<string, object>(),
                new Dictionary<string, object>());

            // OA
            RunAssertion(env, "OA", FOAWTYPE, new object[0], new object[0], new object[0], new object[0]);

            // Avro
            var fake = SchemaBuilder.Record("fake");
            RunAssertion(
                env,
                "Avro",
                FAVROWTYPE,
                new GenericRecord(fake),
                new GenericRecord(fake),
                new GenericRecord(fake),
                new GenericRecord(fake));
        }

        private void RunAssertion(
            RegressionEnvironment env,
            string typePrefix,
            FunctionSendEventWType sender,
            object root,
            object type1,
            object type2,
            object type21)
        {
            var typeNames = "Type_Root,Type_1,Type_2,Type_2_1".SplitCsv();
            var statements = new EPStatement[4];
            var listeners = new SupportUpdateListener[4];
            for (var i = 0; i < typeNames.Length; i++) {
                env.CompileDeploy("@Name('s" + i + "') select * from " + typePrefix + "_" + typeNames[i]);
                statements[i] = env.Statement("s" + i);
                listeners[i] = new SupportUpdateListener();
                statements[i].AddListener(listeners[i]);
            }

            sender.Invoke(env, root, typePrefix + "_" + typeNames[0]);
            EPAssertionUtil.AssertEqualsExactOrder(
                new[] {true, false, false, false},
                GetInvokedFlagsAndReset(listeners));

            sender.Invoke(env, type1, typePrefix + "_" + typeNames[1]);
            EPAssertionUtil.AssertEqualsExactOrder(
                new[] {true, true, false, false},
                GetInvokedFlagsAndReset(listeners));

            sender.Invoke(env, type2, typePrefix + "_" + typeNames[2]);
            EPAssertionUtil.AssertEqualsExactOrder(
                new[] {true, false, true, false},
                GetInvokedFlagsAndReset(listeners));

            sender.Invoke(env, type21, typePrefix + "_" + typeNames[3]);
            EPAssertionUtil.AssertEqualsExactOrder(new[] {true, false, true, true}, GetInvokedFlagsAndReset(listeners));

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