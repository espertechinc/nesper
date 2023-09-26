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

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.join.querygraph
{
    /// <summary>
    ///     Property lists stored as a value for each stream-to-stream relationship, for use by
    ///     <seealso cref="QueryGraphForge" />.
    /// </summary>
    public class QueryGraphValueForge
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        public QueryGraphValueForge()
        {
            Items = new List<QueryGraphValueDescForge>();
        }

        public IList<QueryGraphValueDescForge> Items { get; }

        public bool IsEmptyNotNavigable => Items.IsEmpty();

        /// <summary>
        ///     Add key and index property.
        /// </summary>
        /// <param name="keyProperty">key property</param>
        /// <param name="indexPropertyIdent">index property</param>
        /// <param name="keyPropNode">key node</param>
        /// <returns>true if added and either property did not exist, false if either already existed</returns>
        public bool AddStrictCompare(
            string keyProperty,
            ExprIdentNode keyPropNode,
            ExprIdentNode indexPropertyIdent)
        {
            var value = FindIdentEntry(indexPropertyIdent);
            if (value != null && value.Entry is QueryGraphValueEntryHashKeyedForgeExpr expr) {
                // if this index property exists and is compared to a constant, ignore the index prop
                if (expr.IsConstant) {
                    return false;
                }
            }

            if (value != null && value.Entry is QueryGraphValueEntryHashKeyedForgeProp) {
                return false; // second comparison, ignore
            }

            Items.Add(
                new QueryGraphValueDescForge(
                    new ExprNode[] { indexPropertyIdent },
                    new QueryGraphValueEntryHashKeyedForgeProp(
                        keyPropNode,
                        keyProperty,
                        keyPropNode.ExprEvaluatorIdent.Getter)));
            return true;
        }

        public void AddRange(
            QueryGraphRangeEnum rangeType,
            ExprNode propertyStart,
            ExprNode propertyEnd,
            ExprIdentNode propertyValueIdent)
        {
            if (!rangeType.IsRange()) {
                throw new ArgumentException("Expected range type, received " + rangeType);
            }

            // duplicate can be removed right away
            if (FindIdentEntry(propertyValueIdent) != null) {
                return;
            }

            Items.Add(
                new QueryGraphValueDescForge(
                    new ExprNode[] { propertyValueIdent },
                    new QueryGraphValueEntryRangeInForge(rangeType, propertyStart, propertyEnd, true)));
        }

        public void AddRelOp(
            ExprNode propertyKey,
            QueryGraphRangeEnum op,
            ExprIdentNode propertyValueIdent,
            bool isBetweenOrIn)
        {
            // Note: Read as follows:
            // System.out.println("If I have an index on '" + propertyValue + "' I'm evaluating " + propertyKey + " and finding all values of " + propertyValue + " " + op + " then " + propertyKey);

            // Check if there is an opportunity to convert this to a range or remove an earlier specification
            var existing = FindIdentEntry(propertyValueIdent);
            if (existing == null) {
                Items.Add(
                    new QueryGraphValueDescForge(
                        new ExprNode[] { propertyValueIdent },
                        new QueryGraphValueEntryRangeRelOpForge(op, propertyKey, isBetweenOrIn)));
                return;
            }

            if (!(existing.Entry is QueryGraphValueEntryRangeRelOpForge relOp)) {
                return; // another comparison exists already, don't add range
            }

            var opsDesc = QueryGraphRangeUtil.GetCanConsolidate(op, relOp.Type);
            if (opsDesc != null) {
                var start = !opsDesc.IsReverse ? relOp.Expression : propertyKey;
                var end = !opsDesc.IsReverse ? propertyKey : relOp.Expression;
                Items.Remove(existing);
                AddRange(opsDesc.Type, start, end, propertyValueIdent);
            }
        }

        public void AddUnkeyedExpr(
            ExprIdentNode indexedPropIdent,
            ExprNode exprNodeNoIdent)
        {
            Items.Add(
                new QueryGraphValueDescForge(
                    new ExprNode[] { indexedPropIdent },
                    new QueryGraphValueEntryHashKeyedForgeExpr(exprNodeNoIdent, false)));
        }

        public void AddKeyedExpr(
            ExprIdentNode indexedPropIdent,
            ExprNode exprNodeNoIdent)
        {
            Items.Add(
                new QueryGraphValueDescForge(
                    new ExprNode[] { indexedPropIdent },
                    new QueryGraphValueEntryHashKeyedForgeExpr(exprNodeNoIdent, true)));
        }

        public QueryGraphValuePairHashKeyIndexForge HashKeyProps {
            get {
                IList<QueryGraphValueEntryHashKeyedForge> keys = new List<QueryGraphValueEntryHashKeyedForge>();
                Deque<string> indexed = new ArrayDeque<string>();
                foreach (var desc in Items) {
                    if (desc.Entry is QueryGraphValueEntryHashKeyedForge keyprop) {
                        keys.Add(keyprop);
                        indexed.Add(GetSingleIdentNodeProp(desc.IndexExprs));
                    }
                }

                var strictKeys = new string[indexed.Count];
                var count = 0;
                foreach (var desc in Items) {
                    if (desc.Entry is QueryGraphValueEntryHashKeyedForge) {
                        if (desc.Entry is QueryGraphValueEntryHashKeyedForgeProp keyprop) {
                            strictKeys[count] = keyprop.KeyProperty;
                        }

                        count++;
                    }
                }

                return new QueryGraphValuePairHashKeyIndexForge(indexed.ToArray(), keys, strictKeys);
            }
        }

        public QueryGraphValuePairRangeIndexForge RangeProps {
            get {
                Deque<string> indexed = new ArrayDeque<string>();
                IList<QueryGraphValueEntryRangeForge> keys = new List<QueryGraphValueEntryRangeForge>();
                foreach (var desc in Items) {
                    if (desc.Entry is QueryGraphValueEntryRangeForge keyprop) {
                        keys.Add(keyprop);
                        indexed.Add(GetSingleIdentNodeProp(desc.IndexExprs));
                    }
                }

                return new QueryGraphValuePairRangeIndexForge(indexed.ToArray(), keys);
            }
        }

        public override string ToString()
        {
            var writer = new StringWriter();
            writer.Write("QueryGraphValue ");
            var delimiter = "";
            foreach (var desc in Items) {
                writer.Write(delimiter);
                writer.Write(ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceAsList(desc.IndexExprs));
                writer.Write(": ");
                writer.Write(desc.Entry.ToString());
                delimiter = ", ";
            }

            return writer.ToString();
        }

        public void AddInKeywordMultiIdx(
            ExprNode testPropExpr,
            ExprNode[] setProps)
        {
            Items.Add(
                new QueryGraphValueDescForge(setProps, new QueryGraphValueEntryInKeywordMultiIdxForge(testPropExpr)));
        }

        public void AddInKeywordSingleIdx(
            ExprNode testPropIdent,
            ExprNode[] setPropExpr)
        {
            var indexExpressions = new ExprNode[] { testPropIdent };
            var found = FindEntry(indexExpressions);

            var setExpressions = setPropExpr;
            if (found != null && found.Entry is QueryGraphValueEntryInKeywordSingleIdxForge existing) {
                setExpressions = (ExprNode[])CollectionUtil.AddArrays(existing.KeyExprs, setPropExpr);
                Items.Remove(found);
            }

            Items.Add(
                new QueryGraphValueDescForge(
                    new[] { testPropIdent },
                    new QueryGraphValueEntryInKeywordSingleIdxForge(setExpressions)));
        }

        public QueryGraphValuePairInKWSingleIdxForge InKeywordSingles {
            get {
                IList<string> indexedProps = new List<string>();
                IList<QueryGraphValueEntryInKeywordSingleIdxForge> single =
                    new List<QueryGraphValueEntryInKeywordSingleIdxForge>();
                foreach (var desc in Items) {
                    if (desc.Entry is QueryGraphValueEntryInKeywordSingleIdxForge keyprop) {
                        single.Add(keyprop);
                        indexedProps.Add(GetSingleIdentNodeProp(desc.IndexExprs));
                    }
                }

                return new QueryGraphValuePairInKWSingleIdxForge(indexedProps.ToArray(), single);
            }
        }

        public IList<QueryGraphValuePairInKWMultiIdx> InKeywordMulti {
            get {
                IList<QueryGraphValuePairInKWMultiIdx> multi = new List<QueryGraphValuePairInKWMultiIdx>();
                foreach (var desc in Items) {
                    if (desc.Entry is QueryGraphValueEntryInKeywordMultiIdxForge keyprop) {
                        multi.Add(new QueryGraphValuePairInKWMultiIdx(desc.IndexExprs, keyprop));
                    }
                }

                return multi;
            }
        }

        public void AddCustom(
            ExprNode[] indexExpressions,
            string operationName,
            int expressionPosition,
            ExprNode expression)
        {
            // find existing custom-entry for same index expressions
            QueryGraphValueEntryCustomForge found = null;
            foreach (var desc in Items) {
                if (desc.Entry is QueryGraphValueEntryCustomForge forge) {
                    if (ExprNodeUtilityCompare.DeepEquals(desc.IndexExprs, indexExpressions, true)) {
                        found = forge;
                        break;
                    }
                }
            }

            if (found == null) {
                found = new QueryGraphValueEntryCustomForge();
                Items.Add(new QueryGraphValueDescForge(indexExpressions, found));
            }

            // find/create operation against the indexed fields
            var key = new QueryGraphValueEntryCustomKeyForge(operationName, indexExpressions);
            var op = found.Operations.Get(key);
            if (op == null) {
                op = new QueryGraphValueEntryCustomOperationForge();
                found.Operations.Put(key, op);
            }

            op.PositionalExpressions.Put(expressionPosition, expression);
        }

        private QueryGraphValueDescForge FindIdentEntry(ExprIdentNode search)
        {
            foreach (var desc in Items) {
                if (desc.IndexExprs.Length > 1 || !(desc.IndexExprs[0] is ExprIdentNode)) {
                    continue;
                }

                var other = (ExprIdentNode)desc.IndexExprs[0];
                if (search.ResolvedPropertyName.Equals(other.ResolvedPropertyName)) {
                    return desc;
                }
            }

            return null;
        }

        private QueryGraphValueDescForge FindEntry(ExprNode[] search)
        {
            foreach (var desc in Items) {
                if (ExprNodeUtilityCompare.DeepEquals(search, desc.IndexExprs, true)) {
                    return desc;
                }
            }

            return null;
        }

        private string GetSingleIdentNodeProp(ExprNode[] indexExprs)
        {
            if (indexExprs.Length != 1) {
                throw new IllegalStateException("Incorrect number of index expressions");
            }

            var identNode = (ExprIdentNode)indexExprs[0];
            return identNode.ResolvedPropertyName;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(QueryGraphValue), GetType(), classScope);
            method.Block.DeclareVar<IList<QueryGraphValueDesc>>(
                "items",
                NewInstance<List<QueryGraphValueDesc>>(Constant(Items.Count)));
            for (var i = 0; i < Items.Count; i++) {
                method.Block.ExprDotMethod(Ref("items"), "Add", Items[i].Make(method, symbols, classScope));
            }

            method.Block.MethodReturn(NewInstance<QueryGraphValue>(Ref("items")));
            return LocalMethod(method);
        }
    }
} // end of namespace