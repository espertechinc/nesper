///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.join.lookup
{
    /// <summary>
    /// Holds property information for joined properties in a lookup.
    /// </summary>
    public class IndexedPropDesc : IComparable<IndexedPropDesc>
    {
        private readonly string indexPropName;
        private readonly Type coercionType;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name = "indexPropName">is the property name of the indexed field</param>
        /// <param name = "coercionType">is the type to coerce to</param>
        public IndexedPropDesc(
            string indexPropName,
            Type coercionType)
        {
            this.indexPropName = indexPropName;
            this.coercionType = coercionType;
        }

        public CodegenExpression Make()
        {
            return NewInstance(typeof(IndexedPropDesc), Constant(indexPropName), Constant(coercionType));
        }

        /// <summary>
        /// Returns the index property names given an array of descriptors.
        /// </summary>
        /// <param name = "descList">descriptors of joined properties</param>
        /// <returns>array of index property names</returns>
        public static string[] GetIndexProperties(IndexedPropDesc[] descList)
        {
            var result = new string[descList.Length];
            var count = 0;
            foreach (var desc in descList) {
                result[count++] = desc.IndexPropName;
            }

            return result;
        }

        public static string[] GetIndexProperties(IList<IndexedPropDesc> descList)
        {
            var result = new string[descList.Count];
            var count = 0;
            foreach (var desc in descList) {
                result[count++] = desc.IndexPropName;
            }

            return result;
        }

        public static int GetPropertyIndex(
            string propertyName,
            IndexedPropDesc[] descList)
        {
            for (var i = 0; i < descList.Length; i++) {
                if (descList[i].IndexPropName.Equals(propertyName)) {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Returns the key coercion types.
        /// </summary>
        /// <param name = "descList">a list of descriptors</param>
        /// <returns>key coercion types</returns>
        public static Type[] GetCoercionTypes(IndexedPropDesc[] descList)
        {
            var result = new Type[descList.Length];
            var count = 0;
            foreach (var desc in descList) {
                result[count++] = desc.CoercionType;
            }

            return result;
        }

        public int CompareTo(IndexedPropDesc other)
        {
            return String.Compare(indexPropName, other.IndexPropName, StringComparison.Ordinal);
        }

        public int CompareTo(object o)
        {
            return CompareTo((IndexedPropDesc)o);
        }

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (IndexedPropDesc)o;
            if (!coercionType.Equals(that.coercionType)) {
                return false;
            }

            if (!indexPropName.Equals(that.indexPropName)) {
                return false;
            }

            return true;
        }

        public static bool Compare(
            IList<IndexedPropDesc> first,
            IList<IndexedPropDesc> second)
        {
            if (first.Count != second.Count) {
                return false;
            }

            var copyFirst = new List<IndexedPropDesc>(first);
            var copySecond = new List<IndexedPropDesc>(second);
            Comparison<IndexedPropDesc> comparator = (
                    o1,
                o2) => o1.IndexPropName.CompareTo(o2.IndexPropName);

            copyFirst.Sort(comparator);
            copySecond.Sort(comparator);
            for (var i = 0; i < copyFirst.Count; i++) {
                if (!copyFirst[i].Equals(copySecond[i])) {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            int result;
            result = indexPropName.GetHashCode();
            result = 31 * result + coercionType.GetHashCode();
            return result;
        }

        public static void ToQueryPlan(
            TextWriter writer,
            IndexedPropDesc[] indexedProps)
        {
            var delimiter = "";
            foreach (var prop in indexedProps) {
                writer.Write(delimiter);
                writer.Write(prop.IndexPropName);
                writer.Write("(");
                writer.Write(TypeHelper.GetSimpleNameForType(prop.CoercionType));
                writer.Write(")");
                delimiter = ",";
            }
        }

        public static CodegenExpression MakeArray(ICollection<IndexedPropDesc> items)
        {
            return MakeArray(items.ToArray());
        }

        public static CodegenExpression MakeArray(IndexedPropDesc[] items)
        {
            var expressions = new CodegenExpression[items.Length];
            for (var i = 0; i < items.Length; i++) {
                expressions[i] = items[i].Make();
            }

            return NewArrayWithInit(typeof(IndexedPropDesc), expressions);
        }

        public string IndexPropName => indexPropName;

        public Type CoercionType => coercionType;
    }
} // end of namespace