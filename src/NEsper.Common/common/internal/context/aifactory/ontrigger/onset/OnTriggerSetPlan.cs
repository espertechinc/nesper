///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.fabric;


namespace com.espertech.esper.common.@internal.context.aifactory.ontrigger.onset
{
    public class OnTriggerSetPlan
    {
        private readonly StmtClassForgeableAIFactoryProviderBase forgeable;
        private readonly IList<StmtClassForgeable> forgeables;
        private readonly SelectSubscriberDescriptor selectSubscriberDescriptor;
        private readonly IList<StmtClassForgeableFactory> additionalForgeables;
        private readonly FabricCharge fabricCharge;

        public OnTriggerSetPlan(
            StmtClassForgeableAIFactoryProviderBase forgeable,
            IList<StmtClassForgeable> forgeables,
            SelectSubscriberDescriptor selectSubscriberDescriptor,
            IList<StmtClassForgeableFactory> additionalForgeables,
            FabricCharge fabricCharge)
        {
            this.forgeable = forgeable;
            this.forgeables = forgeables;
            this.selectSubscriberDescriptor = selectSubscriberDescriptor;
            this.additionalForgeables = additionalForgeables;
            this.fabricCharge = fabricCharge;
        }

        public StmtClassForgeableAIFactoryProviderBase Forgeable => forgeable;

        public IList<StmtClassForgeable> Forgeables => forgeables;

        public SelectSubscriberDescriptor SelectSubscriberDescriptor => selectSubscriberDescriptor;

        public IList<StmtClassForgeableFactory> AdditionalForgeables => additionalForgeables;

        public FabricCharge FabricCharge => fabricCharge;
    }
} // end of namespace