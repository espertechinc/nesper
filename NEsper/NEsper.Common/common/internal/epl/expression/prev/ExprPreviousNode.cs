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
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.name;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.access;
using com.espertech.esper.common.@internal.view.previous;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.CodegenRelational;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;

namespace com.espertech.esper.common.@internal.epl.expression.prev
{
    /// <summary>
    ///     Represents the 'prev' previous event function in an expression node tree.
    /// </summary>
    public class ExprPreviousNode : ExprNodeBase,
        ExprEvaluator,
        ExprEnumerationForge,
        ExprEnumerationEval,
        ExprForgeInstrumentable
    {
        private EventType enumerationMethodType;
        private CodegenFieldName previousStrategyFieldName;

        public ExprPreviousNode(ExprPreviousNodePreviousType previousType)
        {
            PreviousType = previousType;
        }

        public int StreamNumber { get; private set; }

        public int? ConstantIndexNumber { get; private set; }

        public bool IsConstantIndex { get; private set; }

        public Type ResultType { get; private set; }

        public override ExprForge Forge => this;

        public ExprPreviousNodePreviousType PreviousType { get; }

        public bool IsConstantResult => false;

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        ExprNodeRenderable ExprEnumerationForge.ForgeRenderable => ForgeRenderable;

        ExprNodeRenderable ExprForge.ForgeRenderable => ForgeRenderable;

        public ExprNode ForgeRenderable => this;

        public ICollection<EventBean> EvaluateGetROCollectionEvents(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
        }

        public EventBean EvaluateGetEventBean(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
        }

        public ICollection<object> EvaluateGetROCollectionScalar(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
        }

        public ExprEnumerationEval ExprEvaluatorEnumeration => this;

        public CodegenExpression EvaluateGetROCollectionEventsCodegen(
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            if (PreviousType == ExprPreviousNodePreviousType.PREV ||
                PreviousType == ExprPreviousNodePreviousType.PREVTAIL ||
                PreviousType == ExprPreviousNodePreviousType.PREVCOUNT) {
                return ConstantNull();
            }

            if (PreviousType != ExprPreviousNodePreviousType.PREVWINDOW) {
                throw new IllegalStateException("Unrecognized previous type " + PreviousType);
            }

            var method = parent.MakeChild(typeof(ICollection<object>), GetType(), codegenClassScope);
            method.Block.DeclareVar(
                typeof(PreviousGetterStrategy), "strategy",
                ExprDotMethod(GetterField(codegenClassScope), "getStrategy", exprSymbol.GetAddExprEvalCtx(method)));

            method.Block.IfCondition(Not(exprSymbol.GetAddIsNewData(method))).BlockReturn(ConstantNull());
            var randomAccess = method.Block.IfCondition(InstanceOf(Ref("strategy"), typeof(RandomAccessByIndexGetter)));
            {
                randomAccess
                    .DeclareVar(
                        typeof(RandomAccessByIndexGetter), "getter",
                        Cast(typeof(RandomAccessByIndexGetter), Ref("strategy")))
                    .DeclareVar(
                        typeof(RandomAccessByIndex), "randomAccess", ExprDotMethod(Ref("getter"), "getAccessor"))
                    .BlockReturn(ExprDotMethod(Ref("randomAccess"), "getWindowCollectionReadOnly"));
            }
            var relativeAccess = randomAccess.IfElse();
            {
                relativeAccess
                    .DeclareVar(
                        typeof(RelativeAccessByEventNIndexGetter), "getter",
                        Cast(typeof(RelativeAccessByEventNIndexGetter), Ref("strategy")))
                    .DeclareVar(
                        typeof(EventBean), "evalEvent",
                        ArrayAtIndex(exprSymbol.GetAddEPS(method), Constant(StreamNumber)))
                    .DeclareVar(
                        typeof(RelativeAccessByEventNIndex), "relativeAccess",
                        ExprDotMethod(Ref("getter"), "getAccessor", Ref("evalEvent")))
                    .IfRefNullReturnNull("relativeAccess")
                    .BlockReturn(ExprDotMethod(Ref("relativeAccess"), "getWindowToEventCollReadOnly"));
            }
            return LocalMethod(method);
        }

        public CodegenExpression EvaluateGetEventBeanCodegen(
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            if (PreviousType == ExprPreviousNodePreviousType.PREVWINDOW ||
                PreviousType == ExprPreviousNodePreviousType.PREVCOUNT) {
                return ConstantNull();
            }

            var method = parent.MakeChild(typeof(EventBean), GetType(), codegenClassScope);
            method.Block
                .IfCondition(Not(exprSymbol.GetAddIsNewData(method))).BlockReturn(ConstantNull())
                .MethodReturn(
                    LocalMethod(
                        GetSubstituteCodegen(method, exprSymbol, codegenClassScope), exprSymbol.GetAddEPS(method),
                        exprSymbol.GetAddExprEvalCtx(method)));
            return LocalMethod(method);
        }

        public CodegenExpression EvaluateGetROCollectionScalarCodegen(
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            if (PreviousType == ExprPreviousNodePreviousType.PREVCOUNT) {
                return ConstantNull();
            }

            if (PreviousType == ExprPreviousNodePreviousType.PREVWINDOW) {
                var methodX = parent.MakeChild(typeof(ICollection<object>), GetType(), codegenClassScope);

                methodX.Block
                    .DeclareVar(
                        typeof(PreviousGetterStrategy), "strategy",
                        ExprDotMethod(
                            GetterField(codegenClassScope), "getStrategy", exprSymbol.GetAddExprEvalCtx(methodX)))
                    .Apply(new PreviousBlockGetSizeAndIterator(methodX, exprSymbol, StreamNumber, Ref("strategy")).Accept);

                var eps = exprSymbol.GetAddEPS(methodX);
                var innerEval = CodegenLegoMethodExpression.CodegenExpression(
                    ChildNodes[1].Forge, methodX, codegenClassScope);

                methodX.Block.DeclareVar(typeof(EventBean), "originalEvent", ArrayAtIndex(eps, Constant(StreamNumber)))
                    .DeclareVar(typeof(ICollection<object>), "result", NewInstance(typeof(ArrayDeque<object>), Ref("size")))
                    .ForLoopIntSimple("i", Ref("size"))
                    .AssignArrayElement(
                        eps, Constant(StreamNumber), Cast(typeof(EventBean), ExprDotMethod(Ref("events"), "next")))
                    .ExprDotMethod(
                        Ref("result"), "add",
                        LocalMethod(innerEval, eps, ConstantTrue(), exprSymbol.GetAddExprEvalCtx(methodX)))
                    .BlockEnd()
                    .AssignArrayElement(eps, Constant(StreamNumber), Ref("originalEvent"))
                    .MethodReturn(Ref("result"));
                return LocalMethod(methodX);
            }

            var method = parent.MakeChild(typeof(ICollection<object>), GetType(), codegenClassScope);
            method.Block.DeclareVar(
                    typeof(object), "result", EvaluateCodegenPrevAndTail(method, exprSymbol, codegenClassScope))
                .IfRefNullReturnNull("result")
                .MethodReturn(StaticMethod(typeof(Collections), "singletonList", Ref("result")));
            return LocalMethod(method);
        }

        public EventType GetEventTypeCollection(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            if (PreviousType == ExprPreviousNodePreviousType.PREV ||
                PreviousType == ExprPreviousNodePreviousType.PREVTAIL) {
                return null;
            }

            return enumerationMethodType;
        }

        public EventType GetEventTypeSingle(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            if (PreviousType == ExprPreviousNodePreviousType.PREV ||
                PreviousType == ExprPreviousNodePreviousType.PREVTAIL) {
                return enumerationMethodType;
            }

            return null;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            throw new UnsupportedOperationException();
        }

        public ExprEvaluator ExprEvaluator => this;

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public Type EvaluationType => ResultType;

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            if (PreviousType == ExprPreviousNodePreviousType.PREV ||
                PreviousType == ExprPreviousNodePreviousType.PREVTAIL) {
                return EvaluateCodegenPrevAndTail(codegenMethodScope, exprSymbol, codegenClassScope);
            }

            if (PreviousType == ExprPreviousNodePreviousType.PREVWINDOW) {
                return EvaluateCodegenPrevWindow(requiredType, codegenMethodScope, exprSymbol, codegenClassScope);
            }

            if (PreviousType == ExprPreviousNodePreviousType.PREVCOUNT) {
                return EvaluateCodegenPrevCount(requiredType, codegenMethodScope, exprSymbol, codegenClassScope);
            }

            throw new IllegalStateException("Unrecognized previous type " + PreviousType);
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return new InstrumentationBuilderExpr(
                    GetType(), this, "ExprPrev", requiredType, codegenMethodScope, exprSymbol, codegenClassScope)
                .Qparam(exprSymbol.GetAddIsNewData(codegenMethodScope)).Build();
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (ChildNodes.Length > 2 || ChildNodes.Length == 0) {
                throw new ExprValidationException("Previous node must have 1 or 2 parameters");
            }

            // add constant of 1 for previous index
            if (ChildNodes.Length == 1) {
                if (PreviousType == ExprPreviousNodePreviousType.PREV) {
                    AddChildNodeToFront(new ExprConstantNodeImpl(1));
                }
                else {
                    AddChildNodeToFront(new ExprConstantNodeImpl(0));
                }
            }

            // the row recognition patterns allows "prev(prop, index)", we switch index the first position
            if (ExprNodeUtilityQuery.IsConstant(ChildNodes[1])) {
                var first = ChildNodes[0];
                var second = ChildNodes[1];
                this.ChildNodes = new[] {second, first};
            }

            // Determine if the index is a constant value or an expression to evaluate
            if (ChildNodes[0].Forge.ForgeConstantType.IsCompileTimeConstant) {
                var constantNode = ChildNodes[0];
                var value = constantNode.Forge.ExprEvaluator.Evaluate(null, false, null);
                if (!value.IsNumber()) {
                    throw new ExprValidationException(
                        "Previous function requires an integer index parameter or expression");
                }

                var valueNumber = value;
                if (valueNumber.IsFloatingPointNumber()) {
                    throw new ExprValidationException(
                        "Previous function requires an integer index parameter or expression");
                }

                ConstantIndexNumber = valueNumber.AsInt();
                IsConstantIndex = true;
            }

            // Determine stream number
            if (ChildNodes[1] is ExprIdentNode) {
                var identNode = (ExprIdentNode) ChildNodes[1];
                StreamNumber = identNode.StreamId;
                ResultType = ChildNodes[1].Forge.EvaluationType.GetBoxedType();
            }
            else if (ChildNodes[1] is ExprStreamUnderlyingNode) {
                var streamNode = (ExprStreamUnderlyingNode) ChildNodes[1];
                StreamNumber = streamNode.StreamId;
                ResultType = ChildNodes[1].Forge.EvaluationType.GetBoxedType();
                enumerationMethodType = validationContext.StreamTypeService.EventTypes[streamNode.StreamId];
            }
            else {
                throw new ExprValidationException("Previous function requires an event property as parameter");
            }

            if (PreviousType == ExprPreviousNodePreviousType.PREVCOUNT) {
                ResultType = typeof(long?);
            }

            if (PreviousType == ExprPreviousNodePreviousType.PREVWINDOW) {
                ResultType = TypeHelper.GetArrayType(ResultType);
            }

            if (validationContext.ViewResourceDelegate == null) {
                throw new ExprValidationException("Previous function cannot be used in this context");
            }

            validationContext.ViewResourceDelegate.AddPreviousRequest(this);
            previousStrategyFieldName = validationContext.MemberNames.PreviousStrategy(StreamNumber);
            return null;
        }

