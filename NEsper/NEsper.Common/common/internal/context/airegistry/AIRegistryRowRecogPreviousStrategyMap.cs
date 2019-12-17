///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.rowrecog.core;
using com.espertech.esper.common.@internal.epl.rowrecog.state;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.airegistry
{
    public class AIRegistryRowRecogPreviousStrategyMap : AIRegistryRowRecogPreviousStrategy
    {
        private readonly IDictionary<int, RowRecogPreviousStrategy> services;

        protected internal AIRegistryRowRecogPreviousStrategyMap()
        {
            services = new Dictionary<int, RowRecogPreviousStrategy>();
        }

        public void AssignService(
            int serviceId,
            RowRecogPreviousStrategy service)
        {
            services.Put(serviceId, service);
        }

        public void DeassignService(int serviceId)
        {
            services.Remove(serviceId);
        }

        public int InstanceCount => services.Count;

        public RowRecogStateRandomAccess GetAccess(ExprEvaluatorContext exprEvaluatorContext)
        {
            return services.Get(exprEvaluatorContext.AgentInstanceId).GetAccess(exprEvaluatorContext);
        }
    }
} // end of namespace