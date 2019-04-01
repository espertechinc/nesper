///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.join.querygraph;

namespace com.espertech.esper.common.@internal.epl.join.exec.composite
{
    public class CompositeIndexQueryFactory
    {
        public static CompositeIndexQuery MakeSubordinate(
            bool isNWOnTrigger,
            int numOuterStreams,
            ExprEvaluator hashEval,
            QueryGraphValueEntryRange[] rangeEvals)
        {
            // construct chain
            IList<CompositeIndexQuery> queries = new List<CompositeIndexQuery>();
            if (hashEval != null)
            {
                queries.Add(new CompositeIndexQueryKeyed(isNWOnTrigger, -1, numOuterStreams, hashEval));
            }

            foreach (QueryGraphValueEntryRange rangeProp in rangeEvals)
            {
                queries.Add(new CompositeIndexQueryRange(isNWOnTrigger, -1, numOuterStreams, rangeProp));
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

        public static CompositeIndexQuery MakeJoinSingleLookupStream(
            bool isNWOnTrigger,
            int lookupStream,
            ExprEvaluator hashGetter,
            QueryGraphValueEntryRange[] rangeProps)
        {
            // construct chain
            IList<CompositeIndexQuery> queries = new List<CompositeIndexQuery>();
            if (hashGetter != null)
            {
                queries.Add(new CompositeIndexQueryKeyed(false, lookupStream, -1, hashGetter));
            }

            foreach (QueryGraphValueEntryRange rangeProp in rangeProps)
            {
                queries.Add(new CompositeIndexQueryRange(isNWOnTrigger, lookupStream, -1, rangeProp));
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
} // end of namespace