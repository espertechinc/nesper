///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.join.lookup
{
    /// <summary>
    ///     Holds property information for joined properties in a lookup.
    /// </summary>
    public class IndexedPropDesc : IComparable
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="indexPropName">is the property name of the indexed field</param>
        /// <param name="coercionType">is the type to coerce to</param>
        public IndexedPropDesc(
            string indexPropName,
            Type coercionType)
        {
            IndexPropName = indexPropName;
            CoercionType = coercionType;
        }

        /// <summary>
        ///     Returns the property name of the indexed field.
        /// </summary>
        /// <returns>property name of indexed field</returns>
        public string IndexPropName { get; }

        /// <summary>
        ///     Returns the coercion type of key to index field.
        /// </summary>
        /// <returns>type to coerce to</returns>
        public Type CoercionType { get; }

        public int CompareTo(object o)
        {
            var other = (IndexedPropDesc) o;
            return IndexPropName.CompareTo(other.IndexPropName);
        }

        public CodegenExpression Make()
        {
            return NewInstance<IndexedPropDesc>(Constant(IndexPropName), Constant(CoercionType));
        }

        /// <summary>
        ///     Returns the index property names given an array of descriptors.
        /// </summary>
        /// <param name="descList">descriptors of joined properties</param>
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
        ///     Returns the key coercion types.
        /// </summary>
        /// <param name="descList">a list of descriptors</param>
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

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (IndexedPropDesc) o;

            if (CoercionType != that.CoercionType) {
                return false;
            }

            if (!IndexPropName.Equals(that.IndexPropName)) {
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
            result = IndexPropName.GetHashCode();
            result = 31 * result + CoercionType.GetHashCode();
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
    }
} // end of namespace