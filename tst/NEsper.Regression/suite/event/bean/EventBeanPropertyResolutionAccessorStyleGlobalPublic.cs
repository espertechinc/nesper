///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

namespace com.espertech.esper.regressionlib.suite.@event.bean
{
    public class EventBeanPropertyResolutionAccessorStyleGlobalPublic : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            env.CompileDeploy("@name('s0') select fieldLegacyVal from SupportLegacyBean").AddListener("s0");

            var theEvent = new SupportLegacyBean("E1");
            theEvent.fieldLegacyVal = "val1";
            env.SendEventBean(theEvent);
            env.AssertEqualsNew("s0", "fieldLegacyVal", "val1");

            env.UndeployAll();
        }
    }
} // end of namespace