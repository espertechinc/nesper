///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.epl.join.plan
{
    public class QueryGraphValueEntryCustom : QueryGraphValueEntry
    {
        private readonly IDictionary<QueryGraphValueEntryCustomKey, QueryGraphValueEntryCustomOperation> operations = 
            new Dictionary<QueryGraphValueEntryCustomKey, QueryGraphValueEntryCustomOperation>();
    
        public IDictionary<QueryGraphValueEntryCustomKey, QueryGraphValueEntryCustomOperation> Operations
        {
            get => operations;
        }
    
        public void MergeInto(IDictionary<QueryGraphValueEntryCustomKey, QueryGraphValueEntryCustomOperation> customIndexOps)
        {
            foreach (var operation in operations)
            {
                var existing = customIndexOps.Get(operation.Key);
                if (existing == null)
                {
                    customIndexOps.Put(operation.Key, operation.Value);
                    continue;
                }
                existing.PositionalExpressions.PutAll(operation.Value.PositionalExpressions);
            }
        }
    }
    
} // end of namespace
