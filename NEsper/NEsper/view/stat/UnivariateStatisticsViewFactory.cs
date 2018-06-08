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
using com.espertech.esper.epl.expression;
using com.espertech.esper.util;

namespace com.espertech.esper.view.stat
{
    /// <summary>
    /// Factory for <seealso cref="UnivariateStatisticsView"/> instances.
    /// </summary>
    public class UnivariateStatisticsViewFactory : ViewFactory
    {
        internal readonly static String NAME = "Univariate statistics";

        private IList<ExprNode> _viewParameters;
        private int _streamNumber;

        /// <summary>Property name of data field. </summary>
        private ExprNode _fieldExpression;
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
            if (validated.Length < 1)
            {
                throw new ViewParameterException(ViewParamMessage);
            }
            if (!validated[0].ExprEvaluator.ReturnType.IsNumeric())
            {
                throw new ViewParameterException(ViewParamMessage);
            }
            _fieldExpression = validated[0];

            _additionalProps = StatViewAdditionalProps.Make(validated, 1, parentEventType);
            _eventType = UnivariateStatisticsView.CreateEventType(statementContext, _additionalProps, _streamNumber);
        }

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            return new UnivariateStatisticsView(this, agentInstanceViewFactoryContext);
        }

        public EventType EventType
        {
            get { return _eventType; }
            set { _eventType = value; }
        }

        public bool CanReuse(View view, AgentInstanceContext agentInstanceContext)
        {
            if (!(view is UnivariateStatisticsView))
            {
                return false;
            }
            if (_additionalProps != null)
            {
                return false;
            }

            var other = (UnivariateStatisticsView)view;
            if (!ExprNodeUtility.DeepEquals(other.FieldExpression, _fieldExpression, false))
            {
                return false;
            }

            return true;
        }

        public string ViewName
        {
            get { return NAME; }
        }

        private string ViewParamMessage
        {
            get { return ViewName + " view require a single expression returning a numeric value as a parameter"; }
        }

        public StatViewAdditionalProps AdditionalProps
        {
            get { return _additionalProps; }
            set { _additionalProps = value; }
        }

        public ExprNode FieldExpression
        {
            get { return _fieldExpression; }
            set { _fieldExpression = value; }
        }
    }
}
