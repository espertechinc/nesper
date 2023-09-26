///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.collection;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.select.typable;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.etc
{
    public class ExprEvalEnumerationAtBeanCollTable : ExprForge,
        SelectExprProcessorTypableForge
    {
        private readonly ExprEnumerationForge _enumerationForge;
        private readonly TableMetaData _table;

        public ExprEvalEnumerationAtBeanCollTable(
            ExprEnumerationForge enumerationForge,
            TableMetaData table)
        {
            _enumerationForge = enumerationForge;
            _table = table;
        }

        public ExprEvaluator ExprEvaluator {
            get {
                return new ProxyExprEvaluator() {
                    ProcEvaluate = (
                        eventsPerStream,
                        isNewData,
                        context) => {
                        throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
                    }
                };
            }
        }

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var eventToPublic =
                TableDeployTimeResolver.MakeTableEventToPublicField(_table, codegenClassScope, GetType());
            var methodNode = codegenMethodScope.MakeChild(
                typeof(EventBean[]),
                GetType(),
                codegenClassScope);

            var refEPS = exprSymbol.GetAddEPS(methodNode);
            var refIsNewData = exprSymbol.GetAddIsNewData(methodNode);
            var refExprEvalCtx = exprSymbol.GetAddExprEvalCtx(methodNode);

            methodNode.Block
                .DeclareVar<object>(
                    "result",
                    _enumerationForge.EvaluateGetROCollectionEventsCodegen(methodNode, exprSymbol, codegenClassScope))
                .IfRefNullReturnNull("result")
                .MethodReturn(
                    StaticMethod(
                        typeof(ExprEvalEnumerationAtBeanCollTable),
                        "ConvertToTableType",
                        Ref("result"),
                        eventToPublic,
                        refEPS,
                        refIsNewData,
                        refExprEvalCtx));
            return LocalMethod(methodNode);
        }

        public Type EvaluationType => typeof(EventBean[]);

        public Type UnderlyingEvaluationType => TypeHelper.GetArrayType(_table.PublicEventType.UnderlyingType);

        public ExprNodeRenderable ExprForgeRenderable => _enumerationForge.EnumForgeRenderable;

        public static EventBean[] ConvertToTableTypeImpl(
            ICollection<EventBean> eventBeanCollection,
            TableMetadataInternalEventToPublic eventToPublic,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var @out = new EventBean[eventBeanCollection.Count];
            var index = 0;
            foreach (var @event in eventBeanCollection) {
                @out[index++] = eventToPublic.Convert(@event, eventsPerStream, isNewData, exprEvaluatorContext);
            }

            return @out;
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="result">result</param>
        /// <param name="eventToPublic">conversion</param>
        /// <param name="eventsPerStream">events</param>
        /// <param name="isNewData">flag</param>
        /// <param name="exprEvaluatorContext">context</param>
        /// <returns>beans</returns>
        public static EventBean[] ConvertToTableType(
            object result,
            TableMetadataInternalEventToPublic eventToPublic,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (result is FlexCollection eventsDowncastFlex) {
                return ConvertToTableTypeImpl(
                    eventsDowncastFlex.EventBeanCollection,
                    eventToPublic,
                    eventsPerStream,
                    isNewData,
                    exprEvaluatorContext);
            }
            else if (result is ICollection<EventBean> eventsDowncast) {
                return ConvertToTableTypeImpl(
                    eventsDowncast,
                    eventToPublic,
                    eventsPerStream,
                    isNewData,
                    exprEvaluatorContext);
            }

            var events = (EventBean[])result;
            for (var i = 0; i < events.Length; i++) {
                events[i] = eventToPublic.Convert(events[i], eventsPerStream, isNewData, exprEvaluatorContext);
            }

            return events;
        }
    }
} // end of namespace