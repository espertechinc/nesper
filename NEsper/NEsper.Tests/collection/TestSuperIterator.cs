///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using NUnit.Framework;

namespace com.espertech.esper.collection
{
    [TestFixture]
	public class TestSuperIterator
    {
        [Test]
	    public void TestEmpty() {
	        SuperIterator<string> it = new SuperIterator<string>(Make(null), Make(null));
	        Assert.IsFalse(it.HasNext);
	        try {
	            it.Next();
	            Fail();
	        } catch (NoSuchElementException ex) {
	        }
	    }

        [Test]
	    public void TestFlow() {
	        SuperIterator<string> it = new SuperIterator<string>(Make("a"), Make(null));
	        EPAssertionUtil.AssertEqualsExactOrder(new object[]{"a"}, it);

	        it = new SuperIterator<string>(Make("a,b"), Make(null));
	        EPAssertionUtil.AssertEqualsExactOrder(new object[]{"a", "b"}, it);

	        it = new SuperIterator<string>(Make("a"), Make("b"));
	        EPAssertionUtil.AssertEqualsExactOrder(new object[]{"a", "b"}, it);

	        it = new SuperIterator<string>(Make(null), Make("a,b"));
	        EPAssertionUtil.AssertEqualsExactOrder(new object[]{"a", "b"}, it);
	    }

	    private IEnumerator<string> Make(string csv) {
	        if ((csv == null) || (csv.Length() == 0)) {
	            return new NullIterator<string>();
	        }
	        string[] fields = csv.SplitCsv();
	        return Arrays.AsList(fields).Iterator();
	    }

	}
} // end of namespace
