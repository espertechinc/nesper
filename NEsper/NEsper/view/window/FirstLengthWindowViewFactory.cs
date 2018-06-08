///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.view.window
{
    /// <summary>Factory for <seealso cref="FirstLengthWindowView" />.</summary>
    public class FirstLengthWindowViewFactory : AsymetricDataWindowViewFactory
    {
        private EventType _eventType;

        /// <summary>Size of length first window.</summary>
        private ExprEvaluator _sizeEvaluator;

        public void SetViewParameters(ViewFactoryContext viewFactoryContext, IList<ExprNode> expressionParameters)
        {
            _sizeEvaluator = ViewFactorySupport.ValidateSizeSingleParam(
                ViewName, viewFactoryContext, expressionParameters);
        }

        public void Attach(
            EventType parentEventType,
            StatementContext statementContext,
            ViewFactory optionalParentFactory,
            IList<ViewFactory> parentViewFactories)
        {
            _eventType = parentEventType;
        }

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            int size = ViewFactorySupport.EvaluateSizeParam(
                ViewName, _sizeEvaluator, agentInstanceViewFactoryContext.AgentInstanceContext);
            return new FirstLengthWindowView(agentInstanceViewFactoryContext, this, size);
        }

        public EventType EventType => _eventType;

        public bool CanReuse(View view, AgentInstanceContext agentInstanceContext)
        {
            if (!(view is FirstLengthWindowView))
            {
                return false;
            }

            var myView = (FirstLengthWindowView) view;
            int size = ViewFactorySupport.EvaluateSizeParam(ViewName, _sizeEvaluator, agentInstanceContext);
            return myView.Size == size && myView.IsEmpty();
        }

        public string ViewName => "First-Length";
    }
} // end of namespace