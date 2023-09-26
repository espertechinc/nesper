///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Threading;

using com.espertech.esper.common.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.infra.tbl
{
    /// <summary>
    ///     NOTE: More table-related tests in "nwtable"
    /// </summary>
    public class InfraTableMTGroupedWContextIntoTableWriteAsContextTable : RegressionExecution
    {
        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.MULTITHREADED);
        }

        /// <summary>
        ///     Multiple writers share a key space that they aggregate into.
        ///     Writer utilize a hash partition context.
        ///     After all writers are done validate the space.
        /// </summary>
        public void Run(RegressionEnvironment env)
        {
            // with T, N, G:  Each of T threads loops N times and sends for each loop G events for each group.
            // for a total of T*N*G events being processed, and G aggregations retained in a shared variable.
            // Group is the innermost loop.
            try {
                TryMT(env, 8, 1000, 64);
            }
            catch (ThreadInterruptedException ex) {
                throw new EPException(ex);
            }
        }

        private static void TryMT(
            RegressionEnvironment env,
            int numThreads,
            int numLoops,
            int numGroups)
        {
            var eplDeclare =
                "@public create context ByStringHash\n" +
                "  coalesce by consistent_hash_crc32(TheString) from SupportBean, " +
                "    consistent_hash_crc32(P00) from SupportBean_S0 " +
                "  granularity 16 preallocate\n;" +
                "@public context ByStringHash create table varTotal (key string primary key, total sum(int));\n" +
                "@public context ByStringHash into table varTotal select TheString, sum(IntPrimitive) as total from SupportBean group by TheString;\n";
            var eplAssert = "context ByStringHash select varTotal[P00].total as c0 from SupportBean_S0";

            InfraTableMTGroupedWContextIntoTableWriteAsSharedTable.RunAndAssert(
                env,
                eplDeclare,
                eplAssert,
                numThreads,
                numLoops,
                numGroups);
        }
    }
} // end of namespace