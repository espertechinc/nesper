///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.@event.map
{
    public class EventMapInheritanceRuntime : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            var epl =
                "create schema RootEvent(base string);\n" +
                "create schema Sub1Event(sub1 string) inherits RootEvent;\n" +
                "create schema Sub2Event(sub2 string) inherits RootEvent;\n" +
                "create schema SubAEvent(suba string) inherits Sub1Event;\n" +
                "create schema SubBEvent(subb string) inherits Sub1Event, Sub2Event;\n";
            env.CompileDeployWBusPublicType(epl, path);

            EventMapInheritanceInitTime.RunAssertionMapInheritance(env, path);
        }
    }
} // end of namespace