///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.@event.bean
{
    public class EventBeanPropertyResolutionCaseDistinctInsensitive : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            env.CompileDeploy("@name('s0') select MYPROPERTY, myproperty, myProperty from SupportBeanDupProperty");
            env.AddListener("s0");

            env.SendEventBean(new SupportBeanDupProperty("lowercamel", "uppercamel", "upper", "lower"));
            env.AssertEventNew(
                "s0",
                result => {
                    Assert.AreEqual("upper", result.Get("MYPROPERTY"));
                    Assert.AreEqual("lower", result.Get("myproperty"));
                    Assert.AreEqual("lowercamel", result.Get("myProperty"));
                });

            env.TryInvalidCompile(
                "select mYpropertY from SupportBeanDupProperty",
                "Unable to determine which property to use for \"mYpropertY\" because more than one property matched [");

            env.UndeployAll();
        }
    }
} // end of namespace