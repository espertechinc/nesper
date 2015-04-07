///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat.collections;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.join.plan
{
	/// <summary>
	/// Specifies an index to build as part of an overall query plan.
	/// </summary>
	public class QueryPlanIndex
	{
	    private readonly IDictionary<TableLookupIndexReqKey, QueryPlanIndexItem> _items;

	    public QueryPlanIndex(IDictionary<TableLookupIndexReqKey, QueryPlanIndexItem> items)
        {
	        if (items == null)
            {
	            throw new ArgumentException("Null value not allowed for items");
	        }
	        _items = items;
	    }

	    public IDictionary<TableLookupIndexReqKey, QueryPlanIndexItem> Items
	    {
	        get { return _items; }
	    }

	    public static QueryPlanIndex MakeIndex(params QueryPlanIndexItem[] items)
        {
	        IDictionary<TableLookupIndexReqKey, QueryPlanIndexItem> result = new LinkedHashMap<TableLookupIndexReqKey, QueryPlanIndexItem>();
	        foreach (QueryPlanIndexItem item in items) {
	            result.Put(new TableLookupIndexReqKey(UuidGenerator.Generate()), item);
	        }
	        return new QueryPlanIndex(result);
	    }

	    public static QueryPlanIndex MakeIndex(IList<QueryPlanIndexItem> indexesSet)
        {
	        IDictionary<TableLookupIndexReqKey, QueryPlanIndexItem> items = new LinkedHashMap<TableLookupIndexReqKey, QueryPlanIndexItem>();
	        foreach (QueryPlanIndexItem item in indexesSet) {
	            items.Put(new TableLookupIndexReqKey(UuidGenerator.Generate()), item);
	        }
	        return new QueryPlanIndex(items);
	    }

	    /// <summary>
	    /// Find a matching index for the property names supplied.
	    /// </summary>
	    /// <param name="indexProps">property names to search for</param>
	    /// <returns>-1 if not found, or offset within indexes if found</returns>
	    internal Pair<TableLookupIndexReqKey, int[]> GetIndexNum(string[] indexProps, string[] rangeProps)
	    {
	        // find an exact match first
	        QueryPlanIndexItem proposed = new QueryPlanIndexItem(indexProps, null, rangeProps, null, false);
	        foreach (KeyValuePair<TableLookupIndexReqKey, QueryPlanIndexItem> entry in _items) {
	            if (entry.Value.EqualsCompareSortedProps(proposed)) {
	                return new Pair<TableLookupIndexReqKey, int[]>(entry.Key, null);
	            }
	        }

	        // find partial match second, i.e. for unique indexes where the where-clause is overspecific
	        foreach (KeyValuePair<TableLookupIndexReqKey, QueryPlanIndexItem> entry in _items) {
	            if (entry.Value.RangeProps == null || entry.Value.RangeProps.Count == 0) {
	                int[] indexes = QueryPlanIndexUniqueHelper.CheckSufficientGetAssignment(entry.Value.IndexProps, indexProps);
	                if (indexes != null && indexes.Length != 0) {
	                    return new Pair<TableLookupIndexReqKey, int[]>(entry.Key, indexes);
	                }
	            }
	        }

	        return null;
	    }

	    protected TableLookupIndexReqKey FirstIndexNum
	    {
	        get { return _items.Keys.First(); }
	    }

	    /// <summary>
	    /// Add an index specification element.
	    /// </summary>
	    /// <param name="indexProperties">list of property names to index</param>
	    /// <param name="coercionTypes">list of coercion types if required, or null if no coercion required</param>
	    /// <returns>number indicating position of index that was added</returns>
	    public string AddIndex(string[] indexProperties, Type[] coercionTypes)
	    {
	        string uuid = UuidGenerator.Generate();
	        _items.Put(new TableLookupIndexReqKey(uuid), new QueryPlanIndexItem(indexProperties, coercionTypes, null, null, false));
	        return uuid;
	    }

	    /// <summary>
	    /// For testing - Returns property names of all indexes.
	    /// </summary>
	    /// <value>property names array</value>
	    public string[][] IndexProps
	    {
	        get
	        {
	            return _items.Values
                    .Select(v => v.IndexProps != null ? v.IndexProps.ToArray() : null)
                    .ToArray();
	        }
	    }

	    /// <summary>
	    /// Returns a list of coercion types for a given index.
	    /// </summary>
	    /// <param name="indexProperties">is the index field names</param>
	    /// <returns>coercion types, or null if no coercion is required</returns>
	    public Type[] GetCoercionTypes(string[] indexProperties)
	    {
	        foreach (var value in _items.Values)
	        {
	            if (CompatExtensions.DeepEquals(value.IndexProps, indexProperties))
	            {
	                return value.OptIndexCoercionTypes.ToArray();
	            }
	        }
	        throw new ArgumentException("Index properties not found");
	    }

	    /// <summary>
	    /// Sets the coercion types for a given index.
	    /// </summary>
	    /// <param name="indexProperties">is the index property names</param>
	    /// <param name="coercionTypes">is the coercion types</param>
	    public void SetCoercionTypes(string[] indexProperties, Type[] coercionTypes)
	    {
	        bool found = false;
	        foreach (var entry in _items.Values)
	        {
	            if (CompatExtensions.DeepEquals(entry.IndexProps, indexProperties))
	            {
	                entry.OptIndexCoercionTypes = coercionTypes;
	                found = true;
	            }
	        }
	        if (!found)
	        {
	            throw new ArgumentException("Index properties not found");
	        }
	    }

	    public override string ToString()
	    {
	        if (_items.IsEmpty()) {
	            return "    (none)";
	        }
	        var buf = new StringBuilder();
	        var delimiter = "";
	        foreach (var entry in _items)
	        {
	            buf.Append(delimiter);
	            string info = entry.Value == null ? "" : " : " + entry.Value;
	            buf.Append("    index " + entry.Key + info);
	            delimiter = "\n";
	        }
	        return buf.ToString();
	    }

	    /// <summary>
	    /// Print index specifications in readable format.
	    /// </summary>
	    /// <param name="indexSpecs">define indexes</param>
	    /// <returns>readable format of index info</returns>

	    public static string Print(QueryPlanIndex[] indexSpecs)
	    {
	        StringBuilder buffer = new StringBuilder();
	        buffer.Append("QueryPlanIndex[]\n");

	        string delimiter = "";
	        for (int i = 0; i < indexSpecs.Length; i++)
	        {
	            buffer.Append(delimiter);
	            buffer.AppendFormat("  index spec stream {0} : \n{1}", i, (indexSpecs[i] == null ? "    null" : indexSpecs[i].ToString()));
	            delimiter = "\n";
	        }

	        return buffer + "\n";
	    }

	    public static QueryPlanIndex MakeIndexTableAccess(TableLookupIndexReqKey indexName)
        {
	        IDictionary<TableLookupIndexReqKey, QueryPlanIndexItem> indexMap = new Dictionary<TableLookupIndexReqKey, QueryPlanIndexItem>();
	        indexMap.Put(indexName, null);
	        return new QueryPlanIndex(indexMap);
	    }
	}
} // end of namespace