        public Type ComponentTypeCollection {
            get {
                if (ResultType.IsArray) {
                    return ResultType.GetElementType();
                }

                return ResultType;
            }
        }

        private CodegenExpression EvaluateCodegenPrevCount(
            Type requiredType,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var method = parent.MakeChild(ResultType, GetType(), codegenClassScope);

            method.Block
                .DeclareVar(typeof(long), "size", Constant(0))
                .DeclareVar(
                    typeof(PreviousGetterStrategy), "strategy",
                    ExprDotMethod(GetterField(codegenClassScope), "getStrategy", exprSymbol.GetAddExprEvalCtx(method)));

            var randomAccess = method.Block.IfCondition(InstanceOf(Ref("strategy"), typeof(RandomAccessByIndexGetter)));
            {
                randomAccess
                    .IfCondition(Not(exprSymbol.GetAddIsNewData(method))).BlockReturn(ConstantNull())
                    .DeclareVar(
                        typeof(RandomAccessByIndexGetter), "getter",
                        Cast(typeof(RandomAccessByIndexGetter), Ref("strategy")))
                    .DeclareVar(
                        typeof(RandomAccessByIndex), "randomAccess", ExprDotMethod(Ref("getter"), "getAccessor"))
                    .AssignRef("size", ExprDotMethod(Ref("randomAccess"), "getWindowCount"));
            }
            var relativeAccess = randomAccess.IfElse();
            {
                relativeAccess
                    .DeclareVar(
                        typeof(RelativeAccessByEventNIndexGetter), "getter",
                        Cast(typeof(RelativeAccessByEventNIndexGetter), Ref("strategy")))
                    .DeclareVar(
                        typeof(EventBean), "evalEvent",
                        ArrayAtIndex(exprSymbol.GetAddEPS(method), Constant(StreamNumber)))
                    .DeclareVar(
                        typeof(RelativeAccessByEventNIndex), "relativeAccess",
                        ExprDotMethod(Ref("getter"), "getAccessor", Ref("evalEvent")))
                    .IfRefNullReturnNull("relativeAccess")
                    .AssignRef("size", ExprDotMethod(Ref("relativeAccess"), "getWindowToEventCount"));
            }

            method.Block.MethodReturn(Ref("size"));
            return LocalMethod(method);
        }

