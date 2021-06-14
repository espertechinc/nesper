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
            "SimpleProperty",
            "Mapped",
            "Indexed",
            "IndexedProps",
            "MapProperty",
            "MappedProps",
            "ArrayProperty",
            "Nested",
            "ObjectArray"
        };

        private int[] _arrayProperty;
        private int[] _indexedProps;
        private Properties _mappedProps;
        private IDictionary<string, string> _mapProperty;
        private SupportBeanSpecialGetterNested _nested;
        private object[] _objectArray;
        private string _simpleProperty;

        public SupportBeanComplexProps()
        {
        }

        public SupportBeanComplexProps(int[] indexedProps)
        {
            _indexedProps = indexedProps;
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
            _simpleProperty = simpleProperty;
            _mappedProps = mappedProps;
            _indexedProps = indexedProps;
            _mapProperty = mapProperty;
            _arrayProperty = arrayProperty;
            _nested = new SupportBeanSpecialGetterNested(nestedValue, nestedNestedValue);
        }

        public string SimpleProperty {
            get => _simpleProperty;
            set => _simpleProperty = value;
        }

        public SupportBeanSpecialGetterNested Nested {
            get => _nested;
            set => _nested = value;
        }

        public int[] ArrayProperty {
            get => _arrayProperty;
            set => _arrayProperty = value;
        }

        public int[] IndexedProps {
            get => _indexedProps;
            set => _indexedProps = value;
        }

        public Properties MappedProps {
            get => _mappedProps;
            set => _mappedProps = value;
        }

        public IDictionary<string, string> MapProperty {
            get => _mapProperty;
            set => _mapProperty = value;
        }

        public object[] ObjectArray {
            get => _objectArray;
            set => _objectArray = value;
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
                "simple",
                properties,
                new[] {1, 2},
                mapProp,
                arrayProp,
                "NestedValue",
                "NestedNestedValue");
        }

        public string GetMapped(string key)
        {
            return _mappedProps.Get(key);
        }

        public int GetIndexed(int index)
        {
            return _indexedProps[index];
        }

        public string GetSimpleProperty()
        {
            return SimpleProperty;
        }

        public void SetIndexed(
            int index,
            int value)
        {
            _indexedProps[index] = value;
        }

        public SupportBeanSpecialGetterNested GetNested()
        {
            return Nested;
        }

        [Serializable]
        public class SupportBeanSpecialGetterNested
        {
            private string _nestedValue;

            public SupportBeanSpecialGetterNested(
                string nestedValue,
                string nestedNestedValue)
            {
                _nestedValue = nestedValue;
                NestedNested = new SupportBeanSpecialGetterNestedNested(nestedNestedValue);
            }

            public string NestedValue {
                get => _nestedValue;
                set => _nestedValue = value;
            }

            public SupportBeanSpecialGetterNestedNested NestedNested { get; }

            public SupportBeanSpecialGetterNestedNested GetNestedNested()
            {
                return NestedNested;
            }
            
            public override bool Equals(object o)
            {
                if (this == o) {
                    return true;
                }

                if (o == null || GetType() != o.GetType()) {
                    return false;
                }

                var that = (SupportBeanSpecialGetterNested) o;

                if (!_nestedValue.Equals(that._nestedValue)) {
                    return false;
                }

                return true;
            }

            public override int GetHashCode()
            {
                return _nestedValue.GetHashCode();
            }
        }

        [Serializable]
        public class SupportBeanSpecialGetterNestedNested
        {
            private string _nestedNestedValue;

            public SupportBeanSpecialGetterNestedNested(string nestedNestedValue)
            {
                _nestedNestedValue = nestedNestedValue;
            }

            public string NestedNestedValue {
                get => _nestedNestedValue;
                set => _nestedNestedValue = value;
            }
        }
    }
} // end of namespace