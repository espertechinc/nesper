///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.@join.exec.@base;
using com.espertech.esper.epl.@join.plan;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;

using NUnit.Framework;

namespace com.espertech.esper.epl.join.@base
{
    [TestFixture]
    public class TestJoinSetComposerImpl 
    {
        private JoinSetComposerImpl _joinSetComposerImpl;
        private EventBean[] _indexedEventOne, _indexedEventTwo, _newEventOne, _newEventTwo;
        private UnindexedEventTable _indexLeft;
        private UnindexedEventTable _indexRight;
    
        [SetUp]
        public void SetUp()
        {
            _indexedEventOne = SupportEventBeanFactory.MakeEvents(new String[] { "s1_1", "s1_2"});
            _indexedEventTwo = SupportEventBeanFactory.MakeEvents(new String[] { "s2_1", "s2_2"});
    
            _newEventOne = SupportEventBeanFactory.MakeEvents(new String[] { "s1_3"});
            _newEventTwo = SupportEventBeanFactory.MakeEvents(new String[] { "s2_3"});
    
            _indexLeft = new UnindexedEventTableImpl(1);
            _indexLeft.Add(_indexedEventOne, null);
            _indexRight = new UnindexedEventTableImpl(1);
            _indexRight.Add(_indexedEventTwo, null);
    
            var queryStrategies = new QueryStrategy[2];
            var lookupLeft = new TableLookupExecNode(1, new FullTableScanLookupStrategy(_indexRight));
            var lookupRight = new TableLookupExecNode(0, new FullTableScanLookupStrategy(_indexLeft));
            queryStrategies[0] = new ExecNodeQueryStrategy(0, 2, lookupLeft);
            queryStrategies[1] = new ExecNodeQueryStrategy(1, 2, lookupRight);

            var indexes = new IDictionary<TableLookupIndexReqKey, EventTable>[2];
            indexes[0] = new NullableDictionary<TableLookupIndexReqKey, EventTable>();
            indexes[1] = new NullableDictionary<TableLookupIndexReqKey, EventTable>();
            indexes[0].Put(new TableLookupIndexReqKey("idxLeft"), _indexLeft);
            indexes[1].Put(new TableLookupIndexReqKey("idxLeft"), _indexRight);
    
            _joinSetComposerImpl = new JoinSetComposerImpl(true, indexes, queryStrategies, false, null, true);
        }
    
        [Test]
        public void TestJoin()
        {
            // Should return all possible combinations, not matching performed, remember: duplicate pairs have been removed
            var result = _joinSetComposerImpl.Join(
                    new EventBean[][] {_newEventOne, _newEventTwo},                 // new left and right
                    new EventBean[][] {new EventBean[] {_indexedEventOne[0]}, new EventBean[] {_indexedEventTwo[1]}} // old left and right
                    ,null);
    
            Assert.AreEqual(3, result.First.Count);      // check old events joined
            var eventStringText = ToString(result.Second);
            Assert.IsTrue(eventStringText.Contains("s1_1|s2_1"));
            Assert.IsTrue(eventStringText.Contains("s1_1|s2_2"));
            Assert.IsTrue(eventStringText.Contains("s1_2|s2_2"));
    
            // check new events joined, remember: duplicate pairs have been removed
            Assert.AreEqual(3, result.Second.Count);
            eventStringText = ToString(result.First);
            Assert.IsTrue(eventStringText.Contains("s1_3|s2_1"));
            Assert.IsTrue(eventStringText.Contains("s1_3|s2_3"));
            Assert.IsTrue(eventStringText.Contains("s1_2|s2_3"));
        }
    
        private String ToString(ICollection<MultiKey<EventBean>> events)
        {
            var delimiter = "";
            var buf = new StringBuilder();
    
            foreach (var key in events)
            {
                buf.Append(delimiter);
                buf.Append(ToString(key.Array));
                delimiter = ",";
            }
            return buf.ToString();
        }
    
        private String ToString(EventBean[] events)
        {
            var delimiter = "";
            var buf = new StringBuilder();
            foreach (var theEvent in events)
            {
                buf.Append(delimiter);
                buf.Append(((SupportBean) theEvent.Underlying).TheString);
                delimiter = "|";
            }
            return buf.ToString();
        }
    }
}
