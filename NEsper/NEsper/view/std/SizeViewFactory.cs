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
using com.espertech.esper.epl.expression;
using com.espertech.esper.view.stat;

namespace com.espertech.esper.view.std
{
    /// <summary>Factory for <seealso cref="SizeView" /> instances. </summary>
    public class SizeViewFactory : ViewFactory
    {
        public readonly static String NAME = "Count";
    
        private IList<ExprNode> _viewParameters;
        private int _streamNumber;
    
        protected StatViewAdditionalProps AdditionalProps;
    
        private EventType _eventType;
    
        public void SetViewParameters(ViewFactoryContext viewFactoryContext, IList<ExprNode> expressionParameters)
        {
            _viewParameters = expressionParameters;
            _streamNumber = viewFactoryContext.StreamNum;
        }
    
        public void Attach(EventType parentEventType, StatementContext statementContext, ViewFactory optionalParentFactory, IList<ViewFactory> parentViewFactories)
        {
            ExprNode[] validated = ViewFactorySupport.Validate(ViewName, parentEventType, statementContext, _viewParameters, true);
            AdditionalProps = StatViewAdditionalProps.Make(validated, 0, parentEventType);
            _eventType = SizeView.CreateEventType(statementContext, AdditionalProps, _streamNumber);
        }
    
        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            return new SizeView(agentInstanceViewFactoryContext.AgentInstanceContext, _eventType, AdditionalProps);
        }

        public EventType EventType
        {
            get { return _eventType; }
        }

        public bool CanReuse(View view)
        {
            if (!(view is SizeView))
            {
                return false;
            }
            if (AdditionalProps != null) {
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
