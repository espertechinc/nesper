///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;

using NUnit.Framework;

namespace com.espertech.esper.supportregression.subscriber
{
	public abstract class SupportSubscriberRowByRowObjectArrayBase : SupportSubscriberBase
	{
	    private readonly List<object[]> _indicate = new List<object[]>();

	    protected SupportSubscriberRowByRowObjectArrayBase(bool requiresStatementDelivery)
            : base(requiresStatementDelivery)
        {
	    }

	    protected void AddIndication(object[] row)
	    {
	        _indicate.Add(row);
	    }

	    protected void AddIndication(EPStatement stmt, object[] row)
	    {
	        _indicate.Add(row);
	        AddStmtIndication(stmt);
	    }

	    public void AssertOneAndReset(EPStatement stmt, object[] expected)
        {
	        AssertStmtOneReceived(stmt);

	        Assert.AreEqual(1, _indicate.Count);
	        EPAssertionUtil.AssertEqualsAnyOrder(expected, _indicate[0]);

	        _indicate.Clear();
	        ResetStmts();
	    }
	}
} // end of namespace
