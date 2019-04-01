///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.espertech.esper.collection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.join.queryplan
{
    /// <summary>
    ///     Specifies an index to build as part of an overall query plan.
    /// </summary>
    public class QueryPlanIndexForge : CodegenMakeable<SAIFFInitializeSymbol>
    {
        public QueryPlanIndexForge(IDictionary<TableLookupIndexReqKey, QueryPlanIndexItemForge> items)
        {
            if (items == null) {
                throw new ArgumentException("Null value not allowed for items");
            }

            Items = items;
        }

        public IDictionary<TableLookupIndexReqKey, QueryPlanIndexItemForge> Items { get; }

        protected TableLookupIndexReqKey FirstIndexNum => Items.Keys.First();

        /// <summary>
        ///     For testing - Returns property names of all indexes.
        /// </summary>
        /// <value>property names array</value>
        public string[][] IndexProps {
            get {
                var arr = new string[Items.Count][];
                var count = 0;
                foreach (var entry in Items) {
                    arr[count] = entry.Value.HashProps;
                    count++;
                }

                return arr;
            }
        }

        public CodegenExpression Make(
            CodegenMethodScope parent, SAIFFInitializeSymbol symbols, CodegenClassScope classScope)
        {
            return NewInstance(
                typeof(QueryPlanIndex),
                CodegenMakeableUtil.MakeMap(
                    "items", typeof(TableLookupIndexReqKey), typeof(QueryPlanIndexItem), Items, GetType(), parent,
                    symbols, classScope));
        }

        public static QueryPlanIndexForge MakeIndex(IList<QueryPlanIndexItemForge> indexesSet)
        {
            IDictionary<TableLookupIndexReqKey, QueryPlanIndexItemForge> items =
                new LinkedHashMap<TableLookupIndexReqKey, QueryPlanIndexItemForge>();
            foreach (var item in indexesSet) {
                items.Put(new TableLookupIndexReqKey(UuidGenerator.Generate(), null), item);
            }

            return new QueryPlanIndexForge(items);
        }

        /// <summary>
        ///     Find a matching index for the property names supplied.
        /// </summary>
        /// <param name="indexProps">property names to search for</param>
        /// <param name="rangeProps">range props</param>
        /// <returns>-1 if not found, or offset within indexes if found</returns>
        public Pair<TableLookupIndexReqKey, int[]> GetIndexNum(string[] indexProps, string[] rangeProps)
        {
            // find an exact match first
            var proposed = new QueryPlanIndexItemForge(
                indexProps, new Type[indexProps.Length], rangeProps, new Type[rangeProps.Length], false, null, null);
            foreach (var entry in Items) {
                if (entry.Value.EqualsCompareSortedProps(proposed)) {
                    return new Pair<TableLookupIndexReqKey, int[]>(entry.Key, null);
                }
            }

            // find partial match second, i.e. for unique indexes where the where-clause is overspecific
            foreach (var entry in Items) {
                if (entry.Value.RangeProps == null || entry.Value.RangeProps.Length == 0) {
                    var indexes = QueryPlanIndexUniqueHelper.CheckSufficientGetAssignment(
                        entry.Value.HashProps, indexProps);
                    if (indexes != null && indexes.Length != 0) {
                        return new Pair<TableLookupIndexReqKey, int[]>(entry.Key, indexes);
                    }
                }
            }

            return null;
        }

        public string AddIndex(string[] indexProperties, Type[] coercionTypes, EventType eventType)
        {
            var uuid = UuidGenerator.Generate();
            Items.Put(
                new TableLookupIndexReqKey(uuid, null),
                new QueryPlanIndexItemForge(
                    indexProperties, coercionTypes, new string[0], new Type[0], false, null, eventType));
            return uuid;
        }

        /// <summary>
        ///     Returns a list of coercion types for a given index.
        /// </summary>
        /// <param name="indexProperties">is the index field names</param>
        /// <returns>coercion types, or null if no coercion is required</returns>
        public Type[] GetCoercionTypes(string[] indexProperties)
        {
            foreach (var entry in Items) {
                if (Arrays.DeepEquals(entry.Value.HashProps, indexProperties)) {
                    return entry.Value.HashTypes;
                }
            }

            throw new ArgumentException("Index properties not found");
        }

        /// <summary>
        ///     Sets the coercion types for a given index.
        /// </summary>
        /// <param name="indexProperties">is the index property names</param>
        /// <param name="coercionTypes">is the coercion types</param>
        public void SetCoercionTypes(string[] indexProperties, Type[] coercionTypes)
        {
            var found = false;
            foreach (var entry in Items) {
                if (Arrays.DeepEquals(entry.Value.HashProps, indexProperties)) {
                    entry.Value.HashTypes = coercionTypes;
                    found = true;
                }
            }

            if (!found) {
                throw new ArgumentException("Index properties not found");
            }
        }

        public override string ToString()
        {
            if (Items.IsEmpty()) {
                return "    (none)";
            }

            var buf = new StringBuilder();
            var delimiter = "";
            foreach (var entry in Items) {
                buf.Append(delimiter);
                var info = entry.Value == null ? "" : " : " + entry.Value;
                buf.Append("    index " + entry.Key + info);
                delimiter = "\n";
            }

            return buf.ToString();
        }

        /// <summary>
        ///     Print index specifications in readable format.
        /// </summary>
        /// <param name="indexSpecs">define indexes</param>
        /// <returns>readable format of index info</returns>
        public static string Print(QueryPlanIndexForge[] indexSpecs)
        {
            var buffer = new StringBuilder();
            buffer.Append("QueryPlanIndex[]\n");

            var delimiter = "";
            for (var i = 0; i < indexSpecs.Length; i++) {
                buffer.Append(delimiter);
                buffer.Append(
                    "  index spec stream " + i + " : \n" + (indexSpecs[i] == null ? "    null" : indexSpecs[i]));
                delimiter = "\n";
            }

            return buffer + "\n";
        }

        public static QueryPlanIndexForge MakeIndexTableAccess(TableLookupIndexReqKey indexName)
        {
            IDictionary<TableLookupIndexReqKey, QueryPlanIndexItemForge> indexMap =
                new Dictionary<TableLookupIndexReqKey, QueryPlanIndexItemForge>();
            indexMap.Put(indexName, null);
            return new QueryPlanIndexForge(indexMap);
        }
    }
} // end of namespace