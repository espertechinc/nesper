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
using com.espertech.esper.compat.threading;
using com.espertech.esper.epl.join.exec.@base;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.epl.virtualdw;
using com.espertech.esper.view;

using NUnit.Framework;

namespace com.espertech.esper.epl.join.plan
{
    [TestFixture]
    public class TestTableLookupPlan 
    {
        [Test]
        public void TestMakeExec()
        {
            var indexesPerStream = new IDictionary<TableLookupIndexReqKey, EventTable>[2];
            indexesPerStream[1] = new NullableDictionary<TableLookupIndexReqKey, EventTable>();
            indexesPerStream[1].Put(new TableLookupIndexReqKey("idx1"), new UnindexedEventTableImpl(0));

            var spec = new TableLookupNode(new FullTableScanLookupPlan(0, 1, new TableLookupIndexReqKey("idx1")));
            var execNode = spec.MakeExec("ABC", 1, null, indexesPerStream, null, new Viewable[2], null, new VirtualDWView[2], new ILockable[2]);
            var exec = (TableLookupExecNode)execNode;

            Assert.AreSame(indexesPerStream[1].Get(new TableLookupIndexReqKey("idx1")), ((FullTableScanLookupStrategy)exec.LookupStrategy).EventIndex);
            Assert.AreEqual(1, exec.IndexedStream);
        }
    }
}
