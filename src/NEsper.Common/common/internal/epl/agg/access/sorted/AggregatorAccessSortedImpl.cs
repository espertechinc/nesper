///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.agg.access.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.path;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.common.@internal.serde.compiletime.sharable;
using com.espertech.esper.common.@internal.serde.serdeset.additional;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.util.CodegenFieldSharableComparator.CodegenSharableSerdeName;
using static com.espertech.esper.common.@internal.epl.agg.method.core.AggregatorCodegenUtil;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;
//GETEVENTSERDEFACTORY;
using static com.espertech.esper.common.@internal.serde.compiletime.sharable.CodegenSharableSerdeEventTyped.CodegenSharableSerdeName; //REFCOUNTEDSETATOMICINTEGER;

namespace com.espertech.esper.common.@internal.epl.agg.access.sorted
{
	public class AggregatorAccessSortedImpl : AggregatorAccessWFilterBase,
		AggregatorAccessSorted
	{
		private readonly AggregationStateSortedForge _forge;
		private readonly CodegenExpressionMember _sorted;
		private readonly CodegenExpressionInstanceField _sortedSerde;
		private readonly CodegenExpressionMember _size;
		private readonly CodegenExpressionInstanceField _comparator;
		private readonly CodegenExpressionMember _joinRefs;
		private readonly CodegenExpressionInstanceField _joinRefsSerde;

		public AggregatorAccessSortedImpl(
			bool join,
			AggregationStateSortedForge forge,
			int col,
			CodegenCtor ctor,
			CodegenMemberCol membersColumnized,
			CodegenClassScope classScope,
			ExprNode optionalFilter)
			: base(optionalFilter)
		{
			this._forge = forge;
			_sorted = membersColumnized.AddMember(col, typeof(IOrderedDictionary<object, object>), "sorted");
			_size = membersColumnized.AddMember(col, typeof(int), "size");
			var types = ExprNodeUtilityQuery.GetExprResultTypes(forge.Spec.Criteria);
			_comparator = classScope.AddOrGetDefaultFieldSharable(
				new CodegenFieldSharableComparator(
					COMPARATORHASHABLEMULTIKEYS,
					types,
					forge.Spec.IsSortUsingCollator,
					forge.Spec.SortDescending));
			ctor.Block.AssignRef(_sorted, NewInstance<OrderedListDictionary<object, object>>(_comparator));

			_sortedSerde = classScope.AddOrGetDefaultFieldSharable(
				new ProxyCodegenFieldSharable() {
					ProcType = () => { return typeof(DIOSerdeTreeMapEventsMayDeque); },

					ProcInitCtorScoped = () => {
						var type = EventTypeUtility
							.ResolveTypeCodegen(forge.Spec.StreamEventType, EPStatementInitServicesConstants.REF);
						var criteriaSerdes = DataInputOutputSerdeForgeExtensions
							.CodegenArray(forge.Spec.CriteriaSerdes, classScope.NamespaceScope.InitMethod, classScope, null);
						return ExprDotMethodChain(EPStatementInitServicesConstants.REF)
							.Get(EPStatementInitServicesConstants.EVENTTYPERESOLVER)
							.Add(EventTypeResolverConstants.GETEVENTSERDEFACTORY)
							.Add("TreeMapEventsMayDeque", criteriaSerdes, type);
					},
				});

			if (join) {
				_joinRefs = membersColumnized.AddMember(col, typeof(RefCountedSetAtomicInteger<object>), "refs");
				ctor.Block.AssignRef(_joinRefs, NewInstance(typeof(RefCountedSetAtomicInteger<object>)));
				_joinRefsSerde = classScope.AddOrGetDefaultFieldSharable(
					new CodegenSharableSerdeEventTyped(
						REFCOUNTEDSETATOMICINTEGER, forge.Spec.StreamEventType));
			}
			else {
				_joinRefs = null;
				_joinRefsSerde = null;
			}
		}

		internal override void ApplyEnterFiltered(
			CodegenMethod method,
			ExprForgeCodegenSymbol symbols,
			CodegenClassScope classScope,
			CodegenNamedMethods namedMethods)
		{
			var eps = symbols.GetAddEPS(method);
			var ctx = symbols.GetAddExprEvalCtx(method);
			var referenceAddToColl = ReferenceAddToCollCodegen(method, namedMethods, classScope);
			method.Block
				.DeclareVar<EventBean>("theEvent", ArrayAtIndex(eps, Constant(_forge.Spec.StreamNum)))
				.IfRefNull("theEvent")
				.BlockReturnNoValue();

			if (_joinRefs == null) {
				method.Block
					.LocalMethod(referenceAddToColl, Ref("theEvent"), eps, ctx);
			}
			else {
				method.Block
					.IfCondition(ExprDotMethod(_joinRefs, "Add", Ref("theEvent")))
					.LocalMethod(referenceAddToColl, Ref("theEvent"), eps, ctx);
			}
		}

		internal override void ApplyLeaveFiltered(
			CodegenMethod method,
			ExprForgeCodegenSymbol symbols,
			CodegenClassScope classScope,
			CodegenNamedMethods namedMethods)
		{
			var eps = symbols.GetAddEPS(method);
			var ctx = symbols.GetAddExprEvalCtx(method);
			var dereferenceRemove = DereferenceRemoveFromCollCodegen(method, namedMethods, classScope);
			method.Block.DeclareVar<EventBean>("theEvent", ArrayAtIndex(eps, Constant(_forge.Spec.StreamNum)))
				.IfRefNull("theEvent")
				.BlockReturnNoValue();

			if (_joinRefs == null) {
				method.Block
					.LocalMethod(dereferenceRemove, Ref("theEvent"), eps, ctx);
			}
			else {
				method.Block
					.IfCondition(ExprDotMethod(_joinRefs, "Remove", Ref("theEvent")))
					.LocalMethod(dereferenceRemove, Ref("theEvent"), eps, ctx);
			}
		}

		public override void ClearCodegen(
			CodegenMethod method,
			CodegenClassScope classScope)
		{
			method.Block
				.ExprDotMethod(_sorted, "Clear")
				.AssignRef(_size, Constant(0));
			if (_joinRefs != null) {
				method.Block.ExprDotMethod(_joinRefs, "Clear");
			}
		}

		public CodegenExpression GetFirstValueCodegen(
			CodegenClassScope classScope,
			CodegenMethod parent)
		{
			var method = parent.MakeChildWithScope(typeof(EventBean), GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope);
			method.Block
				.IfCondition(ExprDotMethod(_sorted, "IsEmpty"))
				.BlockReturn(ConstantNull())
				.DeclareVar<KeyValuePair<object, object>>("max", ExprDotName(_sorted, "FirstEntry"))
				.MethodReturn(StaticMethod(typeof(AggregatorAccessSortedImpl), "CheckedPayloadMayDeque", ExprDotName(Ref("max"), "Value")));
			return LocalMethod(method);
		}

		public CodegenExpression GetLastValueCodegen(
			CodegenClassScope classScope,
			CodegenMethod parent)
		{
			var method = parent.MakeChildWithScope(typeof(EventBean), GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope);
			method.Block
				.IfCondition(ExprDotMethod(_sorted, "IsEmpty"))
				.BlockReturn(ConstantNull())
				.DeclareVar<KeyValuePair<object, object>>("min", ExprDotName(_sorted, "LastEntry"))
				.MethodReturn(StaticMethod(typeof(AggregatorAccessSortedImpl), "CheckedPayloadMayDeque", ExprDotName(Ref("min"), "Value")));
			return LocalMethod(method);
		}

		public CodegenExpression EnumeratorCodegen()
		{
			return NewInstance(typeof(AggregationStateSortedEnumerator), _sorted, ConstantFalse());
		}

		public CodegenExpression ReverseEnumeratorCodegen()
		{
			return NewInstance(typeof(AggregationStateSortedEnumerator), _sorted, ConstantTrue());
		}

		public CodegenExpression CollectionReadOnlyCodegen()
		{
			return NewInstance(typeof(AggregationStateSortedWrappingCollection), _sorted, _size);
		}

		public CodegenExpression SizeCodegen()
		{
			return _size;
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
			method.Block
				.Apply(WriteInt(output, row, _size))
				.ExprDotMethod(_sortedSerde, "Write", RowDotMember(row, _sorted), output, unitKey, writer);
			if (_joinRefs != null) {
				method.Block.ExprDotMethod(_joinRefsSerde, "Write", RowDotMember(row, _joinRefs), output, unitKey, writer);
			}
		}

		public override void ReadCodegen(
			CodegenExpressionRef row,
			int col,
			CodegenExpressionRef input,
			CodegenMethod method,
			CodegenExpressionRef unitKey,
			CodegenClassScope classScope)
		{
			method.Block
				.Apply(ReadInt(row, _size, input))
				.AssignRef(RowDotMember(row, _sorted), NewInstance(typeof(OrderedListDictionary<object, object>), _comparator))
				.ExprDotMethod(_sortedSerde, "Read", RowDotMember(row, _sorted), input, unitKey);
			if (_joinRefs != null) {
				method.Block.AssignRef(
					RowDotMember(row, _joinRefs),
					Cast(typeof(RefCountedSetAtomicInteger<object>), ExprDotMethod(_joinRefsSerde, "Read", input, unitKey)));
			}
		}

		private static CodegenMethod GetComparableWMultiKeyCodegen(
			ExprNode[] criteria,
			CodegenExpressionMember member,
			CodegenNamedMethods namedMethods,
			CodegenClassScope classScope)
		{
			var methodName = "GetComparable_" + member.Ref;
			Consumer<CodegenMethod> code = method => {
				if (criteria.Length == 1) {
					method.Block.MethodReturn(
						LocalMethod(
							CodegenLegoMethodExpression.CodegenExpression(criteria[0].Forge, method, classScope),
							REF_EPS,
							REF_ISNEWDATA,
							REF_EXPREVALCONTEXT));
				}
				else {
					var exprSymbol = new ExprForgeCodegenSymbol(true, null);
					var expressions = new CodegenExpression[criteria.Length];
					for (var i = 0; i < criteria.Length; i++) {
						expressions[i] = criteria[i].Forge.EvaluateCodegen(typeof(object), method, exprSymbol, classScope);
					}

					exprSymbol.DerivedSymbolsCodegen(method, method.Block, classScope);

					method.Block.DeclareVar<object[]>("result", NewArrayByLength(typeof(object), Constant(criteria.Length)));
					for (var i = 0; i < criteria.Length; i++) {
						method.Block.AssignArrayElement(Ref("result"), Constant(i), expressions[i]);
					}

					method.Block.MethodReturn(NewInstance(typeof(HashableMultiKey), Ref("result")));
				}
			};

			return namedMethods.AddMethod(
				typeof(object),
				methodName,
				CodegenNamedParam.From(typeof(EventBean[]), NAME_EPS, typeof(bool), NAME_ISNEWDATA, typeof(ExprEvaluatorContext), NAME_EXPREVALCONTEXT),
				typeof(AggregatorAccessSortedImpl),
				classScope,
				code);
		}

		private CodegenMethod ReferenceAddToCollCodegen(
			CodegenMethod parent,
			CodegenNamedMethods namedMethods,
			CodegenClassScope classScope)
		{
			var getComparable = GetComparableWMultiKeyCodegen(_forge.Spec.Criteria, _sorted, namedMethods, classScope);

			var method = parent
				.MakeChildWithScope(typeof(void),  GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.AddParam(typeof(EventBean), "theEvent")
				.AddParam(typeof(EventBean[]), NAME_EPS)
				.AddParam(typeof(ExprEvaluatorContext), NAME_EXPREVALCONTEXT);
			method.Block
				.DeclareVar<object>("comparable", LocalMethod(getComparable, REF_EPS, ConstantTrue(), REF_EXPREVALCONTEXT))
				.DeclareVar<object>("existing", ExprDotMethod(_sorted, "Get", Ref("comparable")))
				.IfRefNull("existing")
				.ExprDotMethod(_sorted, "Put", Ref("comparable"), Ref("theEvent"))
				.IfElseIf(InstanceOf(Ref("existing"), typeof(EventBean)))
				.DeclareVar<ArrayDeque<EventBean>>("coll", NewInstance<ArrayDeque<EventBean>>(Constant(2)))
				.ExprDotMethod(Ref("coll"), "Add", Cast<EventBean>(Ref("existing")))
				.ExprDotMethod(Ref("coll"), "Add", Ref("theEvent"))
				.ExprDotMethod(_sorted, "Put", Ref("comparable"), Ref("coll"))
				.IfElse()
				.DeclareVar<ArrayDeque<EventBean>>("q", Cast<ArrayDeque<EventBean>>(Ref("existing")))
				.ExprDotMethod(Ref("q"), "Add", Ref("theEvent"))
				.BlockEnd()
				.Increment(_size);

			return method;
		}

		private CodegenMethod DereferenceRemoveFromCollCodegen(
			CodegenMethod parent,
			CodegenNamedMethods namedMethods,
			CodegenClassScope classScope)
		{
			var getComparable = GetComparableWMultiKeyCodegen(_forge.Spec.Criteria, _sorted, namedMethods, classScope);

			var method = parent
				.MakeChildWithScope(typeof(void),  GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.AddParam(typeof(EventBean), "theEvent")
				.AddParam(typeof(EventBean[]), NAME_EPS)
				.AddParam(typeof(ExprEvaluatorContext), NAME_EXPREVALCONTEXT);
			method.Block
				.DeclareVar<object>("comparable", LocalMethod(getComparable, REF_EPS, ConstantTrue(), REF_EXPREVALCONTEXT))
				.DeclareVar<object>("existing", ExprDotMethod(_sorted, "Get", Ref("comparable")))
				
				.IfRefNull("existing")
				.BlockReturnNoValue()
				
				.IfCondition(StaticMethod<object>("Equals", Ref("existing"), Ref("theEvent")))
				.ExprDotMethod(_sorted, "Remove", Ref("comparable"))
				.Decrement(_size)

				.IfElseIf(InstanceOf(Ref("existing"), typeof(ArrayDeque<object>)))
				.DeclareVar<ArrayDeque<object>>("q", Cast(typeof(ArrayDeque<object>), Ref("existing")))
				.ExprDotMethod(Ref("q"), "Remove", Ref("theEvent"))
				.IfCondition(ExprDotMethod(Ref("q"), "IsEmpty"))
				.ExprDotMethod(_sorted, "Remove", Ref("comparable"))
				.BlockEnd()
				
				.IfElseIf(InstanceOf(Ref("existing"), typeof(ArrayDeque<EventBean>)))
				.DeclareVar<ArrayDeque<EventBean>>("q", Cast(typeof(ArrayDeque<EventBean>), Ref("existing")))
				.ExprDotMethod(Ref("q"), "Remove", Ref("theEvent"))
				.IfCondition(ExprDotMethod(Ref("q"), "IsEmpty"))
				.ExprDotMethod(_sorted, "Remove", Ref("comparable"))
				.BlockEnd()
				
				.Decrement(_size);

			return method;
		}

		public static CodegenExpression CodegenGetAccessTableState(
			int column,
			CodegenMethodScope parent,
			CodegenClassScope classScope)
		{
			var method = parent
				.MakeChild(typeof(AggregationStateSorted), typeof(AggregatorAccessSortedImpl), classScope);
			method.Block
				.DeclareVarNewInstance<AggregationStateSorted>("state")
				.SetProperty(Ref("state"), "Count", MemberCol("size", column))
				.SetProperty(Ref("state"), "Sorted", MemberCol("sorted", column))
				.MethodReturn(Ref("state"));
			return LocalMethod(method);
		}

		public static void CheckedPayloadAddAll(
			ArrayDeque<EventBean> events,
			object value)
		{
			if (value is EventBean) {
				events.Add((EventBean) value);
				return;
			}

			var q = (ArrayDeque<EventBean>) value;
			events.AddAll(q);
		}

		public static object CheckedPayloadGetUnderlyingArray(
			object value,
			Type underlyingClass)
		{
			if (value is EventBean eventBean) {
				var arrayX = Arrays.CreateInstanceChecked(underlyingClass, 1);
				arrayX.SetValue(eventBean.Underlying, 0);
				return arrayX;
			}

			var q = (ArrayDeque<EventBean>) value;
			var array = Arrays.CreateInstanceChecked(underlyingClass, q.Count);
			var index = 0;
			foreach (var @event in q) {
				array.SetValue(@event.Underlying, index++);
			}

			return array;
		}

		public static ICollection<EventBean> CheckedPayloadGetCollEvents(object value)
		{
			if (value is EventBean eventBean) {
				return Collections.SingletonList(eventBean);
			}

			return (ICollection<EventBean>) value;
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="value">payload to check</param>
		/// <returns>bean</returns>
		public static EventBean CheckedPayloadMayDeque(object value)
		{
			if (value is EventBean) {
				return (EventBean) value;
			}
			else if (value is ArrayDeque<EventBean> arrayEventDeque) {
				return arrayEventDeque.First;
			}
			else if (value is ArrayDeque<object> arrayObjectDeque) {
				return (EventBean) arrayObjectDeque.First;
			}

			throw new ArgumentException(nameof(value));
		}
	}
} // end of namespace
