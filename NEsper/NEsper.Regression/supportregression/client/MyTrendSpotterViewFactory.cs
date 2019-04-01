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
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.view;

namespace com.espertech.esper.supportregression.client
{
    public class MyTrendSpotterViewFactory : ViewFactorySupport
    {
        private IList<ExprNode> _viewParameters;
    
        private ExprNode _expression;
        private EventType _eventType;
    
        public override void SetViewParameters(ViewFactoryContext viewFactoryContext, IList<ExprNode> viewParameters)
        {
            this._viewParameters = viewParameters;
        }

        public override void Attach(EventType parentEventType, StatementContext statementContext, ViewFactory optionalParentFactory, IList<ViewFactory> parentViewFactories)
        {
            ExprNode[] validated = Validate("Trend spotter view", parentEventType, statementContext, _viewParameters, false);

            const string message = "Trend spotter view accepts a single integer or double value";
            if (validated.Length != 1)
            {
                throw new ViewParameterException(message);
            }
            var resultType = validated[0].ExprEvaluator.ReturnType;
            if ((resultType != typeof(int?)) && 
                (resultType != typeof(int)) &&
                (resultType != typeof(double?)) && 
                (resultType != typeof(double)))
            {
                throw new ViewParameterException(message);
            }
            _expression = validated[0];
            _eventType = MyTrendSpotterView.CreateEventType(statementContext);
        }
    
        public override View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            return new MyTrendSpotterView(agentInstanceViewFactoryContext, _expression);
        }

        public override EventType EventType
        {
            get { return _eventType; }
        }

        public override String ViewName
        {
            get { return "Trend-spotter"; }
        }
    }
}
