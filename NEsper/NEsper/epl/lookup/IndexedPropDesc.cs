///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using com.espertech.esper.util;

namespace com.espertech.esper.epl.lookup
{
    /// <summary>
    /// Holds property information for joined properties in a lookup.
    /// </summary>
    public class IndexedPropDesc : IComparable
    {
        /// <summary>Ctor. </summary>
        /// <param name="indexPropName">is the property name of the indexed field</param>
        /// <param name="coercionType">is the type to coerce to</param>
        public IndexedPropDesc(String indexPropName, Type coercionType)
        {
            IndexPropName = indexPropName;
            CoercionType = coercionType;
        }

        /// <summary>Returns the property name of the indexed field. </summary>
        /// <value>property name of indexed field</value>
        public string IndexPropName { get; private set; }

        /// <summary>Returns the coercion type of key to index field. </summary>
        /// <value>type to coerce to</value>
        public Type CoercionType { get; private set; }

        /// <summary>Returns the index property names given an array of descriptors. </summary>
        /// <param name="descList">descriptors of joined properties</param>
        /// <returns>array of index property names</returns>
        public static String[] GetIndexProperties(IndexedPropDesc[] descList)
        {
            var result = new String[descList.Length];
            int count = 0;
            foreach (IndexedPropDesc desc in descList)
            {
                result[count++] = desc.IndexPropName;
            }
            return result;
        }
    
        public static String[] GetIndexProperties(List<IndexedPropDesc> descList) {
            var result = new String[descList.Count];
            int count = 0;
            foreach (IndexedPropDesc desc in descList)
            {
                result[count++] = desc.IndexPropName;
            }
            return result;
        }
    
        public static int GetPropertyIndex(String propertyName, IndexedPropDesc[] descList) {
            for (int i = 0; i < descList.Length; i++) {
                if (descList[i].IndexPropName.Equals(propertyName)) {
                    return i;
                }
            }
            return -1;
        }
    
        /// <summary>Returns the key coercion types. </summary>
        /// <param name="descList">a list of descriptors</param>
        /// <returns>key coercion types</returns>
        public static Type[] GetCoercionTypes(IndexedPropDesc[] descList)
        {
            var result = new Type[descList.Length];
            int count = 0;
            foreach (IndexedPropDesc desc in descList)
            {
                result[count++] = desc.CoercionType;
            }
            return result;
        }
    
        public int CompareTo(Object o)
        {
            var other = (IndexedPropDesc) o;
            return IndexPropName.CompareTo(other.IndexPropName);
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
    
            var that = (IndexedPropDesc) o;
    
            if (!CoercionType.Equals(that.CoercionType))
            {
                return false;
            }
            if (!IndexPropName.Equals(that.IndexPropName))
            {
                return false;
            }
            return true;
        }
    
        public static bool Compare(IList<IndexedPropDesc> first, IList<IndexedPropDesc> second) {
            if (first.Count != second.Count) {
                return false;
            }
            var copyFirst = first.OrderBy(o => o.IndexPropName).ToList();
            var copySecond = second.OrderBy(o => o.IndexPropName).ToList();
            return !copyFirst.Where((t,i) => !Equals(t, copySecond[i])).Any();
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
            unchecked
            {
                return
                    ((IndexPropName != null ? IndexPropName.GetHashCode() : 0) * 397) ^
                    (CoercionType != null ? CoercionType.GetHashCode() : 0);
            }
        }

        public static void ToQueryPlan(StringWriter writer, IndexedPropDesc[] indexedProps)
        {
            String delimiter = "";
            foreach (IndexedPropDesc prop in indexedProps)
            {
                writer.Write(delimiter);
                writer.Write(prop.IndexPropName);
                writer.Write("(");
                writer.Write(TypeHelper.GetSimpleNameForType(prop.CoercionType));
                writer.Write(")");
                delimiter = ",";
            }
        }
    }
}
