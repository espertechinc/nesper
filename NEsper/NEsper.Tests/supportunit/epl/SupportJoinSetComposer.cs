///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.core.context.factory;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.join.@base;

namespace com.espertech.esper.supportunit.epl
{
    public class SupportJoinSetComposer : JoinSetComposer
    {
        private readonly UniformPair<ISet<MultiKey<EventBean>>> _result;

        public SupportJoinSetComposer(UniformPair<ISet<MultiKey<EventBean>>> result)
        {
            _result = result;
        }
    
        public void Init(EventBean[][] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {        
        }

        public UniformPair<ISet<MultiKey<EventBean>>> Join(EventBean[][] newDataPerStream, EventBean[][] oldDataPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            return _result;
        }
    
        public ISet<MultiKey<EventBean>> StaticJoin()
        {
            return null;
        }
    
        public void Destroy()
        {        
        }

        public void VisitIndexes(StatementAgentInstancePostLoadIndexVisitor visitor)
        {
            throw new System.NotImplementedException();
        }

        public bool AllowsInit
        {
            get { return true; }
        }
    }
}
