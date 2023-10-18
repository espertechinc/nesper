///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.@event.map
{
    public class EventMapInheritanceRuntime : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            var epl =
                "@buseventtype @public create schema RootEvent(base string);\n" +
                "@buseventtype @public create schema Sub1Event(sub1 string) inherits RootEvent;\n" +
                "@buseventtype @public create schema Sub2Event(sub2 string) inherits RootEvent;\n" +
                "@buseventtype @public create schema SubAEvent(suba string) inherits Sub1Event;\n" +
                "@buseventtype @public create schema SubBEvent(subb string) inherits Sub1Event, Sub2Event;\n";
            env.CompileDeploy(epl, path);

            EventMapInheritanceInitTime.RunAssertionMapInheritance(env, path);
        }

        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.OBSERVEROPS);
        }
    }
} // end of namespace