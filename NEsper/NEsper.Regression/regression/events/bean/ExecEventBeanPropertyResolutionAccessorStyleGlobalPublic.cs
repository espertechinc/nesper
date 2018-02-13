///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregession.bean;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

// using static org.junit.Assert.assertEquals;

using NUnit.Framework;

namespace com.espertech.esper.regression.events.bean
{
    public class ExecEventBeanPropertyResolutionAccessorStyleGlobalPublic : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.EventMeta.DefaultAccessorStyle = AccessorStyleEnum.PUBLIC;
            configuration.AddEventType("SupportLegacyBean", typeof(SupportLegacyBean));
        }
    
        public override void Run(EPServiceProvider epService) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select fieldLegacyVal from SupportLegacyBean");
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            var theEvent = new SupportLegacyBean("E1");
            theEvent.fieldLegacyVal = "val1";
            epService.EPRuntime.SendEvent(theEvent);
            Assert.AreEqual("val1", listener.AssertOneGetNewAndReset().Get("fieldLegacyVal"));
        }
    }
} // end of namespace