        private CodegenExpression EvaluateCodegenPrevWindow(
            Type requiredType,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var method = parent.MakeChild(ResultType, GetType(), codegenClassScope);

            method.Block
                .DeclareVar(
                    typeof(PreviousGetterStrategy), "strategy",
                    ExprDotMethod(GetterField(codegenClassScope), "getStrategy", exprSymbol.GetAddExprEvalCtx(method)))
                .Apply(new PreviousBlockGetSizeAndIterator(method, exprSymbol, StreamNumber, Ref("strategy")).Accept);

            var eps = exprSymbol.GetAddEPS(method);
            var innerEval = CodegenLegoMethodExpression.CodegenExpression(
                ChildNodes[1].Forge, method, codegenClassScope);

            method.Block.DeclareVar(typeof(EventBean), "originalEvent", ArrayAtIndex(eps, Constant(StreamNumber)))
                .DeclareVar(ResultType, "result", NewArrayByLength(ResultType.GetElementType(), Ref("size")))
                .ForLoopIntSimple("i", Ref("size"))
                .AssignArrayElement(
                    eps, Constant(StreamNumber), Cast(typeof(EventBean), ExprDotMethod(Ref("events"), "next")))
                .AssignArrayElement(
                    "result", Ref("i"),
                    LocalMethod(innerEval, eps, ConstantTrue(), exprSymbol.GetAddExprEvalCtx(method)))
                .BlockEnd()
                .AssignArrayElement(eps, Constant(StreamNumber), Ref("originalEvent"))
                .MethodReturn(Ref("result"));
            return LocalMethod(method);
        }

