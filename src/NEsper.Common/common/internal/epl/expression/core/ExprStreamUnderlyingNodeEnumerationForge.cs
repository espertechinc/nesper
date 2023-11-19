///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.codegen;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public class ExprStreamUnderlyingNodeEnumerationForge : ExprEnumerationForge
    {
        private readonly string streamName;
        private readonly int streamNum;
        private readonly EventType eventType;

        public ExprStreamUnderlyingNodeEnumerationForge(
            string streamName,
            int streamNum,
            EventType eventType)
        {
            this.streamName = streamName;
            this.streamNum = streamNum;
            this.eventType = eventType;
        }

        public Type ComponentTypeCollection => null;

        public EventType GetEventTypeCollection(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            return null;
        }

        public EventType GetEventTypeSingle(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            return eventType;
        }

        public CodegenExpression EvaluateGetROCollectionEventsCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        public CodegenExpression EvaluateGetROCollectionScalarCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        public CodegenExpression EvaluateGetEventBeanCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ArrayAtIndex(exprSymbol.GetAddEps(codegenMethodScope), Constant(streamNum));
        }


        public ExprNodeRenderable EnumForgeRenderable {
            get {
                return new ProxyExprNodeRenderable() {
                    ProcToEPL = (
                        writer,
                        parentPrecedence,
                        flags) => {
                        writer.Write(streamName);
                    }
                };
            }
        }

        public ExprEnumerationEval ExprEvaluatorEnumeration {
            get {
                return new ProxyExprEnumerationEval() {
                    ProcEvaluateGetROCollectionEvents = (
                        eventsPerStream,
                        isNewData,
                        context) => null,
                    ProcEvaluateGetROCollectionScalar = (
                        eventsPerStream,
                        isNewData,
                        context) => null,
                    ProcEvaluateGetEventBean = (
                        eventsPerStream,
                        isNewData,
                        context) => eventsPerStream[streamNum]
                };
            }
        }
    }
} // end of namespace