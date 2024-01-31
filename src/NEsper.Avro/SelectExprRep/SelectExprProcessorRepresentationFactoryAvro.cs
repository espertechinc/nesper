///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.@select.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.@event.avro;

using NEsper.Avro.Writer;

namespace NEsper.Avro.SelectExprRep
{
    public class SelectExprProcessorRepresentationFactoryAvro : SelectExprProcessorRepresentationFactory
    {
        public SelectExprProcessorForge MakeSelectNoWildcard(
            SelectExprForgeContext selectExprForgeContext,
            ExprForge[] exprForges,
            EventType resultEventType,
            TableCompileTimeResolver tableService,
            string statementName)
        {
            return new EvalSelectNoWildcardAvro(selectExprForgeContext, exprForges, resultEventType, statementName);
        }

        public SelectExprProcessorForge MakeRecast(
            EventType[] eventTypes,
            SelectExprForgeContext selectExprForgeContext,
            int streamNumber,
            AvroSchemaEventType insertIntoTargetType,
            ExprNode[] exprNodes,
            string statementName)
        {
            return AvroRecastFactory.Make(
                eventTypes,
                selectExprForgeContext,
                streamNumber,
                insertIntoTargetType,
                exprNodes,
                statementName);
        }

        public SelectExprProcessorForge MakeJoinWildcard(
            string[] streamNames,
            EventType resultEventType)
        {
            return new SelectExprJoinWildcardProcessorAvro(resultEventType);
        }
    }
} // end of namespace