        private CodegenExpression EvaluateCodegenPrevAndTail(
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var method = parent.MakeChild(ResultType, GetType(), codegenClassScope);

            var eps = exprSymbol.GetAddEPS(method);
            var innerEval = CodegenLegoMethodExpression.CodegenExpression(
                ChildNodes[1].Forge, method, codegenClassScope);

            method.Block
                .IfCondition(Not(exprSymbol.GetAddIsNewData(method))).BlockReturn(ConstantNull())
                .DeclareVar(
                    typeof(EventBean), "substituteEvent",
                    LocalMethod(
                        GetSubstituteCodegen(method, exprSymbol, codegenClassScope), eps,
                        exprSymbol.GetAddExprEvalCtx(method)))
                .IfRefNullReturnNull("substituteEvent")
                .DeclareVar(typeof(EventBean), "originalEvent", ArrayAtIndex(eps, Constant(StreamNumber)))
                .AssignArrayElement(eps, Constant(StreamNumber), Ref("substituteEvent"))
                .DeclareVar(
                    ResultType, "evalResult",
                    LocalMethod(
                        innerEval, eps, exprSymbol.GetAddIsNewData(method), exprSymbol.GetAddExprEvalCtx(method)))
                .AssignArrayElement(eps, Constant(StreamNumber), Ref("originalEvent"))
                .MethodReturn(Ref("evalResult"));

            return LocalMethod(method);
        }

