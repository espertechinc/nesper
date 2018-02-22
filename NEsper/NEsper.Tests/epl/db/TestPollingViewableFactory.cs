///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.client;
using com.espertech.esper.compat.container;
using com.espertech.esper.core.support;
using com.espertech.esper.epl.spec;
using com.espertech.esper.events;
using com.espertech.esper.supportunit.epl;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.view;

using NUnit.Framework;

namespace com.espertech.esper.epl.db
{
    [TestFixture]
    public class TestPollingViewableFactory 
    {
        [Test]
        public void TestDBStatementViewFactory()
        {
            var container = SupportContainer.Instance;
            var spec = new DBStatementStreamSpec("s0", ViewSpec.EMPTY_VIEWSPEC_ARRAY,
                    "mydb", "select * from mytesttable where mybigint=${idnum}", null);

            EventCollection eventCollection = DatabasePollingViewableFactory.CreateDBStatementView(
                1, 1, spec,
                SupportDatabaseService.MakeService(),
                container.Resolve<EventAdapterService>(),
                null, null, null, null, true, new DataCacheFactory(),
                SupportStatementContextFactory.MakeContext(container));
            
            Assert.AreEqual(typeof(long?), eventCollection.EventType.GetPropertyType("mybigint"));
            Assert.AreEqual(typeof(string), eventCollection.EventType.GetPropertyType("myvarchar"));
            Assert.AreEqual(typeof(bool?), eventCollection.EventType.GetPropertyType("mybool"));
            Assert.AreEqual(typeof(decimal?), eventCollection.EventType.GetPropertyType("mynumeric"));
            Assert.AreEqual(typeof(decimal?), eventCollection.EventType.GetPropertyType("mydecimal"));
        }
    
        [Test]
        public void TestLexSampleSQL()
        {
            String[][] testcases =
            {
                new [] {"select * from A where a=b and c=d", "select * from A where 1=0 and a=b and c=d"},
                new [] {"select * from A where 1=0", "select * from A where 1=0 and 1=0"},
                new [] {"select * from A", "select * from A where 1=0"},
                new [] {"select * from A group by x", "select * from A where 1=0 group by x"},
                new [] {"select * from A having a>b", "select * from A where 1=0 having a>b"},
                new [] {"select * from A order by d", "select * from A where 1=0 order by d"},
                new [] {"select * from A group by a having b>c order by d", "select * from A where 1=0 group by a having b>c order by d"},
                new [] {"select * from A where (7<4) group by a having b>c order by d", "select * from A where 1=0 and (7<4) group by a having b>c order by d"},
                new [] {"select * from A union select * from B", "select * from A  where 1=0 union  select * from B where 1=0"},
                new [] {"select * from A where a=2 union select * from B where 2=3", "select * from A where 1=0 and a=2 union  select * from B where 1=0 and 2=3"},
                new [] {"select * from A union select * from B union select * from C", "select * from A  where 1=0 union  select * from B  where 1=0 union  select * from C where 1=0"},
            };
    
            for (var i = 0; i < testcases.Length; i++)
            {
                String result = null;
                try
                {
                    result = DatabasePollingViewableFactory.LexSampleSQL(testcases[i][0]).Trim();
                }
                catch (Exception)
                {
                    Assert.Fail("failed case with exception:" + testcases[i][0]);
                }
                var expected = testcases[i][1].Trim();
                Assert.AreEqual(expected, result, "failed case " + i + " :" + testcases[i][0]);
            }
        }
    }
}
