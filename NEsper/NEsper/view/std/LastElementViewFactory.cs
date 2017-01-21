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
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.view.std
{
    /// <summary>
    /// Factory for <seealso cref="LastElementView"/> instances.
    /// </summary>
    public class LastElementViewFactory : DataWindowViewFactory
    {
        public readonly static String NAME = "Last-Event";
    
        private EventType _eventType;
    
        public void SetViewParameters(ViewFactoryContext viewFactoryContext, IList<ExprNode> expressionParameters)
        {
            IList<Object> viewParameters = ViewFactorySupport.ValidateAndEvaluate(ViewName, viewFactoryContext.StatementContext, expressionParameters);
            if (viewParameters.IsNotEmpty())
            {
                String errorMessage = ViewName + " view does not take any parameters";
                throw new ViewParameterException(errorMessage);
            }
        }
    
        public void Attach(EventType parentEventType, StatementContext statementContext, ViewFactory optionalParentFactory, IList<ViewFactory> parentViewFactories)
        {
            this._eventType = parentEventType;
        }
    
        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            return new LastElementView(this);
        }

        public EventType EventType
        {
            get { return _eventType; }
        }

        public bool CanReuse(View view)
        {
            if (!(view is LastElementView))
            {
                return false;
            }
            return true;
        }

        public string ViewName
        {
            get { return NAME; }
        }
    }
}
