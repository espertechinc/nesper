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
using com.espertech.esper.epl.join.rep;
using com.espertech.esper.supportunit.epl.join;

using NUnit.Framework;

namespace com.espertech.esper.epl.join.assemble
{
    [TestFixture]
    public class TestLeafAssemblyNode 
    {
        private SupportJoinProcNode _parentNode;
        private LeafAssemblyNode _leafNode;
    
        [SetUp]
        public void SetUp()
        {
            _leafNode = new LeafAssemblyNode(1, 4);
            _parentNode = new SupportJoinProcNode(-1, 4);
            _parentNode.AddChild(_leafNode);
        }
    
        [Test]
        public void TestProcess()
        {
            IList<Node>[] result = SupportJoinResultNodeFactory.MakeOneStreamResult(4, 1, 2, 2);
    
            _leafNode.Process(result, new List<EventBean[]>(), null);
    
            Assert.AreEqual(4, _parentNode.RowsList.Count);
            Assert.AreEqual(result[1][0].Events.First(), _parentNode.RowsList[0][1]);   // compare event
        }
    
        [Test]
        public void TestChildResult()
        {
            try
            {
                _leafNode.Result(null, 0, null, null, null, null);
                Assert.Fail();
            }
            catch (NotSupportedException)
            {
                // expected
            }
        }
    }
}
