///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.threading.locks;
using com.espertech.esper.runtime.@internal.support;

using NUnit.Framework;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    [TestFixture]
    public class TestFilterParamIndexNotIn : AbstractTestBase
    {
        private SupportEventEvaluator[] testEvaluators;
        private SupportBean testBean;
        private EventBean testEventBean;
        private EventType testEventType;
        private IList<FilterHandle> matchesList;

        [SetUp]
        public void SetUp()
        {
            testEvaluators = new SupportEventEvaluator[4];
            for (int i = 0; i < testEvaluators.Length; i++)
            {
                testEvaluators[i] = new SupportEventEvaluator();
            }

            testBean = new SupportBean();
            testEventBean = SupportEventBeanFactory
                .GetInstance(container)
                .CreateObject(testBean);
            testEventType = testEventBean.EventType;
            matchesList = new List<FilterHandle>();
        }

        [Test]
        public void TestIndex()
        {
            FilterParamIndexNotIn index = new FilterParamIndexNotIn(MakeLookupable("longBoxed"), new SlimReaderWriterLock());
            Assert.AreEqual(FilterOperator.NOT_IN_LIST_OF_VALUES, index.FilterOperator);

            index.Put(new HashableMultiKey(new object[] { 2L, 5L }), testEvaluators[0]);
            index.Put(new HashableMultiKey(new object[] { 3L, 4L, 5L }), testEvaluators[1]);
            index.Put(new HashableMultiKey(new object[] { 1L, 4L, 5L }), testEvaluators[2]);
            index.Put(new HashableMultiKey(new object[] { 2L, 5L }), testEvaluators[3]);

            Verify(index, 0L, new bool[] { true, true, true, true });
            Verify(index, 1L, new bool[] { true, true, false, true });
            Verify(index, 2L, new bool[] { false, true, true, false });
            Verify(index, 3L, new bool[] { true, false, true, true });
            Verify(index, 4L, new bool[] { true, false, false, true });
            Verify(index, 5L, new bool[] { false, false, false, false });
            Verify(index, 6L, new bool[] { true, true, true, true });

            HashableMultiKey inList = new HashableMultiKey(new object[] { 3L, 4L, 5L });
            Assert.AreEqual(testEvaluators[1], index.Get(inList));
            Assert.IsTrue(index.ReadWriteLock != null);
            index.Remove(inList);
            index.Remove(inList);
            Assert.AreEqual(null, index.Get(inList));

            // now that {3,4,5} is removed, verify results again
            Verify(index, 0L, new bool[] { true, false, true, true });
            Verify(index, 1L, new bool[] { true, false, false, true });
            Verify(index, 2L, new bool[] { false, false, true, false });
            Verify(index, 3L, new bool[] { true, false, true, true });
            Verify(index, 4L, new bool[] { true, false, false, true });
            Verify(index, 5L, new bool[] { false, false, false, false });
            Verify(index, 6L, new bool[] { true, false, true, true });

            try
            {
                index.Put("a", testEvaluators[0]);
                Assert.IsTrue(false);
            }
            catch (Exception ex)
            {
                // Expected
            }
        }

        private void Verify(FilterParamIndexBase index, long? testValue, bool[] expected)
        {
            testBean.LongBoxed = testValue;
            index.MatchEvent(testEventBean, matchesList);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], testEvaluators[i].GetAndResetCountInvoked() == 1);
            }
        }

        private ExprFilterSpecLookupable MakeLookupable(string fieldName)
        {
            return new ExprFilterSpecLookupable(fieldName, testEventType.GetGetter(fieldName), testEventType.GetPropertyType(fieldName), false);
        }
    }
} // end of namespace