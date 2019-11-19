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
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.compat.container;

using NUnit.Framework;

namespace NEsper.Examples.ATM
{
	[TestFixture]
	public class FraudMonitorTest : IDisposable
	{
	    private const string FraudText = "card reported stolen";

		private EPServiceProvider _epService;
		private SupportUpdateListener _listener;

		[SetUp]
	    public void SetUp()
		{
		    var container = ContainerExtensions.CreateDefaultContainer()
		        .InitializeDefaultServices()
		        .InitializeDatabaseDrivers();

	        var configuration = new Configuration(container);
	        configuration.AddEventType("FraudWarning", typeof(FraudWarning).FullName);
	        configuration.AddEventType("Withdrawal", typeof(Withdrawal).FullName);
		    configuration.EngineDefaults.EventMeta.ClassPropertyResolutionStyle = PropertyResolutionStyle.CASE_INSENSITIVE;
	
	        _epService = EPServiceProviderManager.GetProvider(container, "FraudMonitorTest", configuration);
	        _epService.EPRuntime.SendEvent(new TimerControlEvent(TimerControlEvent.ClockTypeEnum.CLOCK_EXTERNAL));
	
	        _listener = new SupportUpdateListener();
	        new FraudMonitor(_epService, _listener.Update);
	    }
	    
		[Test]
		public void TestJoin()
	    {
	        SendWithdrawal(1001, 100);
	        SendFraudWarn(1004, FraudText);
	        SendWithdrawal(1001, 60);
	        SendWithdrawal(1002, 400);
	        SendWithdrawal(1001, 300);
	
	        Assert.IsFalse(_listener.GetAndClearIsInvoked());
	
	        SendWithdrawal(1004, 100);
	        Assert.IsTrue(_listener.GetAndClearIsInvoked());
	
	        Assert.AreEqual(1, _listener.LastNewData.Length);
	        EventBean _event = _listener.LastNewData[0];
	        Assert.AreEqual(1004L, _event["accountNumber"]);
	        Assert.AreEqual(FraudText, _event["warning"]);
	        Assert.AreEqual(100, _event["amount"]);
	        Assert.IsTrue( ((long) _event["timestamp"]) > (DateTimeHelper.CurrentTimeMillis - 100));
	        Assert.AreEqual("withdrawlFraudWarn", _event["descr"]);
	    }
	
	    private void SendWithdrawal(long acctNum, int amount)
	    {
	        Withdrawal eventBean = new Withdrawal(acctNum, amount);
	        _epService.EPRuntime.SendEvent(eventBean);
	    }
	
	    private void SendFraudWarn(long acctNum, String text)
	    {
	        FraudWarning eventBean = new FraudWarning(acctNum, text);
	        _epService.EPRuntime.SendEvent(eventBean);
	    }

	    public void Dispose()
	    {
	    }
	}
}
