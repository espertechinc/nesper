///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.@event.map
{
    public class EventMapNestedEscapeDot : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var statementText = "@Name('s0') select a\\.b, a\\.b\\.c, nes\\., nes\\.nes2.x\\.y from DotMap";
            env.CompileDeploy(statementText).AddListener("s0");

            var data = EventMapCore.MakeMap(
                new[] {
                    new object[] {"a.b", 10},
                    new object[] {"a.b.c", 20},
                    new object[] {"nes.", 30},
                    new object[] {"nes.nes2", EventMapCore.MakeMap(new[] {new object[] {"x.y", 40}})}
                });
            env.SendEventMap(data, "DotMap");

            var fields = "a.b,a.b.c,nes.,nes.nes2.x.y".SplitCsv();
            var received = env.Listener("s0").AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(
                received,
                fields,
                new object[] {10, 20, 30, 40});

            env.UndeployAll();
        }
    }
} // end of namespace