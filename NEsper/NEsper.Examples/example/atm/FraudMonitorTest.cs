///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using NUnit.Framework;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;

namespace com.espertech.esper.example.atm
{
	[TestFixture]
	public class FraudMonitorTest : IDisposable
	{
		private EPServiceProvider epService;
		private SupportUpdateListener listener;

		[SetUp]
	    public void SetUp()
    	{
	        Configuration configuration = new Configuration();
	        configuration.AddEventType("FraudWarning", typeof(FraudWarning).FullName);
	        configuration.AddEventType("Withdrawal", typeof(Withdrawal).FullName);
		    configuration.EngineDefaults.EventMetaConfig.ClassPropertyResolutionStyle = PropertyResolutionStyle.CASE_INSENSITIVE;
	
	        epService = EPServiceProviderManager.GetProvider("FraudMonitorTest", configuration);
	        epService.EPRuntime.SendEvent(new TimerControlEvent(TimerControlEvent.ClockTypeEnum.CLOCK_EXTERNAL));
	
	        listener = new SupportUpdateListener();
	        new FraudMonitor(epService, listener.Update);
	    }
	    
		[Test]
		public void TestJoin()
	    {
	        const string FRAUD_TEXT = "card reported stolen";
	
	        sendWithdrawal(1001, 100);
	        sendFraudWarn(1004, FRAUD_TEXT);
	        sendWithdrawal(1001, 60);
	        sendWithdrawal(1002, 400);
	        sendWithdrawal(1001, 300);
	
	        Assert.IsFalse(listener.GetAndClearIsInvoked());
	
	        sendWithdrawal(1004, 100);
	        Assert.IsTrue(listener.GetAndClearIsInvoked());
	
	        Assert.AreEqual(1, listener.LastNewData.Length);
	        EventBean _event = listener.LastNewData[0];
	        Assert.AreEqual(1004L, _event["accountNumber"]);
	        Assert.AreEqual(FRAUD_TEXT, _event["warning"]);
	        Assert.AreEqual(100, _event["amount"]);
	        Assert.IsTrue( ((long) _event["timestamp"]) > (DateTimeHelper.CurrentTimeMillis - 100));
	        Assert.AreEqual("withdrawlFraudWarn", _event["descr"]);
	    }
	
	    private void sendWithdrawal(long acctNum, int amount)
	    {
	        Withdrawal eventBean = new Withdrawal(acctNum, amount);
	        epService.EPRuntime.SendEvent(eventBean);
	    }
	
	    private void sendFraudWarn(long acctNum, String text)
	    {
	        FraudWarning eventBean = new FraudWarning(acctNum, text);
	        epService.EPRuntime.SendEvent(eventBean);
	    }

	    public void Dispose()
	    {
	    }
	}
}
