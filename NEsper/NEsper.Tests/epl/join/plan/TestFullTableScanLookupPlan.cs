///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.join.exec.@base;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.epl.virtualdw;

using NUnit.Framework;

namespace com.espertech.esper.epl.join.plan
{
    [TestFixture]
    public class TestFullTableScanLookupPlan 
    {
        private UnindexedEventTable _unindexedEventIndex;
    
        [SetUp]
        public void SetUp()
        {
            _unindexedEventIndex = new UnindexedEventTableImpl(0);
        }
    
        [Test]
        public void TestLookup()
        {
            var spec = new FullTableScanLookupPlan(0, 1, new TableLookupIndexReqKey("idx2"));

            var indexes = new IDictionary<TableLookupIndexReqKey, EventTable>[2];
            indexes[0] = new Dictionary<TableLookupIndexReqKey, EventTable>();
            indexes[1] = new Dictionary<TableLookupIndexReqKey, EventTable>();
            indexes[1].Put(new TableLookupIndexReqKey("idx2"), _unindexedEventIndex);
    
            var lookupStrategy = spec.MakeStrategy("ABC", 1, null, indexes, null, new VirtualDWView[2]);
    
            var strategy = (FullTableScanLookupStrategy) lookupStrategy;
            Assert.AreEqual(_unindexedEventIndex, strategy.EventIndex);
        }
    }
}
