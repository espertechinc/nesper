///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.runtime.client;

using NEsper.Examples.Support;

namespace NEsper.Examples.ATM
{
	/// <summary>
	/// Demonstrates a simple join between fraud warning and withdrawal event streams.
	/// See the unit test with the same name for any example events generated to test
	/// this example.
	/// </summary>
	public class FraudMonitor
	{
	    private EPStatement joinView;
	
	    public FraudMonitor(EPRuntime runtime, EventHandler<UpdateEventArgs> eventHandler)
	    {
	        string joinStatement = "select fraud.accountNumber as accountNumber, fraud.warning as warning, withdraw.amount as amount, " +
	                               "max(fraud.timestamp, withdraw.timestamp) as timestamp, 'withdrawlFraudWarn' as descr from " +
	                                    "FraudWarning.win:time(30 min) as fraud," +
	                                    "Withdrawal.win:time(30 sec) as withdraw" +
	                " where fraud.accountNumber = withdraw.accountNumber";
	
	        joinView = runtime.DeployStatement(joinStatement);
	        joinView.Events += eventHandler;
	    }
	}
}
