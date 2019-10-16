///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using Avro;
using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.resultset.@select.core;
using com.espertech.esper.common.@internal.@event.core;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

namespace NEsper.Avro.SelectExprRep
{
    /// <summary>
    /// Processor for select-clause expressions that handles wildcards. Computes results based on matching events.
    /// </summary>
    public class SelectExprJoinWildcardProcessorAvro : SelectExprProcessorForge
    {
        private readonly EventType _resultEventTypeAvro;

        public SelectExprJoinWildcardProcessorAvro(EventType resultEventTypeAvro)
        {
            _resultEventTypeAvro = resultEventTypeAvro;
        }

        public CodegenMethod ProcessCodegen(
            CodegenExpression resultEventTypeOuter,
            CodegenExpression eventBeanFactory,
            CodegenMethodScope codegenMethodScope,
            SelectExprProcessorCodegenSymbol selectSymbol,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            // NOTE: Maintaining result-event-type as out own field as we may be an "inner" select-expr-processor
            var mType = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(EventType),
                EventTypeUtility.ResolveTypeCodegen(_resultEventTypeAvro, EPStatementInitServicesConstants.REF));
            var schema = codegenClassScope.NamespaceScope.AddDefaultFieldUnshared(
                true,
                typeof(RecordSchema),
                CodegenExpressionBuilder.StaticMethod(
                    typeof(AvroSchemaUtil),
                    "ResolveRecordSchema",
                    EventTypeUtility.ResolveTypeCodegen(_resultEventTypeAvro, EPStatementInitServicesConstants.REF)));
            var methodNode = codegenMethodScope.MakeChild(typeof(EventBean), GetType(), codegenClassScope);
            var refEPS = exprSymbol.GetAddEPS(methodNode);
            methodNode.Block.MethodReturn(
                CodegenExpressionBuilder.StaticMethod(
                    typeof(SelectExprJoinWildcardProcessorAvro),
                    "ProcessSelectExprJoinWildcardAvro",
                    refEPS,
                    schema,
                    eventBeanFactory,
                    mType));
            return methodNode;
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="eventsPerStream">events</param>
        /// <param name="schema">schema</param>
        /// <param name="eventAdapterService">event svc</param>
        /// <param name="resultEventType">result event type</param>
        /// <returns>bean</returns>
        public static EventBean ProcessSelectExprJoinWildcardAvro(
            EventBean[] eventsPerStream,
            Schema schema,
            EventBeanTypedEventFactory eventAdapterService,
            EventType resultEventType)
        {
            var fields = schema.AsRecordSchema().Fields;
            var @event = new GenericRecord(schema.AsRecordSchema());
            for (var i = 0; i < eventsPerStream.Length; i++) {
                var streamEvent = eventsPerStream[i];
                if (streamEvent != null) {
                    var record = (GenericRecord) streamEvent.Underlying;
                    @event.Put(fields[i], record);
                }
            }

            return eventAdapterService.AdapterForTypedAvro(@event, resultEventType);
        }

        public EventType ResultEventType {
            get => _resultEventTypeAvro;
        }
    }
} // end of namespace