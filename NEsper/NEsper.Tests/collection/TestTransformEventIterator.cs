///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;

using NUnit.Framework;

namespace com.espertech.esper.collection
{
    [TestFixture]
	public class TestTransformEventIterator
    {
	    private IEnumerator<EventBean> _enumerator;

        [Test]
	    public void TestEmpty() {
	        _enumerator = MakeEnumerator(new int[0]);
	        Assert.IsFalse(_enumerator.MoveNext());
	    }

        [Test]
	    public void TestOne() {
	        _enumerator = MakeEnumerator(new int[]{10});
            Assert.IsTrue(_enumerator.MoveNext());
	        Assert.AreEqual(10, _enumerator.Current.Get("Id"));
            Assert.IsFalse(_enumerator.MoveNext());
	    }

        [Test]
	    public void TestTwo() {
	        _enumerator = MakeEnumerator(new int[]{10, 20});
            Assert.IsTrue(_enumerator.MoveNext());
	        Assert.AreEqual(10, _enumerator.Current.Get("Id"));
            Assert.IsTrue(_enumerator.MoveNext());
	        Assert.AreEqual(20, _enumerator.Current.Get("Id"));
            Assert.IsFalse(_enumerator.MoveNext());
	    }

	    private IEnumerator<EventBean> MakeEnumerator(int[] values)
        {
	        IList<EventBean> events = new List<EventBean>();
	        for (int i = 0; i < values.Length; i++) {
	            SupportBean bean = new SupportBean();
	            bean.IntPrimitive = values[i];
	            EventBean theEvent = SupportEventBeanFactory.CreateObject(bean);
	            events.Add(theEvent);
	        }

	        return events
	            .Select(MyTransform)
	            .GetEnumerator();
	    }

        private EventBean MyTransform(EventBean theEvent) {
	        var value = theEvent.Get("IntPrimitive").AsInt();
	        return SupportEventBeanFactory.CreateObject(new SupportBean_S0(value));
	    }

	    public EventBean[] Transform(EventBean[] events) {
	        return new EventBean[0];
	    }
	}
} // end of namespace
