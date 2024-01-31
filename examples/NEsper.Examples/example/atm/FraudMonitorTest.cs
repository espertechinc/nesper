///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.compat;
using com.espertech.esper.container;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;
using NUnit.Framework.Legacy;
using Configuration = com.espertech.esper.common.client.configuration.Configuration;

namespace NEsper.Examples.ATM
{
	[TestFixture]
	public class FraudMonitorTest : IDisposable
	{
	    private const string FraudText = "card reported stolen";

		private EPRuntime _runtime;
		private SupportUpdateListener _listener;
		private EventSender _fraudWarningSender;
		private EventSender _withdrawalSender;

		[SetUp]
	    public void SetUp()
		{
		    var container = ContainerExtensions.CreateDefaultContainer()
		        .InitializeDefaultServices()
		        .InitializeDatabaseDrivers();

	        var configuration = new Configuration(container);
	        configuration.Common.AddEventType("FraudWarning", typeof(FraudWarning));
	        configuration.Common.AddEventType("Withdrawal", typeof(Withdrawal));
		    configuration.Common.EventMeta.ClassPropertyResolutionStyle = PropertyResolutionStyle.CASE_INSENSITIVE;
	
	        _runtime = EPRuntimeProvider.GetRuntime("FraudMonitorTest", configuration);
	        _runtime.EventService.ClockExternal();

	        _fraudWarningSender = _runtime.EventService.GetEventSender("FraudWarning");
	        _withdrawalSender = _runtime.EventService.GetEventSender("Withdrawal");
	
	        _listener = new SupportUpdateListener();
	        new FraudMonitor(_runtime, _listener.Update);
	    }
	    
		[Test]
		public void TestJoin()
	    {
	        SendWithdrawal(1001, 100);
	        SendFraudWarn(1004, FraudText);
	        SendWithdrawal(1001, 60);
	        SendWithdrawal(1002, 400);
	        SendWithdrawal(1001, 300);
	
	        ClassicAssert.IsFalse(_listener.GetAndClearIsInvoked());
	
	        SendWithdrawal(1004, 100);
	        ClassicAssert.IsTrue(_listener.GetAndClearIsInvoked());
	
	        ClassicAssert.AreEqual(1, _listener.LastNewData.Length);
	        EventBean _event = _listener.LastNewData[0];
	        ClassicAssert.AreEqual(1004L, _event["accountNumber"]);
	        ClassicAssert.AreEqual(FraudText, _event["warning"]);
	        ClassicAssert.AreEqual(100, _event["amount"]);
	        ClassicAssert.IsTrue( ((long) _event["timestamp"]) > (DateTimeHelper.CurrentTimeMillis - 100));
	        ClassicAssert.AreEqual("withdrawlFraudWarn", _event["descr"]);
	    }
	
	    private void SendWithdrawal(long acctNum, int amount)
	    {
	        Withdrawal eventBean = new Withdrawal(acctNum, amount);
	        _withdrawalSender.SendEvent(eventBean);
	    }
	
	    private void SendFraudWarn(long acctNum, String text)
	    {
	        FraudWarning eventBean = new FraudWarning(acctNum, text);
	        _fraudWarningSender.SendEvent(eventBean);
	    }

	    public void Dispose()
	    {
	    }
	}
}
