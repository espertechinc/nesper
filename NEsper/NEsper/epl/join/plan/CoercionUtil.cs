///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.join.plan
{
    public class CoercionUtil {
    
        private static readonly Type[] NULL_ARRAY = new Type[0];
    
        public static CoercionDesc GetCoercionTypesRange(EventType[] typesPerStream, int indexedStream, IList<string> indexedProp, IList<QueryGraphValueEntryRange> rangeEntries)
        {
            if (rangeEntries.IsEmpty()) {
                return new CoercionDesc(false, NULL_ARRAY);
            }
    
            var coercionTypes = new Type[rangeEntries.Count];
            bool mustCoerce = false;
            for (int i = 0; i < rangeEntries.Count; i++)
            {
                QueryGraphValueEntryRange entry = rangeEntries[i];
    
                String indexed = indexedProp[i];
                Type valuePropType = typesPerStream[indexedStream].GetPropertyType(indexed).GetBoxedType();
                Type coercionType;
    
                if (entry.RangeType.IsRange()) {
                    var rangeIn = (QueryGraphValueEntryRangeIn) entry;
                    coercionType = GetCoercionTypeRangeIn(valuePropType, rangeIn.ExprStart, rangeIn.ExprEnd);
                }
                else {
                    var relOp = (QueryGraphValueEntryRangeRelOp) entry;
                    coercionType = GetCoercionType(valuePropType, relOp.Expression.ExprEvaluator.ReturnType);
                }
    
                if (coercionType == null) {
                    coercionTypes[i] = valuePropType;
                }
                else {
                    mustCoerce = true;
                    coercionTypes[i] = coercionType;
                }
            }
    
            return new CoercionDesc(mustCoerce, coercionTypes);
        }
    
        /// <summary>Returns null if no coercion is required, or an array of classes for use in coercing the lookup keys and index keys into a common type. </summary>
        /// <param name="typesPerStream">is the event types for each stream</param>
        /// <param name="lookupStream">is the stream looked up from</param>
        /// <param name="indexedStream">is the indexed stream</param>
        /// <param name="keyProps">is the properties to use to look up</param>
        /// <param name="indexProps">is the properties to index on</param>
        /// <returns>coercion types, or null if none required</returns>
        public static CoercionDesc GetCoercionTypesHash(EventType[] typesPerStream,
                                                        int lookupStream,
                                                        int indexedStream,
                                                        IList<QueryGraphValueEntryHashKeyed> keyProps,
                                                        IList<string> indexProps)
        {
            if (indexProps.Count == 0 && keyProps.Count == 0) {
                return new CoercionDesc(false, NULL_ARRAY);
            }
            if (indexProps.Count != keyProps.Count)
            {
                throw new IllegalStateException("Mismatch in the number of key and index properties");
            }

            var coercionTypes = new Type[indexProps.Count];
            bool mustCoerce = false;
            for (int i = 0; i < keyProps.Count; i++)
            {
                Type keyPropType;
                if (keyProps[i] is QueryGraphValueEntryHashKeyedExpr)
                {
                    var hashExpr = (QueryGraphValueEntryHashKeyedExpr) keyProps[i];
                    keyPropType = hashExpr.KeyExpr.ExprEvaluator.ReturnType;
                }
                else
                {
                    var hashKeyProp = (QueryGraphValueEntryHashKeyedProp) keyProps[i];
                    keyPropType = typesPerStream[lookupStream].GetPropertyType(hashKeyProp.KeyProperty).GetBoxedType();
                }

                Type indexedPropType = typesPerStream[indexedStream].GetPropertyType(indexProps[i]).GetBoxedType();
                Type coercionType = indexedPropType;
                if (keyPropType != indexedPropType)
                {
                    coercionType = keyPropType.GetCompareToCoercionType(indexedPropType);
                    mustCoerce = true;
                }
                coercionTypes[i] = coercionType;
            }
            return new CoercionDesc(mustCoerce, coercionTypes);
        }
    
        public static Type GetCoercionTypeRange(EventType indexedType, String indexedProp, SubordPropRangeKey rangeKey) {
            QueryGraphValueEntryRange desc = rangeKey.RangeInfo;
            if (desc.RangeType.IsRange()) {
                var rangeIn = (QueryGraphValueEntryRangeIn) desc;
                return GetCoercionTypeRangeIn(indexedType.GetPropertyType(indexedProp), rangeIn.ExprStart, rangeIn.ExprEnd);
            }

            var relOp = (QueryGraphValueEntryRangeRelOp) desc;
            return GetCoercionType(indexedType.GetPropertyType(indexedProp), relOp.Expression.ExprEvaluator.ReturnType);
        }
    
        public static CoercionDesc GetCoercionTypesRange(EventType viewableEventType, IDictionary<String, SubordPropRangeKey> rangeProps, EventType[] typesPerStream) {
            if (rangeProps.IsEmpty()) {
                return new CoercionDesc(false, NULL_ARRAY);
            }
    
            var coercionTypes = new Type[rangeProps.Count];
            bool mustCoerce = false;
            int count = 0;
            foreach (KeyValuePair<String, SubordPropRangeKey> entry in rangeProps)
            {
                SubordPropRangeKey subQRange = entry.Value;
                QueryGraphValueEntryRange rangeDesc = entry.Value.RangeInfo;

                Type valuePropType = viewableEventType.GetPropertyType(entry.Key).GetBoxedType();
                Type coercionType;
    
                if (rangeDesc.RangeType.IsRange()) {
                    var rangeIn = (QueryGraphValueEntryRangeIn) rangeDesc;
                    coercionType = GetCoercionTypeRangeIn(valuePropType, rangeIn.ExprStart, rangeIn.ExprEnd);
                }
                else {
                    var relOp = (QueryGraphValueEntryRangeRelOp) rangeDesc;
                    coercionType = GetCoercionType(valuePropType, relOp.Expression.ExprEvaluator.ReturnType);
                }
    
                if (coercionType == null) {
                    coercionTypes[count++] = valuePropType;
                }
                else {
                    mustCoerce = true;
                    coercionTypes[count++] = coercionType;
                }
            }
            return new CoercionDesc(mustCoerce, coercionTypes);
        }

        private static Type GetCoercionType(Type valuePropType, Type keyPropTypeExpr)
        {
            Type coercionType = null;
            Type keyPropType = keyPropTypeExpr.GetBoxedType();
            if (valuePropType != keyPropType)
            {
                coercionType = valuePropType.GetCompareToCoercionType(keyPropType);
            }
            return coercionType;
        }
    
        public static CoercionDesc GetCoercionTypesHash(EventType viewableEventType, String[] indexProps, IList<SubordPropHashKey> hashKeys) {
            if (indexProps.Length == 0 && hashKeys.Count == 0) {
                return new CoercionDesc(false, NULL_ARRAY);
            }
            if (indexProps.Length != hashKeys.Count) {
                throw new IllegalStateException("Mismatch in the number of key and index properties");
            }
    
            var coercionTypes = new Type[indexProps.Length];
            bool mustCoerce = false;
            for (int i = 0; i < hashKeys.Count; i++)
            {
                Type keyPropType = hashKeys[i].HashKey.KeyExpr.ExprEvaluator.ReturnType.GetBoxedType();
                Type indexedPropType = viewableEventType.GetPropertyType(indexProps[i]).GetBoxedType();
                Type coercionType = indexedPropType;
                if (keyPropType != indexedPropType)
                {
                    coercionType = keyPropType.GetCompareToCoercionType(indexedPropType);
                    mustCoerce = true;
                }
                coercionTypes[i] = coercionType;
            }
            return new CoercionDesc(mustCoerce, coercionTypes);
        }

        public static Type GetCoercionTypeRangeIn(Type valuePropType, ExprNode exprStart, ExprNode exprEnd)
        {
            Type coercionType = null;
            Type startPropType = exprStart.ExprEvaluator.ReturnType.GetBoxedType();
            Type endPropType = exprEnd.ExprEvaluator.ReturnType.GetBoxedType();
    
            if (valuePropType != startPropType)
            {
                coercionType = valuePropType.GetCompareToCoercionType(startPropType);
            }
            if (valuePropType != endPropType)
            {
                coercionType = coercionType.GetCompareToCoercionType(endPropType);
            }
            return coercionType;
        }
    }
}
