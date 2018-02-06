///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.join.exec.@base;
using com.espertech.esper.epl.join.rep;
using com.espertech.esper.epl.lookup;

namespace com.espertech.esper.supportunit.epl.join
{
    public class SupportTableLookupStrategy : JoinExecTableLookupStrategy
    {
        private readonly int _numResults;
    
        public SupportTableLookupStrategy(int numResults)
        {
            _numResults = numResults;
        }
    
        public ICollection<EventBean> Lookup(EventBean theEvent, Cursor cursor, ExprEvaluatorContext exprEvaluatorContext)
        {
            return SupportJoinResultNodeFactory.MakeEventSet(_numResults);
        }

        public LookupStrategyDesc StrategyDesc { get; private set; }
    }
}
