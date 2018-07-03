///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.join.plan
{
    /// <summary>
    /// Property lists stored as a value for each stream-to-stream relationship, for use by <seealso cref="QueryGraph" />.
    /// </summary>
    public class QueryGraphValue
    {
        private readonly IList<QueryGraphValueDesc> _items;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        public QueryGraphValue()
        {
            _items = new List<QueryGraphValueDesc>();
        }

        public bool IsEmptyNotNavigable
        {
            get { return _items.IsEmpty(); }
        }

        public IList<QueryGraphValueDesc> Items
        {
            get { return _items; }
        }

        /// <summary>
        /// Add key and index property.
        /// </summary>
        /// <param name="keyProperty">key property</param>
        /// <param name="keyPropNode">The key property node.</param>
        /// <param name="indexPropertyIdent">index property</param>
        /// <returns>
        /// true if added and either property did not exist, false if either already existed
        /// </returns>
        public bool AddStrictCompare(string keyProperty, ExprIdentNode keyPropNode, ExprIdentNode indexPropertyIdent)
        {
            var value = FindIdentEntry(indexPropertyIdent);
            if (value != null && value.Entry is QueryGraphValueEntryHashKeyedExpr) {
                // if this index property exists and is compared to a constant, ignore the index prop
                var expr = (QueryGraphValueEntryHashKeyedExpr) value.Entry;
                if (expr.IsConstant) {
                    return false;
                }
            }
            if (value != null && value.Entry is QueryGraphValueEntryHashKeyedProp) {
                return false;   // second comparison, ignore
            }
    
            _items.Add(new QueryGraphValueDesc(new ExprNode[]{indexPropertyIdent}, new QueryGraphValueEntryHashKeyedProp(keyPropNode, keyProperty)));
            return true;
        }
    
        public void AddRange(QueryGraphRangeEnum rangeType, ExprNode propertyStart, ExprNode propertyEnd, ExprIdentNode propertyValueIdent)
        {
            if (!rangeType.IsRange())
            {
                throw new ArgumentException("Expected range type, received " + rangeType);
            }
    
            // duplicate can be removed right away
            if (FindIdentEntry(propertyValueIdent) != null) {
                return;
            }
    
            _items.Add(new QueryGraphValueDesc(new ExprNode[] {propertyValueIdent}, new QueryGraphValueEntryRangeIn(rangeType, propertyStart, propertyEnd, true)));
        }
    
        public void AddRelOp(ExprNode propertyKey, QueryGraphRangeEnum op, ExprIdentNode propertyValueIdent, bool isBetweenOrIn)
        {
            // Note: Read as follows:
            // If I have an index on {propertyValue} I'm evaluating {propertyKey} and finding all values of {propertyValue} {op} then {propertyKey}

            // Check if there is an opportunity to convert this to a range or remove an earlier specification
            var existing = FindIdentEntry(propertyValueIdent);
            if (existing == null) {
                _items.Add(new QueryGraphValueDesc(new ExprNode[]{propertyValueIdent}, new QueryGraphValueEntryRangeRelOp(op, propertyKey, isBetweenOrIn)));
                return;
            }
    
            if (!(existing.Entry is QueryGraphValueEntryRangeRelOp)) {
                return; // another comparison exists already, don't add range
            }
    
            var relOp = (QueryGraphValueEntryRangeRelOp) existing.Entry;
            var opsDesc = QueryGraphRangeUtil.GetCanConsolidate(op, relOp.RangeType);
            if (opsDesc != null) {
                var start = !opsDesc.IsReverse ? relOp.Expression : propertyKey;
                var end = !opsDesc.IsReverse ?  propertyKey : relOp.Expression;
                _items.Remove(existing);
                AddRange(opsDesc.RangeType, start, end, propertyValueIdent);
            }
        }
    
        public void AddUnkeyedExpr(ExprIdentNode indexedPropIdent, ExprNode exprNodeNoIdent) {
            _items.Add(new QueryGraphValueDesc(new ExprNode[] {indexedPropIdent}, new QueryGraphValueEntryHashKeyedExpr(exprNodeNoIdent, false)));
        }
    
        public void AddKeyedExpr(ExprIdentNode indexedPropIdent, ExprNode exprNodeNoIdent) {
            _items.Add(new QueryGraphValueDesc(new ExprNode[]{indexedPropIdent}, new QueryGraphValueEntryHashKeyedExpr(exprNodeNoIdent, true)));
        }

        public QueryGraphValuePairHashKeyIndex HashKeyProps
        {
            get
            {
                IList<QueryGraphValueEntryHashKeyed> keys = new List<QueryGraphValueEntryHashKeyed>();
                Deque<string> indexed = new ArrayDeque<string>();
                foreach (var desc in _items)
                {
                    if (desc.Entry is QueryGraphValueEntryHashKeyed)
                    {
                        var keyprop = (QueryGraphValueEntryHashKeyed) desc.Entry;
                        keys.Add(keyprop);
                        indexed.Add(GetSingleIdentNodeProp(desc.IndexExprs));
                    }
                }

                var strictKeys = new string[indexed.Count];
                var count = 0;
                foreach (var desc in _items)
                {
                    if (desc.Entry is QueryGraphValueEntryHashKeyed)
                    {
                        if (desc.Entry is QueryGraphValueEntryHashKeyedProp)
                        {
                            var keyprop = (QueryGraphValueEntryHashKeyedProp) desc.Entry;
                            strictKeys[count] = keyprop.KeyProperty;
                        }
                        count++;
                    }
                }

                return new QueryGraphValuePairHashKeyIndex(indexed.ToArray(), keys, strictKeys);
            }
        }

        public QueryGraphValuePairRangeIndex RangeProps
        {
            get
            {
                Deque<string> indexed = new ArrayDeque<string>();
                IList<QueryGraphValueEntryRange> keys = new List<QueryGraphValueEntryRange>();
                foreach (var desc in _items)
                {
                    if (desc.Entry is QueryGraphValueEntryRange)
                    {
                        var keyprop = (QueryGraphValueEntryRange) desc.Entry;
                        keys.Add(keyprop);
                        indexed.Add(GetSingleIdentNodeProp(desc.IndexExprs));
                    }
                }
                return new QueryGraphValuePairRangeIndex(indexed.ToArray(), keys);
            }
        }

        public override string ToString()
        {
            var writer = new StringWriter();
            writer.Write("QueryGraphValue ");
            var delimiter = "";
            foreach (var desc in _items) {
                writer.Write(delimiter);
                writer.Write(ExprNodeUtility.ToExpressionStringMinPrecedenceAsList(desc.IndexExprs));
                writer.Write(": ");
                writer.Write(desc.Entry.ToString());
                delimiter = ", ";
            }
            return writer.ToString();
        }
    
        public void AddInKeywordMultiIdx(ExprNode testPropExpr, IList<ExprNode> setProps) {
            _items.Add(new QueryGraphValueDesc(setProps, new QueryGraphValueEntryInKeywordMultiIdx(testPropExpr)));
        }
    
        public void AddInKeywordSingleIdx(ExprNode testPropIdent, IList<ExprNode> setPropExpr) {
            var indexExpressions = new ExprNode[]{testPropIdent};
            var found = FindEntry(indexExpressions);
    
            var setExpressions = setPropExpr;
            if (found != null && found.Entry is QueryGraphValueEntryInKeywordSingleIdx) {
                var existing = (QueryGraphValueEntryInKeywordSingleIdx) found.Entry;
                setExpressions = (ExprNode[]) CollectionUtil.AddArrays(existing.KeyExprs, setPropExpr);
                _items.Remove(found);
            }
            _items.Add(new QueryGraphValueDesc(
                new ExprNode[]{ testPropIdent },
                new QueryGraphValueEntryInKeywordSingleIdx(setExpressions)));
        }

        public QueryGraphValuePairInKWSingleIdx InKeywordSingles
        {
            get
            {
                IList<string> indexedProps = new List<string>();
                IList<QueryGraphValueEntryInKeywordSingleIdx> single = new List<QueryGraphValueEntryInKeywordSingleIdx>();
                foreach (var desc in _items)
                {
                    if (desc.Entry is QueryGraphValueEntryInKeywordSingleIdx)
                    {
                        var keyprop = (QueryGraphValueEntryInKeywordSingleIdx) desc.Entry;
                        single.Add(keyprop);
                        indexedProps.Add(GetSingleIdentNodeProp(desc.IndexExprs));
                    }
                }
                return new QueryGraphValuePairInKWSingleIdx(indexedProps.ToArray(), single);
            }
        }

        public IList<QueryGraphValuePairInKWMultiIdx> InKeywordMulti
        {
            get
            {
                IList<QueryGraphValuePairInKWMultiIdx> multi = new List<QueryGraphValuePairInKWMultiIdx>();
                foreach (var desc in _items)
                {
                    if (desc.Entry is QueryGraphValueEntryInKeywordMultiIdx)
                    {
                        var keyprop = (QueryGraphValueEntryInKeywordMultiIdx) desc.Entry;
                        multi.Add(new QueryGraphValuePairInKWMultiIdx(desc.IndexExprs, keyprop));
                    }
                }
                return multi;
            }
        }

        public void AddCustom(IList<ExprNode> indexExpressions, String operationName, int expressionPosition, ExprNode expression)
        {
            // find existing custom-entry for same index expressions
            QueryGraphValueEntryCustom found = null;
            foreach (QueryGraphValueDesc desc in _items)
            {
                if (desc.Entry is QueryGraphValueEntryCustom)
                {
                    if (ExprNodeUtility.DeepEquals(desc.IndexExprs, indexExpressions, true))
                    {
                        found = (QueryGraphValueEntryCustom) desc.Entry;
                        break;
                    }
                }
            }

            if (found == null)
            {
                found = new QueryGraphValueEntryCustom();
                _items.Add(new QueryGraphValueDesc(indexExpressions, found));
            }

            // find/create operation against the indexed fields
            QueryGraphValueEntryCustomKey key = new QueryGraphValueEntryCustomKey(operationName, indexExpressions);
            QueryGraphValueEntryCustomOperation op = found.Operations.Get(key);
            if (op == null) {
                op = new QueryGraphValueEntryCustomOperation();
                found.Operations.Put(key, op);
            }

            op.PositionalExpressions.Put(expressionPosition, expression);
        }

        private QueryGraphValueDesc FindIdentEntry(ExprIdentNode search) {
            foreach (var desc in _items) {
                if (desc.IndexExprs.Count > 1 || !(desc.IndexExprs[0] is ExprIdentNode)) {
                    continue;
                }
                var other = (ExprIdentNode) desc.IndexExprs[0];
                if (search.ResolvedPropertyName.Equals(other.ResolvedPropertyName)) {
                    return desc;
                }
            }
            return null;
        }
    
        private QueryGraphValueDesc FindEntry(IList<ExprNode> search) {
            foreach (var desc in _items) {
                if (ExprNodeUtility.DeepEquals(search, desc.IndexExprs, true)) {
                    return desc;
                }
            }
            return null;
        }
    
        private string GetSingleIdentNodeProp(IList<ExprNode> indexExprs) {
            if (indexExprs.Count != 1) {
                throw new IllegalStateException("Incorrect number of index expressions");
            }
            var identNode = (ExprIdentNode) indexExprs[0];
            return identNode.ResolvedPropertyName;
        }
    }
    
}
