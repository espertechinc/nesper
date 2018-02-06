///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.epl.lookup;

namespace com.espertech.esper.epl.index.service
{
    public class EventTableFactoryCustomIndex : EventTableFactory
    {
        protected readonly EventType eventType;
        protected readonly EventAdvancedIndexProvisionDesc advancedIndexProvisionDesc;
        protected readonly EventTableOrganization organization;
    
        public EventTableFactoryCustomIndex(string indexName, int indexedStreamNum, EventType eventType, bool unique, EventAdvancedIndexProvisionDesc advancedIndexProvisionDesc) {
            this.eventType = eventType;
            this.advancedIndexProvisionDesc = advancedIndexProvisionDesc;
            string[] expressions = ExprNodeUtility.ToExpressionStringMinPrecedenceAsArray(advancedIndexProvisionDesc.IndexDesc.IndexedExpressions);
            this.organization = new EventTableOrganization(indexName, unique, false, indexedStreamNum, expressions, EventTableOrganizationType.APPLICATION);
        }

        public Type EventTableType => typeof(EventTable);

        public EventTable[] MakeEventTables(EventTableFactoryTableIdent tableIdent, ExprEvaluatorContext exprEvaluatorContext) {
            AdvancedIndexConfigContextPartition configCP = advancedIndexProvisionDesc.Factory.ConfigureContextPartition(eventType, advancedIndexProvisionDesc.IndexDesc, advancedIndexProvisionDesc.Parameters, exprEvaluatorContext, organization, advancedIndexProvisionDesc.ConfigStatement);
            EventTable eventTable = advancedIndexProvisionDesc.Factory.Make(advancedIndexProvisionDesc.ConfigStatement, configCP, organization);
            return new EventTable[]{eventTable};
        }
    
        public string ToQueryPlan() {
            return this.GetType().Name +
                    " streamNum=" + organization.StreamNum +
                    " indexName=" + organization.IndexName;
        }
    }
} // end of namespace
