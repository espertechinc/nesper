///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.view.std
{
    /// <summary>
    /// Factory for <seealso cref="GroupByView"/> instances.
    /// </summary>
    public class GroupByViewFactory
        : ViewFactory
        , GroupByViewFactoryMarker
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>View parameters. </summary>
        protected IList<ExprNode> ViewParameters;

        /// <summary>List of criteria expressions. </summary>
        private ExprNode[] _criteriaExpressions;

        private EventType _eventType;

        private bool _isReclaimAged;

        private double _reclaimMaxAge;
        private double _reclaimFrequency;

        public void SetViewParameters(ViewFactoryContext viewFactoryContext, IList<ExprNode> expressionParameters)
        {
            ViewParameters = expressionParameters;

            var reclaimGroupAged = HintEnum.RECLAIM_GROUP_AGED.GetHint(viewFactoryContext.StatementContext.Annotations);

            if (reclaimGroupAged != null)
            {
                _isReclaimAged = true;
                String hintValueMaxAge = HintEnum.RECLAIM_GROUP_AGED.GetHintAssignedValue(reclaimGroupAged);
                if (hintValueMaxAge == null)
                {
                    throw new ViewParameterException("Required hint value for hint '" + HintEnum.RECLAIM_GROUP_AGED + "' has not been provided");
                }
                try
                {
                    _reclaimMaxAge = Double.Parse(hintValueMaxAge);
                }
                catch (Exception)
                {
                    throw new ViewParameterException("Required hint value for hint '" + HintEnum.RECLAIM_GROUP_AGED + "' value '" + hintValueMaxAge + "' could not be parsed as a double value");
                }

                String hintValueFrequency = HintEnum.RECLAIM_GROUP_FREQ.GetHintAssignedValue(reclaimGroupAged);
                if (hintValueFrequency == null)
                {
                    _reclaimFrequency = _reclaimMaxAge;
                }
                else
                {
                    try
                    {
                        _reclaimFrequency = Double.Parse(hintValueFrequency);
                    }
                    catch (Exception)
                    {
                        throw new ViewParameterException("Required hint value for hint '" + HintEnum.RECLAIM_GROUP_FREQ + "' value '" + hintValueFrequency + "' could not be parsed as a double value");
                    }
                }
                if (_reclaimMaxAge < 0.100)
                {
                    Log.Warn("Reclaim max age parameter is less then 100 milliseconds, are your sure?");
                }

                if (Log.IsDebugEnabled)
                {
                    Log.Debug("Using reclaim-aged strategy for group-window age " + _reclaimMaxAge + " frequency " + _reclaimFrequency);
                }
            }
        }

        public void Attach(EventType parentEventType, StatementContext statementContext, ViewFactory optionalParentFactory, IList<ViewFactory> parentViewFactories)
        {
            _criteriaExpressions = ViewFactorySupport.Validate(ViewName, parentEventType, statementContext, ViewParameters, false);

            if (_criteriaExpressions.Length == 0)
            {
                String errorMessage = ViewName + " view requires a one or more expressions provinding unique values as parameters";
                throw new ViewParameterException(errorMessage);
            }

            _eventType = parentEventType;
        }

        /// <summary>Returns the names of fields to group by </summary>
        /// <value>field names</value>
        public ExprNode[] CriteriaExpressions
        {
            get { return _criteriaExpressions; }
        }

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            if (IsReclaimAged)
            {
                return new GroupByViewReclaimAged(agentInstanceViewFactoryContext, _criteriaExpressions, ExprNodeUtility.GetEvaluators(_criteriaExpressions), _reclaimMaxAge, _reclaimFrequency);
            }
            return new GroupByViewImpl(agentInstanceViewFactoryContext, _criteriaExpressions, ExprNodeUtility.GetEvaluators(_criteriaExpressions));
        }

        public EventType EventType
        {
            get { return _eventType; }
        }

        public bool CanReuse(View view, AgentInstanceContext agentInstanceContext)
        {
            if (!(view is GroupByView))
            {
                return false;
            }

            if (IsReclaimAged)
            {
                return false;
            }

            GroupByView myView = (GroupByView)view;
            if (!ExprNodeUtility.DeepEquals(myView.CriteriaExpressions, _criteriaExpressions, false))
            {
                return false;
            }

            return true;
        }

        public bool IsReclaimAged
        {
            get { return _isReclaimAged; }
        }

        public double ReclaimMaxAge
        {
            get { return _reclaimMaxAge; }
        }

        public double ReclaimFrequency
        {
            get { return _reclaimFrequency; }
        }

        public string ViewName
        {
            get { return "Group-By"; }
        }
    }
}
