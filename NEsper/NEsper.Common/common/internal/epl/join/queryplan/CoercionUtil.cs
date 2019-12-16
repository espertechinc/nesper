///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.epl.lookupplan;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.queryplan
{
    public class CoercionUtil
    {
        private static readonly Type[] NULL_ARRAY = new Type[0];

        public static CoercionDesc GetCoercionTypesRange(
            EventType[] typesPerStream,
            int indexedStream,
            string[] indexedProp,
            IList<QueryGraphValueEntryRangeForge> rangeEntries)
        {
            if (rangeEntries.IsEmpty()) {
                return new CoercionDesc(false, NULL_ARRAY);
            }

            var coercionTypes = new Type[rangeEntries.Count];
            var mustCoerce = false;
            for (var i = 0; i < rangeEntries.Count; i++) {
                var entry = rangeEntries[i];

                var indexed = indexedProp[i];
                var valuePropType = typesPerStream[indexedStream].GetPropertyType(indexed).GetBoxedType();
                Type coercionType;

                if (entry.Type.IsRange()) {
                    var rangeIn = (QueryGraphValueEntryRangeInForge) entry;
                    coercionType = GetCoercionTypeRangeIn(valuePropType, rangeIn.ExprStart, rangeIn.ExprEnd);
                }
                else {
                    var relOp = (QueryGraphValueEntryRangeRelOpForge) entry;
                    coercionType = GetCoercionType(valuePropType, relOp.Expression.Forge.EvaluationType);
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

        /// <summary>
        ///     Returns null if no coercion is required, or an array of classes for use in coercing the
        ///     lookup keys and index keys into a common type.
        /// </summary>
        /// <param name="typesPerStream">is the event types for each stream</param>
        /// <param name="lookupStream">is the stream looked up from</param>
        /// <param name="indexedStream">is the indexed stream</param>
        /// <param name="keyProps">is the properties to use to look up</param>
        /// <param name="indexProps">is the properties to index on</param>
        /// <returns>coercion types, or null if none required</returns>
        public static CoercionDesc GetCoercionTypesHash(
            EventType[] typesPerStream,
            int lookupStream,
            int indexedStream,
            IList<QueryGraphValueEntryHashKeyedForge> keyProps,
            string[] indexProps)
        {
            if (indexProps.Length == 0 && keyProps.Count == 0) {
                return new CoercionDesc(false, NULL_ARRAY);
            }

            if (indexProps.Length != keyProps.Count) {
                throw new IllegalStateException("Mismatch in the number of key and index properties");
            }

            var coercionTypes = new Type[indexProps.Length];
            var mustCoerce = false;
            for (var i = 0; i < keyProps.Count; i++) {
                Type keyPropType;
                if (keyProps[i] is QueryGraphValueEntryHashKeyedForgeExpr) {
                    var hashExpr = (QueryGraphValueEntryHashKeyedForgeExpr) keyProps[i];
                    keyPropType = hashExpr.KeyExpr.Forge.EvaluationType.GetBoxedType();
                }
                else {
                    var hashKeyProp = (QueryGraphValueEntryHashKeyedForgeProp) keyProps[i];
                    keyPropType = typesPerStream[lookupStream].GetPropertyType(hashKeyProp.KeyProperty).GetBoxedType();
                }

                var indexedPropType = typesPerStream[indexedStream].GetPropertyType(indexProps[i]).GetBoxedType();
                var coercionType = indexedPropType;
                if (keyPropType != indexedPropType) {
                    coercionType = keyPropType.GetCompareToCoercionType(indexedPropType);
                    mustCoerce = true;
                }

                coercionTypes[i] = coercionType;
            }

            return new CoercionDesc(mustCoerce, coercionTypes);
        }

        public static Type GetCoercionTypeRange(
            EventType indexedType,
            string indexedProp,
            SubordPropRangeKeyForge rangeKey)
        {
            var desc = rangeKey.RangeInfo;
            if (desc.Type.IsRange()) {
                var rangeIn = (QueryGraphValueEntryRangeInForge) desc;
                return GetCoercionTypeRangeIn(
                    indexedType.GetPropertyType(indexedProp),
                    rangeIn.ExprStart,
                    rangeIn.ExprEnd);
            }

            var relOp = (QueryGraphValueEntryRangeRelOpForge) desc;
            return GetCoercionType(indexedType.GetPropertyType(indexedProp), relOp.Expression.Forge.EvaluationType);
        }

        public static CoercionDesc GetCoercionTypesRange(
            EventType viewableEventType,
            IDictionary<string, SubordPropRangeKeyForge> rangeProps,
            EventType[] typesPerStream)
        {
            if (rangeProps.IsEmpty()) {
                return new CoercionDesc(false, NULL_ARRAY);
            }

            var coercionTypes = new Type[rangeProps.Count];
            var mustCoerce = false;
            var count = 0;
            foreach (var entry in rangeProps) {
                var subQRange = entry.Value;
                var rangeDesc = entry.Value.RangeInfo;

                var valuePropType = viewableEventType.GetPropertyType(entry.Key).GetBoxedType();
                Type coercionType;

                if (rangeDesc.Type.IsRange()) {
                    var rangeIn = (QueryGraphValueEntryRangeInForge) rangeDesc;
                    coercionType = GetCoercionTypeRangeIn(valuePropType, rangeIn.ExprStart, rangeIn.ExprEnd);
                }
                else {
                    var relOp = (QueryGraphValueEntryRangeRelOpForge) rangeDesc;
                    coercionType = GetCoercionType(valuePropType, relOp.Expression.Forge.EvaluationType);
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

        private static Type GetCoercionType(
            Type valuePropType,
            Type keyPropTypeExpr)
        {
            Type coercionType = null;
            var keyPropType = keyPropTypeExpr.GetBoxedType();
            if (valuePropType != keyPropType) {
                coercionType = valuePropType.GetCompareToCoercionType(keyPropType);
            }

            return coercionType;
        }

        public static CoercionDesc GetCoercionTypesHash(
            EventType viewableEventType,
            string[] indexProps,
            IList<SubordPropHashKeyForge> hashKeys)
        {
            if (indexProps.Length == 0 && hashKeys.Count == 0) {
                return new CoercionDesc(false, NULL_ARRAY);
            }

            if (indexProps.Length != hashKeys.Count) {
                throw new IllegalStateException("Mismatch in the number of key and index properties");
            }

            var coercionTypes = new Type[indexProps.Length];
            var mustCoerce = false;
            for (var i = 0; i < hashKeys.Count; i++) {
                var keyPropType = hashKeys[i].HashKey.KeyExpr.Forge.EvaluationType.GetBoxedType();
                var indexedPropType = viewableEventType.GetPropertyType(indexProps[i]).GetBoxedType();
                var coercionType = indexedPropType;
                if (keyPropType != indexedPropType) {
                    coercionType = keyPropType.GetCompareToCoercionType(indexedPropType);
                    mustCoerce = true;
                }

                coercionTypes[i] = coercionType;
            }

            return new CoercionDesc(mustCoerce, coercionTypes);
        }

        public static Type GetCoercionTypeRangeIn(
            Type valuePropType,
            ExprNode exprStart,
            ExprNode exprEnd)
        {
            Type coercionType = null;
            var startPropType = exprStart.Forge.EvaluationType.GetBoxedType();
            var endPropType = exprEnd.Forge.EvaluationType.GetBoxedType();

            if (valuePropType != startPropType) {
                coercionType = valuePropType.GetCompareToCoercionType(startPropType);
            }

            if (valuePropType != endPropType) {
                coercionType = coercionType.GetCompareToCoercionType(endPropType);
            }

            return coercionType;
        }
    }
} // end of namespace