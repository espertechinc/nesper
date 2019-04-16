///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.metrics.audit;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.join.hint
{
    public class ExcludePlanHint
    {
        private static readonly ILog QUERY_PLAN_LOG = LogManager.GetLogger(AuditPath.QUERYPLAN_LOG);

        private readonly string[] streamNames;
        private readonly IList<ExprEvaluator> evaluators;
        private readonly bool queryPlanLogging;

        public ExcludePlanHint(
            string[] streamNames,
            IList<ExprEvaluator> evaluators,
            StatementCompileTimeServices services)
        {
            this.streamNames = streamNames;
            this.evaluators = evaluators;
            this.queryPlanLogging = services.Configuration.Common.Logging.IsEnableQueryPlan;
        }

        public static ExcludePlanHint GetHint(
            string[] streamNames,
            StatementRawInfo rawInfo,
            StatementCompileTimeServices services)
        {
            IList<string> hints = HintEnum.EXCLUDE_PLAN.GetHintAssignedValues(rawInfo.Annotations);
            if (hints == null) {
                return null;
            }

            IList<ExprEvaluator> filters = new List<ExprEvaluator>();
            foreach (string hint in hints) {
                if (hint.Trim().IsEmpty()) {
                    continue;
                }

                ExprForge forge = ExcludePlanHintExprUtil.ToExpression(hint, rawInfo, services);
                if (Boxing.GetBoxedType(forge.EvaluationType) != typeof(bool?)) {
                    throw new ExprValidationException("Expression provided for hint " + HintEnum.EXCLUDE_PLAN.Value + " must return a boolean value");
                }

                filters.Add(forge.ExprEvaluator);
            }

            return new ExcludePlanHint(streamNames, filters, services);
        }

        public bool Filter(
            int streamLookup,
            int streamIndexed,
            ExcludePlanFilterOperatorType opType,
            params ExprNode[] exprNodes)
        {
            EventBean @event = ExcludePlanHintExprUtil.ToEvent(
                streamLookup,
                streamIndexed, streamNames[streamLookup], streamNames[streamIndexed],
                opType.GetName().ToLowerInvariant(), exprNodes);
            if (queryPlanLogging && QUERY_PLAN_LOG.IsInfoEnabled) {
                QUERY_PLAN_LOG.Info("Exclude-plan-hint combination " + EventBeanUtility.PrintEvent(@event));
            }

            EventBean[] eventsPerStream = new EventBean[] {@event};

            foreach (ExprEvaluator evaluator in evaluators) {
                var pass = evaluator.Evaluate(eventsPerStream, true, null);
                if (pass != null && true.Equals(pass)) {
                    if (queryPlanLogging && QUERY_PLAN_LOG.IsInfoEnabled) {
                        QUERY_PLAN_LOG.Info("Exclude-plan-hint combination : true");
                    }

                    return true;
                }
            }

            if (queryPlanLogging && QUERY_PLAN_LOG.IsInfoEnabled) {
                QUERY_PLAN_LOG.Info("Exclude-plan-hint combination : false");
            }

            return false;
        }
    }
} // end of namespace