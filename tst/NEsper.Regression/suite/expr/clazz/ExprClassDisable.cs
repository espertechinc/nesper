///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.regressionlib.framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.expr.clazz
{
    public class ExprClassDisable : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var epl = "@Name('s0') inlined_class \"\"\"\n" +
                      "    public class MyClass {}\n" +
                      "\"\"\" " +
                      "select * from SupportBean\n";
            TryInvalidCompile(env, epl, "Inlined-class compilation has been disabled by configuration");
        }
    }
} // end of namespace