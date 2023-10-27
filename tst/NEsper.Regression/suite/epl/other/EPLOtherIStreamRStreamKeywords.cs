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
#if REGRESSION_EXECUTIONS
            WithRStreamOnlyOM(execs);
            WithRStreamOnlyCompile(execs);
            WithRStreamOnly(execs);
            WithRStreamInsertInto(execs);
            WithRStreamInsertIntoRStream(execs);
            WithRStreamJoin(execs);
            WithIStreamOnly(execs);
            WithIStreamInsertIntoRStream(execs);
            WithIStreamJoin(execs);
            With(RStreamOutputSnapshot)(execs);
#endif
            return execs;
        }

        public static IList<RegressionExecution> WithRStreamOutputSnapshot(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherRStreamOutputSnapshot());
            return execs;
        }

        public static IList<RegressionExecution> WithIStreamJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherIStreamJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithIStreamInsertIntoRStream(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherIStreamInsertIntoRStream());
            return execs;
        }

        public static IList<RegressionExecution> WithIStreamOnly(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherIStreamOnly());
            return execs;
        }

        public static IList<RegressionExecution> WithRStreamJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherRStreamJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithRStreamInsertIntoRStream(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherRStreamInsertIntoRStream());
            return execs;
        }

        public static IList<RegressionExecution> WithRStreamInsertInto(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherRStreamInsertInto());
            return execs;
        }

        public static IList<RegressionExecution> WithRStreamOnly(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherRStreamOnly());
            return execs;
        }

        public static IList<RegressionExecution> WithRStreamOnlyCompile(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherRStreamOnlyCompile());
            return execs;
        }

        public static IList<RegressionExecution> WithRStreamOnlyOM(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherRStreamOnlyOM());
            return execs;
        }

        private class EPLOtherRStreamOutputSnapshot : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "select rstream * from SupportBean#time(30 minutes) output snapshot";
                env.CompileDeploy(epl).UndeployAll();
            }
        }

        private class EPLOtherRStreamOnlyOM : RegressionExecution
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
                env.AssertListenerNotInvoked("s0");

                SendEvents(env, new string[] { "a", "b" });
                env.AssertListenerNotInvoked("s0");

                SendEvent(env, "d", 2);
                env.AssertListener(
                    "s0",
                    listener => {
                        Assert.AreSame(theEvent, listener.LastNewData[0].Underlying); // receive 'a' as new data
                        Assert.IsNull(listener.LastOldData); // receive no more old data
                    });

                env.UndeployAll();
            }
        }

        private class EPLOtherRStreamOnlyCompile : RegressionExecution
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
                env.AssertListenerNotInvoked("s0");

                SendEvents(env, new string[] { "a", "b" });
                env.AssertListenerNotInvoked("s0");

                SendEvent(env, "d", 2);
                env.AssertListener(
                    "s0",
                    listener => {
                        Assert.AreSame(theEvent, listener.LastNewData[0].Underlying); // receive 'a' as new data
                        Assert.IsNull(listener.LastOldData); // receive no more old data
                    });

                env.UndeployAll();
            }
        }

        private class EPLOtherRStreamOnly : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@name('s0') select rstream * from SupportBean#length(3)").AddListener("s0");

                var theEvent = SendEvent(env, "a", 2);
                env.AssertListenerNotInvoked("s0");

                SendEvents(env, new string[] { "a", "b" });
                env.AssertListenerNotInvoked("s0");

                SendEvent(env, "d", 2);
                env.AssertListener(
                    "s0",
                    listener => {
                        Assert.AreSame(theEvent, listener.LastNewData[0].Underlying); // receive 'a' as new data
                        Assert.IsNull(listener.LastOldData); // receive no more old data
                    });

                env.UndeployAll();
            }
        }

        private class EPLOtherRStreamInsertInto : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('s0') @public insert into NextStream " +
                    "select rstream s0.TheString as TheString from SupportBean#length(3) as s0",
                    path);
                env.AddListener("s0");
                env.CompileDeploy("@name('ii') select * from NextStream", path).AddListener("ii");

                SendEvent(env, "a", 2);
                env.AssertListenerNotInvoked("s0");
                env.AssertEqualsNew("ii", "TheString", "a");

                SendEvents(env, new string[] { "b", "c" });
                env.AssertListenerNotInvoked("s0");
                env.AssertListener(
                    "ii",
                    listener => {
                        Assert.AreEqual(2, listener.NewDataList.Count); // insert into unchanged
                        listener.Reset();
                    });

                SendEvent(env, "d", 2);
                env.AssertListener(
                    "s0",
                    listener => {
                        Assert.AreSame("a", listener.LastNewData[0].Get("TheString")); // receive 'a' as new data
                        Assert.IsNull(listener.LastOldData); // receive no more old data
                    });
                env.AssertListener(
                    "ii",
                    listener => {
                        Assert.AreEqual("d", listener.LastNewData[0].Get("TheString")); // insert into unchanged
                        Assert.IsNull(listener.LastOldData); // receive no old data in insert into
                    });

                env.UndeployAll();
            }
        }

        private class EPLOtherRStreamInsertIntoRStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('s0') @public insert rstream into NextStream " +
                    "select rstream s0.TheString as TheString from SupportBean#length(3) as s0",
                    path);
                env.AddListener("s0");

                env.CompileDeploy("@name('ii') select * from NextStream", path).AddListener("ii");

                SendEvent(env, "a", 2);
                env.AssertListenerNotInvoked("s0");
                env.AssertListenerNotInvoked("ii");

                SendEvents(env, new string[] { "b", "c" });
                env.AssertListenerNotInvoked("s0");
                env.AssertListenerNotInvoked("ii");

                SendEvent(env, "d", 2);
                env.AssertListener(
                    "s0",
                    listener => {
                        Assert.AreSame("a", listener.LastNewData[0].Get("TheString")); // receive 'a' as new data
                        Assert.IsNull(listener.LastOldData); // receive no more old data
                    });
                env.AssertListener(
                    "ii",
                    listener => {
                        Assert.AreEqual("a", listener.LastNewData[0].Get("TheString")); // insert into unchanged
                        Assert.IsNull(listener.LastOldData); // receive no old data in insert into
                    });

                env.UndeployAll();
            }
        }

        private class EPLOtherRStreamJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                        "@name('s0') select rstream s1.IntPrimitive as aID, s2.IntPrimitive as bID " +
                        "from SupportBean(TheString='a')#length(2) as s1, " +
                        "SupportBean(TheString='b')#keepall as s2" +
                        " where s1.IntPrimitive = s2.IntPrimitive")
                    .AddListener("s0");

                SendEvent(env, "a", 1);
                SendEvent(env, "b", 1);
                env.AssertListenerNotInvoked("s0");

                SendEvent(env, "a", 2);
                env.AssertListenerNotInvoked("s0");

                SendEvent(env, "a", 3);
                env.AssertListener(
                    "s0",
                    listener => {
                        Assert.AreEqual(1, listener.LastNewData[0].Get("aID")); // receive 'a' as new data
                        Assert.AreEqual(1, listener.LastNewData[0].Get("bID"));
                        Assert.IsNull(listener.LastOldData); // receive no more old data
                    });

                env.UndeployAll();
            }
        }

        private class EPLOtherIStreamOnly : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@name('s0') select istream * from SupportBean#length(1)").AddListener("s0");

                var eventOne = SendEvent(env, "a", 2);
                env.AssertEventNew("s0", @event => Assert.AreSame(eventOne, @event.Underlying));

                var eventTwo = SendEvent(env, "b", 2);
                env.AssertListener(
                    "s0",
                    listener => {
                        Assert.AreSame(eventTwo, listener.LastNewData[0].Underlying);
                        Assert.IsNull(listener.LastOldData); // receive no old data, just istream events
                    });

                env.UndeployAll();
            }
        }

        private class EPLOtherIStreamInsertIntoRStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('s0') @public insert rstream into NextStream " +
                    "select istream a.TheString as TheString from SupportBean#length(1) as a",
                    path);
                env.AddListener("s0");

                env.CompileDeploy("@name('ii') select * from NextStream", path).AddListener("ii");

                SendEvent(env, "a", 2);
                env.AssertEqualsNew("s0", "TheString", "a");
                env.AssertListenerNotInvoked("ii");

                SendEvent(env, "b", 2);
                env.AssertListener(
                    "s0",
                    listener => {
                        Assert.AreEqual("b", listener.LastNewData[0].Get("TheString"));
                        Assert.IsNull(listener.LastOldData);
                    });
                env.AssertListener(
                    "ii",
                    listener => {
                        Assert.AreEqual("a", listener.LastNewData[0].Get("TheString"));
                        Assert.IsNull(listener.LastOldData);
                    });

                env.UndeployAll();
            }
        }

        private class EPLOtherIStreamJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                        "@name('s0') " +
                        "select istream s1.IntPrimitive as aID, s2.IntPrimitive as bID " +
                        "from SupportBean(TheString='a')#length(2) as s1, " +
                        "SupportBean(TheString='b')#keepall as s2" +
                        " where s1.IntPrimitive = s2.IntPrimitive")
                    .AddListener("s0");

                SendEvent(env, "a", 1);
                SendEvent(env, "b", 1);
                env.AssertListener(
                    "s0",
                    listener => {
                        Assert.AreEqual(1, listener.LastNewData[0].Get("aID")); // receive 'a' as new data
                        Assert.AreEqual(1, listener.LastNewData[0].Get("bID"));
                        Assert.IsNull(listener.LastOldData); // receive no more old data
                        listener.Reset();
                    });

                SendEvent(env, "a", 2);
                env.AssertListenerNotInvoked("s0");

                SendEvent(env, "a", 3);
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
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
    }
} // end of namespace