        private CodegenMethod GetSubstituteCodegen(
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var method = parent.MakeChildWithScope(
                    typeof(EventBean), GetType(), CodegenSymbolProviderEmpty.INSTANCE, codegenClassScope)
                .AddParam(typeof(EventBean[]), REF_EPS.Ref).AddParam(
                    typeof(ExprEvaluatorContext), REF_EXPREVALCONTEXT.Ref);

            method.Block.DeclareVar(
                typeof(PreviousGetterStrategy), "strategy",
                ExprDotMethod(GetterField(codegenClassScope), "getStrategy", exprSymbol.GetAddExprEvalCtx(method)));
            if (IsConstantIndex) {
                method.Block.DeclareVar(typeof(int), "index", Constant(ConstantIndexNumber));
            }
            else {
                var index = ChildNodes[0].Forge;
                var indexMethod = CodegenLegoMethodExpression.CodegenExpression(index, method, codegenClassScope);
                CodegenExpression indexCall = LocalMethod(indexMethod, REF_EPS, ConstantTrue(), REF_EXPREVALCONTEXT);
                method.Block
                    .DeclareVar(typeof(object), "indexResult", indexCall)
                    .IfRefNullReturnNull("indexResult")
                    .DeclareVar(
                        typeof(int), "index", ExprDotMethod(Cast(typeof(object), Ref("indexResult")), "intValue"));
            }

            var randomAccess = method.Block.IfCondition(InstanceOf(Ref("strategy"), typeof(RandomAccessByIndexGetter)));
            {
                randomAccess
                    .DeclareVar(
                        typeof(RandomAccessByIndexGetter), "getter",
                        Cast(typeof(RandomAccessByIndexGetter), Ref("strategy")))
                    .DeclareVar(
                        typeof(RandomAccessByIndex), "randomAccess", ExprDotMethod(Ref("getter"), "getAccessor"));
                if (PreviousType == ExprPreviousNodePreviousType.PREV) {
                    randomAccess.BlockReturn(ExprDotMethod(Ref("randomAccess"), "getNewData", Ref("index")));
                }
                else if (PreviousType == ExprPreviousNodePreviousType.PREVTAIL) {
                    randomAccess.BlockReturn(ExprDotMethod(Ref("randomAccess"), "getNewDataTail", Ref("index")));
                }
                else {
                    throw new IllegalStateException("Previous type not recognized: " + PreviousType);
                }
            }
            var relativeAccess = randomAccess.IfElse();
            {
                relativeAccess
                    .DeclareVar(
                        typeof(RelativeAccessByEventNIndexGetter), "getter",
                        Cast(typeof(RelativeAccessByEventNIndexGetter), Ref("strategy")))
                    .DeclareVar(
                        typeof(EventBean), "evalEvent",
                        ArrayAtIndex(exprSymbol.GetAddEPS(method), Constant(StreamNumber)))
                    .DeclareVar(
                        typeof(RelativeAccessByEventNIndex), "relativeAccess",
                        ExprDotMethod(Ref("getter"), "getAccessor", Ref("evalEvent")))
                    .IfRefNullReturnNull("relativeAccess");
                if (PreviousType == ExprPreviousNodePreviousType.PREV) {
                    relativeAccess.BlockReturn(
                        ExprDotMethod(Ref("relativeAccess"), "getRelativeToEvent", Ref("evalEvent"), Ref("index")));
                }
                else if (PreviousType == ExprPreviousNodePreviousType.PREVTAIL) {
                    relativeAccess.BlockReturn(ExprDotMethod(Ref("relativeAccess"), "getRelativeToEnd", Ref("index")));
                }
                else {
                    throw new IllegalStateException("Previous type not recognized: " + PreviousType);
                }
            }
            return method;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write(PreviousType.ToString().ToLowerInvariant());
            writer.Write("(");
            if (PreviousType == ExprPreviousNodePreviousType.PREVCOUNT ||
                PreviousType == ExprPreviousNodePreviousType.PREVWINDOW) {
                ChildNodes[1].ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
            }
            else {
                ChildNodes[0].ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
                if (ChildNodes.Length > 1) {
                    writer.Write(",");
                    ChildNodes[1].ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
                }
            }

            writer.Write(')');
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (node == null || GetType() != node.GetType()) {
                return false;
            }

            var that = (ExprPreviousNode) node;

            if (PreviousType != that.PreviousType) {
                return false;
            }

            return true;
        }

