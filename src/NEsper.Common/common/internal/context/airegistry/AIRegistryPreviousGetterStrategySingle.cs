///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.view.previous;

namespace com.espertech.esper.common.@internal.context.airegistry
{
    public class AIRegistryPreviousGetterStrategySingle : AIRegistryPreviousGetterStrategy
    {
        private PreviousGetterStrategy service;

        public void AssignService(
            int serviceId,
            PreviousGetterStrategy previousGetterStrategy)
        {
            service = previousGetterStrategy;
        }

        public void DeassignService(int serviceId)
        {
            service = null;
        }

        public int InstanceCount => service == null ? 0 : 1;

        public PreviousGetterStrategy GetStrategy(ExprEvaluatorContext ctx)
        {
            return service.GetStrategy(ctx);
        }
    }
} // end of namespace