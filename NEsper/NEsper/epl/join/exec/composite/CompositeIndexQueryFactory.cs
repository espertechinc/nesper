///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.lookup;

namespace com.espertech.esper.epl.join.exec.composite
{
    public class CompositeIndexQueryFactory
    {
        public static CompositeIndexQuery MakeSubordinate(
            bool isNWOnTrigger,
            int numOuterStreams,
            ICollection<SubordPropHashKey> keyExpr,
            Type[] coercionKeyTypes,
            ICollection<SubordPropRangeKey> rangeProps,
            Type[] rangeCoercionTypes,
            IList<String> expressionTexts)
        {
            // construct chain
            IList<CompositeIndexQuery> queries = new List<CompositeIndexQuery>();
            if (keyExpr.Count > 0) {
                IList<QueryGraphValueEntryHashKeyed> hashKeys = new List<QueryGraphValueEntryHashKeyed>();
                foreach (SubordPropHashKey keyExp in keyExpr) {
                    expressionTexts.Add(ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(keyExp.HashKey.KeyExpr));
                    hashKeys.Add(keyExp.HashKey);
                }
                queries.Add(new CompositeIndexQueryKeyed(isNWOnTrigger, -1, numOuterStreams, hashKeys, coercionKeyTypes));
            }
            int count = 0;
            foreach (SubordPropRangeKey rangeProp in rangeProps) {
                Type coercionType = rangeCoercionTypes == null ? null : rangeCoercionTypes[count];
                queries.Add(new CompositeIndexQueryRange(isNWOnTrigger, -1, numOuterStreams, rangeProp, coercionType, expressionTexts));
                count++;
            }
    
            // Hook up as chain for remove
            CompositeIndexQuery last = null;
            foreach (CompositeIndexQuery action in queries) {
                if (last != null)
                {
                    last.SetNext(action);
                }
                last = action;
            }
            return queries[0];
        }

        public static CompositeIndexQuery MakeJoinSingleLookupStream(
            bool isNWOnTrigger,
            int lookupStream,
            IList<QueryGraphValueEntryHashKeyed> hashKeys,
            IList<Type> keyCoercionTypes,
            IList<QueryGraphValueEntryRange> rangeProps,
            IList<Type> rangeCoercionTypes)
        {
            // construct chain
            IList<CompositeIndexQuery> queries = new List<CompositeIndexQuery>();
            if (hashKeys.Count > 0)
            {
                queries.Add(new CompositeIndexQueryKeyed(false, lookupStream, -1, hashKeys, keyCoercionTypes));
            }
            int count = 0;
            foreach (QueryGraphValueEntryRange rangeProp in rangeProps)
            {
                Type coercionType = rangeCoercionTypes == null ? null : rangeCoercionTypes[count];
                SubordPropRangeKey rkey = new SubordPropRangeKey(rangeProp, coercionType);
                queries.Add(
                    new CompositeIndexQueryRange(isNWOnTrigger, lookupStream, -1, rkey, coercionType, new List<String>()));
                count++;
            }

            // Hook up as chain for remove
            CompositeIndexQuery last = null;
            foreach (CompositeIndexQuery action in queries)
            {
                if (last != null)
                {
                    last.SetNext(action);
                }
                last = action;
            }
            return queries[0];
        }
    }
}
