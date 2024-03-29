///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.rowrecog.core;
using com.espertech.esper.common.@internal.epl.rowrecog.state;

namespace com.espertech.esper.common.@internal.context.airegistry
{
    public class AIRegistryRowRecogPreviousStrategySingle : AIRegistryRowRecogPreviousStrategy,
        RowRecogPreviousStrategy
    {
        private RowRecogPreviousStrategy service;

        public void AssignService(
            int serviceId,
            RowRecogPreviousStrategy strategy)
        {
            service = strategy;
        }

        public void DeassignService(int serviceId)
        {
            service = null;
        }

        public RowRecogStateRandomAccess GetAccess(ExprEvaluatorContext exprEvaluatorContext)
        {
            return service.GetAccess(exprEvaluatorContext);
        }

        public int InstanceCount => service == null ? 0 : 1;
    }
} // end of namespace