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
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.db
{
    public class SupportPollingStrategy : PollExecStrategy
    {
        private IDictionary<MultiKey<Object>, IList<EventBean>> results;
    
        public SupportPollingStrategy(IDictionary<MultiKey<Object>, IList<EventBean>> results)
        {
            this.results = results;
        }
    
        public void Start()
        {
    
        }

        public IList<EventBean> Poll(Object[] lookupValues, ExprEvaluatorContext exprEvaluatorContext)
        {
            return results.Get(new MultiKey<Object>(lookupValues));
        }
    
        public void Done()
        {
    
        }
    
        public void Dispose()
        {
    
        }
    }
}
