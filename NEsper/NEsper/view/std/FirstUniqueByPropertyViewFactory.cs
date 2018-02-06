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
    /// Factory for <seealso cref="FirstUniqueByPropertyView"/> instances.
    /// </summary>
    public class FirstUniqueByPropertyViewFactory : AsymetricDataWindowViewFactory, DataWindowViewFactoryUniqueCandidate
    {
        public static readonly String NAME = "First-Unique-By";

        /// <summary>View parameters. </summary>
        internal IList<ExprNode> ViewParameters;

        /// <summary>Property name to evaluate unique values. </summary>
        internal ExprNode[] CriteriaExpressions;

        private EventType _eventType;

        public void SetViewParameters(ViewFactoryContext viewFactoryContext, IList<ExprNode> expressionParameters)
        {
            ViewParameters = expressionParameters;
        }

        public void Attach(EventType parentEventType, StatementContext statementContext, ViewFactory optionalParentFactory, IList<ViewFactory> parentViewFactories)
        {
            CriteriaExpressions = ViewFactorySupport.Validate(ViewName, parentEventType, statementContext, ViewParameters, false);

            if (CriteriaExpressions.Length == 0)
            {
                String errorMessage = ViewName + " view requires a one or more expressions provinding unique values as parameters";
                throw new ViewParameterException(errorMessage);
            }

            _eventType = parentEventType;
        }

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            return new FirstUniqueByPropertyView(this, agentInstanceViewFactoryContext);
        }

        public EventType EventType
        {
            get { return _eventType; }
        }

        public bool CanReuse(View view, AgentInstanceContext agentInstanceContext)
        {
            if (!(view is FirstUniqueByPropertyView))
            {
                return false;
            }

            FirstUniqueByPropertyView myView = (FirstUniqueByPropertyView)view;
            if (!ExprNodeUtility.DeepEquals(CriteriaExpressions, myView.UniqueCriteria, false))
            {
                return false;
            }

            return myView.IsEmpty();
        }

        public ICollection<string> UniquenessCandidatePropertyNames
        {
            get { return ExprNodeUtility.GetPropertyNamesIfAllProps(CriteriaExpressions); }
        }

        public string ViewName
        {
            get { return NAME; }
        }
    }
}
