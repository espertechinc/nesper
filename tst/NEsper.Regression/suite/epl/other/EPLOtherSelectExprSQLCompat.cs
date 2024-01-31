///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.epl.other
{
    public class EPLOtherSelectExprSQLCompat
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
#if REGRESSION_EXECUTIONS
            With(y)(execs);
#endif
            return execs;
        }

        public static IList<RegressionExecution> Withy(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherProperty());
            return execs;
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string s,
            int intPrimitive)
        {
            var bean = new SupportBean(s, intPrimitive);
            env.SendEventBean(bean);
        }

        internal class EPLOtherProperty : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select SupportBean.TheString as val1, SupportBean.IntPrimitive as val2 from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, "E1", 10);
                env.AssertEventNew(
                    "s0",
                    received => {
                        ClassicAssert.AreEqual("E1", received.Get("val1"));
                        ClassicAssert.AreEqual(10, received.Get("val2"));
                    });

                env.UndeployAll();
            }
        }
    }
} // end of namespace