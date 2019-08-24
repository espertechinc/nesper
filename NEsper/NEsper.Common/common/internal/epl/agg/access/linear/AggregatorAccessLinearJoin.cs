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
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.
    CodegenRelational;
using static com.espertech.esper.common.@internal.epl.agg.method.core.AggregatorCodegenUtil;
using static com.espertech.esper.common.@internal.serde.CodegenSharableSerdeEventTyped.CodegenSharableSerdeName;
using static com.espertech.esper.common.@internal.util.CollectionUtil;

namespace com.espertech.esper.common.@internal.epl.agg.access.linear
{
    /// <summary>
    ///     Implementation of access function for joins.
    /// </summary>
    public class AggregatorAccessLinearJoin : AggregatorAccessWFilterBase,
        AggregatorAccessLinear
    {
        private readonly CodegenExpressionRef array;

        private readonly AggregationStateLinearForge forge;
        private readonly CodegenExpressionRef refSet;

        public AggregatorAccessLinearJoin(
            AggregationStateLinearForge forge,
            int col,
            CodegenCtor rowCtor,
            CodegenMemberCol membersColumnized,
            CodegenClassScope classScope,
            ExprNode optionalFilter)
            : base(optionalFilter)

        {
            this.forge = forge;
            refSet = membersColumnized.AddMember(col, typeof(LinkedHashMap<string, object>), "refSet");
            array = membersColumnized.AddMember(col, typeof(EventBean[]), "array");
            rowCtor.Block.AssignRef(refSet, NewInstance(typeof(LinkedHashMap<string, object>)));
        }

        public override void ClearCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(refSet, "Clear")
                .AssignRef(array, ConstantNull());
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
            method.Block.ExprDotMethod(GetSerde(classScope), "Write", RowDotRef(row, refSet), output, unitKey, writer);
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
                RowDotRef(row, refSet),
                Cast(
                    typeof(LinkedHashMap<string, object>),
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
                .IfCondition(ExprDotMethod(refSet, "IsEmpty"))
                .BlockReturn(ConstantNull())
                .IfCondition(Relational(Ref("index"), GE, ExprDotName(refSet, "Count")))
                .BlockReturn(ConstantNull())
                .IfCondition(EqualsNull(array))
                .InstanceMethod(initArray)
                .BlockEnd()
                .MethodReturn(ArrayAtIndex(array, Ref("index")));
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
                .IfCondition(ExprDotMethod(refSet, "IsEmpty"))
                .BlockReturn(ConstantNull())
                .IfCondition(Relational(Ref("index"), GE, ExprDotName(refSet, "Count")))
                .BlockReturn(ConstantNull())
                .IfCondition(EqualsNull(array))
                .InstanceMethod(initArray)
                .BlockEnd()
                .MethodReturn(ArrayAtIndex(array, Op(Op(ArrayLength(array), "-", Ref("index")), "-", Constant(1))));
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
            method.Block.IfCondition(ExprDotMethod(refSet, "IsEmpty"))
                .BlockReturn(ConstantNull())
                .DeclareVar<KeyValuePair<string, object>>(
                    "entry",
                    Cast(
                        typeof(KeyValuePair<string, object>),
                        ExprDotMethodChain(refSet).Add("First")))
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
            method.Block.IfCondition(ExprDotMethod(refSet, "IsEmpty"))
                .BlockReturn(ConstantNull())
                .IfCondition(EqualsNull(array))
                .InstanceMethod(initArray)
                .BlockEnd()
                .MethodReturn(ArrayAtIndex(array, Op(ArrayLength(array), "-", Constant(1))));
            return LocalMethod(method);
        }

        public CodegenExpression IteratorCodegen(
            CodegenClassScope classScope,
            CodegenMethod parentMethod,
            CodegenNamedMethods namedMethods)
        {
            var initArray = InitArrayCodegen(namedMethods, classScope);
            var method = parentMethod.MakeChildWithScope(
                typeof(IEnumerator),
                typeof(AggregatorAccessLinearJoin),
                CodegenSymbolProviderEmpty.INSTANCE,
                classScope);
            method.Block.IfRefNull(array)
                .InstanceMethod(initArray)
                .BlockEnd()
                .MethodReturn(
                    ExprDotMethod(
                        StaticMethod(typeof(Arrays), "Enumerate", array),
                        "GetEnumerator"));
            return LocalMethod(method);
        }

        public CodegenExpression CollectionReadOnlyCodegen(
            CodegenMethod parentMethod,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            var initArray = InitArrayCodegen(namedMethods, classScope);
            var method = parentMethod.MakeChildWithScope(
                typeof(ICollection<object>),
                typeof(AggregatorAccessLinearJoin),
                CodegenSymbolProviderEmpty.INSTANCE,
                classScope);
            method.Block.IfRefNull(array)
                .InstanceMethod(initArray)
                .BlockEnd()
                .MethodReturn(StaticMethod(typeof(CompatExtensions), "AsList", array));
            return LocalMethod(method);
        }

        public CodegenExpression SizeCodegen()
        {
            return ExprDotName(refSet, "Count");
        }

        internal override void ApplyEnterFiltered(
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            CodegenExpression eps = symbols.GetAddEPS(method);
            var addEvent = AddEventCodegen(method, classScope);
            method.Block.DeclareVar<EventBean>("theEvent", ArrayAtIndex(eps, Constant(forge.StreamNum)))
                .IfRefNull("theEvent")
                .BlockReturnNoValue()
                .InstanceMethod(addEvent, Ref("theEvent"));
        }

        internal override void ApplyLeaveFiltered(
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            CodegenExpression eps = symbols.GetAddEPS(method);
            var removeEvent = RemoveEventCodegen(method, classScope);
            method.Block.DeclareVar<EventBean>("theEvent", ArrayAtIndex(eps, Constant(forge.StreamNum)))
                .IfRefNull("theEvent")
                .BlockReturnNoValue()
                .InstanceMethod(removeEvent, Ref("theEvent"));
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
            method.Block.AssignRef(array, ConstantNull())
                .DeclareVar<int>("value", Cast(typeof(int), ExprDotMethod(refSet, "Get", Ref("theEvent"))))
                .IfRefNull("value")
                .ExprDotMethod(refSet, "Put", Ref("theEvent"), Constant(1))
                .BlockReturnNoValue()
                .Increment("value")
                .ExprDotMethod(refSet, "Put", Ref("theEvent"), Ref("value"));
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
            method.Block.AssignRef(array, ConstantNull())
                .DeclareVar<int>("value", Cast(typeof(int), ExprDotMethod(refSet, "Get", Ref("theEvent"))))
                .IfRefNull("value")
                .BlockReturnNoValue()
                .IfCondition(EqualsIdentity(Ref("value"), Constant(1)))
                .ExprDotMethod(refSet, "Remove", Ref("theEvent"))
                .BlockReturnNoValue()
                .Decrement("value")
                .ExprDotMethod(refSet, "Put", Ref("theEvent"), Ref("value"));
            return method;
        }

        private CodegenMethod InitArrayCodegen(
            CodegenNamedMethods namedMethods,
            CodegenClassScope classScope)
        {
            Consumer<CodegenMethod> code = method => {
                method.Block
                    .DeclareVar<ISet<EventBean>>("events", ExprDotName(refSet, "Keys"))
                    .AssignRef(array, StaticMethod(typeof(CollectionUtil), METHOD_TOARRAYEVENTS, Ref("events")));
            };
            return namedMethods.AddMethod(
                typeof(void),
                "initArray_" + array.Ref,
                Collections.GetEmptyList<CodegenNamedParam>(),
                typeof(AggregatorAccessLinearJoin),
                classScope,
                code);
        }

        private CodegenExpressionField GetSerde(CodegenClassScope classScope)
        {
            return classScope.AddOrGetFieldSharable(
                new CodegenSharableSerdeEventTyped(LINKEDMAPEVENTSANDINT, forge.EventType));
        }
    }
} // end of namespace