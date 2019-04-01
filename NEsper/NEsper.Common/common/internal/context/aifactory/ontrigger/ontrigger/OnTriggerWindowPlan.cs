///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.aifactory.ontrigger.core;
using com.espertech.esper.common.@internal.epl.expression.subquery;
using com.espertech.esper.common.@internal.epl.subselect;

namespace com.espertech.esper.common.@internal.context.aifactory.ontrigger.ontrigger
{
    public class OnTriggerWindowPlan
    {
        public OnTriggerWindowPlan(
            OnTriggerWindowDesc onTriggerDesc,
            string contextName, 
            OnTriggerActivatorDesc activatorResult,
            StreamSelector optionalStreamSelector,
            IDictionary<ExprSubselectNode, SubSelectActivationPlan> subselectActivation, 
            StreamSpecCompiled streamSpec)
        {
            OnTriggerDesc = onTriggerDesc;
            ContextName = contextName;
            ActivatorResult = activatorResult;
            OptionalStreamSelector = optionalStreamSelector;
            SubselectActivation = subselectActivation;
            StreamSpec = streamSpec;
        }

        public OnTriggerWindowDesc OnTriggerDesc { get; }

        public string ContextName { get; }

        public OnTriggerActivatorDesc ActivatorResult { get; }

        public StreamSelector OptionalStreamSelector { get; }

        public IDictionary<ExprSubselectNode, SubSelectActivationPlan> SubselectActivation { get; }

        public StreamSpecCompiled StreamSpec { get; }
    }
} // end of namespace