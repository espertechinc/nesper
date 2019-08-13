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

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLJoinDerivedValueViews : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var epl = "@Name('s0') select\n" +
                      "Math.Sign(stream1.slope) as s1,\n" +
                      "Math.Sign(stream2.slope) as s2\n" +
                      "from\n" +
                      "SupportBean#length_batch(3)#linest(IntPrimitive, LongPrimitive) as stream1,\n" +
                      "SupportBean#length_batch(2)#linest(IntPrimitive, LongPrimitive) as stream2";
            env.CompileDeployAddListenerMileZero(epl, "s0");
            env.SendEventBean(MakeEvent("E3", 1, 100));
            env.SendEventBean(MakeEvent("E4", 1, 100));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.UndeployAll();
        }

        private static SupportBean MakeEvent(
            string id,
            int intPrimitive,
            long longPrimitive)
        {
            var bean = new SupportBean(id, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            return bean;
        }
    }
} // end of namespace