        private CodegenExpression GetterField(CodegenClassScope classScope)
        {
            return classScope.PackageScope.AddOrGetFieldWellKnown(
                previousStrategyFieldName, typeof(PreviousGetterStrategy));
        }

        private class PreviousBlockGetSizeAndIterator
        {
            private readonly ExprForgeCodegenSymbol exprSymbol;
            private readonly CodegenExpression getter;
            private readonly CodegenMethod method;
            private readonly int streamNumber;

            public PreviousBlockGetSizeAndIterator(
                CodegenMethod method,
                ExprForgeCodegenSymbol exprSymbol,
                int streamNumber,
                CodegenExpression getter)
            {
                this.method = method;
                this.exprSymbol = exprSymbol;
                this.streamNumber = streamNumber;
                this.getter = getter;
            }

            public void Accept(CodegenBlock block)
            {
                block
                    .IfCondition(Not(exprSymbol.GetAddIsNewData(method))).BlockReturn(ConstantNull())
                    .DeclareVar(typeof(IEnumerator<EventBean>), typeof(EventBean), "events", ConstantNull())
                    .DeclareVar(typeof(int), "size", Constant(0));

                var randomAccess = method.Block.IfCondition(InstanceOf(getter, typeof(RandomAccessByIndexGetter)));
                {
                    randomAccess
                        .DeclareVar(
                            typeof(RandomAccessByIndexGetter), "getter",
                            Cast(typeof(RandomAccessByIndexGetter), getter))
                        .DeclareVar(
                            typeof(RandomAccessByIndex), "randomAccess", ExprDotMethod(Ref("getter"), "getAccessor"))
                        .AssignRef("events", ExprDotMethod(Ref("randomAccess"), "getWindowIterator"))
                        .AssignRef("size", ExprDotMethod(Ref("randomAccess"), "getWindowCount"));
                }
                var relativeAccess = randomAccess.IfElse();
                {
                    relativeAccess
                        .DeclareVar(
                            typeof(RelativeAccessByEventNIndexGetter), "getter",
                            Cast(typeof(RelativeAccessByEventNIndexGetter), getter))
                        .DeclareVar(
                            typeof(EventBean), "evalEvent",
                            ArrayAtIndex(exprSymbol.GetAddEPS(method), Constant(streamNumber)))
                        .DeclareVar(
                            typeof(RelativeAccessByEventNIndex), "relativeAccess",
                            ExprDotMethod(Ref("getter"), "getAccessor", Ref("evalEvent")))
                        .IfRefNullReturnNull("relativeAccess")
                        .AssignRef("events", ExprDotMethod(Ref("relativeAccess"), "getWindowToEvent"))
                        .AssignRef("size", ExprDotMethod(Ref("relativeAccess"), "getWindowToEventCount"));
                }

                method.Block.IfCondition(Relational(Ref("size"), LE, Constant(0)))
                    .BlockReturn(ConstantNull());
            }
        }
    }
} // end of namespace