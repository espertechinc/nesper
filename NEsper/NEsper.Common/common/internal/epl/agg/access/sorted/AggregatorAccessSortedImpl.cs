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
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.agg.access.core;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.serde;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.util.CodegenFieldSharableComparator.CodegenSharableSerdeName;
using static com.espertech.esper.common.@internal.epl.agg.method.core.AggregatorCodegenUtil;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;
using static com.espertech.esper.common.@internal.serde.CodegenSharableSerdeEventTyped.CodegenSharableSerdeName;

namespace com.espertech.esper.common.@internal.epl.agg.access.sorted
{
    public class AggregatorAccessSortedImpl : AggregatorAccessWFilterBase,
        AggregatorAccessSorted
    {
        internal readonly CodegenExpressionField comparator;

        internal readonly AggregationStateSortedForge forge;
        internal readonly CodegenExpressionRef joinRefs;
        internal readonly CodegenExpressionField joinRefsSerde;
        internal readonly CodegenExpressionRef size;
        internal readonly CodegenExpressionRef sorted;
        internal readonly CodegenExpressionField sortedSerde;

        public AggregatorAccessSortedImpl(
            bool join, AggregationStateSortedForge forge, int col, CodegenCtor ctor, CodegenMemberCol membersColumnized,
            CodegenClassScope classScope, ExprNode optionalFilter)
            : base(optionalFilter)

        {
            this.forge = forge;
            sorted = membersColumnized.AddMember(col, typeof(OrderedDictionary<,>), "sorted");
            size = membersColumnized.AddMember(col, typeof(int), "size");
            Type[] types = ExprNodeUtilityQuery.GetExprResultTypes(forge.Spec.Criteria);
            comparator = classScope.AddOrGetFieldSharable(
                new CodegenFieldSharableComparator(
                    COMPARATORHASHABLEMULTIKEYS, types, forge.Spec.IsSortUsingCollator, forge.Spec.SortDescending));
            ctor.Block.AssignRef(sorted, NewInstance(typeof(OrderedDictionary<object, object>), comparator));

            sortedSerde = classScope.AddOrGetFieldSharable(
                new ProxyCodegenFieldSharable() {
                    ProcType = () => { return typeof(DIOSerdeTreeMapEventsMayDeque); },
                    ProcInitCtorScoped = () => {
                        CodegenExpression type = EventTypeUtility.ResolveTypeCodegen(
                            forge.Spec.StreamEventType, EPStatementInitServicesConstants.REF);
                        return ExprDotMethodChain(EPStatementInitServicesConstants.REF)
                            .Add(EPStatementInitServicesConstants.GETDATAINPUTOUTPUTSERDEPROVIDER).Add(
                                "treeMapEventsMayDeque", Constant(forge.Spec.CriteriaTypes), type);
                    }
                });

            if (join) {
                joinRefs = membersColumnized.AddMember(col, typeof(RefCountedSetAtomicInteger<object>), "refs");
                ctor.Block.AssignRef(joinRefs, NewInstance(typeof(RefCountedSetAtomicInteger<object>)));
                joinRefsSerde = classScope.AddOrGetFieldSharable(
                    new CodegenSharableSerdeEventTyped(REFCOUNTEDSETATOMICINTEGER, forge.Spec.StreamEventType));
            }
            else {
                joinRefs = null;
                joinRefsSerde = null;
            }
        }

        public CodegenExpression ReverseIteratorCodegen => NewInstance(
            typeof(AggregationStateSortedIterator), sorted, ConstantTrue());

        public override void ClearCodegen(CodegenMethod method, CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(sorted, "clear")
                .AssignRef(size, Constant(0));
            if (joinRefs != null) {
                method.Block.ExprDotMethod(joinRefs, "clear");
            }
        }

        public CodegenExpression GetFirstValueCodegen(CodegenClassScope classScope, CodegenMethod parent)
        {
            var method = parent.MakeChildWithScope(
                typeof(EventBean), GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope);
            method.Block.IfCondition(ExprDotMethod(sorted, "isEmpty"))
                .BlockReturn(ConstantNull())
                .DeclareVar(typeof(KeyValuePair<object,object>), "max", ExprDotMethod(sorted, "firstEntry"))
                .MethodReturn(
                    StaticMethod(
                        typeof(AggregatorAccessSortedImpl), "checkedPayloadMayDeque",
                        ExprDotMethod(Ref("max"), "getValue")));
            return LocalMethod(method);
        }

        public CodegenExpression GetLastValueCodegen(CodegenClassScope classScope, CodegenMethod parent)
        {
            var method = parent.MakeChildWithScope(
                typeof(EventBean), GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope);
            method.Block.IfCondition(ExprDotMethod(sorted, "isEmpty"))
                .BlockReturn(ConstantNull())
                .DeclareVar(typeof(KeyValuePair<object, object>), "min", ExprDotMethod(sorted, "lastEntry"))
                .MethodReturn(
                    StaticMethod(
                        typeof(AggregatorAccessSortedImpl), "checkedPayloadMayDeque",
                        ExprDotMethod(Ref("min"), "getValue")));
            return LocalMethod(method);
        }

        public CodegenExpression IteratorCodegen()
        {
            return NewInstance(typeof(AggregationStateSortedIterator), sorted, ConstantFalse());
        }

        public CodegenExpression CollectionReadOnlyCodegen()
        {
            return NewInstance(typeof(AggregationStateSortedWrappingCollection), sorted, size);
        }

        public CodegenExpression SizeCodegen()
        {
            return size;
        }

        public override void WriteCodegen(
            CodegenExpressionRef row, int col, CodegenExpressionRef output, CodegenExpressionRef unitKey,
            CodegenExpressionRef writer, CodegenMethod method, CodegenClassScope classScope)
        {
            method.Block
                .Apply(WriteInt(output, row, size))
                .ExprDotMethod(sortedSerde, "write", RowDotRef(row, sorted), output, unitKey, writer);
            if (joinRefs != null) {
                method.Block.ExprDotMethod(joinRefsSerde, "write", RowDotRef(row, joinRefs), output, unitKey, writer);
            }
        }

        public override void ReadCodegen(
            CodegenExpressionRef row, int col, CodegenExpressionRef input, CodegenMethod method,
            CodegenExpressionRef unitKey, CodegenClassScope classScope)
        {
            method.Block
                .Apply(ReadInt(row, size, input))
                .AssignRef(RowDotRef(row, sorted), NewInstance(typeof(OrderedDictionary<object, object>), comparator))
                .ExprDotMethod(sortedSerde, "read", RowDotRef(row, sorted), input, unitKey);
            if (joinRefs != null) {
                method.Block.AssignRef(
                    RowDotRef(row, joinRefs),
                    Cast(typeof(RefCountedSetAtomicInteger<object>), ExprDotMethod(joinRefsSerde, "read", input, unitKey)));
            }
        }

        internal override void ApplyEnterFiltered(
            CodegenMethod method, ExprForgeCodegenSymbol symbols, CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            CodegenExpressionRef eps = symbols.GetAddEPS(method);
            CodegenExpressionRef ctx = symbols.GetAddExprEvalCtx(method);
            var referenceAddToColl = ReferenceAddToCollCodegen(method, namedMethods, classScope);
            method.Block.DeclareVar(typeof(EventBean), "theEvent", ArrayAtIndex(eps, Constant(forge.Spec.StreamNum)))
                .IfRefNull("theEvent").BlockReturnNoValue();

            if (joinRefs == null) {
                method.Block.LocalMethod(referenceAddToColl, Ref("theEvent"), eps, ctx);
            }
            else {
                method.Block.IfCondition(ExprDotMethod(joinRefs, "add", Ref("theEvent")))
                    .LocalMethod(referenceAddToColl, Ref("theEvent"), eps, ctx);
            }
        }

        internal override void ApplyLeaveFiltered(
            CodegenMethod method, ExprForgeCodegenSymbol symbols, CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            CodegenExpressionRef eps = symbols.GetAddEPS(method);
            CodegenExpressionRef ctx = symbols.GetAddExprEvalCtx(method);
            var dereferenceRemove = DereferenceRemoveFromCollCodegen(method, namedMethods, classScope);
            method.Block.DeclareVar(typeof(EventBean), "theEvent", ArrayAtIndex(eps, Constant(forge.Spec.StreamNum)))
                .IfRefNull("theEvent").BlockReturnNoValue();

            if (joinRefs == null) {
                method.Block.LocalMethod(dereferenceRemove, Ref("theEvent"), eps, ctx);
            }
            else {
                method.Block.IfCondition(ExprDotMethod(joinRefs, "remove", Ref("theEvent")))
                    .LocalMethod(dereferenceRemove, Ref("theEvent"), eps, ctx);
            }
        }

        private static CodegenMethod GetComparableWMultiKeyCodegen(
            ExprNode[] criteria, CodegenExpressionRef @ref, CodegenNamedMethods namedMethods,
            CodegenClassScope classScope)
        {
            var methodName = "getComparable_" + @ref.Ref;
            Consumer<CodegenMethod> code = method => {
                if (criteria.Length == 1) {
                    method.Block.MethodReturn(
                        LocalMethod(
                            CodegenLegoMethodExpression.CodegenExpression(criteria[0].Forge, method, classScope),
                            REF_EPS, REF_ISNEWDATA, REF_EXPREVALCONTEXT));
                }
                else {
                    var exprSymbol = new ExprForgeCodegenSymbol(true, null);
                    var expressions = new CodegenExpression[criteria.Length];
                    for (var i = 0; i < criteria.Length; i++) {
                        expressions[i] = criteria[i].Forge.EvaluateCodegen(
                            typeof(object), method, exprSymbol, classScope);
                    }

                    exprSymbol.DerivedSymbolsCodegen(method, method.Block, classScope);

                    method.Block.DeclareVar(
                        typeof(object[]), "result", NewArrayByLength(typeof(object), Constant(criteria.Length)));
                    for (var i = 0; i < criteria.Length; i++) {
                        method.Block.AssignArrayElement(Ref("result"), Constant(i), expressions[i]);
                    }

                    method.Block.MethodReturn(NewInstance(typeof(HashableMultiKey), Ref("result")));
                }
            };
            return namedMethods.AddMethod(
                typeof(object), methodName,
                CodegenNamedParam.From(
                    typeof(EventBean[]), NAME_EPS, typeof(bool), NAME_ISNEWDATA, typeof(ExprEvaluatorContext),
                    NAME_EXPREVALCONTEXT), typeof(AggregatorAccessSortedImpl), classScope, code);
        }

        private CodegenMethod ReferenceAddToCollCodegen(
            CodegenMethod parent, CodegenNamedMethods namedMethods, CodegenClassScope classScope)
        {
            var getComparable = GetComparableWMultiKeyCodegen(forge.Spec.Criteria, sorted, namedMethods, classScope);

            var method = parent
                .MakeChildWithScope(typeof(void), GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam(typeof(EventBean), "theEvent").AddParam(typeof(EventBean[]), NAME_EPS).AddParam(
                    typeof(ExprEvaluatorContext), NAME_EXPREVALCONTEXT);
            method.Block.DeclareVar(
                    typeof(object), "comparable",
                    LocalMethod(getComparable, REF_EPS, ConstantTrue(), REF_EXPREVALCONTEXT))
                .DeclareVar(typeof(object), "existing", ExprDotMethod(sorted, "get", Ref("comparable")))
                .IfRefNull("existing")
                .ExprDotMethod(sorted, "put", Ref("comparable"), Ref("theEvent"))
                .IfElseIf(InstanceOf(Ref("existing"), typeof(EventBean)))
                .DeclareVar(typeof(ArrayDeque<EventBean>), "coll", NewInstance(typeof(ArrayDeque<EventBean>), Constant(2)))
                .ExprDotMethod(Ref("coll"), "add", Ref("existing"))
                .ExprDotMethod(Ref("coll"), "add", Ref("theEvent"))
                .ExprDotMethod(sorted, "put", Ref("comparable"), Ref("coll"))
                .IfElse()
                .DeclareVar(typeof(ArrayDeque<EventBean>), "q", Cast(typeof(ArrayDeque<EventBean>), Ref("existing")))
                .ExprDotMethod(Ref("q"), "add", Ref("theEvent"))
                .BlockEnd()
                .Increment(size);

            return method;
        }

        private CodegenMethod DereferenceRemoveFromCollCodegen(
            CodegenMethod parent, CodegenNamedMethods namedMethods, CodegenClassScope classScope)
        {
            var getComparable = GetComparableWMultiKeyCodegen(forge.Spec.Criteria, sorted, namedMethods, classScope);

            var method = parent
                .MakeChildWithScope(typeof(void), GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam(typeof(EventBean), "theEvent").AddParam(typeof(EventBean[]), NAME_EPS).AddParam(
                    typeof(ExprEvaluatorContext), NAME_EXPREVALCONTEXT);
            method.Block.DeclareVar(
                    typeof(object), "comparable",
                    LocalMethod(getComparable, REF_EPS, ConstantTrue(), REF_EXPREVALCONTEXT))
                .DeclareVar(typeof(object), "existing", ExprDotMethod(sorted, "get", Ref("comparable")))
                .IfRefNull("existing").BlockReturnNoValue()
                .IfCondition(ExprDotMethod(Ref("existing"), "equals", Ref("theEvent")))
                .ExprDotMethod(sorted, "remove", Ref("comparable"))
                .Decrement(size)
                .IfElseIf(InstanceOf(Ref("existing"), typeof(ArrayDeque<EventBean>)))
                .DeclareVar(typeof(ArrayDeque<EventBean>), "q", Cast(typeof(ArrayDeque<EventBean>), Ref("existing")))
                .ExprDotMethod(Ref("q"), "remove", Ref("theEvent"))
                .IfCondition(ExprDotMethod(Ref("q"), "isEmpty"))
                .ExprDotMethod(sorted, "remove", Ref("comparable"))
                .BlockEnd()
                .Decrement(size);

            return method;
        }

        public static CodegenExpression CodegenGetAccessTableState(
            int column, CodegenMethodScope parent, CodegenClassScope classScope)
        {
            var method = parent.MakeChild(
                typeof(AggregationStateSorted), typeof(AggregatorAccessSortedImpl), classScope);
            method.Block
                .DeclareVar(typeof(AggregationStateSorted), "state", NewInstance(typeof(AggregationStateSorted)))
                .ExprDotMethod(Ref("state"), "setSize", RefCol("size", column))
                .ExprDotMethod(Ref("state"), "setSorted", RefCol("sorted", column))
                .MethodReturn(Ref("state"));
            return LocalMethod(method);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="value">payload to check</param>
        /// <returns>bean</returns>
        public static EventBean CheckedPayloadMayDeque(object value)
        {
            if (value is EventBean) {
                return (EventBean) value;
            }

            var q = (ArrayDeque<EventBean>) value;
            return q.First;
        }
    }
} // end of namespace