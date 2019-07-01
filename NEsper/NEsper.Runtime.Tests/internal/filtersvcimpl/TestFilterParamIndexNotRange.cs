///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

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
    public class TestFilterParamIndexNotRange : AbstractTestBase
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
        public void TestClosedRange()
        {
            FilterParamIndexDoubleRangeInverted index = MakeOne("LongBoxed", FilterOperator.NOT_RANGE_CLOSED, testEventType);
            Assert.AreEqual(FilterOperator.NOT_RANGE_CLOSED, index.FilterOperator);

            index.Put(new DoubleRange(2d, 4d), testEvaluators[0]);
            index.Put(new DoubleRange(2d, 5d), testEvaluators[1]);
            index.Put(new DoubleRange(1d, 3d), testEvaluators[2]);
            index.Put(new DoubleRange(1d, 1d), testEvaluators[3]);

            Verify(index, 0L, new bool[] { true, true, true, true });
            Verify(index, 1L, new bool[] { true, true, false, false });
            Verify(index, 2L, new bool[] { false, false, false, true });
            Verify(index, 3L, new bool[] { false, false, false, true });
            Verify(index, 4L, new bool[] { false, false, true, true });
            Verify(index, 5L, new bool[] { true, false, true, true });
            Verify(index, 6L, new bool[] { true, true, true, true });
        }

        [Test]
        public void TestOpenRange()
        {
            FilterParamIndexDoubleRangeInverted index = MakeOne("LongBoxed", FilterOperator.NOT_RANGE_OPEN, testEventType);

            index.Put(new DoubleRange(2d, 4d), testEvaluators[0]);
            index.Put(new DoubleRange(2d, 5d), testEvaluators[1]);
            index.Put(new DoubleRange(1d, 3d), testEvaluators[2]);
            index.Put(new DoubleRange(1d, 1d), testEvaluators[3]);

            Verify(index, 0L, new bool[] { true, true, true, true });
            Verify(index, 1L, new bool[] { true, true, true, true });
            Verify(index, 2L, new bool[] { true, true, false, true });
            Verify(index, 3L, new bool[] { false, false, true, true });
            Verify(index, 4L, new bool[] { true, false, true, true });
            Verify(index, 5L, new bool[] { true, true, true, true });
            Verify(index, 6L, new bool[] { true, true, true, true });
        }

        [Test]
        public void TestHalfOpenRange()
        {
            FilterParamIndexDoubleRangeInverted index = MakeOne("LongBoxed", FilterOperator.NOT_RANGE_HALF_OPEN, testEventType);

            index.Put(new DoubleRange(2d, 4d), testEvaluators[0]);
            index.Put(new DoubleRange(2d, 5d), testEvaluators[1]);
            index.Put(new DoubleRange(1d, 3d), testEvaluators[2]);
            index.Put(new DoubleRange(1d, 1d), testEvaluators[3]);

            Verify(index, 0L, new bool[] { true, true, true, true });
            Verify(index, 1L, new bool[] { true, true, false, true });
            Verify(index, 2L, new bool[] { false, false, false, true });
            Verify(index, 3L, new bool[] { false, false, true, true });
            Verify(index, 4L, new bool[] { true, false, true, true });
            Verify(index, 5L, new bool[] { true, true, true, true });
            Verify(index, 6L, new bool[] { true, true, true, true });
        }

        [Test]
        public void TestHalfClosedRange()
        {
            FilterParamIndexDoubleRangeInverted index = MakeOne("LongBoxed", FilterOperator.NOT_RANGE_HALF_CLOSED, testEventType);

            index.Put(new DoubleRange(2d, 4d), testEvaluators[0]);
            index.Put(new DoubleRange(2d, 5d), testEvaluators[1]);
            index.Put(new DoubleRange(1d, 3d), testEvaluators[2]);
            index.Put(new DoubleRange(1d, 1d), testEvaluators[3]);

            Verify(index, 0L, new bool[] { true, true, true, true });
            Verify(index, 1L, new bool[] { true, true, true, true });
            Verify(index, 2L, new bool[] { true, true, false, true });
            Verify(index, 3L, new bool[] { false, false, false, true });
            Verify(index, 4L, new bool[] { false, false, true, true });
            Verify(index, 5L, new bool[] { true, false, true, true });
            Verify(index, 6L, new bool[] { true, true, true, true });
        }

        private FilterParamIndexDoubleRangeInverted MakeOne(string field, FilterOperator notRangeHalfClosed, EventType testEventType)
        {
            return new FilterParamIndexDoubleRangeInverted(
                MakeLookupable(field), new SlimReaderWriterLock(), notRangeHalfClosed);
        }

        private void Verify(FilterParamIndexBase index, long? testValue, bool[] expected)
        {
            testBean.LongBoxed = testValue;
            index.MatchEvent(testEventBean, matchesList);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], testEvaluators[i].GetAndResetCountInvoked() == 1, "Unexpected result for eval " + i);
            }
        }

        private ExprFilterSpecLookupable MakeLookupable(string fieldName)
        {
            return new ExprFilterSpecLookupable(fieldName, testEventType.GetGetter(fieldName), testEventType.GetPropertyType(fieldName), false);
        }
    }
} // end of namespace
