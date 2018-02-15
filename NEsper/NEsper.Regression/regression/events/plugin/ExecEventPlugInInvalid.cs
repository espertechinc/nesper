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


using NUnit.Framework;

namespace com.espertech.esper.regression.events.plugin
{
    public class ExecEventPlugInInvalid : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            if (SupportConfigFactory.SkipTest(typeof(ExecEventPlugInInvalid))) {
                return;
            }
            try {
                epService.EPRuntime.GetEventSender(new Uri[0]);
                Assert.Fail();
            } catch (EventTypeException ex) {
                Assert.AreEqual("Event sender for resolution URIs '[]' did not return at least one event representation's event factory", ex.Message);
            }
        }
    }
} // end of namespace
