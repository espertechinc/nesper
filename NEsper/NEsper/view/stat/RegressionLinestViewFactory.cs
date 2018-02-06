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
    /// Factory for <seealso cref="RegressionLinestView"/> instances.
    /// </summary>
    public class RegressionLinestViewFactory : ViewFactory
    {
        private IList<ExprNode> _viewParameters;
        private int _streamNumber;

        /// <summary>Expression X field. </summary>
        private ExprNode _expressionX;

        /// <summary>Expression Y field. </summary>
        private ExprNode _expressionY;

        private StatViewAdditionalProps _additionalProps;

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
            _eventType = RegressionLinestView.CreateEventType(statementContext, _additionalProps, _streamNumber);
        }

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            return new RegressionLinestView(
                this, agentInstanceViewFactoryContext.AgentInstanceContext, ExpressionX, ExpressionY, EventType, AdditionalProps);
        }


        public bool CanReuse(View view, AgentInstanceContext agentInstanceContext)
        {
            if (!(view is RegressionLinestView))
            {
                return false;
            }

            if (_additionalProps != null)
            {
                return false;
            }

            RegressionLinestView myView = (RegressionLinestView)view;
            if ((!ExprNodeUtility.DeepEquals(myView.ExpressionX, _expressionX, false)) ||
                (!ExprNodeUtility.DeepEquals(myView.ExpressionY, _expressionY, false)))
            {
                return false;
            }
            return true;
        }

        public EventType EventType
        {
            get { return _eventType; }
            set { _eventType = value; }
        }

        public string ViewName
        {
            get { return "Regression"; }
        }

        private string ViewParamMessage
        {
            get { return ViewName + " view requires two expressions providing x and y values as properties"; }
        }

        public StatViewAdditionalProps AdditionalProps
        {
            get { return _additionalProps; }
            set { _additionalProps = value; }
        }

        public ExprNode ExpressionY
        {
            get { return _expressionY; }
            set { _expressionY = value; }
        }

        public ExprNode ExpressionX
        {
            get { return _expressionX; }
            set { _expressionX = value; }
        }
    }
}
