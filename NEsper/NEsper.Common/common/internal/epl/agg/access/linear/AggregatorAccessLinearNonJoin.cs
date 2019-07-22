///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.agg.access.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.serde;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.
    CodegenRelational;
using static com.espertech.esper.common.@internal.epl.agg.method.core.AggregatorCodegenUtil;
using static com.espertech.esper.common.@internal.serde.CodegenSharableSerdeEventTyped.CodegenSharableSerdeName;

namespace com.espertech.esper.common.@internal.epl.agg.access.linear
{
    /// <summary>
    ///     Implementation of access function for single-stream (not joins).
    /// </summary>
    public class AggregatorAccessLinearNonJoin : AggregatorAccessWFilterBase,
        AggregatorAccessLinear
    {
        private readonly CodegenExpressionRef events;
        private readonly AggregationStateLinearForge forge;

        public AggregatorAccessLinearNonJoin(
            AggregationStateLinearForge forge,
            int col,
            CodegenCtor rowCtor,
            CodegenMemberCol membersColumnized,
            CodegenClassScope classScope,
            ExprNode optionalFilter)
            : base(optionalFilter)

        {
            this.forge = forge;
            events = membersColumnized.AddMember(col, typeof(IList<EventBean>), "events");
            rowCtor.Block.AssignRef(events, NewInstance(typeof(List<EventBean>)));
        }

        internal override void ApplyEnterFiltered(
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            CodegenExpressionRef eps = symbols.GetAddEPS(method);
            method.Block
                .DeclareVar<EventBean>("theEvent", ArrayAtIndex(eps, Constant(forge.StreamNum)))
                .IfRefNull("theEvent")
                .BlockReturnNoValue()
                .ExprDotMethod(events, "add", Ref("theEvent"));
        }

        internal override void ApplyLeaveFiltered(
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            CodegenExpressionRef eps = symbols.GetAddEPS(method);
            method.Block
                .DeclareVar<EventBean>("theEvent", ArrayAtIndex(eps, Constant(forge.StreamNum)))
                .IfRefNull("theEvent")
                .BlockReturnNoValue()
                .ExprDotMethod(events, "remove", Ref("theEvent"));
        }

        public override void ClearCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(events, "clear");
        }

        public CodegenExpression GetFirstNthValueCodegen(
            CodegenExpressionRef index,
            CodegenMethod parentMethod,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            CodegenMethod method = parentMethod.MakeChildWithScope(
                    typeof(EventBean),
                    typeof(AggregatorAccessLinearNonJoin),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(typeof(int), "index");
            method.Block.IfCondition(Relational(Ref("index"), LT, Constant(0)))
                .BlockReturn(ConstantNull())
                .IfCondition(Relational(Ref("index"), GE, ExprDotMethod(events, "size")))
                .BlockReturn(ConstantNull())
                .MethodReturn(Cast(typeof(EventBean), ExprDotMethod(events, "get", Ref("index"))));
            return LocalMethod(method, index);
        }

        public CodegenExpression GetLastNthValueCodegen(
            CodegenExpressionRef index,
            CodegenMethod parentMethod,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            CodegenMethod method = parentMethod.MakeChildWithScope(
                    typeof(EventBean),
                    typeof(AggregatorAccessLinearNonJoin),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(typeof(int), "index");
            method.Block.IfCondition(Relational(Ref("index"), LT, Constant(0)))
                .BlockReturn(ConstantNull())
                .IfCondition(Relational(Ref("index"), GE, ExprDotMethod(events, "size")))
                .BlockReturn(ConstantNull())
                .MethodReturn(
                    Cast(
                        typeof(EventBean),
                        ExprDotMethod(
                            events,
                            "get",
                            Op(Op(ExprDotMethod(events, "size"), "-", Ref("index")), "-", Constant(1)))));
            return LocalMethod(method, index);
        }

        public CodegenExpression GetFirstValueCodegen(
            CodegenClassScope classScope,
            CodegenMethod parentMethod)
        {
            CodegenMethod method = parentMethod.MakeChildWithScope(
                typeof(EventBean),
                typeof(AggregatorAccessLinearNonJoin),
                CodegenSymbolProviderEmpty.INSTANCE,
                classScope);
            method.Block.IfCondition(ExprDotMethod(events, "isEmpty"))
                .BlockReturn(ConstantNull())
                .MethodReturn(Cast(typeof(EventBean), ExprDotMethod(events, "get", Constant(0))));
            return LocalMethod(method);
        }

        public CodegenExpression GetLastValueCodegen(
            CodegenClassScope classScope,
            CodegenMethod parentMethod,
            CodegenNamedMethods namedMethods)
        {
            CodegenMethod method = parentMethod.MakeChildWithScope(
                typeof(EventBean),
                typeof(AggregatorAccessLinearNonJoin),
                CodegenSymbolProviderEmpty.INSTANCE,
                classScope);
            method.Block.IfCondition(ExprDotMethod(events, "isEmpty"))
                .BlockReturn(ConstantNull())
                .MethodReturn(
                    Cast(
                        typeof(EventBean),
                        ExprDotMethod(events, "get", Op(ExprDotMethod(events, "size"), "-", Constant(1)))));
            return LocalMethod(method);
        }

        public CodegenExpression IteratorCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenNamedMethods namedMethods)
        {
            return ExprDotMethod(events, "iterator");
        }

        public CodegenExpression CollectionReadOnlyCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            return events;
        }

        public CodegenExpression SizeCodegen()
        {
            return ExprDotMethod(events, "size");
        }

        public override void WriteCodegen(
            CodegenExpressionRef row,
            int col,
            CodegenExpressionRef output,
            CodegenExpressionRef unitKey,
            CodegenExpressionRef writer,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(GetSerde(classScope), "Write", RowDotRef(row, events), output, unitKey, writer);
        }

        public override void ReadCodegen(
            CodegenExpressionRef row,
            int col,
            CodegenExpressionRef input,
            CodegenMethod method,
            CodegenExpressionRef unitKey,
            CodegenClassScope classScope)
        {
            method.Block.AssignRef(
                RowDotRef(row, events),
                Cast(typeof(IList<object>), ExprDotMethod(GetSerde(classScope), "Read", input, unitKey)));
        }

        private CodegenExpressionField GetSerde(CodegenClassScope classScope)
        {
            return classScope.AddOrGetFieldSharable(new CodegenSharableSerdeEventTyped(LISTEVENTS, forge.EventType));
        }
    }
} // end of namespace