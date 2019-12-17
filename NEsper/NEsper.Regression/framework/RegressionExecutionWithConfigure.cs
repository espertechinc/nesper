///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;

namespace com.espertech.esper.regressionlib.framework
{
    public interface RegressionExecutionWithConfigure : RegressionExecution
    {
        // public bool EnableHATest => true;
        // public bool HAWithCOnly => false;

        bool EnableHATest { get; }

        bool HAWithCOnly { get; }

        void Configure(Configuration configuration);
    }
}