///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.view.window
{
    /// <summary>
    /// Factory for <seealso cref="com.espertech.esper.view.window.KeepAllView"/>.
    /// </summary>
    public class KeepAllViewFactory : DataWindowViewFactory, DataWindowViewWithPrevious
    {
        private EventType _eventType;
    
        public void SetViewParameters(ViewFactoryContext viewFactoryContext, IList<ExprNode> expressionParameters)
        {
            var viewParameters = ViewFactorySupport.ValidateAndEvaluate(ViewName, viewFactoryContext.StatementContext, expressionParameters);
            if (viewParameters.Count != 0)
            {
                String errorMessage = ViewName + " view requires an empty parameter list";
                throw new ViewParameterException(errorMessage);
            }
        }
    
        public void Attach(EventType parentEventType, StatementContext statementContext, ViewFactory optionalParentFactory, IList<ViewFactory> parentViewFactories)
        {
            _eventType = parentEventType;
        }
    
        public Object MakePreviousGetter()
        {
            return new RandomAccessByIndexGetter();
        }
    
        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            var randomAccess = agentInstanceViewFactoryContext.StatementContext.ViewServicePreviousFactory.GetOptPreviousExprRandomAccess(agentInstanceViewFactoryContext); 
            return new KeepAllView(agentInstanceViewFactoryContext, this, randomAccess);
        }

        public EventType EventType
        {
            get { return _eventType; }
        }

        public bool CanReuse(View view)
        {
            if (!(view is KeepAllView))
            {
                return false;
            }

            return ((KeepAllView) view).IsEmpty();
        }

        public string ViewName
        {
            get { return "Keep-All"; }
        }
    }
}
