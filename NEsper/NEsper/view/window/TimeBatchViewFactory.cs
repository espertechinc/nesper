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
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.util;

namespace com.espertech.esper.view.window
{
    /// <summary>
    /// Factory for <seealso cref="TimeBatchView" />.
    /// </summary>
    public class TimeBatchViewFactory
        : TimeBatchViewFactoryParams
        , DataWindowViewFactory
        , DataWindowViewWithPrevious
        , DataWindowBatchingViewFactory
    {
        /// <summary>The reference point, or null if none supplied.</summary>
        private long? _optionalReferencePoint;

        public void SetViewParameters(ViewFactoryContext viewFactoryContext, IList<ExprNode> expressionParameters)
        {
            if ((expressionParameters.Count < 1) || (expressionParameters.Count > 3))
            {
                throw new ViewParameterException(ViewParamMessage);
            }
            var viewParamValues = new Object[expressionParameters.Count];
            for (int i = 1; i < viewParamValues.Length; i++)
            {
                viewParamValues[i] = ViewFactorySupport.ValidateAndEvaluate(
                    ViewName, viewFactoryContext.StatementContext, expressionParameters[i]);
            }

            timeDeltaComputationFactory = ViewFactoryTimePeriodHelper.ValidateAndEvaluateTimeDeltaFactory(
                ViewName, viewFactoryContext.StatementContext, expressionParameters[0], ViewParamMessage, 0);

            if ((viewParamValues.Length == 2) && (viewParamValues[1] is string))
            {
                ProcessKeywords(viewParamValues[1], ViewParamMessage);
            }
            else
            {
                if (viewParamValues.Length >= 2)
                {
                    var paramRef = viewParamValues[1];
                    if ((!(paramRef.IsNumber())) || (paramRef.IsFloatingPointNumber()))
                    {
                        throw new ViewParameterException(
                            ViewName + " view requires a long-typed reference point in msec as a second parameter");
                    }
                    _optionalReferencePoint = paramRef.AsLong();
                }
                if (viewParamValues.Length == 3)
                {
                    ProcessKeywords(viewParamValues[2], ViewParamMessage);
                }
            }
        }

        public void Attach(
            EventType parentEventType,
            StatementContext statementContext,
            ViewFactory optionalParentFactory,
            IList<ViewFactory> parentViewFactories)
        {
            eventType = parentEventType;
        }

        public Object MakePreviousGetter()
        {
            return new RelativeAccessByEventNIndexGetterImpl();
        }

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            ExprTimePeriodEvalDeltaConst timeDeltaComputation = timeDeltaComputationFactory.Make(
                ViewName, "view", agentInstanceViewFactoryContext.AgentInstanceContext);
            ViewUpdatedCollection viewUpdatedCollection =
                agentInstanceViewFactoryContext.StatementContext.ViewServicePreviousFactory
                    .GetOptPreviousExprRelativeAccess(agentInstanceViewFactoryContext);
            if (agentInstanceViewFactoryContext.IsRemoveStream)
            {
                return new TimeBatchViewRStream(
                    this, agentInstanceViewFactoryContext, timeDeltaComputation, _optionalReferencePoint, IsForceUpdate,
                    IsStartEager);
            }
            else
            {
                return new TimeBatchView(
                    this, agentInstanceViewFactoryContext, timeDeltaComputation, _optionalReferencePoint, IsForceUpdate,
                    IsStartEager, viewUpdatedCollection);
            }
        }

        public EventType EventType => eventType;

        public bool CanReuse(View view, AgentInstanceContext agentInstanceContext)
        {
            if (!(view is TimeBatchView))
            {
                return false;
            }

            TimeBatchView myView = (TimeBatchView) view;
            ExprTimePeriodEvalDeltaConst timeDeltaComputation = timeDeltaComputationFactory.Make(
                ViewName, "view", agentInstanceContext);
            if (!timeDeltaComputation.EqualsTimePeriod(myView.TimeDeltaComputation))
            {
                return false;
            }

            if ((myView.InitialReferencePoint != null) && (_optionalReferencePoint != null))
            {
                if (!myView.InitialReferencePoint.Equals(_optionalReferencePoint.Value))
                {
                    return false;
                }
            }
            if (((myView.InitialReferencePoint == null) && (_optionalReferencePoint != null)) ||
                ((myView.InitialReferencePoint != null) && (_optionalReferencePoint == null)))
            {
                return false;
            }

            if (myView.IsForceOutput != IsForceUpdate)
            {
                return false;
            }

            if (myView.IsStartEager)
            {
                // since it's already started
                return false;
            }

            return myView.IsEmpty();
        }

        public string ViewName => "TimeInMillis-Batch";

        private string ViewParamMessage => ViewName +
                                           " view requires a single numeric or time period parameter, and an optional long-typed reference point in msec, and an optional list of control keywords as a string parameter (please see the documentation)";
    }
} // end of namespace
