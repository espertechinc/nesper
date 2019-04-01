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
	public class CoercionUtil {

	    private static readonly Type[] NULL_ARRAY = new Type[0];

	    public static CoercionDesc GetCoercionTypesRange(EventType[] typesPerStream, int indexedStream, string[] indexedProp, IList<QueryGraphValueEntryRangeForge> rangeEntries) {
	        if (rangeEntries.IsEmpty()) {
	            return new CoercionDesc(false, NULL_ARRAY);
	        }

	        Type[] coercionTypes = new Type[rangeEntries.Count];
	        bool mustCoerce = false;
	        for (int i = 0; i < rangeEntries.Count; i++) {
	            QueryGraphValueEntryRangeForge entry = rangeEntries.Get(i);

	            string indexed = indexedProp[i];
	            Type valuePropType = Boxing.GetBoxedType(typesPerStream[indexedStream].GetPropertyType(indexed));
	            Type coercionType;

	            if (entry.Type.IsRange) {
	                QueryGraphValueEntryRangeInForge rangeIn = (QueryGraphValueEntryRangeInForge) entry;
	                coercionType = GetCoercionTypeRangeIn(valuePropType, rangeIn.ExprStart, rangeIn.ExprEnd);
	            } else {
	                QueryGraphValueEntryRangeRelOpForge relOp = (QueryGraphValueEntryRangeRelOpForge) entry;
	                coercionType = GetCoercionType(valuePropType, relOp.Expression.Forge.EvaluationType);
	            }

	            if (coercionType == null) {
	                coercionTypes[i] = valuePropType;
	            } else {
	                mustCoerce = true;
	                coercionTypes[i] = coercionType;
	            }
	        }

	        return new CoercionDesc(mustCoerce, coercionTypes);
	    }

	    /// <summary>
	    /// Returns null if no coercion is required, or an array of classes for use in coercing the
	    /// lookup keys and index keys into a common type.
	    /// </summary>
	    /// <param name="typesPerStream">is the event types for each stream</param>
	    /// <param name="lookupStream">is the stream looked up from</param>
	    /// <param name="indexedStream">is the indexed stream</param>
	    /// <param name="keyProps">is the properties to use to look up</param>
	    /// <param name="indexProps">is the properties to index on</param>
	    /// <returns>coercion types, or null if none required</returns>
	    public static CoercionDesc GetCoercionTypesHash(EventType[] typesPerStream,
	                                                    int lookupStream,
	                                                    int indexedStream,
	                                                    IList<QueryGraphValueEntryHashKeyedForge> keyProps,
	                                                    string[] indexProps) {
	        if (indexProps.Length == 0 && keyProps.Count == 0) {
	            return new CoercionDesc(false, NULL_ARRAY);
	        }
	        if (indexProps.Length != keyProps.Count) {
	            throw new IllegalStateException("Mismatch in the number of key and index properties");
	        }

	        Type[] coercionTypes = new Type[indexProps.Length];
	        bool mustCoerce = false;
	        for (int i = 0; i < keyProps.Count; i++) {
	            Type keyPropType;
	            if (keyProps.Get(i) is QueryGraphValueEntryHashKeyedForgeExpr) {
	                QueryGraphValueEntryHashKeyedForgeExpr hashExpr = (QueryGraphValueEntryHashKeyedForgeExpr) keyProps.Get(i);
	                keyPropType = hashExpr.KeyExpr.Forge.EvaluationType;
	            } else {
	                QueryGraphValueEntryHashKeyedForgeProp hashKeyProp = (QueryGraphValueEntryHashKeyedForgeProp) keyProps.Get(i);
	                keyPropType = Boxing.GetBoxedType(typesPerStream[lookupStream].GetPropertyType(hashKeyProp.KeyProperty));
	            }

	            Type indexedPropType = Boxing.GetBoxedType(typesPerStream[indexedStream].GetPropertyType(indexProps[i]));
	            Type coercionType = indexedPropType;
	            if (keyPropType != indexedPropType) {
	                coercionType = TypeHelper.GetCompareToCoercionType(keyPropType, indexedPropType);
	                mustCoerce = true;
	            }
	            coercionTypes[i] = coercionType;
	        }
	        return new CoercionDesc(mustCoerce, coercionTypes);
	    }

	    public static Type GetCoercionTypeRange(EventType indexedType, string indexedProp, SubordPropRangeKeyForge rangeKey) {
	        QueryGraphValueEntryRangeForge desc = rangeKey.RangeInfo;
	        if (desc.Type.IsRange) {
	            QueryGraphValueEntryRangeInForge rangeIn = (QueryGraphValueEntryRangeInForge) desc;
	            return GetCoercionTypeRangeIn(indexedType.GetPropertyType(indexedProp), rangeIn.ExprStart, rangeIn.ExprEnd);
	        } else {
	            QueryGraphValueEntryRangeRelOpForge relOp = (QueryGraphValueEntryRangeRelOpForge) desc;
	            return GetCoercionType(indexedType.GetPropertyType(indexedProp), relOp.Expression.Forge.EvaluationType);
	        }
	    }

	    public static CoercionDesc GetCoercionTypesRange(EventType viewableEventType, IDictionary<string, SubordPropRangeKeyForge> rangeProps, EventType[] typesPerStream) {
	        if (rangeProps.IsEmpty()) {
	            return new CoercionDesc(false, NULL_ARRAY);
	        }

	        Type[] coercionTypes = new Type[rangeProps.Count];
	        bool mustCoerce = false;
	        int count = 0;
	        foreach (KeyValuePair<string, SubordPropRangeKeyForge> entry in rangeProps) {
	            SubordPropRangeKeyForge subQRange = entry.Value;
	            QueryGraphValueEntryRangeForge rangeDesc = entry.Value.RangeInfo;

	            Type valuePropType = Boxing.GetBoxedType(viewableEventType.GetPropertyType(entry.Key));
	            Type coercionType;

	            if (rangeDesc.Type.IsRange) {
	                QueryGraphValueEntryRangeInForge rangeIn = (QueryGraphValueEntryRangeInForge) rangeDesc;
	                coercionType = GetCoercionTypeRangeIn(valuePropType, rangeIn.ExprStart, rangeIn.ExprEnd);
	            } else {
	                QueryGraphValueEntryRangeRelOpForge relOp = (QueryGraphValueEntryRangeRelOpForge) rangeDesc;
	                coercionType = GetCoercionType(valuePropType, relOp.Expression.Forge.EvaluationType);
	            }

	            if (coercionType == null) {
	                coercionTypes[count++] = valuePropType;
	            } else {
	                mustCoerce = true;
	                coercionTypes[count++] = coercionType;
	            }
	        }
	        return new CoercionDesc(mustCoerce, coercionTypes);
	    }

	    private static Type GetCoercionType(Type valuePropType, Type keyPropTypeExpr) {
	        Type coercionType = null;
	        Type keyPropType = Boxing.GetBoxedType(keyPropTypeExpr);
	        if (valuePropType != keyPropType) {
	            coercionType = TypeHelper.GetCompareToCoercionType(valuePropType, keyPropType);
	        }
	        return coercionType;
	    }

	    public static CoercionDesc GetCoercionTypesHash(EventType viewableEventType, string[] indexProps, IList<SubordPropHashKeyForge> hashKeys) {
	        if (indexProps.Length == 0 && hashKeys.Count == 0) {
	            return new CoercionDesc(false, NULL_ARRAY);
	        }
	        if (indexProps.Length != hashKeys.Count) {
	            throw new IllegalStateException("Mismatch in the number of key and index properties");
	        }

	        Type[] coercionTypes = new Type[indexProps.Length];
	        bool mustCoerce = false;
	        for (int i = 0; i < hashKeys.Count; i++) {
	            Type keyPropType = Boxing.GetBoxedType(hashKeys.Get(i).HashKey.KeyExpr.Forge.EvaluationType);
	            Type indexedPropType = Boxing.GetBoxedType(viewableEventType.GetPropertyType(indexProps[i]));
	            Type coercionType = indexedPropType;
	            if (keyPropType != indexedPropType) {
	                coercionType = TypeHelper.GetCompareToCoercionType(keyPropType, indexedPropType);
	                mustCoerce = true;
	            }
	            coercionTypes[i] = coercionType;
	        }
	        return new CoercionDesc(mustCoerce, coercionTypes);
	    }

	    public static Type GetCoercionTypeRangeIn(Type valuePropType, ExprNode exprStart, ExprNode exprEnd) {
	        Type coercionType = null;
	        Type startPropType = Boxing.GetBoxedType(exprStart.Forge.EvaluationType);
	        Type endPropType = Boxing.GetBoxedType(exprEnd.Forge.EvaluationType);

	        if (valuePropType != startPropType) {
	            coercionType = TypeHelper.GetCompareToCoercionType(valuePropType, startPropType);
	        }
	        if (valuePropType != endPropType) {
	            coercionType = TypeHelper.GetCompareToCoercionType(coercionType, endPropType);
	        }
	        return coercionType;
	    }
	}
} // end of namespace