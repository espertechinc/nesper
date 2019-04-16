///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.support
{
    [Serializable]
    public class SupportBeanComplexProps : SupportMarkerInterface
    {
        public static readonly string[] PROPERTIES = {
            "simpleProperty", "mapped", "indexed", "mapProperty", "arrayProperty", "nested", "objectArray"
        };

        private int[] arrayProperty;
        private int[] indexedProps;
        private Properties mappedProps;
        private IDictionary<string, string> mapProperty;
        private SupportBeanSpecialGetterNested nested;
        private object[] objectArray;
        private string simpleProperty;

        public SupportBeanComplexProps()
        {
        }

        public SupportBeanComplexProps(int[] indexedProps)
        {
            this.indexedProps = indexedProps;
        }

        public SupportBeanComplexProps(
            string simpleProperty,
            Properties mappedProps,
            int[] indexedProps,
            IDictionary<string, string> mapProperty,
            int[] arrayProperty,
            string nestedValue,
            string nestedNestedValue)
        {
            this.simpleProperty = simpleProperty;
            this.mappedProps = mappedProps;
            this.indexedProps = indexedProps;
            this.mapProperty = mapProperty;
            this.arrayProperty = arrayProperty;
            nested = new SupportBeanSpecialGetterNested(nestedValue, nestedNestedValue);
        }

        public string SimpleProperty {
            get => simpleProperty;
            set => simpleProperty = value;
        }

        public SupportBeanSpecialGetterNested Nested {
            get => nested;
            set => nested = value;
        }

        public int[] ArrayProperty {
            get => arrayProperty;
            set => arrayProperty = value;
        }

        public int[] IndexedProps {
            get => indexedProps;
            set => indexedProps = value;
        }

        public Properties MappedProps {
            get => mappedProps;
            set => mappedProps = value;
        }

        public IDictionary<string, string> MapProperty {
            get => mapProperty;
            set => mapProperty = value;
        }

        public object[] ObjectArray {
            get => objectArray;
            set => objectArray = value;
        }

        public static SupportBeanComplexProps MakeDefaultBean()
        {
            var properties = new Properties();
            properties.Put("keyOne", "valueOne");
            properties.Put("keyTwo", "valueTwo");

            IDictionary<string, string> mapProp = new Dictionary<string, string>();
            mapProp.Put("xOne", "yOne");
            mapProp.Put("xTwo", "yTwo");

            int[] arrayProp = {10, 20, 30};

            return new SupportBeanComplexProps(
                "simple", properties, new[] {1, 2}, mapProp, arrayProp, "nestedValue", "nestedNestedValue");
        }

        public string GetMapped(string key)
        {
            return mappedProps.Get(key);
        }

        public int GetIndexed(int index)
        {
            return indexedProps[index];
        }

        public void SetIndexed(
            int index,
            int value)
        {
            indexedProps[index] = value;
        }

        [Serializable]
        public class SupportBeanSpecialGetterNested
        {
            private string nestedValue;

            public SupportBeanSpecialGetterNested(
                string nestedValue,
                string nestedNestedValue)
            {
                this.nestedValue = nestedValue;
                NestedNested = new SupportBeanSpecialGetterNestedNested(nestedNestedValue);
            }

            public string NestedValue {
                get => nestedValue;
                set => nestedValue = value;
            }

            public SupportBeanSpecialGetterNestedNested NestedNested { get; }

            public override bool Equals(object o)
            {
                if (this == o) {
                    return true;
                }

                if (o == null || GetType() != o.GetType()) {
                    return false;
                }

                var that = (SupportBeanSpecialGetterNested) o;

                if (!nestedValue.Equals(that.nestedValue)) {
                    return false;
                }

                return true;
            }

            public override int GetHashCode()
            {
                return nestedValue.GetHashCode();
            }
        }

        [Serializable]
        public class SupportBeanSpecialGetterNestedNested
        {
            private string nestedNestedValue;

            public SupportBeanSpecialGetterNestedNested(string nestedNestedValue)
            {
                this.nestedNestedValue = nestedNestedValue;
            }

            public string NestedNestedValue {
                get => nestedNestedValue;
                set => nestedNestedValue = value;
            }
        }
    }
} // end of namespace