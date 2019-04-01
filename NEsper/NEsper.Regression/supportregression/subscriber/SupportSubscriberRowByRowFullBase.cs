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
using com.espertech.esper.compat.collections;

using NUnit.Framework;

namespace com.espertech.esper.supportregression.subscriber
{
	public class SupportSubscriberRowByRowFullBase : SupportSubscriberBase
	{
	    private readonly List<UniformPair<int?>> _indicateStart = new List<UniformPair<int?>>();
	    private readonly List<object> _indicateEnd = new List<object>();
	    private readonly List<object[]> _indicateIStream = new List<object[]>();
	    private readonly List<object[]> _indicateRStream = new List<object[]>();

	    public SupportSubscriberRowByRowFullBase(bool requiresStatementDelivery)
            : base(requiresStatementDelivery)
        {
	    }

	    protected void AddUpdateStart(int lengthIStream, int lengthRStream)
	    {
	        _indicateStart.Add(new UniformPair<int?>(lengthIStream, lengthRStream));
	    }

	    protected void AddUpdate(object[] values)
	    {
	        _indicateIStream.Add(values);
	    }

	    protected void AddUpdateRStream(object[] values)
	    {
	        _indicateRStream.Add(values);
	    }

	    protected void AddUpdateEnd()
	    {
	        _indicateEnd.Add(this);
	    }

	    protected void AddUpdateStart(EPStatement statement, int lengthIStream, int lengthRStream)
	    {
	        _indicateStart.Add(new UniformPair<int?>(lengthIStream, lengthRStream));
	        AddStmtIndication(statement);
	    }

	    protected void AddUpdate(EPStatement statement, object[] values)
	    {
	        _indicateIStream.Add(values);
	        AddStmtIndication(statement);
	    }

	    protected void AddUpdateRStream(EPStatement statement, object[] values)
	    {
	        _indicateRStream.Add(values);
	        AddStmtIndication(statement);
	    }

	    protected void AddUpdateEnd(EPStatement statement)
	    {
	        _indicateEnd.Add(this);
	        AddStmtIndication(statement);
	    }

	    public void Reset()
        {
	        _indicateStart.Clear();
	        _indicateIStream.Clear();
	        _indicateRStream.Clear();
	        _indicateEnd.Clear();
	        ResetStmts();
	    }

	    public void AssertNoneReceived()
        {
	        Assert.IsTrue(_indicateStart.IsEmpty());
	        Assert.IsTrue(_indicateIStream.IsEmpty());
	        Assert.IsTrue(_indicateRStream.IsEmpty());
	        Assert.IsTrue(_indicateEnd.IsEmpty());
	    }

	    public void AssertOneReceivedAndReset(EPStatement stmt, int expectedLenIStream, int expectedLenRStream, object[][] expectedIStream, object[][] expectedRStream)
        {
	        int stmtCount = 2 + expectedLenIStream + expectedLenRStream;
	        AssertStmtMultipleReceived(stmt, stmtCount);

	        Assert.AreEqual(1, _indicateStart.Count);
	        UniformPair<int?> pairLength = _indicateStart[0];
	        Assert.AreEqual(expectedLenIStream, (int) pairLength.First);
	        Assert.AreEqual(expectedLenRStream, (int) pairLength.Second);

	        EPAssertionUtil.AssertEqualsExactOrder(expectedIStream, _indicateIStream);
	        EPAssertionUtil.AssertEqualsExactOrder(expectedRStream, _indicateRStream);

	        Assert.AreEqual(1, _indicateEnd.Count);

	        Reset();
	    }
	}
} // end of namespace
