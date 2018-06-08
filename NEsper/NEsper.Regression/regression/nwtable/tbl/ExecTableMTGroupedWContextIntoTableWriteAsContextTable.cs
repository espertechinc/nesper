///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.tbl
{
    public class ExecTableMTGroupedWContextIntoTableWriteAsContextTable : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType("SupportBean_S0", typeof(SupportBean_S0));
        }
    
        /// <summary>
        /// Multiple writers share a key space that they aggregate into.
        /// Writer utilize a hash partition context.
        /// After all writers are done validate the space.
        /// </summary>
        public override void Run(EPServiceProvider epService) {
            // with T, N, G:  Each of T threads loops N times and sends for each loop G events for each group.
            // for a total of T*N*G events being processed, and G aggregations retained in a shared variable.
            // Group is the innermost loop.
            TryMT(epService, 8, 1000, 64);
        }
    
        private void TryMT(EPServiceProvider epService, int numThreads, int numLoops, int numGroups) {
            string eplDeclare =
                    "create context ByStringHash\n" +
                            "  coalesce by Consistent_hash_crc32(TheString) from SupportBean, " +
                            "    Consistent_hash_crc32(p00) from SupportBean_S0 " +
                            "  granularity 16 preallocate\n;" +
                            "context ByStringHash create table varTotal (key string primary key, total sum(int));\n" +
                            "context ByStringHash into table varTotal select TheString, sum(IntPrimitive) as total from SupportBean group by TheString;\n";
            string eplAssert = "context ByStringHash select varTotal[p00].total as c0 from SupportBean_S0";
    
            ExecTableMTGroupedWContextIntoTableWriteAsSharedTable.RunAndAssert(epService, eplDeclare, eplAssert, numThreads, numLoops, numGroups);
        }
    }
} // end of namespace
