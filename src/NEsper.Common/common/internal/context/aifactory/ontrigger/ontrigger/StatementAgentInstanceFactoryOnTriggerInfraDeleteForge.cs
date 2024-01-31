///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.activator;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.subquery;
using com.espertech.esper.common.@internal.epl.expression.table;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.common.@internal.epl.subselect;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.strategy;

namespace com.espertech.esper.common.@internal.context.aifactory.ontrigger.ontrigger
{
    public class StatementAgentInstanceFactoryOnTriggerInfraDeleteForge :
        StatementAgentInstanceFactoryOnTriggerInfraBaseForge
    {
        public StatementAgentInstanceFactoryOnTriggerInfraDeleteForge(
            ViewableActivatorForge activator,
            EventType resultEventType,
            IDictionary<ExprSubselectNode, SubSelectFactoryForge> subselects,
            IDictionary<ExprTableAccessNode, ExprTableEvalStrategyFactoryForge> tableAccesses,
            string nonSelectRSPProviderClassName,
            NamedWindowMetaData namedWindow,
            TableMetaData table,
            SubordinateWMatchExprQueryPlanForge queryPlanForge)
            : base(
                activator,
                resultEventType,
                subselects,
                tableAccesses,
                nonSelectRSPProviderClassName,
                namedWindow,
                table,
                queryPlanForge)
        {
        }

        public override Type TypeOfSubclass()
        {
            return typeof(StatementAgentInstanceFactoryOnTriggerInfraDelete);
        }

        protected override void InlineInitializeOnTriggerSpecific(
            CodegenExpressionRef saiff,
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
        }
    }
} // end of namespace