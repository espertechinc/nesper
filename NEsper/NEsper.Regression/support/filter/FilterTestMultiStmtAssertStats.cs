namespace com.espertech.esper.regressionlib.support.filter
{
    ///////////////////////////////////////////////////////////////////////////////////////
    // Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
    // http://esper.codehaus.org                                                          /
    // ---------------------------------------------------------------------------------- /
    // The software in this package is published under the terms of the GPL license       /
    // a copy of which has been included with this distribution in the license.txt file.  /
    ///////////////////////////////////////////////////////////////////////////////////////

    public class FilterTestMultiStmtAssertStats
    {
        public FilterTestMultiStmtAssertStats(
            string stats,
            params int[] permutation)
        {
            Stats = stats;
            Permutation = permutation;
        }

        public string Stats { get; }

        public int[] Permutation { get; }

        public static FilterTestMultiStmtAssertStats[] MakeSingleStat(string stats)
        {
            return new[] {
                new FilterTestMultiStmtAssertStats(stats, 0)
            };
        }

        public static FilterTestMultiStmtAssertStats[] MakeTwoSameStat(string stats)
        {
            return new[] {
                new FilterTestMultiStmtAssertStats(stats, 0, 1),
                new FilterTestMultiStmtAssertStats(stats, 1, 0)
            };
        }
    }
} // end of namespace