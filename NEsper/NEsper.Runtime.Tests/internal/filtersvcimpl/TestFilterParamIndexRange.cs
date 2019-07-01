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
    public class TestFilterParamIndexRange : AbstractTestBase
    {
        private SupportEventEvaluator testEvaluator;
        private SupportBean testBean;
        private EventBean testEventBean;
        private EventType testEventType;
        private IList<FilterHandle> matchesList;
        private DoubleRange testRange;

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

            testRange = new DoubleRange(10d, 20d);
        }

        [Test]
        public void TestLongBothEndpointsIncluded()
        {
            FilterParamIndexDoubleRange index = this.GetLongDataset(FilterOperator.RANGE_CLOSED);
            VerifyLongPrimitive(index, -1, 0);
            VerifyLongPrimitive(index, 0, 2);
            VerifyLongPrimitive(index, 1, 5);
            VerifyLongPrimitive(index, 2, 5);
            VerifyLongPrimitive(index, 3, 7);
            VerifyLongPrimitive(index, 4, 6);
            VerifyLongPrimitive(index, 5, 6);
            VerifyLongPrimitive(index, 6, 6);
            VerifyLongPrimitive(index, 7, 6);
            VerifyLongPrimitive(index, 8, 6);
            VerifyLongPrimitive(index, 9, 5);
            VerifyLongPrimitive(index, 10, 3);
            VerifyLongPrimitive(index, 11, 1);

            index.Put(testRange, testEvaluator);
            Assert.AreEqual(testEvaluator, index.Get(testRange));
            Assert.IsTrue(index.ReadWriteLock != null);
            index.Remove(testRange);
            index.Remove(testRange);
            Assert.AreEqual(null, index.Get(testRange));
            Assert.That(() => index.Put("a", testEvaluator), Throws.ArgumentException);
        }

        [Test]
        public void TestLongLowEndpointIncluded()
        {
            FilterParamIndexDoubleRange index = this.GetLongDataset(FilterOperator.RANGE_HALF_OPEN);
            VerifyLongPrimitive(index, -1, 0);
            VerifyLongPrimitive(index, 0, 2);
            VerifyLongPrimitive(index, 1, 5);
            VerifyLongPrimitive(index, 2, 5);
            VerifyLongPrimitive(index, 3, 6);
            VerifyLongPrimitive(index, 4, 6);
            VerifyLongPrimitive(index, 5, 3);
            VerifyLongPrimitive(index, 6, 5);
            VerifyLongPrimitive(index, 7, 4);
            VerifyLongPrimitive(index, 8, 5);
            VerifyLongPrimitive(index, 9, 3);
            VerifyLongPrimitive(index, 10, 1);
            VerifyLongPrimitive(index, 11, 1);
        }

        [Test]
        public void TestLongHighEndpointIncluded()
        {
            FilterParamIndexDoubleRange index = this.GetLongDataset(FilterOperator.RANGE_HALF_CLOSED);
            VerifyLongPrimitive(index, -1, 0);
            VerifyLongPrimitive(index, 0, 0);
            VerifyLongPrimitive(index, 1, 2);
            VerifyLongPrimitive(index, 2, 5);
            VerifyLongPrimitive(index, 3, 5);
            VerifyLongPrimitive(index, 4, 6);
            VerifyLongPrimitive(index, 5, 6);
            VerifyLongPrimitive(index, 6, 3);
            VerifyLongPrimitive(index, 7, 5);
            VerifyLongPrimitive(index, 8, 4);
            VerifyLongPrimitive(index, 9, 5);
            VerifyLongPrimitive(index, 10, 3);
            VerifyLongPrimitive(index, 11, 1);
        }

        [Test]
        public void TestLongNeitherEndpointIncluded()
        {
            FilterParamIndexDoubleRange index = this.GetLongDataset(FilterOperator.RANGE_OPEN);
            VerifyLongPrimitive(index, -1, 0);
            VerifyLongPrimitive(index, 0, 0);
            VerifyLongPrimitive(index, 1, 2);
            VerifyLongPrimitive(index, 2, 5);
            VerifyLongPrimitive(index, 3, 4);
            VerifyLongPrimitive(index, 4, 6);
            VerifyLongPrimitive(index, 5, 3);
            VerifyLongPrimitive(index, 6, 2);
            VerifyLongPrimitive(index, 7, 3);
            VerifyLongPrimitive(index, 8, 3);
            VerifyLongPrimitive(index, 9, 3);
            VerifyLongPrimitive(index, 10, 1);
            VerifyLongPrimitive(index, 11, 1);
        }

        [Test]
        public void TestDoubleBothEndpointsIncluded()
        {
            FilterParamIndexDoubleRange index = this.GetDoubleDataset(FilterOperator.RANGE_CLOSED);
            VerifyDoublePrimitive(index, 1.49, 0);
            VerifyDoublePrimitive(index, 1.5, 1);
            VerifyDoublePrimitive(index, 2.5, 1);
            VerifyDoublePrimitive(index, 2.51, 0);
            VerifyDoublePrimitive(index, 3.5, 2);
            VerifyDoublePrimitive(index, 4.4, 2);
            VerifyDoublePrimitive(index, 4.5, 2);
            VerifyDoublePrimitive(index, 4.5001, 1);
            VerifyDoublePrimitive(index, 5.1, 1);
            VerifyDoublePrimitive(index, 5.8, 2);
            VerifyDoublePrimitive(index, 6.7, 2);
            VerifyDoublePrimitive(index, 6.8, 1);
            VerifyDoublePrimitive(index, 9.5, 1);
            VerifyDoublePrimitive(index, 10.1, 0);
        }

        [Test]
        public void TestDoubleFixedRangeSize()
        {
            FilterParamIndexDoubleRange index = MakeOne("DoublePrimitive", FilterOperator.RANGE_OPEN, testEventType);

            for (int i = 0; i < 10000; i++)
            {
                DoubleRange range = new DoubleRange(i, i + 1);
                index.Put(range, testEvaluator);
            }

            VerifyDoublePrimitive(index, 5000, 0);
            VerifyDoublePrimitive(index, 5000.5, 1);
            VerifyDoublePrimitive(index, 5001, 0);
        }

        [Test]
        public void TestDoubleVariableRangeSize()
        {
            FilterParamIndexDoubleRange index = MakeOne("DoublePrimitive", FilterOperator.RANGE_CLOSED, testEventType);

            for (int i = 0; i < 100; i++)
            {
                DoubleRange range = new DoubleRange(i, i * 2);
                index.Put(range, testEvaluator);
            }

            // 1 to 2
            // 2 to 4
            // 3 to 6
            // and so on

            VerifyDoublePrimitive(index, 1, 1);
            VerifyDoublePrimitive(index, 2, 2);
            VerifyDoublePrimitive(index, 2.001, 1);
            VerifyDoublePrimitive(index, 3, 2);
            VerifyDoublePrimitive(index, 4, 3);
            VerifyDoublePrimitive(index, 4.5, 2);
            VerifyDoublePrimitive(index, 50, 26);
        }

        private FilterParamIndexDoubleRange GetLongDataset(FilterOperator operatorType)
        {
            FilterParamIndexDoubleRange index = MakeOne("LongPrimitive", operatorType, testEventType);

            AddToIndex(index, 0, 5);
            AddToIndex(index, 0, 6);
            AddToIndex(index, 1, 3);
            AddToIndex(index, 1, 5);
            AddToIndex(index, 1, 7);
            AddToIndex(index, 3, 5);
            AddToIndex(index, 3, 7);
            AddToIndex(index, 6, 9);
            AddToIndex(index, 6, 10);
            AddToIndex(index, 6, Int32.MaxValue - 1);
            AddToIndex(index, 7, 8);
            AddToIndex(index, 8, 9);
            AddToIndex(index, 8, 10);

            return index;
        }

        private FilterParamIndexDoubleRange GetDoubleDataset(FilterOperator operatorType)
        {
            FilterParamIndexDoubleRange index = MakeOne("DoublePrimitive", operatorType, testEventType);

            AddToIndex(index, 1.5, 2.5);
            AddToIndex(index, 3.5, 4.5);
            AddToIndex(index, 3.5, 9.5);
            AddToIndex(index, 5.6, 6.7);

            return index;
        }

        private FilterParamIndexDoubleRange MakeOne(string fieldName, FilterOperator operatorType, EventType testEventType)
        {
            return new FilterParamIndexDoubleRange(MakeLookupable(fieldName), new SlimReaderWriterLock(), operatorType);
        }

        private void VerifyDoublePrimitive(FilterParamIndexBase index, double testValue, int numExpected)
        {
            testBean.DoublePrimitive = testValue;
            index.MatchEvent(testEventBean, matchesList);
            Assert.AreEqual(numExpected, testEvaluator.GetAndResetCountInvoked());
        }

        private void VerifyLongPrimitive(FilterParamIndexBase index, long testValue, int numExpected)
        {
            testBean.LongPrimitive = testValue;
            index.MatchEvent(testEventBean, matchesList);
            Assert.AreEqual(numExpected, testEvaluator.GetAndResetCountInvoked());
        }

        private void AddToIndex(FilterParamIndexDoubleRange index, double min, double max)
        {
            DoubleRange r = new DoubleRange(min, max);
            index.Put(r, testEvaluator);
        }

        private ExprFilterSpecLookupable MakeLookupable(string fieldName)
        {
            return new ExprFilterSpecLookupable(fieldName, testEventType.GetGetter(fieldName), testEventType.GetPropertyType(fieldName), false);
        }
    }
} // end of namespace
