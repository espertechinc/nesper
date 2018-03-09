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
using com.espertech.esper.plugin;
using com.espertech.esper.supportregression.events;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.events.plugin
{
    public class ExecEventPlugInContextContent : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddPlugInEventRepresentation(new Uri("type://test/support"), typeof(SupportEventRepresentation), "abc");
        }
    
        public override void Run(EPServiceProvider epService) {
            PlugInEventRepresentationContext initContext = SupportEventRepresentation.InitContext;
            Assert.AreEqual(new Uri("type://test/support"), initContext.EventRepresentationRootURI);
            Assert.AreEqual("abc", initContext.RepresentationInitializer);
            Assert.IsNotNull(initContext.EventAdapterService);
    
            ConfigurationOperations runtimeConfig = epService.EPAdministrator.Configuration;
            runtimeConfig.AddPlugInEventType("TestTypeOne", new Uri[]{new Uri("type://test/support?a=b&c=d")}, "t1");
    
            PlugInEventTypeHandlerContext plugincontext = SupportEventRepresentation.AcceptTypeContext;
            Assert.AreEqual(new Uri("type://test/support?a=b&c=d"), plugincontext.EventTypeResolutionURI);
            Assert.AreEqual("t1", plugincontext.TypeInitializer);
            Assert.AreEqual("TestTypeOne", plugincontext.EventTypeName);
    
            plugincontext = SupportEventRepresentation.EventTypeContext;
            Assert.AreEqual(new Uri("type://test/support?a=b&c=d"), plugincontext.EventTypeResolutionURI);
            Assert.AreEqual("t1", plugincontext.TypeInitializer);
            Assert.AreEqual("TestTypeOne", plugincontext.EventTypeName);
    
            epService.EPRuntime.GetEventSender(new Uri[]{new Uri("type://test/support?a=b")});
            PlugInEventBeanReflectorContext contextBean = SupportEventRepresentation.EventBeanContext;
            Assert.AreEqual("type://test/support?a=b", contextBean.ResolutionURI.ToString());
        }
    }
} // end of namespace
