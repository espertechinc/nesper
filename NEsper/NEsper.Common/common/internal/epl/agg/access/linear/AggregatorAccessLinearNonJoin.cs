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
        private readonly AggregationStateLinearForge _forge;
        private readonly CodegenExpressionMember _events;

        public AggregatorAccessLinearNonJoin(
            AggregationStateLinearForge forge,
            int col,
            CodegenCtor rowCtor,
            CodegenMemberCol membersColumnized,
            CodegenClassScope classScope,
            ExprNode optionalFilter)
            : base(optionalFilter)

        {
            _forge = forge;
            _events = membersColumnized.AddMember(col, typeof(IList<EventBean>), "events");
            rowCtor.Block.AssignRef(_events, NewInstance(typeof(List<EventBean>)));
        }

        internal override void ApplyEnterFiltered(
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            CodegenExpressionRef eps = symbols.GetAddEPS(method);
            method.Block
                .DeclareVar<EventBean>("theEvent", ArrayAtIndex(eps, Constant(_forge.StreamNum)))
                .IfRefNull("theEvent")
                .BlockReturnNoValue()
                .ExprDotMethod(_events, "Add", Ref("theEvent"));
        }

        internal override void ApplyLeaveFiltered(
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            CodegenExpressionRef eps = symbols.GetAddEPS(method);
            method.Block
                .DeclareVar<EventBean>("theEvent", ArrayAtIndex(eps, Constant(_forge.StreamNum)))
                .IfRefNull("theEvent")
                .BlockReturnNoValue()
                .ExprDotMethod(_events, "Remove", Ref("theEvent"));
        }

        public override void ClearCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(_events, "Clear");
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
                .IfCondition(Relational(Ref("index"), GE, ExprDotName(_events, "Count")))
                .BlockReturn(ConstantNull())
                .MethodReturn(
                    Cast(typeof(EventBean),
                    ArrayAtIndex(_events, Ref("index"))));
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
                .IfCondition(Relational(Ref("index"), GE, ExprDotName(_events, "Count")))
                .BlockReturn(ConstantNull())
                .MethodReturn(
                    Cast(
                        typeof(EventBean),
                        ArrayAtIndex(
                            _events,
                            Op(Op(ExprDotName(_events, "Count"), "-", Ref("index")), "-", Constant(1)))));
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
            method.Block.IfCondition(ExprDotMethod(_events, "IsEmpty"))
                .BlockReturn(ConstantNull())
                .MethodReturn(Cast(typeof(EventBean), ArrayAtIndex(_events, Constant(0))));
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
            method.Block.IfCondition(ExprDotMethod(_events, "IsEmpty"))
                .BlockReturn(ConstantNull())
                .MethodReturn(
                    Cast(
                        typeof(EventBean),
                        ArrayAtIndex(_events, Op(ExprDotName(_events, "Count"), "-", Constant(1)))));
            return LocalMethod(method);
        }

        public CodegenExpression EnumeratorCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenNamedMethods namedMethods)
        {
            return ExprDotMethod(_events, "GetEnumerator");
        }

        public CodegenExpression CollectionReadOnlyCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            return _events;
        }

        public CodegenExpression SizeCodegen()
        {
            return ExprDotName(_events, "Count");
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
            method.Block.ExprDotMethod(GetSerde(classScope), "Write", RowDotMember(row, _events), output, unitKey, writer);
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
                RowDotMember(row, _events),
                Cast(typeof(IList<EventBean>), ExprDotMethod(GetSerde(classScope), "Read", input, unitKey)));
        }

        private CodegenExpressionInstanceField GetSerde(CodegenClassScope classScope)
        {
            return classScope.AddOrGetDefaultFieldSharable(new CodegenSharableSerdeEventTyped(LISTEVENTS, _forge.EventType));
        }
    }
} // end of namespace