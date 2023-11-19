///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

namespace com.espertech.esper.regressionlib.suite.rowrecog
{
    public class RowRecogMultikeyWArray
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithWArray(execs);
            WithPlain(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithPlain(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new RowRecogPartitionMultikeyPlain());
            return execs;
        }

        public static IList<RegressionExecution> WithWArray(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new RowRecogPartitionMultikeyWArray());
            return execs;
        }

        private class RowRecogPartitionMultikeyWArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@name('s0') select * from SupportEventWithIntArray " +
                           "match_recognize (" +
                           " partition by Array" +
                           " measures A.Id as a, B.Id as b" +
                           " pattern (A B)" +
                           " define" +
                           " A as A.Value = 1," +
                           " B as B.Value = 2" +
                           ")";

                env.CompileDeploy(text).AddListener("s0");

                SendArray(env, "E1", new int[] { 1, 2 }, 1);
                SendArray(env, "E2", new int[] { 1 }, 1);
                SendArray(env, "E3", null, 1);
                SendArray(env, "E4", new int[] { }, 1);

                env.Milestone(0);

                SendArray(env, "E10", new int[] { 1, 2 }, 2);
                AssertReceived(env, "E1", "E10");

                SendArray(env, "E11", new int[] { }, 2);
                AssertReceived(env, "E4", "E11");

                SendArray(env, "E12", new int[] { 1 }, 2);
                AssertReceived(env, "E2", "E12");

                SendArray(env, "E13", null, 2);
                AssertReceived(env, "E3", "E13");

                env.UndeployAll();
            }
        }

        private class RowRecogPartitionMultikeyPlain : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@name('s0') select * from SupportBean " +
                           "match_recognize (" +
                           " partition by IntPrimitive, LongPrimitive" +
                           " measures A.TheString as a, B.TheString as b" +
                           " pattern (A B)" +
                           " define" +
                           " A as A.DoublePrimitive = 1," +
                           " B as B.DoublePrimitive = 2" +
                           ")";

                env.CompileDeploy(text).AddListener("s0");

                SendSB(env, "E1", 1, 2, 1);
                SendSB(env, "E2", 1, 3, 1);
                SendSB(env, "E3", 2, 2, 1);

                env.Milestone(0);

                SendSB(env, "E10", 2, 2, 2);
                AssertReceived(env, "E3", "E10");

                SendSB(env, "E11", 1, 3, 2);
                AssertReceived(env, "E2", "E11");

                SendSB(env, "E12", 1, 2, 2);
                AssertReceived(env, "E1", "E12");

                env.UndeployAll();
            }
        }

        private static void SendSB(
            RegressionEnvironment env,
            string theString,
            int intPrimitive,
            long longPrimitive,
            double doublePrimitive)
        {
            var sb = new SupportBean(theString, intPrimitive);
            sb.LongPrimitive = longPrimitive;
            sb.DoublePrimitive = doublePrimitive;
            env.SendEventBean(sb);
        }

        private static void SendArray(
            RegressionEnvironment env,
            string id,
            int[] array,
            int value)
        {
            env.SendEventBean(new SupportEventWithIntArray(id, array, value));
        }

        private static void AssertReceived(
            RegressionEnvironment env,
            string a,
            string b)
        {
            env.AssertPropsNew("s0", "a,b".SplitCsv(), new object[] { a, b });
        }
    }
} // end of namespace