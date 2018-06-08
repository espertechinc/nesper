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
using com.espertech.esper.collection;

using NUnit.Framework;

namespace com.espertech.esper.supportregression.subscriber
{
	public abstract class SupportSubscriberMultirowUnderlyingBase : SupportSubscriberBase
	{
	    private readonly List<UniformPair<object[]>> _indicate = new List<UniformPair<object[]>>();

	    protected SupportSubscriberMultirowUnderlyingBase(bool requiresStatementDelivery)
            : base(requiresStatementDelivery)
        {
	    }

	    public void AddIndication(object[] newEvents, object[] oldEvents)
	    {
	        _indicate.Add(new UniformPair<object[]>(newEvents, oldEvents));
	    }

	    public void AddIndication(EPStatement stmt, object[]newEvents, object[] oldEvents)
	    {
	        _indicate.Add(new UniformPair<object[]>(newEvents, oldEvents));
	        AddStmtIndication(stmt);
	    }

	    public void AssertOneReceivedAndReset(EPStatement stmt, object[] firstExpected, object[] secondExpected)
        {
	        AssertStmtOneReceived(stmt);

	        Assert.AreEqual(1, _indicate.Count);
	        UniformPair<object[]> result = _indicate[0];
	        AssertValues(firstExpected, result.First);
	        AssertValues(secondExpected, result.Second);

	        Reset();
	    }

	    private void AssertValues(object[] expected, object[] received)
        {
	        if (expected == null)
            {
	            Assert.IsNull(received);
	            return;
	        }
	        EPAssertionUtil.AssertEqualsExactOrder(expected, received);
	    }

	    private void Reset()
        {
	        ResetStmts();
	        _indicate.Clear();
	    }
	}
} // end of namespace
