///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.regressionlib.framework;

using static com.espertech.esper.regressionlib.suite.@event.map.EventMapNestedConfigStatic;

namespace com.espertech.esper.regressionlib.suite.@event.map
{
    public class EventMapNestedConfigRuntime : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            var epl =
                "create schema N1N1 (n1n1 String);\n" +
                "create schema N1 (n1 string, n2 N1N1);\n" +
                "create schema NestedMapWithSimpleProps (nested N1);\n";
            env.CompileDeployWBusPublicType(epl, path);

            RunAssertion(env, path);
        }
    }
} // end of namespace