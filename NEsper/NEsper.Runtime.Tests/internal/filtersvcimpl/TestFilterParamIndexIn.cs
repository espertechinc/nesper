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
    public class TestFilterParamIndexIn : AbstractRuntimeTest
    {
        private SupportEventEvaluator testEvaluator;
        private SupportBean testBean;
        private EventBean testEventBean;
        private EventType testEventType;
        private IList<FilterHandle> matchesList;

        [SetUp]
        public void SetUp()
        {
            testEvaluator = new SupportEventEvaluator();
            testBean = new SupportBean();
            testEventBean = SupportEventBeanFactory
                .GetInstance(container)
                .CreateObject(testBean);
            testEventType = testEventBean.EventType;
            matchesList = new List<FilterHandle>();
        }

        [Test, RunInApplicationDomain]
        public void TestIndex()
        {
            FilterParamIndexIn index = new FilterParamIndexIn(
                MakeLookupable("LongBoxed"),
                new SlimReaderWriterLock());
            Assert.AreEqual(FilterOperator.IN_LIST_OF_VALUES, index.FilterOperator);

            HashableMultiKey inList = new HashableMultiKey(new object[] { 2L, 5L });
            index.Put(inList, testEvaluator);
            inList = new HashableMultiKey(new object[] { 10L, 5L });
            index.Put(inList, testEvaluator);

            Verify(index, 1L, 0);
            Verify(index, 2L, 1);
            Verify(index, 5L, 2);
            Verify(index, 10L, 1);
            Verify(index, 999L, 0);
            Verify(index, null, 0);

            Assert.AreEqual(testEvaluator, index.Get(inList));
            Assert.IsTrue(index.ReadWriteLock != null);
            index.Remove(inList);
            index.Remove(inList);
            Assert.AreEqual(null, index.Get(inList));

            try
            {
                index.Put("a", testEvaluator);
                Assert.IsTrue(false);
            }
            catch (Exception)
            {
                // Expected
            }
        }

        private void Verify(FilterParamIndexBase index, long? testValue, int numExpected)
        {
            testBean.LongBoxed = testValue;
            index.MatchEvent(testEventBean, matchesList, TODO);
            Assert.AreEqual(numExpected, testEvaluator.GetAndResetCountInvoked());
        }

        private ExprFilterSpecLookupable MakeLookupable(string fieldName)
        {
            return new ExprFilterSpecLookupable(fieldName, testEventType.GetGetter(fieldName), testEventType.GetPropertyType(fieldName), false);
        }
    }
} // end of namespace
