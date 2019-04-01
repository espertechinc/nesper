///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.epl.join.exec.@base;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.supportunit.events;

using NUnit.Framework;

namespace com.espertech.esper.epl.join.exec
{
    [TestFixture]
    public class TestNestedIterationExecNode 
    {
        private NestedIterationExecNode _exec;
        private EventBean[][] _streamEvents;
    
        [SetUp]
        public void SetUp()
        {
            UnindexedEventTable[] indexes = new UnindexedEventTable[4];
            for (int i = 0; i < indexes.Length; i++)
            {
                indexes[i] = new UnindexedEventTableImpl(0);
            }
    
            _exec = new NestedIterationExecNode(new int[] {3, 0, 1});
            _exec.AddChildNode(new TableLookupExecNode(3, new FullTableScanLookupStrategy(indexes[3])));
            _exec.AddChildNode(new TableLookupExecNode(0, new FullTableScanLookupStrategy(indexes[0])));
            _exec.AddChildNode(new TableLookupExecNode(1, new FullTableScanLookupStrategy(indexes[1])));
    
            _streamEvents = new EventBean[4][];
            _streamEvents[0] = SupportEventBeanFactory.MakeEvents_A(new String[] {"a1", "a2"});
            _streamEvents[1] = SupportEventBeanFactory.MakeEvents_B(new String[] {"b1", "b2"});
            _streamEvents[2] = SupportEventBeanFactory.MakeEvents_C(new String[] {"c1", "c2"});
            _streamEvents[3] = SupportEventBeanFactory.MakeEvents_D(new String[] {"d1", "d2"});
    
            // Fill with data
            indexes[0].Add(_streamEvents[0], null);
            indexes[1].Add(_streamEvents[1], null);
            indexes[2].Add(_streamEvents[2], null);
            indexes[3].Add(_streamEvents[3], null);
        }
    
        [Test]
        public void TestLookup()
        {
            var result = new List<EventBean[]>();
            var prefill = new EventBean[4];
            prefill[2] = _streamEvents[2][0];
    
            _exec.Process(_streamEvents[2][0], prefill, result, null);
    
            Assert.AreEqual(8, result.Count);
    
            EventBean[][] received = MakeArray(result);
            EventBean[][] expected = MakeExpected();
            EPAssertionUtil.AssertEqualsAnyOrder(expected, received);
        }
    
        private EventBean[][] MakeExpected()
        {
            EventBean[][] expected = new EventBean[8][];
            int count = 0;
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        expected[count] = new EventBean[4];
                        expected[count][0] = _streamEvents[0][i];
                        expected[count][1] = _streamEvents[1][j];
                        expected[count][2] = _streamEvents[2][0];
                        expected[count][3] = _streamEvents[3][k];
                        count++;
                    }
                }
            }
            return expected;
        }
    
        private EventBean[][] MakeArray(IList<EventBean[]> eventArrList)
        {
            EventBean[][] result = new EventBean[eventArrList.Count][];
            for (int i = 0; i < eventArrList.Count; i++)
            {
                result[i] = eventArrList[i];
            }
            return result;
        }
    }
    
        // Result
        /* 8 combinations
        d1
            a1
                b1
                b2
            a2
                b1
                b2
        d2
            a1
                b1
                b2
            a2
                b1
                b2
        */
}
