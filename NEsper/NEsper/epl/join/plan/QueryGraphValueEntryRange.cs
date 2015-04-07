///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.join.plan
{
    [Serializable]
    public abstract class QueryGraphValueEntryRange : QueryGraphValueEntry
    {
        private readonly QueryGraphRangeEnum _type;
    
        protected QueryGraphValueEntryRange(QueryGraphRangeEnum type)
        {
            _type = type;
        }

        public QueryGraphRangeEnum RangeType
        {
            get { return _type; }
        }

        public abstract String ToQueryPlan();

        public abstract ExprNode[] Expressions { get; }

        public static String ToQueryPlan(IEnumerable<QueryGraphValueEntryRange> rangeKeyPairs)
        {
            var writer = new StringWriter();
            var delimiter = "";
            foreach (QueryGraphValueEntryRange item in rangeKeyPairs) {
                writer.Write(delimiter);
                writer.Write(item.ToQueryPlan());
                delimiter = ", ";
            }
            return writer.ToString();
        }
    }
}
