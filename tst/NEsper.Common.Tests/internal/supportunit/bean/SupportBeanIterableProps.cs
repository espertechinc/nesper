///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.supportunit.bean
{
    public class SupportBeanIterableProps : SupportMarkerInterface
    {
        public IEnumerable<int> IterableInteger => Arrays.AsList(10, 20);

        public IEnumerable<object> IterableUndefined => Arrays.AsList<object>(10, 20);

        public IEnumerable<object> IterableObject => Arrays.AsList<object>(20, 30);

        public IList<SupportBeanSpecialGetterNested> ListNested => Arrays.AsList(
            new SupportBeanSpecialGetterNested("LN1", "LNN1"),
            new SupportBeanSpecialGetterNested("LN2", "LNN2"));

        public IList<int> ListInteger => Arrays.AsList(100, 200);

        public static SupportBeanIterableProps MakeDefaultBean()
        {
            return new SupportBeanIterableProps();
        }

        public IEnumerable<SupportBeanSpecialGetterNested> IterableNested => Arrays.AsList(
            new SupportBeanSpecialGetterNested("IN1", "INN1"),
            new SupportBeanSpecialGetterNested("IN2", "INN2"));

        public IDictionary<string, SupportBeanSpecialGetterNested> MapNested
        {
            get {
                IDictionary<string, SupportBeanSpecialGetterNested> map = new Dictionary<string, SupportBeanSpecialGetterNested>();
                map.Put("a", new SupportBeanSpecialGetterNested("MN1", "MNN1"));
                map.Put("b", new SupportBeanSpecialGetterNested("MN2", "MNN2"));
                return map;
            }
        }

        public IDictionary<string, int> MapInteger
        {
            get {
                IDictionary<string, int> map = new Dictionary<string, int>();
                map.Put("c", 1000);
                map.Put("d", 2000);
                return map;
            }
        }

        public class SupportBeanSpecialGetterNested
        {
            public SupportBeanSpecialGetterNested(
                string nestedValue,
                string nestedNestedValue)
            {
                NestedValue = nestedValue;
                NestedNested = new SupportBeanSpecialGetterNestedNested(nestedNestedValue);
            }

            public string NestedValue { get; set; }

            public SupportBeanSpecialGetterNestedNested NestedNested { get; }

            protected bool Equals(SupportBeanSpecialGetterNested other)
            {
                return string.Equals(NestedValue, other.NestedValue);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                if (obj.GetType() != this.GetType())
                {
                    return false;
                }

                return Equals((SupportBeanSpecialGetterNested) obj);
            }

            public override int GetHashCode()
            {
                return (NestedValue != null ? NestedValue.GetHashCode() : 0);
            }
        }

        public class SupportBeanSpecialGetterNestedNested
        {
            public SupportBeanSpecialGetterNestedNested(string nestedNestedValue)
            {
                NestedNestedValue = nestedNestedValue;
            }

            public string NestedNestedValue { get; set; }
        }
    }
} // end of namespace
