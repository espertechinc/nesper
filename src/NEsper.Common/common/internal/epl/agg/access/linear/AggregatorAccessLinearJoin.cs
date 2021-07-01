///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Diagnostics;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.agg.access.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.serde.compiletime.sharable;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.CodegenRelational;
using static com.espertech.esper.common.@internal.epl.agg.method.core.AggregatorCodegenUtil;
using static com.espertech.esper.common.@internal.util.CollectionUtil;

namespace com.espertech.esper.common.@internal.epl.agg.access.linear
{
    /// <summary>
    ///     Implementation of access function for joins.
    /// </summary>
    public class AggregatorAccessLinearJoin : AggregatorAccessWFilterBase,
        AggregatorAccessLinear
    {
        private readonly AggregationStateLinearForge _forge;
        private readonly CodegenExpressionMember _refSet;
        private readonly CodegenExpressionMember _array;

        public AggregatorAccessLinearJoin(
            AggregationStateLinearForge forge,
            int col,
            CodegenCtor rowCtor,
            CodegenMemberCol membersColumnized,
            CodegenClassScope classScope,
            ExprNode optionalFilter)
            : base(optionalFilter)

        {
            _forge = forge;
            _refSet = membersColumnized.AddMember(col, typeof(LinkedHashMap<EventBean, object>), "refSet");
            _array = membersColumnized.AddMember(col, typeof(EventBean[]), "array");
            rowCtor.Block.AssignRef(_refSet, NewInstance(typeof(LinkedHashMap<EventBean, object>)));
        }

        public override void ClearCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(_refSet, "Clear")
                .AssignRef(_array, ConstantNull());
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
            method.Block.ExprDotMethod(GetSerde(classScope), "Write", RowDotMember(row, _refSet), output, unitKey, writer);
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
                RowDotMember(row, _refSet),
                Cast(
                    typeof(LinkedHashMap<EventBean, object>),
                    ExprDotMethod(GetSerde(classScope), "Read", input, unitKey)));
        }

        public CodegenExpression GetFirstNthValueCodegen(
            CodegenExpressionRef index,
            CodegenMethod parentMethod,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            var initArray = InitArrayCodegen(namedMethods, classScope);
            var method = parentMethod.MakeChildWithScope(
                    typeof(EventBean),
                    typeof(AggregatorAccessLinearJoin),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(typeof(int), "index");
            method.Block.IfCondition(Relational(Ref("index"), LT, Constant(0)))
                .BlockReturn(ConstantNull())
                .IfCondition(ExprDotMethod(_refSet, "IsEmpty"))
                .BlockReturn(ConstantNull())
                .IfCondition(Relational(Ref("index"), GE, ExprDotName(_refSet, "Count")))
                .BlockReturn(ConstantNull())
                .IfCondition(EqualsNull(_array))
                .LocalMethod(initArray)
                .BlockEnd()
                .MethodReturn(ArrayAtIndex(_array, Ref("index")));
            return LocalMethod(method, index);
        }

        public CodegenExpression GetLastNthValueCodegen(
            CodegenExpressionRef index,
            CodegenMethod parentMethod,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            var initArray = InitArrayCodegen(namedMethods, classScope);
            var method = parentMethod.MakeChildWithScope(
                    typeof(EventBean),
                    typeof(AggregatorAccessLinearJoin),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(typeof(int), "index");
            method.Block.IfCondition(Relational(Ref("index"), LT, Constant(0)))
                .BlockReturn(ConstantNull())
                .IfCondition(ExprDotMethod(_refSet, "IsEmpty"))
                .BlockReturn(ConstantNull())
                .IfCondition(Relational(Ref("index"), GE, ExprDotName(_refSet, "Count")))
                .BlockReturn(ConstantNull())
                .IfCondition(EqualsNull(_array))
                .LocalMethod(initArray)
                .BlockEnd()
                .MethodReturn(ArrayAtIndex(_array, Op(Op(ArrayLength(_array), "-", Ref("index")), "-", Constant(1))));
            return LocalMethod(method, index);
        }

        public CodegenExpression GetFirstValueCodegen(
            CodegenClassScope classScope,
            CodegenMethod parentMethod)
        {
            var method = parentMethod.MakeChildWithScope(
                typeof(EventBean),
                typeof(AggregatorAccessLinearJoin),
                CodegenSymbolProviderEmpty.INSTANCE,
                classScope);
            method.Block.IfCondition(ExprDotMethod(_refSet, "IsEmpty"))
                .BlockReturn(ConstantNull())
                .DeclareVar<KeyValuePair<EventBean, object>>(
                    "entry",
                    Cast(
                        typeof(KeyValuePair<EventBean, object>),
                        ExprDotMethodChain(_refSet).Add("First")))
                .MethodReturn(Cast(typeof(EventBean), ExprDotName(Ref("entry"), "Key")));
            return LocalMethod(method);
        }

        public CodegenExpression GetLastValueCodegen(
            CodegenClassScope classScope,
            CodegenMethod parentMethod,
            CodegenNamedMethods namedMethods)
        {
            var initArray = InitArrayCodegen(namedMethods, classScope);
            var method = parentMethod.MakeChildWithScope(
                typeof(EventBean),
                typeof(AggregatorAccessLinearJoin),
                CodegenSymbolProviderEmpty.INSTANCE,
                classScope);
            method.Block.IfCondition(ExprDotMethod(_refSet, "IsEmpty"))
                .BlockReturn(ConstantNull())
                .IfCondition(EqualsNull(_array))
                .LocalMethod(initArray)
                .BlockEnd()
                .MethodReturn(ArrayAtIndex(_array, Op(ArrayLength(_array), "-", Constant(1))));
            return LocalMethod(method);
        }

        public CodegenExpression EnumeratorCodegen(
            CodegenClassScope classScope,
            CodegenMethod parentMethod,
            CodegenNamedMethods namedMethods)
        {
            var initArray = InitArrayCodegen(namedMethods, classScope);
            var method = parentMethod.MakeChildWithScope(
                typeof(IEnumerator<EventBean>),
                typeof(AggregatorAccessLinearJoin),
                CodegenSymbolProviderEmpty.INSTANCE,
                classScope);
            method.Block.IfNull(_array)
                .LocalMethod(initArray)
                .BlockEnd()
                .MethodReturn(
                    ExprDotMethod(
                        StaticMethod(typeof(Arrays), "Enumerate", _array),
                        "GetEnumerator"));
            return LocalMethod(method);
        }

        public CodegenExpression CollectionReadOnlyCodegen(
            CodegenMethod parentMethod,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            Trace.Assert(
                parentMethod.ReturnType == typeof(ICollection<EventBean>),
                "parentMethod.ReturnType != typeof(ICollection<EventBean>)");

            var initArray = InitArrayCodegen(namedMethods, classScope);
            var method = parentMethod.MakeChildWithScope(
                typeof(ICollection<EventBean>),
                typeof(AggregatorAccessLinearJoin),
                CodegenSymbolProviderEmpty.INSTANCE,
                classScope);
            method.Block.IfNull(_array)
                .LocalMethod(initArray)
                .BlockEnd()
                .MethodReturn(StaticMethod(typeof(CompatExtensions), "AsList", _array));
            return LocalMethod(method);
        }

        public CodegenExpression SizeCodegen()
        {
            return ExprDotName(_refSet, "Count");
        }

        internal override void ApplyEnterFiltered(
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            CodegenExpression eps = symbols.GetAddEPS(method);
            var addEvent = AddEventCodegen(method, classScope);
            method.Block.DeclareVar<EventBean>("theEvent", ArrayAtIndex(eps, Constant(_forge.StreamNum)))
                .IfRefNull("theEvent")
                .BlockReturnNoValue()
                .LocalMethod(addEvent, Ref("theEvent"));
        }

        internal override void ApplyLeaveFiltered(
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            CodegenExpression eps = symbols.GetAddEPS(method);
            var removeEvent = RemoveEventCodegen(method, classScope);
            method.Block.DeclareVar<EventBean>("theEvent", ArrayAtIndex(eps, Constant(_forge.StreamNum)))
                .IfRefNull("theEvent")
                .BlockReturnNoValue()
                .LocalMethod(removeEvent, Ref("theEvent"));
        }

        private CodegenMethod AddEventCodegen(
            CodegenMethod parent,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChildWithScope(
                    typeof(void),
                    typeof(AggregatorAccessLinearJoin),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(typeof(EventBean), "theEvent");
            method.Block.AssignRef(_array, ConstantNull())
                .DeclareVar<int?>("value", ExprDotMethod(ExprDotMethod(_refSet, "Get", Ref("theEvent")), "AsBoxedInt32"))
                .IfRefNull("value")
                .ExprDotMethod(_refSet, "Put", Ref("theEvent"), Constant(1))
                .BlockReturnNoValue()
                .IncrementRef("value")
                .ExprDotMethod(_refSet, "Put", Ref("theEvent"), Ref("value"));
            return method;
        }

        private CodegenMethod RemoveEventCodegen(
            CodegenMethod parent,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChildWithScope(
                    typeof(void),
                    typeof(AggregatorAccessLinearJoin),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(typeof(EventBean), "theEvent");
            method.Block.AssignRef(_array, ConstantNull())
                .DeclareVar<int?>("value", ExprDotMethod(ExprDotMethod(_refSet, "Get", Ref("theEvent")), "AsBoxedInt32"))
                .IfRefNull("value")
                .BlockReturnNoValue()
                .IfCondition(EqualsIdentity(Ref("value"), Constant(1)))
                .ExprDotMethod(_refSet, "Remove", Ref("theEvent"))
                .BlockReturnNoValue()
                .DecrementRef("value")
                .ExprDotMethod(_refSet, "Put", Ref("theEvent"), Ref("value"));
            return method;
        }

        private CodegenMethod InitArrayCodegen(
            CodegenNamedMethods namedMethods,
            CodegenClassScope classScope)
        {
            Consumer<CodegenMethod> code = method => {
                method.Block
                    .DeclareVar<ICollection<EventBean>>("events", ExprDotName(_refSet, "Keys"))
                    .AssignRef(_array, StaticMethod(typeof(CollectionUtil), METHOD_TOARRAYEVENTS, Ref("events")));
            };
            return namedMethods.AddMethod(
                typeof(void),
                "InitArray_" + _array.Ref,
                Collections.GetEmptyList<CodegenNamedParam>(),
                typeof(AggregatorAccessLinearJoin),
                classScope,
                code);
        }

        private CodegenExpressionInstanceField GetSerde(CodegenClassScope classScope)
        {
            return classScope.AddOrGetDefaultFieldSharable(
                new CodegenSharableSerdeEventTyped(
                    CodegenSharableSerdeEventTyped.CodegenSharableSerdeName.LINKEDHASHMAPEVENTSANDINT,
                    _forge.EventType));
        }
    }
} // end of namespace