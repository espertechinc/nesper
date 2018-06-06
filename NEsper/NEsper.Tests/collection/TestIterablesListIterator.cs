///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.supportunit.events;

using NUnit.Framework;

namespace com.espertech.esper.collection
{
    [TestFixture]
    public class TestIterablesListIterator
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            _events = EventFactoryHelper.MakeEventMap(new[] {"a", "b", "c", "d", "e", "f", "g", "h", "i", "z"});
        }

        #endregion

        private IDictionary<String, EventBean> _events;

        [Test]
        public void TestIterator()
        {
            IList<IEnumerable<EventBean>> enumerableList = new List<IEnumerable<EventBean>>();
            enumerableList.Add(EventFactoryHelper.MakeList(_events, new[] {"a", "b", "c"}));
            CheckResults(enumerableList, EventFactoryHelper.MakeArray(_events, new[] {"a", "b", "c"}));

            enumerableList = new List<IEnumerable<EventBean>>();
            enumerableList.Add(EventFactoryHelper.MakeList(_events, new[] {"a"}));
            enumerableList.Add(EventFactoryHelper.MakeList(_events, new[] {"b"}));
            enumerableList.Add(EventFactoryHelper.MakeList(_events, new[] {"c"}));
            CheckResults(enumerableList, EventFactoryHelper.MakeArray(_events, new[] {"a", "b", "c"}));

            enumerableList = new List<IEnumerable<EventBean>>();
            enumerableList.Add(EventFactoryHelper.MakeList(_events, new[] {"a", "b"}));
            enumerableList.Add(EventFactoryHelper.MakeList(_events, new[] {"c"}));
            CheckResults(enumerableList, EventFactoryHelper.MakeArray(_events, new[] {"a", "b", "c"}));

            enumerableList = new List<IEnumerable<EventBean>>();
            enumerableList.Add(EventFactoryHelper.MakeList(_events, new String[] {}));
            enumerableList.Add(EventFactoryHelper.MakeList(_events, new[] {"a", "b"}));
            enumerableList.Add(EventFactoryHelper.MakeList(_events, new String[] {}));
            enumerableList.Add(EventFactoryHelper.MakeList(_events, new[] {"c"}));
            enumerableList.Add(EventFactoryHelper.MakeList(_events, new String[] {}));
            CheckResults(enumerableList, EventFactoryHelper.MakeArray(_events, new[] {"a", "b", "c"}));

            enumerableList = new List<IEnumerable<EventBean>>();
            enumerableList.Add(EventFactoryHelper.MakeList(_events, new String[] {}));
            CheckResults(enumerableList, null);

            enumerableList = new List<IEnumerable<EventBean>>();
            enumerableList.Add(EventFactoryHelper.MakeList(_events, new String[] {}));
            enumerableList.Add(EventFactoryHelper.MakeList(_events, new String[] {}));
            enumerableList.Add(EventFactoryHelper.MakeList(_events, new String[] {}));
            CheckResults(enumerableList, null);

            enumerableList = new List<IEnumerable<EventBean>>();
            enumerableList.Add(EventFactoryHelper.MakeList(_events, new String[] {}));
            enumerableList.Add(EventFactoryHelper.MakeList(_events, new String[] {}));
            enumerableList.Add(EventFactoryHelper.MakeList(_events, new String[] {}));
            enumerableList.Add(EventFactoryHelper.MakeList(_events, new[] {"d"}));
            CheckResults(enumerableList, EventFactoryHelper.MakeArray(_events, new[] {"d"}));

            enumerableList = new List<IEnumerable<EventBean>>();
            enumerableList.Add(EventFactoryHelper.MakeList(_events, new[] {"d"}));
            enumerableList.Add(EventFactoryHelper.MakeList(_events, new String[] {}));
            enumerableList.Add(EventFactoryHelper.MakeList(_events, new String[] {}));
            enumerableList.Add(EventFactoryHelper.MakeList(_events, new String[] {}));
            CheckResults(enumerableList, EventFactoryHelper.MakeArray(_events, new[] {"d"}));

            enumerableList = new List<IEnumerable<EventBean>>();
            enumerableList.Add(EventFactoryHelper.MakeList(_events, new[] {"a", "b", "c"}));
            enumerableList.Add(EventFactoryHelper.MakeList(_events, new[] {"d"}));
            enumerableList.Add(EventFactoryHelper.MakeList(_events, new String[] {}));
            enumerableList.Add(EventFactoryHelper.MakeList(_events, new[] {"e", "f"}));
            enumerableList.Add(EventFactoryHelper.MakeList(_events, new[] {"g"}));
            enumerableList.Add(EventFactoryHelper.MakeList(_events, new String[] {}));
            enumerableList.Add(EventFactoryHelper.MakeList(_events, new[] {"h", "i"}));
            enumerableList.Add(EventFactoryHelper.MakeList(_events, new[] {"z"}));
            CheckResults(enumerableList,
                         EventFactoryHelper.MakeArray(_events, new[] {"a", "b", "c", "d", "e", "f", "g", "h", "i", "z"}));

            enumerableList = new List<IEnumerable<EventBean>>();
            CheckResults(enumerableList, null);
        }

        [Test]
        public void TestRemove()
        {
            IList<IEnumerable<EventBean>> enumerableList = new List<IEnumerable<EventBean>>();
            enumerableList.Add(EventFactoryHelper.MakeList(_events, new[] {"a", "b", "c"}));
            var enumerator = enumerableList.SelectMany(e => e).GetEnumerator();

            try {
                enumerator.Reset();
                Assert.IsTrue(false);
            }
            catch (NotSupportedException) {
                // Expected
            }
        }

        private void CheckResults(IEnumerable<IEnumerable<EventBean>> enumerableList, EventBean[] expectedValues)
        {
            var iterator = enumerableList.SelectMany(e => e).GetEnumerator();
            EPAssertionUtil.AssertEqualsExactOrder(expectedValues, iterator);
        }
    }
}
