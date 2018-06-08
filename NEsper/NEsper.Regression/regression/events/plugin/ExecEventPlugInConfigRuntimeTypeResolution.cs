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
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.plugineventrep;

using NUnit.Framework;

namespace com.espertech.esper.regression.events.plugin
{
    public class ExecEventPlugInConfigRuntimeTypeResolution : RegressionExecution {
        public override void Configure(Configuration configuration) {
            ConfigureURIs(configuration);
        }
    
        public static void ConfigureURIs(Configuration configuration) {
            configuration.AddPlugInEventRepresentation(new Uri("type://properties"), typeof(MyPlugInEventRepresentation), "r3");
            configuration.AddPlugInEventRepresentation(new Uri("type://properties/test1"), typeof(MyPlugInEventRepresentation), "r1");
            configuration.AddPlugInEventRepresentation(new Uri("type://properties/test2"), typeof(MyPlugInEventRepresentation), "r2");
        }
    
        public override void Run(EPServiceProvider epService) {
            if (SupportConfigFactory.SkipTest(typeof(ExecEventPlugInConfigRuntimeTypeResolution))) {
                return;
            }
            ConfigurationOperations ops = epService.EPAdministrator.Configuration;
            ops.AddPlugInEventType("TestTypeOne", new Uri[]{new Uri("type://properties/test1/testtype")}, "t1");
            ops.AddPlugInEventType("TestTypeTwo", new Uri[]{new Uri("type://properties/test2")}, "t2");
            ops.AddPlugInEventType("TestTypeThree", new Uri[]{new Uri("type://properties/test3")}, "t3");
            ops.AddPlugInEventType("TestTypeFour", new Uri[]{new Uri("type://properties/test2/x"), new Uri("type://properties/test3")}, "t4");
    
            ExecEventPlugInConfigStaticTypeResolution.RunAssertionCaseStatic(epService);
        }
    }
} // end of namespace
