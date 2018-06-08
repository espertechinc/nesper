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

namespace com.espertech.esper.view.std
{
    /// <summary>
    /// Factory for <seealso cref="UniqueByPropertyView"/> instances.
    /// </summary>
    public class UniqueByPropertyViewFactory : DataWindowViewFactoryUniqueCandidate, DataWindowViewFactory
    {
        public readonly static String NAME = "Unique-By";

        /// <summary>View parameters. </summary>
        private IList<ExprNode> _viewParameters;

        /// <summary>Property name to evaluate unique values. </summary>
        private ExprNode[] _criteriaExpressions;
        private EventType _eventType;

        public void SetViewParameters(ViewFactoryContext viewFactoryContext, IList<ExprNode> expressionParameters)
        {
            _viewParameters = expressionParameters;
        }

        public void Attach(EventType parentEventType, StatementContext statementContext, ViewFactory optionalParentFactory, IList<ViewFactory> parentViewFactories)
        {
            _criteriaExpressions = ViewFactorySupport.Validate(ViewName, parentEventType, statementContext, _viewParameters, false);

            if (_criteriaExpressions.Length == 0)
            {
                String errorMessage = ViewName + " view requires a one or more expressions providing unique values as parameters";
                throw new ViewParameterException(errorMessage);
            }

            _eventType = parentEventType;
        }

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            return new UniqueByPropertyView(this, agentInstanceViewFactoryContext);
        }

        public EventType EventType
        {
            get { return _eventType; }
        }

        public bool CanReuse(View view, AgentInstanceContext agentInstanceContext)
        {
            if (!(view is UniqueByPropertyView))
            {
                return false;
            }

            UniqueByPropertyView myView = (UniqueByPropertyView)view;
            if (!ExprNodeUtility.DeepEquals(_criteriaExpressions, myView.CriteriaExpressions, false))
            {
                return false;
            }

            return myView.IsEmpty();
        }

        public ICollection<string> UniquenessCandidatePropertyNames
        {
            get { return ExprNodeUtility.GetPropertyNamesIfAllProps(_criteriaExpressions); }
        }

        public IList<ExprNode> ViewParameters
        {
            get { return _viewParameters; }
        }

        public ExprNode[] CriteriaExpressions
        {
            get { return _criteriaExpressions; }
            set { _criteriaExpressions = value; }
        }

        public string ViewName
        {
            get { return NAME; }
        }
    }
}
