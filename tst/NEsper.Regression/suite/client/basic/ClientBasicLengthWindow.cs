///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.client.basic
{
    public class ClientBasicLengthWindow : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var epl = "@Name('s0') select irstream * from SupportBean#length(2)";
            env.CompileDeploy(epl).AddListener("s0");

            var sb0 = SendAssertNoRStream(env, "E1");

            var sb1 = SendAssertNoRStream(env, "E2");

            env.Milestone(1);

            SendAssertIR(env, "E3", sb0);

            env.Milestone(2);

            SendAssertIR(env, "E4", sb1);

            env.UndeployAll();
        }

        private void SendAssertIR(
            RegressionEnvironment env,
            string theString,
            SupportBean rstream)
        {
            var sb = SendBean(env, theString);
            var pair = env.Listener("s0").AssertPairGetIRAndReset();
            Assert.AreEqual(rstream, pair.Second.Underlying);
            Assert.AreSame(sb, pair.First.Underlying);
        }

        private SupportBean SendAssertNoRStream(
            RegressionEnvironment env,
            string theString)
        {
            var sb = SendBean(env, theString);
            Assert.AreSame(sb, env.Listener("s0").AssertOneGetNewAndReset().Underlying);
            return sb;
        }

        private SupportBean SendBean(
            RegressionEnvironment env,
            string theString)
        {
            var sb = new SupportBean(theString, 0);
            env.SendEventBean(sb);
            return sb;
        }
    }
} // end of namespace