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
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NEsper.Avro.Extensions;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLJoinEventRepresentation
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            
            foreach (var rep in EventRepresentationChoiceExtensions.Values())
            {
                WithEventRepresentations(rep, execs);
            }

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

        public static IList<RegressionExecution> WithEventRepresentations(
            EventRepresentationChoice rep,
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinJoinEventRepresentations(rep));
            return execs;
        }

        private class EPLJoinJoinEventRepresentations : RegressionExecution
        {
            private readonly EventRepresentationChoice _rep;
            public EPLJoinJoinEventRepresentations(EventRepresentationChoice rep)
            {
                this._rep = rep;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var jsonSchemas =
                    $"@Public @buseventtype create json schema S0_JSON(Id String, P00 int);\n@Public @buseventtype create json schema S1_JSON(Id String, P00 int);\n@Public @buseventtype @JsonSchema(ClassName='{typeof(MyLocalJsonProvidedS0).FullName}') create json schema S0_JSONCLASSPROVIDED();\n@Public @buseventtype @JsonSchema(ClassName='{typeof(MyLocalJsonProvidedS1).FullName}') create json schema S1_JSONCLASSPROVIDED();\n";
                env.CompileDeploy(jsonSchemas, path);
                var milestone = new AtomicLong();

                {
                    var s0Type = $"S0_{_rep.GetPublicName()}";
                    var s1Type = $"S1_{_rep.GetPublicName()}";
                    var eplOne =
                        $"select S0.Id as s0id, S1.Id as s1id, S0.P00 as s0p00, S1.P00 as s1p00 from {s0Type}#keepall as S0, {s1Type}#keepall as S1 where S0.Id = S1.Id";
                    TryJoinAssertion(
                        env,
                        eplOne,
                        _rep,
                        "s0id,s1id,s0p00,s1p00",
                        milestone,
                        path,
                        typeof(MyLocalJsonProvidedWFields));
                }

                {
                    var s0Type = $"S0_{_rep.GetPublicName()}";
                    var s1Type = $"S1_{_rep.GetPublicName()}";
                    var eplTwo = $"select * from {s0Type}#keepall as s0, {s1Type}#keepall as s1 where s0.Id = s1.Id";
                    TryJoinAssertion(
                        env,
                        eplTwo,
                        _rep,
                        "s0.Id,s1.Id,s0.P00,s1.P00",
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

                var s0Name = $"S0_{rep.GetPublicName()}";
                var s1Name = $"S1_{rep.GetPublicName()}";

                SendRepEvent(env, rep, s0Name, "a", 1);
                env.AssertListenerNotInvoked("s0");

                SendRepEvent(env, rep, s1Name, "a", 2);
                env.AssertEventNew(
                    "s0",
                    output => {
                        EPAssertionUtil.AssertProps(output, columnNames.SplitCsv(), new object[] { "a", "a", 1, 2 });
                        ClassicAssert.IsTrue(rep.MatchesClass(output.Underlying.GetType()));
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
                    "@name('s0') select S0.Id, S1.Id, S0.P00, S1.P00 from MapS0#keepall as S0, MapS1#keepall as S1" +
                    " where S0.Id = S1.Id";
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
                          "@name('s0') select * from S0Stream#keepall as a, S1Stream#keepall as b where a.IntBoxed = b.IntBoxed";
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
            theEvent.Put("Id", id);
            theEvent.Put("P00", p00);
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
                theEvent.Put("Id", id);
                theEvent.Put("P00", p00);
                env.SendEventMap(theEvent, name);
            }
            else if (rep.IsObjectArrayEvent()) {
                env.SendEventObjectArray(new object[] { id, p00 }, name);
            }
            else if (rep.IsAvroEvent()) {
                var theEvent = new GenericRecord(env.RuntimeAvroSchemaPreconfigured(name).AsRecordSchema());
                theEvent.Put("Id", id);
                theEvent.Put("P00", p00);
                env.SendEventAvro(theEvent, name);
            }
            else if (rep.IsJsonEvent() || rep.IsJsonProvidedClassEvent()) {
                var json = $"{{\"Id\": \"{id}\", \"P00\": {p00}}}";
                env.SendEventJson(json, name);
            }
            else {
                Assert.Fail();
            }
        }

        public class MyLocalJsonProvidedS0
        {
            public string Id;
            public int P00;
        }

        public class MyLocalJsonProvidedS1
        {
            public string Id;
            public int P00;
        }

        public class MyLocalJsonProvidedWFields
        {
            public string s0id;
            public string s1id;
            public int s0p00;
            public int s1p00;
        }

        public class MyLocalJsonProvidedWildcard
        {
            public MyLocalJsonProvidedS0 s0;
            public MyLocalJsonProvidedS1 s1;
        }
    }
} // end of namespace