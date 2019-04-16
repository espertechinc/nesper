///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.table.strategy;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.airegistry
{
    public class AIRegistryTableAccessMap : AIRegistryTableAccess
    {
        private readonly IDictionary<int, ExprTableEvalStrategy> services;

        protected internal AIRegistryTableAccessMap()
        {
            services = new Dictionary<int, ExprTableEvalStrategy>();
        }

        public void AssignService(
            int num,
            ExprTableEvalStrategy strategy)
        {
            services.Put(num, strategy);
        }

        public void DeassignService(int num)
        {
            services.Remove(num);
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return services.Get(exprEvaluatorContext.AgentInstanceId).Evaluate(
                eventsPerStream, isNewData, exprEvaluatorContext);
        }

        public ICollection<EventBean> EvaluateGetROCollectionEvents(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return services.Get(context.AgentInstanceId)
                .EvaluateGetROCollectionEvents(eventsPerStream, isNewData, context);
        }

        public EventBean EvaluateGetEventBean(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return services.Get(context.AgentInstanceId).EvaluateGetEventBean(eventsPerStream, isNewData, context);
        }

        public ICollection<object> EvaluateGetROCollectionScalar(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return services.Get(context.AgentInstanceId)
                .EvaluateGetROCollectionScalar(eventsPerStream, isNewData, context);
        }

        public object[] EvaluateTypableSingle(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return services.Get(context.AgentInstanceId).EvaluateTypableSingle(eventsPerStream, isNewData, context);
        }

        public int InstanceCount => services.Count;
    }
} // end of namespace