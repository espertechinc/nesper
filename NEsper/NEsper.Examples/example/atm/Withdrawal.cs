///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;

namespace NEsper.Examples.ATM
{
	public class Withdrawal
	{
	    public Withdrawal(long accountNumber, int amount)
	    {
	        AccountNumber = accountNumber;
	        Amount = amount;
	        Timestamp = DateTimeHelper.CurrentTimeMillis;
	    }
	
	    public long AccountNumber { get; }

	    public int Amount { get; }

	    public long Timestamp { get; }
	}
}
