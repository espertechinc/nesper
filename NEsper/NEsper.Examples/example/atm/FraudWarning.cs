///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using com.espertech.esper.compat;

namespace com.espertech.esper.example.atm
{
	public class FraudWarning
	{
	    private long accountNumber;
	    private string warning;
	    private long timestamp;
	
	    public FraudWarning(long accountNumber, string warning)
	    {
	        this.accountNumber = accountNumber;
	        this.warning = warning;
	        this.timestamp = DateTimeHelper.CurrentTimeMillis;
	    }

	    public long AccountNumber
	    {
	    	get { return accountNumber; }
	    }
	
	    public string Warning
	    {
	    	get { return warning; }
	    }
	
	    public long Timestamp
	    {
	    	get { return timestamp; }
	    }
	}
}
