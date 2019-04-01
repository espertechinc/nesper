///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    /// Utility for string comparison based on the Levenshtein algo.
    /// </summary>
    public class LevenshteinDistance
    {
        /// <summary>Make 3 characters an acceptable distance for reporting. </summary>
        public readonly static int ACCEPTABLE_DISTANCE = 3;

        /// <summary>
        /// Compute the distance between two strins using the Levenshtein algorithm, including a case-insensitive string comparison.
        /// </summary>
        /// <param name="str1">first string</param>
        /// <param name="str2">second string</param>
        /// <returns>
        /// distance or zero if case-insensitive string comparison found equal stringsor int.MaxValue for invalid comparison because of null values.
        /// </returns>
        public static int ComputeLevenshteinDistance(string str1, string str2)
        {
            if ((str1 == null) || (str2 == null))
            {
                return int.MaxValue;
            }

            if (string.Equals(str1, str2, StringComparison.InvariantCultureIgnoreCase))
            {
                return 0;
            }

            var distance = new int[str1.Length + 1, str2.Length + 1];

            for (int i = 0; i <= str1.Length; i++)
            {
                distance[i, 0] = i;
            }
            for (int j = 0; j <= str2.Length; j++)
            {
                distance[0, j] = j;
            }

            for (int i = 1; i <= str1.Length; i++)
            {
                for (int j = 1; j <= str2.Length; j++)
                {
                    distance[i, j] = Minimum(
                            distance[i - 1, j] + 1,
                            distance[i, j - 1] + 1,
                            distance[i - 1, j - 1]
                                    + ((str1[i - 1] == str2[j - 1]) ? 0
                                    : 1));
                }
            }

            return distance[str1.Length, str2.Length];
        }

        private static int Minimum(int a, int b, int c)
        {
            return Math.Min(Math.Min(a, b), c);
        }
    }
}