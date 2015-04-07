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
using com.espertech.esper.compat;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.util;

namespace com.espertech.esper.view.window
{
    /// <summary>
    /// Factory for <seealso cref="FirstLengthWindowView"/>.
    /// </summary>
    public class FirstLengthWindowViewFactory : AsymetricDataWindowViewFactory
    {
        /// <summary>Count of length first window. </summary>
        protected int Size;
    
        private EventType _eventType;
    
        public void SetViewParameters(ViewFactoryContext viewFactoryContext, IList<ExprNode> expressionParameters)
        {
            IList<Object> viewParameters = ViewFactorySupport.ValidateAndEvaluate(ViewName, viewFactoryContext.StatementContext, expressionParameters);
            if (viewParameters.Count != 1)
            {
                throw new ViewParameterException(ViewParamMessage);
            }
    
            Object parameter = viewParameters[0];
            if (!(parameter.IsNumber()))
            {
                throw new ViewParameterException(ViewParamMessage);
            }
            var numParam = parameter;
            if ( (numParam.IsFloatingPointNumber()) ||
                 (numParam.IsLongNumber()))
            {
                throw new ViewParameterException(ViewParamMessage);
            }

            Size = numParam.AsInt();
            if (Size <= 0)
            {
                throw new ViewParameterException(ViewName + " view requires a positive number");
            }
        }
    
        public void Attach(EventType parentEventType, StatementContext statementContext, ViewFactory optionalParentFactory, IList<ViewFactory> parentViewFactories)
        {
            _eventType = parentEventType;
        }
    
        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            return new FirstLengthWindowView(agentInstanceViewFactoryContext, this, Size);
        }

        public EventType EventType
        {
            get { return _eventType; }
        }

        public bool CanReuse(View view)
        {
            if (!(view is FirstLengthWindowView))
            {
                return false;
            }
    
            var myView = (FirstLengthWindowView) view;
            if (myView.Size != Size)
            {
                return false;
            }
    
            return myView.IsEmpty();
        }

        public string ViewName
        {
            get { return "First-Length"; }
        }

        private string ViewParamMessage
        {
            get { return ViewName + " view requires an integer-type size parameter"; }
        }
    }
}
