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

namespace com.espertech.esper.regressionlib.suite.epl.other
{
    public class EPLOtherComments : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var lineSeparator = Environment.NewLine;
            var statement = "@Name('s0') select TheString, /* this is my string */\n" +
                            "intPrimitive, // same line comment\n" +
                            "/* comment taking one line */\n" +
                            "// another comment taking a line\n" +
                            "intPrimitive as /* rename */ myPrimitive\n" +
                            "from SupportBean" +
                            lineSeparator +
                            " where /* inside a where */ intPrimitive /* */ = /* */ 100";
            env.CompileDeploy(statement).AddListener("s0");

            env.SendEventBean(new SupportBean("e1", 100));

            var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
            Assert.AreEqual("e1", theEvent.Get("TheString"));
            Assert.AreEqual(100, theEvent.Get("IntPrimitive"));
            Assert.AreEqual(100, theEvent.Get("myPrimitive"));
            env.Listener("s0").Reset();

            env.SendEventBean(new SupportBean("e1", -1));
            Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

            env.UndeployAll();
        }
    }
} // end of namespace