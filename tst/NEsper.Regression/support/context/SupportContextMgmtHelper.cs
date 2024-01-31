///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.@internal.kernel.service;

namespace com.espertech.esper.regressionlib.support.context
{
    public class SupportContextMgmtHelper
    {
        public static int GetContextCount(RegressionEnvironment env)
        {
            var spi = (EPRuntimeSPI) env.Runtime;
            return spi.ServicesContext.ContextManagementService.ContextCount;
        }
    }
} // end of namespace