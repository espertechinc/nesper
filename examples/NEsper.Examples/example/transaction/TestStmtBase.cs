///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;
using com.espertech.esper.container;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

using Configuration = com.espertech.esper.common.client.configuration.Configuration;

namespace NEsper.Examples.Transaction
{
    [TestFixture]
	public abstract class TestStmtBase
	{
	    protected EPRuntime _runtime;
	    protected EPEventService _eventService;

	    [SetUp]
	    public virtual void SetUp()
        {
            var container = ContainerExtensions.CreateDefaultContainer();

	        var configuration = new Configuration(container);
            configuration.Common.EventMeta.ClassPropertyResolutionStyle = PropertyResolutionStyle.CASE_INSENSITIVE;
            configuration.Common.AddEventType("TxnEventA", typeof(TxnEventA));
            configuration.Common.AddEventType("TxnEventB", typeof(TxnEventB));
            configuration.Common.AddEventType("TxnEventC", typeof(TxnEventC));

	        _runtime = EPRuntimeProvider.GetRuntime("TestStmtBase", configuration);
	        _runtime.Initialize();

	        _eventService = _runtime.EventService;
        }

	    protected void SendEvent(object @event)
	    {
	        _eventService.SendEventBean(@event, @event.GetType().FullName);
	    }
	}
} // End of namespace
