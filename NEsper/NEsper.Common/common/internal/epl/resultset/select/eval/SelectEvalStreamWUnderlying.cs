///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.eval
{
    public class SelectEvalStreamWUnderlying : SelectEvalStreamBaseMap,
        SelectExprProcessorForge
    {
        private readonly EventType[] eventTypes;
        private readonly bool singleStreamWrapper;
        private readonly TableMetaData tableMetadata;
        private readonly ExprForge underlyingExprForge;
        private readonly bool underlyingIsFragmentEvent;
        private readonly EventPropertyGetterSPI underlyingPropertyEventGetter;
        private readonly int underlyingStreamNumber;
        private readonly IList<SelectExprStreamDesc> unnamedStreams;

        private readonly WrapperEventType wrapperEventType;

        public SelectEvalStreamWUnderlying(
            SelectExprForgeContext selectExprForgeContext,
            EventType resultEventType,
            IList<SelectClauseStreamCompiledSpec> namedStreams,
            bool usingWildcard,
            IList<SelectExprStreamDesc> unnamedStreams,
            bool singleStreamWrapper,
            bool underlyingIsFragmentEvent,
            int underlyingStreamNumber,
            EventPropertyGetterSPI underlyingPropertyEventGetter,
            ExprForge underlyingExprForge,
            TableMetaData tableMetadata,
            EventType[] eventTypes)
            : base(selectExprForgeContext, resultEventType, namedStreams, usingWildcard)
        {
            wrapperEventType = (WrapperEventType) resultEventType;
            this.unnamedStreams = unnamedStreams;
            this.singleStreamWrapper = singleStreamWrapper;
            this.underlyingIsFragmentEvent = underlyingIsFragmentEvent;
            this.underlyingStreamNumber = underlyingStreamNumber;
            this.underlyingPropertyEventGetter = underlyingPropertyEventGetter;
            this.underlyingExprForge = underlyingExprForge;
            this.tableMetadata = tableMetadata;
            this.eventTypes = eventTypes;
        }

        protected override CodegenExpression ProcessSpecificCodegen(
            CodegenExpression resultEventType,
            CodegenExpression eventBeanFactory,
            CodegenExpression props,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope
                .MakeChild(typeof(EventBean), typeof(SelectEvalStreamWUnderlying), codegenClassScope)
                .AddParam(typeof(IDictionary<string, object>), "props");
            var wrapperUndType = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(EventType),
                EventTypeUtility.ResolveTypeCodegen(
                    wrapperEventType.UnderlyingEventType,
                    EPStatementInitServicesConstants.REF));

            var refEPS = exprSymbol.GetAddEPS(methodNode);
            var refIsNewData = exprSymbol.GetAddIsNewData(methodNode);
            var refExprEvalCtx = exprSymbol.GetAddExprEvalCtx(methodNode);

            var block = methodNode.Block;
            if (singleStreamWrapper) {
                block.DeclareVar<DecoratingEventBean>(
                        "wrapper",
                        Cast(typeof(DecoratingEventBean), ArrayAtIndex(refEPS, Constant(0))))
                    .IfRefNotNull("wrapper")
                    .ExprDotMethod(props, "PutAll", ExprDotName(Ref("wrapper"), "DecoratingProperties"))
                    .BlockEnd();
            }

            if (underlyingIsFragmentEvent) {
                var fragment = ((EventTypeSPI) eventTypes[underlyingStreamNumber])
                    .GetGetterSPI(unnamedStreams[0].StreamSelected.StreamName)
                    .EventBeanFragmentCodegen(
                        Ref("eventBean"),
                        methodNode,
                        codegenClassScope);
                block.DeclareVar<EventBean>("eventBean", ArrayAtIndex(refEPS, Constant(underlyingStreamNumber)))
                    .DeclareVar<EventBean>("theEvent", Cast(typeof(EventBean), fragment));
            }
            else if (underlyingPropertyEventGetter != null) {
                block.DeclareVar<EventBean>("theEvent", ConstantNull())
                    .DeclareVar<object>(
                        "value",
                        underlyingPropertyEventGetter.EventBeanGetCodegen(
                            ArrayAtIndex(refEPS, Constant(underlyingStreamNumber)),
                            methodNode,
                            codegenClassScope))
                    .IfRefNotNull("value")
                    .AssignRef(
                        "theEvent",
                        ExprDotMethod(eventBeanFactory, "AdapterForTypedObject", Ref("value"), wrapperUndType))
                    .BlockEnd();
            }
            else if (underlyingExprForge != null) {
                block.DeclareVar<EventBean>("theEvent", ConstantNull())
                    .DeclareVar<object>(
                        "value",
                        underlyingExprForge.EvaluateCodegen(typeof(object), methodNode, exprSymbol, codegenClassScope))
                    .IfRefNotNull("value")
                    .AssignRef(
                        "theEvent",
                        ExprDotMethod(eventBeanFactory, "AdapterForTypedObject", Ref("value"), wrapperUndType))
                    .BlockEnd();
            }
            else {
                block.DeclareVar<EventBean>("theEvent", ArrayAtIndex(refEPS, Constant(underlyingStreamNumber)));
                if (tableMetadata != null) {
                    var eventToPublic = TableDeployTimeResolver.MakeTableEventToPublicField(
                        tableMetadata,
                        codegenClassScope,
                        GetType());
                    block.IfRefNotNull("theEvent")
                        .AssignRef(
                            "theEvent",
                            ExprDotMethod(
                                eventToPublic,
                                "Convert",
                                Ref("theEvent"),
                                refEPS,
                                refIsNewData,
                                refExprEvalCtx))
                        .BlockEnd();
                }
            }

            block.MethodReturn(
                ExprDotMethod(
                    eventBeanFactory,
                    "AdapterForTypedWrapper",
                    Ref("theEvent"),
                    Ref("props"),
                    resultEventType));
            return LocalMethod(methodNode, props);
        }
    }
} // end of namespace