///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.events.map
{
    public class ExecEventMapNestedConfigStatic : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("NestedMap", ExecEventMapNestedConfigRuntime.TestDefinition);
        }
    
        public override void Run(EPServiceProvider epService) {
            ExecEventMapNestedConfigRuntime.RunAssertion(epService);
        }
    }
} // end of namespace
