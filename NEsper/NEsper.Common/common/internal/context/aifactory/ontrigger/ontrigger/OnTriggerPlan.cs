///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.resultset.@select.core;

namespace com.espertech.esper.common.@internal.context.aifactory.ontrigger.ontrigger
{
    public class OnTriggerPlan
    {
        public OnTriggerPlan(
            StmtClassForgableAIFactoryProviderBase factory,
            IList<StmtClassForgable> forgables,
            SelectSubscriberDescriptor subscriberDescriptor)
        {
            Factory = factory;
            Forgables = forgables;
            SubscriberDescriptor = subscriberDescriptor;
        }

        public StmtClassForgableAIFactoryProviderBase Factory { get; }

        public IList<StmtClassForgable> Forgables { get; }

        public SelectSubscriberDescriptor SubscriberDescriptor { get; }
    }
} // end of namespace