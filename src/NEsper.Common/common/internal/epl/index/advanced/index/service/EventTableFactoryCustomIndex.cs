///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.lookup;

namespace com.espertech.esper.common.@internal.epl.index.advanced.index.service
{
    public class EventTableFactoryCustomIndex : EventTableFactory
    {
        private readonly EventType eventType;
        private readonly EventAdvancedIndexProvisionRuntime advancedIndexProvisionDesc;
        private readonly EventTableOrganization organization;

        public EventTableFactoryCustomIndex(
            string indexName,
            int indexedStreamNum,
            EventType eventType,
            bool unique,
            EventAdvancedIndexProvisionRuntime advancedIndexProvisionDesc)
        {
            this.eventType = eventType;
            this.advancedIndexProvisionDesc = advancedIndexProvisionDesc;
            organization = new EventTableOrganization(
                indexName,
                unique,
                false,
                indexedStreamNum,
                advancedIndexProvisionDesc.IndexExpressionTexts,
                EventTableOrganizationType.APPLICATION);
        }

        public Type EventTableClass {
            get => typeof(EventTable);
        }

        public EventTable[] MakeEventTables(
            ExprEvaluatorContext exprEvaluatorContext,
            int? subqueryNumber)
        {
            AdvancedIndexConfigContextPartition configCP = advancedIndexProvisionDesc.Factory.ConfigureContextPartition(
                exprEvaluatorContext,
                eventType,
                advancedIndexProvisionDesc,
                organization);
            EventTable eventTable = advancedIndexProvisionDesc.Factory.Make(
                advancedIndexProvisionDesc.ConfigStatement,
                configCP,
                organization);
            return new EventTable[] {eventTable};
        }

        public string ToQueryPlan()
        {
            return GetType().Name +
                   " streamNum=" +
                   organization.StreamNum +
                   " indexName=" +
                   organization.IndexName;
        }
    }
} // end of namespace