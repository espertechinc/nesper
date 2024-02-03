///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.fabric;


namespace com.espertech.esper.common.@internal.context.aifactory.ontrigger.ontrigger
{
    public class OnTriggerPlan
    {
        private readonly StmtClassForgeableAIFactoryProviderBase factory;
        private readonly IList<StmtClassForgeable> forgeables;
        private readonly SelectSubscriberDescriptor subscriberDescriptor;
        private readonly IList<StmtClassForgeableFactory> additionalForgeables;
        private readonly FabricCharge fabricCharge;

        public OnTriggerPlan(
            StmtClassForgeableAIFactoryProviderBase factory,
            IList<StmtClassForgeable> forgeables,
            SelectSubscriberDescriptor subscriberDescriptor,
            IList<StmtClassForgeableFactory> additionalForgeables,
            FabricCharge fabricCharge)
        {
            this.factory = factory;
            this.forgeables = forgeables;
            this.subscriberDescriptor = subscriberDescriptor;
            this.additionalForgeables = additionalForgeables;
            this.fabricCharge = fabricCharge;
        }

        public StmtClassForgeableAIFactoryProviderBase Factory => factory;

        public IList<StmtClassForgeable> Forgeables => forgeables;

        public SelectSubscriberDescriptor SubscriberDescriptor => subscriberDescriptor;

        public IList<StmtClassForgeableFactory> AdditionalForgeables => additionalForgeables;

        public FabricCharge FabricCharge => fabricCharge;
    }
} // end of namespace