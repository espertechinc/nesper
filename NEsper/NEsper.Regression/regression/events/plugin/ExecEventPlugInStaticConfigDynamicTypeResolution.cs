///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Net;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.execution;

using static com.espertech.esper.regression.events.plugin.ExecEventPlugInConfigRuntimeTypeResolution;

using NUnit.Framework;

namespace com.espertech.esper.regression.events.plugin
{
    public class ExecEventPlugInStaticConfigDynamicTypeResolution : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            ConfigureURIs(configuration);
            var uriList = new Uri[]{new Uri("type://properties/test2/myresolver")};
            configuration.PlugInEventTypeResolutionURIs = uriList;
        }
    
        public override void Run(EPServiceProvider epService) {
            ExecEventPlugInRuntimeConfigDynamicTypeResolution.RunAssertionCaseDynamic(epService);
        }
    }
} // end of namespace
