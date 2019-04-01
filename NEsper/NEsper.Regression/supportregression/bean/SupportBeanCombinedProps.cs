///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.supportregression.bean
{
    /// <summary>
    /// indexed[0].Mapped('0ma').value = 0ma0
    /// </summary>

    [Serializable]
    public class SupportBeanCombinedProps
    {
        public NestedLevOne[] Array
        {
            get { return _indexed; }
        }

        public static String[] PROPERTIES = new string[]
        {
        	"Indexed",
        	"Array"
        };

        public static SupportBeanCombinedProps MakeDefaultBean()
        {
            NestedLevOne[] nested = new NestedLevOne[4]; // [3] left empty on purpose
            nested[0] = new NestedLevOne(new string[][] {
                new string[] { "0ma", "0ma0" },
                new string[] { "0mb", "0ma1" }
            });
            nested[1] = new NestedLevOne(new string[][] {
                new string[] { "1ma", "1ma0" },
                new string[] { "1mb", "1ma1" }
            });
            nested[2] = new NestedLevOne(new string[][] {
                new string[] { "2ma", "valueOne" }, 
                new string[] { "2mb", "2ma1" }
            });

            return new SupportBeanCombinedProps(nested);
        }

        private readonly NestedLevOne[] _indexed;

        public SupportBeanCombinedProps(NestedLevOne[] indexed)
        {
            _indexed = indexed;
        }

        public NestedLevOne[] GetArray()
        {
            return _indexed;
        }

        public NestedLevOne GetIndexed(int index)
        {
            return _indexed[index];
        }

        [Serializable]
        public class NestedLevOne
        {
            private readonly IDictionary<String, NestedLevTwo> map = new Dictionary<String, NestedLevTwo>();

            public NestedLevOne(String[][] keysAndValues)
            {
                for (int i = 0; i < keysAndValues.Length; i++)
                {
                    map.Put(keysAndValues[i][0], new NestedLevTwo(keysAndValues[i][1]));
                }
            }

            public NestedLevTwo GetMapped(String key)
            {
                return map.Get(key, null);
            }

            public string GetNestLevOneVal()
            {
                return NestLevOneVal;
            }

            public IDictionary<string, NestedLevTwo> Mapprop
            {
                get { return map; }
            }

            public string NestLevOneVal
            {
                get { return "abc"; }
            }
        }

        [Serializable]
        public class NestedLevTwo
        {
            public String Value
            {
                get { return _value;  }
            }

            public string GetValue()
            {
                return _value;
            }

            public void SetValue(string value)
            {
                _value = value;
            }

            private String _value;

            public NestedLevTwo(String value)
            {
                _value = value;
            }
        }
    }
}
