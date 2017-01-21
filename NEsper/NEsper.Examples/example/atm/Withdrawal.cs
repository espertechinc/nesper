///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using com.espertech.esper.compat;

namespace com.espertech.esper.example.atm
{
	public class Withdrawal
	{
	    private long accountNumber;
	    private int amount;
	    private long timestamp;
	
	    public Withdrawal(long accountNumber, int amount)
	    {
	        this.accountNumber = accountNumber;
	        this.amount = amount;
	        timestamp = DateTimeHelper.CurrentTimeMillis;;
	    }
	
	    public long AccountNumber
	    {
	    	get { return accountNumber; }
	    }
	
	    public int Amount
	    {
	    	get { return amount; }
	    }
	
	    public long Timestamp
	    {
	    	get { return timestamp; }
	    }
	}
}
