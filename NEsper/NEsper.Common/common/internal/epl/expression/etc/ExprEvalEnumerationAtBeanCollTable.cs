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
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.etc
{
    public class ExprEvalEnumerationAtBeanCollTable : ExprForge
    {
        internal readonly ExprEnumerationForge enumerationForge;
        internal readonly TableMetaData table;

        public ExprEvalEnumerationAtBeanCollTable(
            ExprEnumerationForge enumerationForge,
            TableMetaData table)
        {
            this.enumerationForge = enumerationForge;
            this.table = table;
        }

        public ExprEvaluator ExprEvaluator {
            get {
                return new ProxyExprEvaluator() {
                    ProcEvaluate = (
                        eventsPerStream,
                        isNewData,
                        context) => {
                        throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
                    },
                };
            }
        }

        public ExprForgeConstantType ForgeConstantType {
            get => ExprForgeConstantType.NONCONST;
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CodegenExpressionInstanceField eventToPublic =
                TableDeployTimeResolver.MakeTableEventToPublicField(table, codegenClassScope, this.GetType());
            CodegenMethod methodNode = codegenMethodScope.MakeChild(
                typeof(EventBean[]),
                this.GetType(),
                codegenClassScope);

            CodegenExpressionRef refEPS = exprSymbol.GetAddEPS(methodNode);
            CodegenExpression refIsNewData = exprSymbol.GetAddIsNewData(methodNode);
            CodegenExpressionRef refExprEvalCtx = exprSymbol.GetAddExprEvalCtx(methodNode);

            methodNode.Block
                .DeclareVar<object>(
                    "result",
                    enumerationForge.EvaluateGetROCollectionEventsCodegen(methodNode, exprSymbol, codegenClassScope))
                .IfRefNullReturnNull("result")
                .MethodReturn(
                    StaticMethod(
                        typeof(ExprEvalEnumerationAtBeanCollTable),
                        "ConvertToTableType",
                        @Ref("result"),
                        eventToPublic,
                        refEPS,
                        refIsNewData,
                        refExprEvalCtx));
            return LocalMethod(methodNode);
        }

        public Type EvaluationType {
            get => TypeHelper.GetArrayType(table.PublicEventType.UnderlyingType);
        }

        public ExprNodeRenderable ExprForgeRenderable {
            get => enumerationForge.EnumForgeRenderable;
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
            if (result is ICollection<EventBean> eventsDowncast) {
                EventBean[] @out = new EventBean[eventsDowncast.Count];
                int index = 0;
                foreach (EventBean @event in eventsDowncast) {
                    @out[index++] = eventToPublic.Convert(@event, eventsPerStream, isNewData, exprEvaluatorContext);
                }

                return @out;
            }

            EventBean[] events = (EventBean[]) result;
            for (int i = 0; i < events.Length; i++) {
                events[i] = eventToPublic.Convert(events[i], eventsPerStream, isNewData, exprEvaluatorContext);
            }

            return events;
        }
    }
} // end of namespace