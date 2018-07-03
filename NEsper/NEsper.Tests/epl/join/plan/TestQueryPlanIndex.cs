///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;

using NUnit.Framework;

namespace com.espertech.esper.epl.join.plan
{
    [TestFixture]
    public class TestQueryPlanIndex 
    {
        private QueryPlanIndex indexSpec;
    
        [SetUp]
        public void SetUp()
        {
            QueryPlanIndexItem itemOne = new QueryPlanIndexItem(new String[] { "p01", "p02" }, null, null, null, false, null);
            QueryPlanIndexItem itemTwo = new QueryPlanIndexItem(new String[] { "p21" }, new Type[0], null, null, false, null);
            QueryPlanIndexItem itemThree = new QueryPlanIndexItem(new String[0], new Type[0], null, null, false, null);
            indexSpec = QueryPlanIndex.MakeIndex(itemOne, itemTwo, itemThree);
        }
    
        [Test]
        public void TestInvalidUse()
        {
            try
            {
                new QueryPlanIndex(null);
                Assert.Fail();
            }
            catch (ArgumentException)
            {
                // Expected
            }
        }
    
        [Test]
        public void TestGetIndexNum()
        {
            Assert.NotNull(indexSpec.GetIndexNum(new String[] { "p01", "p02"}, null));
            Assert.NotNull(indexSpec.GetIndexNum(new String[] {"p21"}, null));
            Assert.NotNull(indexSpec.GetIndexNum(new String[0], null));
    
            Assert.IsNull(indexSpec.GetIndexNum(new String[] { "YY", "XX"}, null));
        }
    
        [Test]
        public void TestAddIndex()
        {
            String indexNum = indexSpec.AddIndex(new String[] {"a", "b"}, null);
            Assert.NotNull(indexNum);
            Assert.AreEqual(indexNum, indexSpec.GetIndexNum(new String[] { "a", "b" }, null).First.Name);
        }
    }
}
