///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat.container;
using NUnit.Framework;

namespace NEsper.Examples.Transaction
{
    [TestFixture]
	public abstract class TestStmtBase
	{
	    protected EPServiceProvider epService;

        [SetUp]
	    public virtual void SetUp()
        {
            var container = ContainerExtensions.CreateDefaultContainer();

	        var configuration = new Configuration(container);
            configuration.EngineDefaults.EventMeta.ClassPropertyResolutionStyle = PropertyResolutionStyle.CASE_INSENSITIVE;
            configuration.AddEventType("TxnEventA", typeof(TxnEventA).FullName);
            configuration.AddEventType("TxnEventB", typeof(TxnEventB).FullName);
            configuration.AddEventType("TxnEventC", typeof(TxnEventC).FullName);

	        epService = EPServiceProviderManager.GetProvider(container, "TestStmtBase", configuration);
	        epService.Initialize();
	    }

	    protected void SendEvent(Object @event)
	    {
	        epService.EPRuntime.SendEvent(@event);
	    }

	}
} // End of namespace
