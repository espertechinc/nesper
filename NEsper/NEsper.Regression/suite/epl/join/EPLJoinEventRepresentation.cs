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
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLJoinEventRepresentation
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLJoinJoinEventRepresentations());
            execs.Add(new EPLJoinJoinMapEventNotUnique());
            execs.Add(new EPLJoinJoinWrapperEventNotUnique());
            return execs;
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
                env.SendEventObjectArray(new object[] {id, p00}, name);
            }
            else if (rep.IsAvroEvent()) {
                var theEvent = new GenericRecord(
                    SupportAvroUtil.GetAvroSchema(env.Runtime.EventTypeService.GetEventTypePreconfigured(name))
                        .AsRecordSchema());
                theEvent.Put("id", id);
                theEvent.Put("p00", p00);
                env.SendEventAvro(theEvent, name);
            }
            else {
                Assert.Fail();
            }
        }

        internal class EPLJoinJoinEventRepresentations : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                    var s0Type = "S0_" + rep.GetUndName();
                    var s1Type = "S1_" + rep.GetUndName();
                    var eplOne = "select S0.Id as S0_Id, S1.Id as S1_Id, S0.p00 as S0_p00, S1.p00 as S1_p00 from " +
                                 s0Type +
                                 "#keepall as S0, " +
                                 s1Type +
                                 "#keepall as S1 where S0.Id = S1.Id";
                    TryJoinAssertion(env, eplOne, rep, "S0_Id,S1_Id,S0_p00,S1_p00", milestone);
                }

                foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                    var s0Type = "S0_" + rep.GetUndName();
                    var s1Type = "S1_" + rep.GetUndName();
                    var eplTwo = "select * from " +
                                 s0Type +
                                 "#keepall as S0, " +
                                 s1Type +
                                 "#keepall as S1 where S0.Id = S1.Id";
                    TryJoinAssertion(env, eplTwo, rep, "S0.Id,S1.Id,S0.p00,S1.p00", milestone);
                }
            }

            private static void TryJoinAssertion(
                RegressionEnvironment env,
                string epl,
                EventRepresentationChoice rep,
                string columnNames,
                AtomicLong milestone)
            {
                env.CompileDeployAddListenerMile(
                    "@Name('s0')" + rep.GetAnnotationText() + epl,
                    "s0",
                    milestone.GetAndIncrement());

                var s0Name = "S0_" + rep.GetUndName();
                var s1Name = "S1_" + rep.GetUndName();

                SendRepEvent(env, rep, s0Name, "a", 1);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendRepEvent(env, rep, s1Name, "a", 2);
                var output = env.Listener("s0").AssertOneGetNewAndReset();
                EPAssertionUtil.AssertProps(
                    output,
                    columnNames.SplitCsv(),
                    new object[] {"a", "a", 1, 2});
                Assert.IsTrue(rep.MatchesClass(output.Underlying.GetType()));

                SendRepEvent(env, rep, s1Name, "b", 3);
                SendRepEvent(env, rep, s0Name, "c", 4);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class EPLJoinJoinMapEventNotUnique : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Test for Esper-122
                var joinStatement =
                    "@Name('s0') select S0.Id, S1.Id, S0.p00, S1.p00 from MapS0#keepall as S0, MapS1#keepall as S1" +
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

        internal class EPLJoinJoinWrapperEventNotUnique : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Test for Esper-122
                var epl = "insert into S0Stream select 's0' as streamone, * from SupportBean;\n" +
                          "insert into S1Stream select 's1' as streamtwo, * from SupportBean;\n" +
                          "@Name('s0') select * from S0Stream#keepall as a, S1Stream#keepall as b where a.IntBoxed = b.IntBoxed";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                for (var i = 0; i < 100; i++) {
                    env.SendEventBean(new SupportBean());
                }

                env.UndeployAll();
            }
        }
    }
} // end of namespace