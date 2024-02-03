///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.regressionlib.support.bean
{
    /// <summary>
    ///     indexed[0].Mapped('0ma').value = 0ma0
    /// </summary>
    public class SupportBeanCombinedProps
    {
        public static readonly string[] PROPERTIES = {"Indexed", "array"};

        public SupportBeanCombinedProps(NestedLevOne[] indexed)
        {
            Array = indexed;
        }

        public NestedLevOne[] Array { get; }

        public NestedLevOne[] GetArray()
        {
            return Array;
        }
        
        public static SupportBeanCombinedProps MakeDefaultBean()
        {
            var nested = new NestedLevOne[4]; // [3] left empty on purpose
            nested[0] = new NestedLevOne(
                new[] {
                    new[] {"0ma", "0ma0"},
                    new[] {"0mb", "0ma1"}
                });
            nested[1] = new NestedLevOne(
                new[] {
                    new[] {"1ma", "1ma0"},
                    new[] {"1mb", "1ma1"}
                });
            nested[2] = new NestedLevOne(
                new[] {
                    new[] {"2ma", "valueOne"},
                    new[] {"2mb", "2ma1"}
                });
            return new SupportBeanCombinedProps(nested);
        }

        public NestedLevOne GetIndexed(int index)
        {
            return Array[index];
        }

        public class NestedLevOne
        {
            public NestedLevOne(string[][] keysAndValues)
            {
                for (var i = 0; i < keysAndValues.Length; i++) {
                    Mapprop.Put(keysAndValues[i][0], new NestedLevTwo(keysAndValues[i][1]));
                }
            }

            public IDictionary<string, NestedLevTwo> Mapprop { get; } = new Dictionary<string, NestedLevTwo>();

            public string NestLevOneVal => "abc";

            public string GetNestLevOneVal()
            {
                return NestLevOneVal;
            }

            public NestedLevTwo GetMapped(string key)
            {
                return Mapprop.Get(key);
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