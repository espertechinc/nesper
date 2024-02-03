///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.expreval;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
    public class ExprCoreEventIdentityEquals
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithSimple(execs);
            WithDocSample(execs);
            WithSubquery(execs);
            WithEnumMethod(execs);
            WithInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreEventIdentityEqualsInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithEnumMethod(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreEventIdentityEqualsEnumMethod());
            return execs;
        }

        public static IList<RegressionExecution> WithSubquery(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreEventIdentityEqualsSubquery());
            return execs;
        }

        public static IList<RegressionExecution> WithDocSample(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreEventIdentityEqualsDocSample());
            return execs;
        }

        public static IList<RegressionExecution> WithSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreEventIdentityEqualsSimple());
            return execs;
        }

        public class ExprCoreEventIdentityEqualsSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "event_identity_equals(e,e)" };
                var builder = new SupportEvalBuilder("SupportBean", "e")
                    .WithExpressions(fields, fields[0]);

                builder.WithAssertion(new SupportBean()).Expect(fields, true);

                builder.Run(env);
                env.UndeployAll();
            }
        }

        public class ExprCoreEventIdentityEqualsDocSample : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var docSample = "create schema OrderEvent(OrderId string, Amount double);\n" +
                                "select * from OrderEvent as arrivingEvent \n" +
                                "  where exists (select * from OrderEvent#time(5) as last5 where not event_identity_equals(arrivingEvent, last5) and arrivingEvent.OrderId = last5.OrderId);\n" +
                                "select OrderId, window(*).aggregate(0d, (result, e) => result + (case when event_identity_equals(oe, e) then 0d else e.Amount end)) as c0 from OrderEvent#time(10) as oe";
                env.CompileDeploy(docSample).UndeployAll();
            }
        }

        public class ExprCoreEventIdentityEqualsSubquery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text =
                    "@name('s0') select * from SupportBean as e where exists (select * from SupportBean#keepall as ka where not event_identity_equals(e, ka) and e.TheString = ka.TheString)";
                env.CompileDeploy(text).AddListener("s0");

                SendAssertNotReceived(env, "E1");
                SendAssertReceived(env, "E1");

                SendAssertNotReceived(env, "E2");
                SendAssertReceived(env, "E2");
                SendAssertReceived(env, "E2");

                env.UndeployAll();
            }
        }

        public class ExprCoreEventIdentityEqualsEnumMethod : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text =
                    "@name('s0') select TheString, window(*).aggregate(0, (result, e) => result + (case when event_identity_equals(sb, e) then 0 else e.IntPrimitive end)) as c0 from SupportBean#time(10) as sb";
                env.CompileDeploy(text).AddListener("s0");

                SendAssert(env, "E1", 10, 0);
                SendAssert(env, "E2", 11, 10);
                SendAssert(env, "E3", 12, 10 + 11);
                SendAssert(env, "E4", 13, 10 + 11 + 12);

                env.UndeployAll();
            }
        }

        public class ExprCoreEventIdentityEqualsInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.TryInvalidCompile(
                    "select event_identity_equals(e) from SupportBean as e",
                    "Failed to validate select-clause expression 'event_identity_equals(e)': event_identity_equalsrequires two parameters");

                env.TryInvalidCompile(
                    "select event_identity_equals(e, 1) from SupportBean as e",
                    "Failed to validate select-clause expression 'event_identity_equals(e,1)': event_identity_equals requires a parameter that resolves to an event but received '1'");

                env.TryInvalidCompile(
                    "select event_identity_equals(e, s0) from SupportBean#lastevent as e, SupportBean_S0#lastevent as s0",
                    "Failed to validate select-clause expression 'event_identity_equals(e,s0)': event_identity_equals received two different event types as parameter, Type 'SupportBean' is not the same as type 'SupportBean_S0'");
            }
        }

        private static void SendAssert(
            RegressionEnvironment env,
            string theString,
            int intPrimitive,
            int expected)
        {
            env.SendEventBean(new SupportBean(theString, intPrimitive));
            env.AssertPropsNew("s0", "TheString,c0".SplitCsv(), new object[] { theString, expected });
        }

        private static void SendAssertNotReceived(
            RegressionEnvironment env,
            string theString)
        {
            env.SendEventBean(new SupportBean(theString, 0));
            env.AssertListenerNotInvoked("s0");
        }

        private static void SendAssertReceived(
            RegressionEnvironment env,
            string theString)
        {
            env.SendEventBean(new SupportBean(theString, 0));
            env.AssertEventNew("s0", ClassicAssert.NotNull);
        }
    }
} // end of namespace