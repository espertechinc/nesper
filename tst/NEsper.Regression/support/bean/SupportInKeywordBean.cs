///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportInKeywordBean
    {
        public SupportInKeywordBean(int[] ints)
        {
            Ints = ints;
        }

        public SupportInKeywordBean(IDictionary<int, string> mapOfIntKey)
        {
            MapOfIntKey = mapOfIntKey;
        }

        public SupportInKeywordBean(ICollection<int> collOfInt)
        {
            CollOfInt = collOfInt;
        }

        public SupportInKeywordBean(long[] longs)
        {
            Longs = longs;
        }

        [JsonConstructor]
        public SupportInKeywordBean(
            int[] ints,
            IDictionary<int, string> mapOfIntKey,
            ICollection<int> collOfInt,
            long[] longs)
        {
            Ints = ints;
            MapOfIntKey = mapOfIntKey;
            CollOfInt = collOfInt;
            Longs = longs;
        }

        public int[] Ints { get; }

        public IDictionary<int, string> MapOfIntKey { get; }

        public ICollection<int> CollOfInt { get; }

        public long[] Longs { get; }
    }
} // end of namespace