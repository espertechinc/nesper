///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.supportregression.bean
{
    [Serializable]
    public class SupportBeanComplexProps  :  SupportMarkerInterface
    {
        public static String[] Properties =
            {
                "SimpleProperty",
                "Mapped",
                "Indexed",
                "IndexedProps",
                "MappedProps",
                "MapProperty",
                "ArrayProperty",
                "AsArrayProperty",
                "Nested",
                "ObjectArray"
            };
    		
        public SupportBeanComplexProps()
        {
        }
    
        public SupportBeanComplexProps(int[] indexedProps)
        {
            IndexedProps = indexedProps;
        }
    
    	public SupportBeanComplexProps(String simpleProperty, IDictionary<string,string> mappedProps, int[] indexedProps, IDictionary<String, String> mapProperty, int[] arrayProperty, String nestedValue, String nestedNestedValue)
    	{
            SimpleProperty = simpleProperty;
    		MappedProps = mappedProps;
    		IndexedProps = indexedProps;
    		MapProperty = mapProperty;
    		ArrayProperty = arrayProperty;
            Nested = new SupportBeanSpecialGetterNested(nestedValue, nestedNestedValue);
    	}

        /// <summary>
        /// Gets or sets the simple property.
        /// </summary>
        /// <value>The simple property.</value>
        public string SimpleProperty { get; set; }

        /// <summary>
        /// Gets or sets the map property.
        /// </summary>
        /// <value>The map property.</value>
        public IDictionary<string, string> MapProperty { get; set; }

        /// <summary>
        /// Gets or sets the mapped props.
        /// </summary>
        /// <value>The mapped props.</value>
        public IDictionary<string, string> MappedProps { get; set; }

        /// <summary>
        /// Gets or sets the indexed props.
        /// </summary>
        /// <value>The indexed props.</value>
        public int[] IndexedProps { get; set; }

        /// <summary>
        /// Gets or sets the array property.
        /// </summary>
        /// <value>The array property.</value>
        public int[] ArrayProperty { get; private set; }

        /// <summary>
        /// Gets as array property.
        /// </summary>
        /// <value>As array property.</value>
        public Array AsArrayProperty { get { return ArrayProperty; } }

        /// <summary>
        /// Gets or sets the object array.
        /// </summary>
        /// <value>The object array.</value>
        public object[] ObjectArray { get; set; }

        /// <summary>
        /// Gets or sets the nested.
        /// </summary>
        /// <value>The nested.</value>
        public SupportBeanSpecialGetterNested Nested { get; set; }

        /// <summary>
        /// Makes the default bean.
        /// </summary>
        /// <returns></returns>
        public static SupportBeanComplexProps MakeDefaultBean()
        {
            var properties = new Properties();
            properties["keyOne"] = "valueOne";
            properties["keyTwo"] = "valueTwo";

            var mapProp = new Dictionary<String, String>();
            mapProp["xOne"] = "yOne";
            mapProp["xTwo"] = "yTwo";

            var arrayProp = new[] { 10, 20, 30 };

            return new SupportBeanComplexProps(
                "Simple", properties, new[] { 1, 2 }, mapProp, arrayProp, "NestedValue", "NestedNestedValue");
        }

        public String GetSimpleProperty()
        {
            return SimpleProperty;
        }

        public String GetMapped(String key)
    	{
    		return MappedProps.Get(key);
    	}

    	public int GetIndexed(int index)
    	{
    		return IndexedProps[index];
    	}

        public void SetIndexed(int index, int value)
        {
            IndexedProps[index] = value;
        }

        public SupportBeanSpecialGetterNested GetNested()
        {
            return Nested;
        }

        public int[] GetArrayProperty()
        {
            return ArrayProperty;
        }
   
        public void SetArrayProperty(int[] arrayProperty)
        {
            ArrayProperty = arrayProperty;
        }
    
        [Serializable]
        public class SupportBeanSpecialGetterNested 
    	{
            /// <summary>
            /// Initializes a new instance of the <see cref="SupportBeanSpecialGetterNested"/> class.
            /// </summary>
            /// <param name="nestedValue">The nested value.</param>
            /// <param name="nestedNestedValue">The nested nested value.</param>
            public SupportBeanSpecialGetterNested(String nestedValue, String nestedNestedValue)
    		{
    			NestedValue = nestedValue;
                NestedNested = new SupportBeanSpecialGetterNestedNested(nestedNestedValue);
    		}

            /// <summary>
            /// Gets or sets the nested value.
            /// </summary>
            /// <value>The nested value.</value>
            public string NestedValue { get; set; }

            /// <summary>
            /// Gets or sets the nested nested.
            /// </summary>
            /// <value>The nested nested.</value>
            public SupportBeanSpecialGetterNestedNested NestedNested { get; private set; }

            /// <summary>
            /// Gets the nested value.
            /// </summary>
            /// <returns></returns>
            public string GetNestedValue()
            {
                return NestedValue;
            }

            /// <summary>
            /// Gets the nested nested.
            /// </summary>
            /// <returns></returns>
            public SupportBeanSpecialGetterNestedNested GetNestedNested()
            {
                return NestedNested;
            }

            public bool Equals(SupportBeanSpecialGetterNested other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(other.NestedValue, NestedValue);
            }

            /// <summary>
            /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
            /// </summary>
            /// <returns>
            /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
            /// </returns>
            /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>. </param><filterpriority>2</filterpriority>
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != typeof(SupportBeanSpecialGetterNested)) return false;
                return Equals((SupportBeanSpecialGetterNested) obj);
            }

            /// <summary>
            /// Serves as a hash function for a particular type. 
            /// </summary>
            /// <returns>
            /// A hash code for the current <see cref="T:System.Object"/>.
            /// </returns>
            /// <filterpriority>2</filterpriority>
            public override int GetHashCode()
            {
                return (NestedValue != null ? NestedValue.GetHashCode() : 0);
            }
    	}
    
        [Serializable]
        public class SupportBeanSpecialGetterNestedNested 
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SupportBeanSpecialGetterNestedNested"/> class.
            /// </summary>
            /// <param name="nestedNestedValue">The nested nested value.</param>
            public SupportBeanSpecialGetterNestedNested(String nestedNestedValue)
            {
                _nestedNestedValue = nestedNestedValue;
            }

            private string _nestedNestedValue;

            /// <summary>
            /// Gets the nested nested value.
            /// </summary>
            /// <value>The nested nested value.</value>
            public string NestedNestedValue
            {
                get { return _nestedNestedValue; }
                set { _nestedNestedValue = value; }
            }

            public string GetNestedNestedValue()
            {
                return _nestedNestedValue;
            }

            public void SetNestedNestedValue(string value)
            {
                _nestedNestedValue = value;
            }
        }
    }
}
