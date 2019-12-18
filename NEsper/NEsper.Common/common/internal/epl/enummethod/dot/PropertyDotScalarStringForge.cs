///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.collection;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.logging;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.dot
{
    public class PropertyDotScalarStringForge : ExprEnumerationEval,
        ExprEnumerationForge,
        ExprEnumerationGivenEvent,
        ExprEnumerationGivenEventForge,
        ExprNodeRenderable
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly EventPropertyGetterSPI _getter;
        private readonly Type _getterReturnType;
        private readonly string _propertyName;
        private readonly int _streamId;

        public PropertyDotScalarStringForge(
            string propertyName,
            int streamId,
            EventPropertyGetterSPI getter)
        {
            _propertyName = propertyName;
            _streamId = streamId;
            _getter = getter;
            _getterReturnType = typeof(string);
            ComponentTypeCollection = typeof(char);
        }

        public ICollection<object> EvaluateGetROCollectionScalar(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var eventInQuestion = eventsPerStream[_streamId];
            return EvaluateEventGetROCollectionScalar(eventInQuestion, context);
        }

        public EventBean EvaluateGetEventBean(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return null;
        }

        public ICollection<EventBean> EvaluateGetROCollectionEvents(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return null;
        }

        public ExprEnumerationEval ExprEvaluatorEnumeration => this;

        public CodegenExpression EvaluateGetROCollectionScalarCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var refEPS = exprSymbol.GetAddEPS(codegenMethodScope);
            var refExprEvalCtx = exprSymbol.GetAddExprEvalCtx(codegenMethodScope);
            return CodegenEvaluateEventGetROCollectionScalar(
                ArrayAtIndex(refEPS, Constant(_streamId)),
                refExprEvalCtx,
                codegenMethodScope,
                codegenClassScope);
        }

        public Type ComponentTypeCollection { get; }

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
            return null;
        }

        public CodegenExpression EvaluateGetEventBeanCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        public ExprNodeRenderable EnumForgeRenderable => this;

        public CodegenExpression EvaluateGetROCollectionEventsCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        public ICollection<object> EvaluateEventGetROCollectionScalar(
            EventBean @event,
            ExprEvaluatorContext context)
        {
            if (@event == null) {
                return null;
            }

            return EvaluateGetInternal(@event);
        }

        public ICollection<EventBean> EvaluateEventGetROCollectionEvents(
            EventBean @event,
            ExprEvaluatorContext context)
        {
            return null;
        }

        public EventBean EvaluateEventGetEventBean(
            EventBean @event,
            ExprEvaluatorContext context)
        {
            return null;
        }

        public CodegenExpression EvaluateEventGetROCollectionScalarCodegen(
            CodegenMethodScope methodScope,
            ExprEnumerationGivenEventSymbol symbols,
            CodegenClassScope codegenClassScope)
        {
            var method = methodScope.MakeChild(
                typeof(FlexCollection),
                typeof(PropertyDotScalarArrayForge),
                codegenClassScope);
            method.Block
                .IfRefNullReturnNull(symbols.GetAddEvent(method))
                .MethodReturn(
                    CodegenEvaluateGetInternal(symbols.GetAddEvent(method), methodScope, codegenClassScope));
            return LocalMethod(method);
        }

        public CodegenExpression EvaluateEventGetEventBeanCodegen(
            CodegenMethodScope methodScope,
            ExprEnumerationGivenEventSymbol symbols,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        public CodegenExpression EvaluateEventGetROCollectionEventsCodegen(
            CodegenMethodScope methodScope,
            ExprEnumerationGivenEventSymbol symbols,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        public void ToEPL(
            TextWriter writer,
            ExprPrecedenceEnum parentPrecedence)
        {
            writer.Write(GetType().Name);
        }

        public CodegenExpression CodegenEvaluateEventGetROCollectionScalar(
            CodegenExpression @event,
            CodegenExpression evalctx,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var method = codegenMethodScope
                .MakeChild(typeof(ICollection<object>), typeof(PropertyDotScalarArrayForge), codegenClassScope)
                .AddParam(typeof(EventBean), "@event")
                .AddParam(typeof(ExprEvaluatorContext), "context")
                .Block
                .IfRefNullReturnNull("@event")
                .MethodReturn(CodegenEvaluateGetInternal(Ref("@event"), codegenMethodScope, codegenClassScope));
            return LocalMethodBuild(method).Pass(@event).Pass(evalctx).Call();
        }

        private FlexCollection EvaluateGetInternal(EventBean @event)
        {
            return ConvertToCollection(_propertyName, _getter.Get(@event));
        }
        
        private CodegenExpression CodegenEvaluateGetInternal(
            CodegenExpression @event,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var block = codegenMethodScope
                .MakeChild(typeof(FlexCollection), typeof(PropertyDotScalarArrayForge), codegenClassScope)
                .AddParam(typeof(EventBean), "@event")
                .Block
                .DeclareVar(
                    _getterReturnType,
                    "value",
                    CodegenLegoCast.CastSafeFromObjectType(
                        _getterReturnType,
                        _getter.EventBeanGetCodegen(Ref("@event"), codegenMethodScope, codegenClassScope)));
            
            var method = block.MethodReturn(
                    StaticMethod(
                        typeof(PropertyDotScalarStringForge),
                        "ConvertToCollection",
                        Constant(_propertyName),
                        Ref("value")));

            return LocalMethodBuild(method).Pass(@event).Call();
        }
        
        public static FlexCollection ConvertToCollection(
            string propertyName, 
            object value)
        {
            if (value == null) {
                return null;
            }

            if (value is string stringValue) {
                // Convert to a collection of characters
                return FlexCollection.Of(
                    stringValue
                        .ToCharArray()
                        .Cast<object>()
                        .ToList());
            }

            Log.Warn(
                "Expected string-type input from property '" + propertyName + "' but received " + value.GetType());
            return null;
        }
    }
} // end of namespace