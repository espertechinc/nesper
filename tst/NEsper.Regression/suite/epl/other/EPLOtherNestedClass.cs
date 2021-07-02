///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.other
{
    public class EPLOtherNestedClass
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithNestedClassEnum(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithNestedClassEnum(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLOtherNestedClassEnum());
            return execs;
        }

        private class EPLOtherNestedClassEnum : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl =
                    $"@public @buseventtype create schema MyEventWithColorEnum as {typeof(MyEventWithColorEnum).MaskTypeName()};\n" +
                    $"@Name('s0') select {typeof(MyEventWithColorEnum).MaskTypeName()}$Color.RED as c0 " +
                    $"from MyEventWithColorEnum(EnumProp={typeof(MyEventWithColorEnum).MaskTypeName()}$Color.GREEN)#firstevent";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new MyEventWithColorEnum(MyEventWithColorEnum.Color.BLUE));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new MyEventWithColorEnum(MyEventWithColorEnum.Color.GREEN));
                Assert.AreEqual(MyEventWithColorEnum.Color.RED, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));

                env.UndeployAll();
            }
        }

        public class MyEventWithColorEnum
        {
            public enum Color
            {
                GREEN,
                BLUE,
                RED
            }

            private readonly Color enumProp;

            public MyEventWithColorEnum(Color enumProp)
            {
                this.enumProp = enumProp;
            }

            public Color EnumProp => enumProp;
        }
    }
} // end of namespace