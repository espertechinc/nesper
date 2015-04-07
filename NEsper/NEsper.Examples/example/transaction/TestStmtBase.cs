///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using NUnit.Framework;
using com.espertech.esper.client;

namespace NEsper.Example.Transaction
{
    [TestFixture]
	public abstract class TestStmtBase
	{
	    protected EPServiceProvider epService;

        [SetUp]
	    public virtual void SetUp()
	    {
	        Configuration configuration = new Configuration();
            configuration.EngineDefaults.EventMetaConfig.ClassPropertyResolutionStyle = PropertyResolutionStyle.CASE_INSENSITIVE;
            configuration.AddEventType("TxnEventA", typeof(TxnEventA).FullName);
            configuration.AddEventType("TxnEventB", typeof(TxnEventB).FullName);
            configuration.AddEventType("TxnEventC", typeof(TxnEventC).FullName);

	        epService = EPServiceProviderManager.GetProvider("TestStmtBase", configuration);
	        epService.Initialize();
	    }

	    protected void SendEvent(Object @event)
	    {
	        epService.EPRuntime.SendEvent(@event);
	    }

	}
} // End of namespace
