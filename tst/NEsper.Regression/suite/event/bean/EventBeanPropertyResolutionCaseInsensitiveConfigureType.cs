///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.@event.bean
{
    public class EventBeanPropertyResolutionCaseInsensitiveConfigureType : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            EventBeanPropertyResolutionCaseInsensitiveEngineDefault.TryCaseInsensitive(
                env,
                "BeanWithCaseInsensitive",
                "@name('s0') select THESTRING, INTPRIMITIVE from BeanWithCaseInsensitive where THESTRING='A'",
                "THESTRING",
                "INTPRIMITIVE");
            EventBeanPropertyResolutionCaseInsensitiveEngineDefault.TryCaseInsensitive(
                env,
                "BeanWithCaseInsensitive",
                "@name('s0') select ThEsTrInG, INTprimitIVE from BeanWithCaseInsensitive where THESTRing='A'",
                "ThEsTrInG",
                "INTprimitIVE");
        }
    }
} // end of namespace