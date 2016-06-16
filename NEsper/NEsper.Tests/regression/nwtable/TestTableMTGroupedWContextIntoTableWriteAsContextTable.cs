///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestTableMTGroupedWContextIntoTableWriteAsContextTable 
    {
        private EPServiceProvider epService;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportBean>();
            config.AddEventType<SupportBean_S0>();
            epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
        }
    
        /// <summary>
        /// Multiple writers share a key space that they aggregate into.
        /// Writer utilize a hash partition context.
        /// After all writers are done validate the space.
        /// </summary>
        [Test]
        public void TestMT() 
        {
            // with T, Count, G:  Each of T threads loops Count times and sends for each loop G events for each group.
            // for a total of T*Count*G events being processed, and G aggregations retained in a shared variable.
            // Group is the innermost loop.
            TryMT(8, 1000, 64);
        }
    
        private void TryMT(int numThreads, int numLoops, int numGroups) 
        {
            string eplDeclare =
                    "create context ByStringHash\n" +
                    "  coalesce by consistent_hash_crc32(TheString) from SupportBean, " +
                    "    consistent_hash_crc32(p00) from SupportBean_S0 " +
                    "  granularity 16 preallocate\n;" +
                    "context ByStringHash create table varTotal (key string primary key, total sum(int));\n" +
                    "context ByStringHash into table varTotal select TheString, sum(IntPrimitive) as total from SupportBean group by TheString;\n";
            string eplAssert = "context ByStringHash select varTotal[p00].total as c0 from SupportBean_S0";
    
            TestTableMTGroupedWContextIntoTableWriteAsSharedTable.RunAndAssert(epService, eplDeclare, eplAssert, numThreads, numLoops, numGroups);
        }
    }
}
