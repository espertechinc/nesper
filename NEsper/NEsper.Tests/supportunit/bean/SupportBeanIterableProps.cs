///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;

namespace com.espertech.esper.supportunit.bean
{
    [Serializable]
    public class SupportBeanIterableProps : SupportMarkerInterface
    {
    	public static SupportBeanIterableProps MakeDefaultBean()
    	{
            return new SupportBeanIterableProps();
    	}

        public IEnumerable<SupportBeanSpecialGetterNested> IterableNested
        {
            get
            {
                return new[]
                       {
                           new SupportBeanSpecialGetterNested("IN1", "INN1"),
                           new SupportBeanSpecialGetterNested("IN2", "INN2")
                       };
            }
        }

        public IEnumerable<int?> IterableInteger
        {
            get { return new int?[] {10, 20}; }
        }

        public IEnumerable IterableUndefined
        {
            get { return new Object[] {10, 20}; }
        }

        public IEnumerable<object> IterableObject
        {
            get { return new Object[] {20, 30}; }
        }

        public IList<SupportBeanSpecialGetterNested> ListNested
        {
            get
            {
                return new[]
                       {
                           new SupportBeanSpecialGetterNested("LN1", "LNN1"),
                           new SupportBeanSpecialGetterNested("LN2", "LNN2")
                       };
            }
        }

        public IList<int?> ListInteger
        {
            get { return new int?[]{ 100, 200 }; }
        }

        public IDictionary<string, SupportBeanSpecialGetterNested> MapNested
        {
            get
            {
                IDictionary<String, SupportBeanSpecialGetterNested> map =
                    new Dictionary<String, SupportBeanSpecialGetterNested>();
                map["a"] = new SupportBeanSpecialGetterNested("MN1", "MNN1");
                map["b"] = new SupportBeanSpecialGetterNested("MN2", "MNN2");
                return map;
            }
        }

        public IDictionary<string, int> MapInteger
        {
            get
            {
                IDictionary<String, int> map = new Dictionary<String, int>();
                map["c"] = 1000;
                map["d"] = 2000;
                return map;
            }
        }

        [Serializable]
        public class SupportBeanSpecialGetterNested
    	{
            private SupportBeanSpecialGetterNestedNested nestedNested;
    
    		public SupportBeanSpecialGetterNested(String nestedValue, String nestedNestedValue)
    		{
    			this.NestedValue = nestedValue;
                this.nestedNested = new SupportBeanIterableProps.SupportBeanSpecialGetterNestedNested(nestedNestedValue);
    		}

            public string NestedValue { get; set; }

            public SupportBeanSpecialGetterNestedNested GetNestedNested()
            {
                return nestedNested;
            }
    
            public override bool Equals(Object o)
            {
                if (this == o)
                {
                    return true;
                }
                if (o == null || GetType() != o.GetType())
                {
                    return false;
                }
    
                SupportBeanIterableProps.SupportBeanSpecialGetterNested that = (SupportBeanSpecialGetterNested) o;
    
                if (!NestedValue.Equals(that.NestedValue))
                {
                    return false;
                }
    
                return true;
            }
    
            public override int GetHashCode()
            {
                return NestedValue.GetHashCode();
            }
        }
    
        [Serializable]
        public class SupportBeanSpecialGetterNestedNested
        {
            public SupportBeanSpecialGetterNestedNested(String nestedNestedValue)
            {
                NestedNestedValue = nestedNestedValue;
            }

            public string NestedNestedValue { get; set; }
        }
    }
}
