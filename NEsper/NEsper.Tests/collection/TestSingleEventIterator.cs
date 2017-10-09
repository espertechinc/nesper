///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using NUnit.Framework;

namespace com.espertech.esper.collection
{
    [TestFixture]
	public class TestSingleEventIterator  {
	    private SingleEventIterator iterator;
	    private EventBean eventBean;

        [SetUp]
	    public void SetUp() {
	        eventBean = SupportEventBeanFactory.CreateObject("a");
	        iterator = new SingleEventIterator(eventBean);
	    }

        [Test]
	    public void TestNext() {
	        Assert.AreEqual(eventBean, iterator.Next());
	        try {
	            iterator.Next();
	            TestCase.Fail();
	        } catch (NoSuchElementException ex) {
	            // Expected exception
	        }
	    }

        [Test]
	    public void TestHasNext() {
	        Assert.IsTrue(iterator.HasNext);
	        iterator.Next();
	        Assert.IsFalse(iterator.HasNext);
	    }

        [Test]
	    public void TestRemove() {
	        try {
	            iterator.Remove();
	            Assert.IsTrue(false);
	        } catch (UnsupportedOperationException ex) {
	            // Expected exception
	        }
	    }
	}
} // end of namespace
