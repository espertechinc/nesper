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
using com.espertech.esper.util;

namespace com.espertech.esper.view.stat
{
    /// <summary>
    /// Factory for <seealso cref="CorrelationView"/> instances.
    /// </summary>
    public class CorrelationViewFactory : ViewFactory
    {
        private IList<ExprNode> _viewParameters;
        private int _streamNumber;

        /// <summary>Property name of X field. </summary>
        private ExprNode _expressionX;

        /// <summary>Property name of Y field. </summary>
        private ExprNode _expressionY;

        /// <summary>Additional properties. </summary>
        private StatViewAdditionalProps _additionalProps;

        /// <summary>Event type. </summary>
        private EventType _eventType;

        public void SetViewParameters(ViewFactoryContext viewFactoryContext, IList<ExprNode> expressionParameters)
        {
            _viewParameters = expressionParameters;
            _streamNumber = viewFactoryContext.StreamNum;
        }

        public void Attach(EventType parentEventType, StatementContext statementContext, ViewFactory optionalParentFactory, IList<ViewFactory> parentViewFactories)
        {
            ExprNode[] validated = ViewFactorySupport.Validate(ViewName, parentEventType, statementContext, _viewParameters, true);
            if (validated.Length < 2)
            {
                throw new ViewParameterException(ViewParamMessage);
            }
            if ((!validated[0].ExprEvaluator.ReturnType.IsNumeric()) || (!validated[1].ExprEvaluator.ReturnType.IsNumeric()))
            {
                throw new ViewParameterException(ViewParamMessage);
            }

            _expressionX = validated[0];
            _expressionY = validated[1];

            _additionalProps = StatViewAdditionalProps.Make(validated, 2, parentEventType);
            _eventType = CorrelationView.CreateEventType(statementContext, _additionalProps, _streamNumber);
        }

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            return new CorrelationView(this, agentInstanceViewFactoryContext.AgentInstanceContext, ExpressionX, ExpressionY, EventType, AdditionalProps);
        }

        public EventType EventType
        {
            get { return _eventType; }
            set { _eventType = value; }
        }

        public bool CanReuse(View view, AgentInstanceContext agentInstanceContext)
        {
            if (!(view is CorrelationView))
            {
                return false;
            }

            if (_additionalProps != null)
            {
                return false;
            }

            CorrelationView other = (CorrelationView)view;
            if ((!ExprNodeUtility.DeepEquals(other.ExpressionX, _expressionX, false) ||
                (!ExprNodeUtility.DeepEquals(other.ExpressionY, _expressionY, false))))
            {
                return false;
            }

            return true;
        }

        public string ViewName
        {
            get { return "CorrelationStatistics"; }
        }

        private string ViewParamMessage
        {
            get { return ViewName + " view requires two expressions providing x and y values as properties"; }
        }

        public ExprNode ExpressionX
        {
            get { return _expressionX; }
            set { _expressionX = value; }
        }

        public ExprNode ExpressionY
        {
            get { return _expressionY; }
            set { _expressionY = value; }
        }

        public StatViewAdditionalProps AdditionalProps
        {
            get { return _additionalProps; }
            set { _additionalProps = value; }
        }
    }
}
