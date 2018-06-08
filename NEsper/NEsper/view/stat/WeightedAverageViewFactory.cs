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
    /// Factory for <seealso cref="WeightedAverageView"/> instances.
    /// </summary>
    public class WeightedAverageViewFactory : ViewFactory
    {
        public readonly static String NAME = "Weighted-average";

        private IList<ExprNode> _viewParameters;
        private int _streamNumber;

        /// <summary>Expression of X field. </summary>
        private ExprNode _fieldNameX;
        /// <summary>Expression of weight field. </summary>
        private ExprNode _fieldNameWeight;

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

            _fieldNameX = validated[0];
            _fieldNameWeight = validated[1];
            _additionalProps = StatViewAdditionalProps.Make(validated, 2, parentEventType);
            _eventType = WeightedAverageView.CreateEventType(statementContext, _additionalProps, _streamNumber);
        }

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            return new WeightedAverageView(this, agentInstanceViewFactoryContext);
        }

        public EventType EventType
        {
            get { return _eventType; }
            set { _eventType = value; }
        }

        public bool CanReuse(View view, AgentInstanceContext agentInstanceContext)
        {
            if (!(view is WeightedAverageView))
            {
                return false;
            }
            if (_additionalProps != null)
            {
                return false;
            }

            var myView = (WeightedAverageView)view;
            if ((!ExprNodeUtility.DeepEquals(_fieldNameWeight, myView.FieldNameWeight, false)) ||
                (!ExprNodeUtility.DeepEquals(_fieldNameX, myView.FieldNameX, false)))
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
            get { return ViewName + " view requires two expressions returning numeric values as parameters"; }
        }

        public ExprNode FieldNameX
        {
            get { return _fieldNameX; }
            set { _fieldNameX = value; }
        }

        public ExprNode FieldNameWeight
        {
            get { return _fieldNameWeight; }
            set { _fieldNameWeight = value; }
        }

        public StatViewAdditionalProps AdditionalProps
        {
            get { return _additionalProps; }
            set { _additionalProps = value; }
        }
    }
}
