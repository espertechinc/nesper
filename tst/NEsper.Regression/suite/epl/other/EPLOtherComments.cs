///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.epl.other
{
    public class EPLOtherComments : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var lineSeparator = Environment.NewLine;
            var statement = "@name('s0') select TheString, /* this is my string */\n" +
                            "IntPrimitive, // same line Comment\n" +
                            "/* Comment taking one line */\n" +
                            "// another Comment taking a line\n" +
                            "IntPrimitive as /* rename */ myPrimitive\n" +
                            "from SupportBean" +
                            lineSeparator +
                            " where /* inside a where */ IntPrimitive /* */ = /* */ 100";
            env.CompileDeploy(statement).AddListener("s0");

            env.SendEventBean(new SupportBean("e1", 100));

            env.AssertEventNew(
                "s0",
                theEvent => {
                    ClassicAssert.AreEqual("e1", theEvent.Get("TheString"));
                    ClassicAssert.AreEqual(100, theEvent.Get("IntPrimitive"));
                    ClassicAssert.AreEqual(100, theEvent.Get("myPrimitive"));
                });

            env.SendEventBean(new SupportBean("e1", -1));
            env.AssertListenerNotInvoked("s0");

            env.UndeployAll();
        }
    }
} // end of namespace