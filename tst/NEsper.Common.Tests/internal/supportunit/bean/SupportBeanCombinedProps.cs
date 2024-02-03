///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.supportunit.bean
{
    /// <summary>
    ///     indexed[0].Mapped('0ma').value = 0ma0
    /// </summary>
    public class SupportBeanCombinedProps
    {
        public static string[] PROPERTIES = { "Indexed", "Array" };

        public SupportBeanCombinedProps(NestedLevOne[] indexed)
        {
            Array = indexed;
        }

        public NestedLevOne[] Array { get; }

        public static SupportBeanCombinedProps MakeDefaultBean()
        {
            var nested = new NestedLevOne[4]; // [3] left empty on purpose
            nested[0] = new NestedLevOne(new string[][] { new[] { "0ma", "0ma0" }, new[] { "0mb", "0ma1" } });
            nested[1] = new NestedLevOne(new string[][] { new[] { "1ma", "1ma0" }, new[] { "1mb", "1ma1" } });
            nested[2] = new NestedLevOne(new string[][] { new[] { "2ma", "valueOne" }, new[] { "2mb", "2ma1" } });

            return new SupportBeanCombinedProps(nested);
        }

        public NestedLevOne GetIndexed(int index)
        {
            return Array[index];
        }

        public class NestedLevOne
        {
            private readonly IDictionary<string, NestedLevTwo> map = new Dictionary<string, NestedLevTwo>();

            public NestedLevOne(string[][] keysAndValues)
            {
                for (var i = 0; i < keysAndValues.Length; i++)
                {
                    map.Put(keysAndValues[i][0], new NestedLevTwo(keysAndValues[i][1]));
                }
            }

            public string NestLevOneVal => "abc";

            public NestedLevTwo GetMapped(string key)
            {
                return map.Get(key);
            }

            public IDictionary<string, NestedLevTwo> GetMapprop()
            {
                return map;
            }
        }

        public class NestedLevTwo
        {
            public NestedLevTwo(string value)
            {
                Value = value;
            }

            public string Value { get; }
        }
    }
} // end of namespace
