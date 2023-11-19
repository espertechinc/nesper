///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Text;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLJoin20Stream : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var buf = new StringBuilder();
            buf.Append("@name('s0') select * from ");

            var delimiter = "";
            for (var i = 0; i < 20; i++) {
                buf.Append(delimiter);
                buf.Append($"SupportBean_S0(Id={i})#lastevent as s_{i}");
                delimiter = ", ";
            }

            env.CompileDeployAddListenerMileZero(buf.ToString(), "s0");

            for (var i = 0; i < 19; i++) {
                env.SendEventBean(new SupportBean_S0(i));
            }

            env.AssertListenerNotInvoked("s0");
            env.SendEventBean(new SupportBean_S0(19));
            env.AssertListenerInvoked("s0");

            env.UndeployAll();
        }
    }
} // end of namespace