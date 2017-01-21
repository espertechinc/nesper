///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.epl.join.plan
{
    public class QueryGraphTestUtil {
    
        public static IList<string> GetStrictKeyProperties(QueryGraph graph, int lookup, int indexed) {
            QueryGraphValue val = graph.GetGraphValue(lookup, indexed);
            QueryGraphValuePairHashKeyIndex pair = val.HashKeyProps;
            return pair.StrictKeys;
        }
    
        public static IList<string> GetIndexProperties(QueryGraph graph, int lookup, int indexed) {
            QueryGraphValue val = graph.GetGraphValue(lookup, indexed);
            QueryGraphValuePairHashKeyIndex pair = val.HashKeyProps;
            return pair.Indexed;
        }
    }
}
