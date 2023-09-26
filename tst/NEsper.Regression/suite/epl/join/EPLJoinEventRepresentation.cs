///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Avro.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NEsper.Avro.Extensions;

// fail
using NUnit.Framework; // assertTrue

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLJoinEventRepresentation
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithEventRepresentations(execs);
            WithMapEventNotUnique(execs);
            WithWrapperEventNotUnique(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithWrapperEventNotUnique(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinJoinWrapperEventNotUnique());
            return execs;
        }

        public static IList<RegressionExecution> WithMapEventNotUnique(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinJoinMapEventNotUnique());
            return execs;
        }

        public static IList<RegressionExecution> WithEventRepresentations(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinJoinEventRepresentations());
            return execs;
        }

        private class EPLJoinJoinEventRepresentations : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var jsonSchemas =
                    $"@Public @buseventtype create json schema S0_JSON(id String, p00 int);\n@Public @buseventtype create json schema S1_JSON(id String, p00 int);\n@Public @buseventtype @JsonSchema(className='{typeof(MyLocalJsonProvidedS0).FullName}') create json schema S0_JSONCLASSPROVIDED();\n@Public @buseventtype @JsonSchema(className='{typeof(MyLocalJsonProvidedS1).FullName}') create json schema S1_JSONCLASSPROVIDED();\n";
                env.CompileDeploy(jsonSchemas, path);
                var milestone = new AtomicLong();

                foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                    var s0Type = $"S0_{rep.GetName()}";
                    var s1Type = $"S1_{rep.GetName()}";
                    var eplOne =
                        $"select S0.id as s0id, S1.id as s1id, S0.p00 as s0p00, S1.p00 as s1p00 from {s0Type}#keepall as S0, {s1Type}#keepall as S1 where S0.id = S1.id";
                    TryJoinAssertion(
                        env,
                        eplOne,
                        rep,
                        "s0id,s1id,s0p00,s1p00",
                        milestone,
                        path,
                        typeof(MyLocalJsonProvidedWFields));
                }

                foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                    var s0Type = $"S0_{rep.GetName()}";
                    var s1Type = $"S1_{rep.GetName()}";
                    var eplTwo = $"select * from {s0Type}#keepall as s0, {s1Type}#keepall as s1 where s0.id = s1.id";
                    TryJoinAssertion(
                        env,
                        eplTwo,
                        rep,
                        "s0.id,s1.id,s0.p00,s1.p00",
                        milestone,
                        path,
                        typeof(MyLocalJsonProvidedWildcard));
                }

                env.UndeployAll();
            }

            private static void TryJoinAssertion(
                RegressionEnvironment env,
                string epl,
                EventRepresentationChoice rep,
                string columnNames,
                AtomicLong milestone,
                RegressionPath path,
                Type jsonClass)
            {
                env.CompileDeploy($"@name('s0'){rep.GetAnnotationTextWJsonProvided(jsonClass)}{epl}", path)
                    .AddListener("s0")
                    .MilestoneInc(milestone);

                var s0Name = $"S0_{rep.GetName()}";
                var s1Name = $"S1_{rep.GetName()}";

                SendRepEvent(env, rep, s0Name, "a", 1);
                env.AssertListenerNotInvoked("s0");

                SendRepEvent(env, rep, s1Name, "a", 2);
                env.AssertEventNew(
                    "s0",
                    output => {
                        EPAssertionUtil.AssertProps(output, columnNames.SplitCsv(), new object[] { "a", "a", 1, 2 });
                        Assert.IsTrue(rep.MatchesClass(output.Underlying.GetType()));
                    });

                SendRepEvent(env, rep, s1Name, "b", 3);
                SendRepEvent(env, rep, s0Name, "c", 4);
                env.AssertListenerNotInvoked("s0");

                env.UndeployModuleContaining("s0");
            }
        }

        private class EPLJoinJoinMapEventNotUnique : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Test for Esper-122
                var joinStatement =
                    "@name('s0') select S0.id, S1.id, S0.p00, S1.p00 from MapS0#keepall as S0, MapS1#keepall as S1" +
                    " where S0.id = S1.id";
                env.CompileDeployAddListenerMileZero(joinStatement, "s0");

                for (var i = 0; i < 100; i++) {
                    if (i % 2 == 1) {
                        SendMapEvent(env, "MapS0", "a", 1);
                    }
                    else {
                        SendMapEvent(env, "MapS1", "a", 1);
                    }
                }

                env.UndeployAll();
            }
        }

        private class EPLJoinJoinWrapperEventNotUnique : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Test for Esper-122
                var epl = "insert into S0Stream select 's0' as streamone, * from SupportBean;\n" +
                          "insert into S1Stream select 's1' as streamtwo, * from SupportBean;\n" +
                          "@name('s0') select * from S0Stream#keepall as a, S1Stream#keepall as b where a.intBoxed = b.intBoxed";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                for (var i = 0; i < 100; i++) {
                    env.SendEventBean(new SupportBean());
                }

                env.UndeployAll();
            }
        }

        private static void SendMapEvent(
            RegressionEnvironment env,
            string name,
            string id,
            int p00)
        {
            IDictionary<string, object> theEvent = new Dictionary<string, object>();
            theEvent.Put("id", id);
            theEvent.Put("p00", p00);
            env.SendEventMap(theEvent, name);
        }

        private static void SendRepEvent(
            RegressionEnvironment env,
            EventRepresentationChoice rep,
            string name,
            string id,
            int p00)
        {
            if (rep.IsMapEvent()) {
                IDictionary<string, object> theEvent = new Dictionary<string, object>();
                theEvent.Put("id", id);
                theEvent.Put("p00", p00);
                env.SendEventMap(theEvent, name);
            }
            else if (rep.IsObjectArrayEvent()) {
                env.SendEventObjectArray(new object[] { id, p00 }, name);
            }
            else if (rep.IsAvroEvent()) {
                var theEvent = new GenericRecord(env.RuntimeAvroSchemaPreconfigured(name).AsRecordSchema());
                theEvent.Put("id", id);
                theEvent.Put("p00", p00);
                env.SendEventAvro(theEvent, name);
            }
            else if (rep.IsJsonEvent() || rep.IsJsonProvidedClassEvent()) {
                var json = $"{{\"id\": \"{id}\", \"p00\": {p00}}}";
                env.SendEventJson(json, name);
            }
            else {
                Assert.Fail();
            }
        }

        [Serializable]
        public class MyLocalJsonProvidedS0
        {
            public string id;
            public int p00;
        }

        [Serializable]
        public class MyLocalJsonProvidedS1
        {
            public string id;
            public int p00;
        }

        [Serializable]
        public class MyLocalJsonProvidedWFields
        {
            public string s0id;
            public string s1id;
            public int s0p00;
            public int s1p00;
        }

        [Serializable]
        public class MyLocalJsonProvidedWildcard
        {
            public MyLocalJsonProvidedS0 s0;
            public MyLocalJsonProvidedS1 s1;
        }
    }
} // end of namespace