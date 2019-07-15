///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.other
{
    public class EPLOtherIStreamRStreamKeywords
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLOtherRStreamOnlyOM());
            execs.Add(new EPLOtherRStreamOnlyCompile());
            execs.Add(new EPLOtherRStreamOnly());
            execs.Add(new EPLOtherRStreamInsertInto());
            execs.Add(new EPLOtherRStreamInsertIntoRStream());
            execs.Add(new EPLOtherRStreamJoin());
            execs.Add(new EPLOtherIStreamOnly());
            execs.Add(new EPLOtherIStreamInsertIntoRStream());
            execs.Add(new EPLOtherIStreamJoin());
            return execs;
        }

        private static void SendEvents(
            RegressionEnvironment env,
            string[] stringValue)
        {
            for (var i = 0; i < stringValue.Length; i++) {
                SendEvent(env, stringValue[i], 2);
            }
        }

        private static object SendEvent(
            RegressionEnvironment env,
            string stringValue,
            int intPrimitive)
        {
            var theEvent = new SupportBean();
            theEvent.TheString = stringValue;
            theEvent.IntPrimitive = intPrimitive;
            env.SendEventBean(theEvent);
            return theEvent;
        }

        internal class EPLOtherRStreamOnlyOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "select rstream * from SupportBean#length(3)";
                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.CreateWildcard(StreamSelector.RSTREAM_ONLY);
                var fromClause = FromClause.Create(
                    FilterStream.Create("SupportBean").AddView(View.Create("length", Expressions.Constant(3))));
                model.FromClause = fromClause;
                model = env.CopyMayFail(model);

                Assert.AreEqual(stmtText, model.ToEPL());
                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0");

                var theEvent = SendEvent(env, "a", 2);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendEvents(env, new[] {"a", "b"});
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendEvent(env, "d", 2);
                Assert.AreSame(theEvent, env.Listener("s0").LastNewData[0].Underlying); // receive 'a' as new data
                Assert.IsNull(env.Listener("s0").LastOldData); // receive no more old data

                env.UndeployAll();
            }
        }

        internal class EPLOtherRStreamOnlyCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "select rstream * from SupportBean#length(3)";
                var model = env.EplToModel(stmtText);
                model = env.CopyMayFail(model);

                Assert.AreEqual(stmtText, model.ToEPL());
                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0");

                var theEvent = SendEvent(env, "a", 2);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendEvents(env, new[] {"a", "b"});
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendEvent(env, "d", 2);
                Assert.AreSame(theEvent, env.Listener("s0").LastNewData[0].Underlying); // receive 'a' as new data
                Assert.IsNull(env.Listener("s0").LastOldData); // receive no more old data

                env.UndeployAll();
            }
        }

        internal class EPLOtherRStreamOnly : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@Name('s0') select rstream * from SupportBean#length(3)").AddListener("s0");

                var theEvent = SendEvent(env, "a", 2);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendEvents(env, new[] {"a", "b"});
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendEvent(env, "d", 2);
                Assert.AreSame(theEvent, env.Listener("s0").LastNewData[0].Underlying); // receive 'a' as new data
                Assert.IsNull(env.Listener("s0").LastOldData); // receive no more old data

                env.UndeployAll();
            }
        }

        internal class EPLOtherRStreamInsertInto : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@Name('s0') insert into NextStream " +
                    "select rstream s0.TheString as TheString from SupportBean#length(3) as s0",
                    path);
                env.AddListener("s0");
                env.CompileDeploy("@Name('ii') select * from NextStream", path).AddListener("ii");

                SendEvent(env, "a", 2);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                Assert.AreEqual(
                    "a",
                    env.Listener("ii").AssertOneGetNewAndReset().Get("TheString")); // insert into unchanged

                SendEvents(env, new[] {"b", "c"});
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                Assert.AreEqual(2, env.Listener("ii").NewDataList.Count); // insert into unchanged
                env.Listener("ii").Reset();

                SendEvent(env, "d", 2);
                Assert.AreSame("a", env.Listener("s0").LastNewData[0].Get("TheString")); // receive 'a' as new data
                Assert.IsNull(env.Listener("s0").LastOldData); // receive no more old data
                Assert.AreEqual("d", env.Listener("ii").LastNewData[0].Get("TheString")); // insert into unchanged
                Assert.IsNull(env.Listener("ii").LastOldData); // receive no old data in insert into

                env.UndeployAll();
            }
        }

        internal class EPLOtherRStreamInsertIntoRStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@Name('s0') insert rstream into NextStream " +
                    "select rstream s0.TheString as TheString from SupportBean#length(3) as s0",
                    path);
                env.AddListener("s0");

                env.CompileDeploy("@Name('ii') select * from NextStream", path).AddListener("ii");

                SendEvent(env, "a", 2);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                Assert.IsFalse(env.Listener("ii").IsInvoked);

                SendEvents(env, new[] {"b", "c"});
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                Assert.IsFalse(env.Listener("ii").IsInvoked);

                SendEvent(env, "d", 2);
                Assert.AreSame("a", env.Listener("s0").LastNewData[0].Get("TheString")); // receive 'a' as new data
                Assert.IsNull(env.Listener("s0").LastOldData); // receive no more old data
                Assert.AreEqual("a", env.Listener("ii").LastNewData[0].Get("TheString")); // insert into unchanged
                Assert.IsNull(env.Listener("s0").LastOldData); // receive no old data in insert into

                env.UndeployAll();
            }
        }

        internal class EPLOtherRStreamJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                        "@Name('s0') select rstream s1.IntPrimitive as aID, s2.IntPrimitive as bID " +
                        "from SupportBean(TheString='a')#length(2) as s1, " +
                        "SupportBean(TheString='b')#keepall as s2" +
                        " where s1.IntPrimitive = s2.IntPrimitive")
                    .AddListener("s0");

                SendEvent(env, "a", 1);
                SendEvent(env, "b", 1);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendEvent(env, "a", 2);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendEvent(env, "a", 3);
                Assert.AreEqual(1, env.Listener("s0").LastNewData[0].Get("aID")); // receive 'a' as new data
                Assert.AreEqual(1, env.Listener("s0").LastNewData[0].Get("bID"));
                Assert.IsNull(env.Listener("s0").LastOldData); // receive no more old data

                env.UndeployAll();
            }
        }

        internal class EPLOtherIStreamOnly : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@Name('s0') select istream * from SupportBean#length(1)").AddListener("s0");

                var theEvent = SendEvent(env, "a", 2);
                Assert.AreSame(theEvent, env.Listener("s0").AssertOneGetNewAndReset().Underlying);

                theEvent = SendEvent(env, "b", 2);
                Assert.AreSame(theEvent, env.Listener("s0").LastNewData[0].Underlying);
                Assert.IsNull(env.Listener("s0").LastOldData); // receive no old data, just istream events

                env.UndeployAll();
            }
        }

        internal class EPLOtherIStreamInsertIntoRStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@Name('s0') insert rstream into NextStream " +
                    "select istream a.TheString as TheString from SupportBean#length(1) as a",
                    path);
                env.AddListener("s0");

                env.CompileDeploy("@Name('ii') select * from NextStream", path).AddListener("ii");

                SendEvent(env, "a", 2);
                Assert.AreEqual("a", env.Listener("s0").AssertOneGetNewAndReset().Get("TheString"));
                Assert.IsFalse(env.Listener("ii").IsInvoked);

                SendEvent(env, "b", 2);
                var listener = env.Listener("s0");
                Assert.AreEqual("b", env.Listener("s0").LastNewData[0].Get("TheString"));
                Assert.IsNull(env.Listener("s0").LastOldData);
                Assert.AreEqual("a", env.Listener("ii").LastNewData[0].Get("TheString"));
                Assert.IsNull(env.Listener("ii").LastOldData);

                env.UndeployAll();
            }
        }

        internal class EPLOtherIStreamJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                        "@Name('s0') " +
                        "select istream s1.IntPrimitive as aID, s2.IntPrimitive as bID " +
                        "from SupportBean(TheString='a')#length(2) as s1, " +
                        "SupportBean(TheString='b')#keepall as s2" +
                        " where s1.IntPrimitive = s2.IntPrimitive")
                    .AddListener("s0");

                SendEvent(env, "a", 1);
                SendEvent(env, "b", 1);
                Assert.AreEqual(1, env.Listener("s0").LastNewData[0].Get("aID")); // receive 'a' as new data
                Assert.AreEqual(1, env.Listener("s0").LastNewData[0].Get("bID"));
                Assert.IsNull(env.Listener("s0").LastOldData); // receive no more old data
                env.Listener("s0").Reset();

                SendEvent(env, "a", 2);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendEvent(env, "a", 3);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }
    }
} // end of namespace