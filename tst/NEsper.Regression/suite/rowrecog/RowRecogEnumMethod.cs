///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.rowrecog
{
    public class RowRecogEnumMethod : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var fields = new[] { "c0", "c1" };
            var epl = "@name('s0') select * from SupportBean match_recognize (" +
                      "partition by TheString " +
                      "measures A.TheString as c0, C.IntPrimitive as c1 " +
                      "pattern (A B+ C) " +
                      "define " +
                      "B as B.IntPrimitive > A.IntPrimitive, " +
                      "C as C.DoublePrimitive > B.firstOf().IntPrimitive)";
            // can also be expressed as: B[0].IntPrimitive
            env.CompileDeploy(epl).AddListener("s0");

            SendEvent(env, "E1", 10, 0);
            SendEvent(env, "E1", 11, 50);
            SendEvent(env, "E1", 12, 11);
            env.AssertListenerNotInvoked("s0");

            env.Milestone(0);

            SendEvent(env, "E2", 10, 0);
            SendEvent(env, "E2", 11, 50);

            env.Milestone(1);

            SendEvent(env, "E2", 12, 12);
            env.AssertPropsNew(
                "s0",
                fields,
                new object[] { "E2", 12 });

            env.UndeployAll();
        }

        private void SendEvent(
            RegressionEnvironment env,
            string theString,
            int intPrimitive,
            double doublePrimitive)
        {
            var bean = new SupportBean(theString, intPrimitive);
            bean.DoublePrimitive = doublePrimitive;
            env.SendEventBean(bean);
        }
    }
} // end of namespace