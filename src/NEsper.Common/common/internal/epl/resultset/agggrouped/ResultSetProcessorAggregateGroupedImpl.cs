///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.output.polled;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.resultset.grouped;
using com.espertech.esper.common.@internal.epl.resultset.rowpergroup;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.resultset.codegen.ResultSetProcessorCodegenNames;
using static com.espertech.esper.common.@internal.epl.resultset.core.ResultSetProcessorUtil;
using static com.espertech.esper.common.@internal.util.CollectionUtil;

namespace com.espertech.esper.common.@internal.epl.resultset.agggrouped
{
	/// <summary>
	/// Result-set processor for the aggregate-grouped case:
	/// there is a group-by and one or more non-aggregation event properties in the select clause are not listed in the group by,
	/// and there are aggregation functions.
	/// <para>
	/// This processor does perform grouping by computing MultiKey group-by keys for each row.
	/// The processor generates one row for each event entering (new event) and one row for each event leaving (old event).
	/// </para>
	/// <para>
	/// Aggregation state is a table of rows held by aggegation service where the row key is the group-by MultiKey.
	/// </para>
	/// </summary>
	public class ResultSetProcessorAggregateGroupedImpl
	{
		private const string NAME_OUTPUTALLHELPER = "outputAllHelper";
		private const string NAME_OUTPUTLASTHELPER = "outputLastHelper";
		private const string NAME_OUTPUTFIRSTHELPER = "outputFirstHelper";
		private const string NAME_OUTPUTALLGROUPREPS = "outputAllGroupReps";

		public static void ApplyViewResultCodegen(
			ResultSetProcessorAggregateGroupedForge forge,
			CodegenClassScope classScope,
			CodegenMethod method,
			CodegenInstanceAux instance)
		{
			method.Block
				.DeclareVar<EventBean[]>("eventsPerStream", NewArrayByLength(typeof(EventBean), Constant(1)))
				.IfCondition(NotEqualsNull(REF_NEWDATA))
				.ForEach(typeof(EventBean), "aNewData", REF_NEWDATA)
				.AssignArrayElement("eventsPerStream", Constant(0), Ref("aNewData"))
				.DeclareVar<object>("mk", LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()))
				.ExprDotMethod(MEMBER_AGGREGATIONSVC, "ApplyEnter", Ref("eventsPerStream"), Ref("mk"), MEMBER_AGENTINSTANCECONTEXT)
				.BlockEnd()
				.BlockEnd()
				.IfCondition(NotEqualsNull(REF_OLDDATA))
				.ForEach(typeof(EventBean), "anOldData", REF_OLDDATA)
				.AssignArrayElement("eventsPerStream", Constant(0), Ref("anOldData"))
				.DeclareVar<object>("mk", LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), ConstantFalse()))
				.ExprDotMethod(MEMBER_AGGREGATIONSVC, "ApplyLeave", Ref("eventsPerStream"), Ref("mk"), MEMBER_AGENTINSTANCECONTEXT)
				.BlockEnd()
				.BlockEnd();
		}

		public static void ApplyJoinResultCodegen(
			ResultSetProcessorAggregateGroupedForge forge,
			CodegenClassScope classScope,
			CodegenMethod method,
			CodegenInstanceAux instance)
		{
			method.Block
				.IfCondition(Not(ExprDotMethod(REF_NEWDATA, "IsEmpty")))
				.ForEach(typeof(MultiKeyArrayOfKeys<EventBean>), "aNewEvent", REF_NEWDATA)
				.DeclareVar<EventBean[]>("eventsPerStream", ExprDotName(Ref("aNewEvent"), "Array"))
				.DeclareVar<object>("mk", LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()))
				.ExprDotMethod(MEMBER_AGGREGATIONSVC, "ApplyEnter", Ref("eventsPerStream"), Ref("mk"), MEMBER_AGENTINSTANCECONTEXT)
				.BlockEnd()
				.BlockEnd()
				.IfCondition(And(NotEqualsNull(REF_OLDDATA), Not(ExprDotMethod(REF_OLDDATA, "IsEmpty"))))
				.ForEach(typeof(MultiKeyArrayOfKeys<EventBean>), "anOldEvent", REF_OLDDATA)
				.DeclareVar<EventBean[]>("eventsPerStream", ExprDotName(Ref("anOldEvent"), "Array"))
				.DeclareVar<object>("mk", LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), ConstantFalse()))
				.ExprDotMethod(MEMBER_AGGREGATIONSVC, "ApplyLeave", Ref("eventsPerStream"), Ref("mk"), MEMBER_AGENTINSTANCECONTEXT)
				.BlockEnd()
				.BlockEnd();
		}

		public static void ProcessJoinResultCodegen(
			ResultSetProcessorAggregateGroupedForge forge,
			CodegenClassScope classScope,
			CodegenMethod method,
			CodegenInstanceAux instance)
		{
			var generateOutputEventsJoin = GenerateOutputEventsJoinCodegen(forge, classScope, instance);

			method.Block
				.DeclareVar<object[]>(
					"newDataGroupByKeys",
					LocalMethod(forge.GenerateGroupKeyArrayJoin, REF_NEWDATA, ConstantTrue()))
				.DeclareVar<object[]>(
					"oldDataGroupByKeys",
					LocalMethod(forge.GenerateGroupKeyArrayJoin, REF_OLDDATA, ConstantFalse()));

			if (forge.IsUnidirectional) {
				method.Block.ExprDotMethod(Ref("this"), "Clear");
			}

			method.Block.StaticMethod(
				typeof(ResultSetProcessorGroupedUtil),
				ResultSetProcessorGroupedUtil.METHOD_APPLYAGGJOINRESULTKEYEDJOIN,
				MEMBER_AGGREGATIONSVC,
				MEMBER_AGENTINSTANCECONTEXT,
				REF_NEWDATA,
				Ref("newDataGroupByKeys"),
				REF_OLDDATA,
				Ref("oldDataGroupByKeys"));

			method.Block.DeclareVar<EventBean[]>(
					"selectOldEvents",
					forge.IsSelectRStream
						? LocalMethod(generateOutputEventsJoin, REF_OLDDATA, Ref("oldDataGroupByKeys"), ConstantFalse(), REF_ISSYNTHESIZE)
						: ConstantNull())
				.DeclareVar<EventBean[]>(
					"selectNewEvents",
					LocalMethod(generateOutputEventsJoin, REF_NEWDATA, Ref("newDataGroupByKeys"), ConstantTrue(), REF_ISSYNTHESIZE))
				.MethodReturn(StaticMethod(typeof(ResultSetProcessorUtil), METHOD_TOPAIRNULLIFALLNULL, Ref("selectNewEvents"), Ref("selectOldEvents")));
		}

		public static void ProcessViewResultCodegen(
			ResultSetProcessorAggregateGroupedForge forge,
			CodegenClassScope classScope,
			CodegenMethod method,
			CodegenInstanceAux instance)
		{
			var generateOutputEventsView = GenerateOutputEventsViewCodegen(forge, classScope, instance);
			var processViewResultNewDepthOne = ProcessViewResultNewDepthOneCodegen(forge, classScope, instance);
			var processViewResultPairDepthOneNoRStream = ProcessViewResultPairDepthOneCodegen(forge, classScope, instance);

			var ifShortcut = method.Block.IfCondition(And(NotEqualsNull(REF_NEWDATA), EqualsIdentity(ArrayLength(REF_NEWDATA), Constant(1))));
			ifShortcut.IfCondition(Or(EqualsNull(REF_OLDDATA), EqualsIdentity(ArrayLength(REF_OLDDATA), Constant(0))))
				.BlockReturn(LocalMethod(processViewResultNewDepthOne, REF_NEWDATA, REF_ISSYNTHESIZE))
				.IfCondition(EqualsIdentity(ArrayLength(REF_OLDDATA), Constant(1)))
				.BlockReturn(LocalMethod(processViewResultPairDepthOneNoRStream, REF_NEWDATA, REF_OLDDATA, REF_ISSYNTHESIZE));

			method.Block
				.DeclareVar<object[]>("newDataGroupByKeys", LocalMethod(forge.GenerateGroupKeyArrayView, REF_NEWDATA, ConstantTrue()))
				.DeclareVar<object[]>("oldDataGroupByKeys", LocalMethod(forge.GenerateGroupKeyArrayView, REF_OLDDATA, ConstantFalse()))
				.DeclareVar<EventBean[]>("eventsPerStream", NewArrayByLength(typeof(EventBean), Constant(1)))
				.StaticMethod(
					typeof(ResultSetProcessorGroupedUtil),
					ResultSetProcessorGroupedUtil.METHOD_APPLYAGGVIEWRESULTKEYEDVIEW,
					MEMBER_AGGREGATIONSVC,
					MEMBER_AGENTINSTANCECONTEXT,
					REF_NEWDATA,
					Ref("newDataGroupByKeys"),
					REF_OLDDATA,
					Ref("oldDataGroupByKeys"),
					Ref("eventsPerStream"));

			method.Block.DeclareVar<EventBean[]>(
					"selectOldEvents",
					forge.IsSelectRStream
						? LocalMethod(
							generateOutputEventsView,
							REF_OLDDATA,
							Ref("oldDataGroupByKeys"),
							ConstantFalse(),
							REF_ISSYNTHESIZE,
							Ref("eventsPerStream"))
						: ConstantNull())
				.DeclareVar<EventBean[]>(
					"selectNewEvents",
					LocalMethod(generateOutputEventsView, REF_NEWDATA, Ref("newDataGroupByKeys"), ConstantTrue(), REF_ISSYNTHESIZE, Ref("eventsPerStream")))
				.MethodReturn(StaticMethod(typeof(ResultSetProcessorUtil), METHOD_TOPAIRNULLIFALLNULL, Ref("selectNewEvents"), Ref("selectOldEvents")));
		}

		private static CodegenMethod GenerateOutputEventsViewCodegen(
			ResultSetProcessorAggregateGroupedForge forge,
			CodegenClassScope classScope,
			CodegenInstanceAux instance)
		{
			Consumer<CodegenMethod> code = methodNode => {
				methodNode.Block.IfNullReturnNull(Ref("outputEvents"))
					.DeclareVar<EventBean[]>("events", NewArrayByLength(typeof(EventBean), ArrayLength(Ref("outputEvents"))))
					.DeclareVar<object[]>("keys", NewArrayByLength(typeof(object), ArrayLength(Ref("outputEvents"))));

				if (forge.IsSorting) {
					methodNode.Block.DeclareVar<EventBean[][]>(
						"currentGenerators",
						NewArrayByLength(typeof(EventBean[]), ArrayLength(Ref("outputEvents"))));
				}

				methodNode.Block.DeclareVar<int>("countOutputRows", Constant(0))
					.DeclareVar<int>("cpid", ExprDotName(MEMBER_AGENTINSTANCECONTEXT, "AgentInstanceId"));

				{
					var forLoop = methodNode.Block.ForLoopIntSimple("countInputRows", ArrayLength(Ref("outputEvents")));
					forLoop.ExprDotMethod(
							MEMBER_AGGREGATIONSVC,
							"SetCurrentAccess",
							ArrayAtIndex(Ref("groupByKeys"), Ref("countInputRows")),
							Ref("cpid"),
							ConstantNull())
						.AssignArrayElement(ExprForgeCodegenNames.REF_EPS, Constant(0), ArrayAtIndex(Ref("outputEvents"), Ref("countInputRows")));

					if (forge.OptionalHavingNode != null) {
						forLoop.IfCondition(
								Not(
									LocalMethod(
										instance.Methods.GetMethod("EvaluateHavingClause"),
										ExprForgeCodegenNames.REF_EPS,
										REF_ISNEWDATA,
										MEMBER_AGENTINSTANCECONTEXT)))
							.BlockContinue();
					}

					forLoop.AssignArrayElement(
							"events",
							Ref("countOutputRows"),
							ExprDotMethod(MEMBER_SELECTEXPRPROCESSOR, "Process", ExprForgeCodegenNames.REF_EPS, REF_ISNEWDATA, REF_ISSYNTHESIZE, MEMBER_AGENTINSTANCECONTEXT))
						.AssignArrayElement("keys", Ref("countOutputRows"), ArrayAtIndex(Ref("groupByKeys"), Ref("countInputRows")));

					if (forge.IsSorting) {
						forLoop.AssignArrayElement(
							"currentGenerators",
							Ref("countOutputRows"),
							NewArrayWithInit(typeof(EventBean), ArrayAtIndex(Ref("outputEvents"), Ref("countInputRows"))));
					}

					forLoop.IncrementRef("countOutputRows")
						.BlockEnd();
				}

				ResultSetProcessorUtil.OutputFromCountMaySortCodegen(
					methodNode.Block,
					Ref("countOutputRows"),
					Ref("events"),
					Ref("keys"),
					Ref("currentGenerators"),
					forge.IsSorting);
			};

			return instance.Methods.AddMethod(
				typeof(EventBean[]),
				"generateOutputEventsView",
				CodegenNamedParam.From(
					typeof(EventBean[]), "outputEvents",
					typeof(object[]), "groupByKeys",
					typeof(bool), NAME_ISNEWDATA,
					typeof(bool), NAME_ISSYNTHESIZE,
					typeof(EventBean[]), ExprForgeCodegenNames.NAME_EPS),
				typeof(ResultSetProcessorAggregateGroupedImpl),
				classScope,
				code);
		}

		public static void AcceptHelperVisitorCodegen(
			CodegenMethod method,
			CodegenInstanceAux instance)
		{
			if (instance.HasMember(NAME_OUTPUTALLGROUPREPS)) {
				method.Block.ExprDotMethod(REF_RESULTSETVISITOR, "Visit", Member(NAME_OUTPUTALLGROUPREPS));
			}

			if (instance.HasMember(NAME_OUTPUTLASTHELPER)) {
				method.Block.ExprDotMethod(REF_RESULTSETVISITOR, "Visit", Member(NAME_OUTPUTLASTHELPER));
			}

			if (instance.HasMember(NAME_OUTPUTALLHELPER)) {
				method.Block.ExprDotMethod(REF_RESULTSETVISITOR, "Visit", Member(NAME_OUTPUTALLHELPER));
			}

			if (instance.HasMember(NAME_OUTPUTFIRSTHELPER)) {
				method.Block.ExprDotMethod(REF_RESULTSETVISITOR, "Visit", Member(NAME_OUTPUTFIRSTHELPER));
			}
		}

		private static CodegenMethod GenerateOutputEventsJoinCodegen(
			ResultSetProcessorAggregateGroupedForge forge,
			CodegenClassScope classScope,
			CodegenInstanceAux instance)
		{
			Consumer<CodegenMethod> code = methodNode => {
				methodNode.Block.IfCondition(ExprDotMethod(Ref("resultSet"), "IsEmpty"))
					.BlockReturn(ConstantNull())
					.DeclareVar<EventBean[]>("events", NewArrayByLength(typeof(EventBean), ExprDotName(Ref("resultSet"), "Count")))
					.DeclareVar<object[]>("keys", NewArrayByLength(typeof(object), ExprDotName(Ref("resultSet"), "Count")));

				if (forge.IsSorting) {
					methodNode.Block.DeclareVar<EventBean[][]>(
						"currentGenerators",
						NewArrayByLength(typeof(EventBean[]), ExprDotName(Ref("resultSet"), "Count")));
				}

				methodNode.Block.DeclareVar<int>("countOutputRows", Constant(0))
					.DeclareVar<int>("countInputRows", Constant(-1))
					.DeclareVar<int>("cpid", ExprDotName(MEMBER_AGENTINSTANCECONTEXT, "AgentInstanceId"));

				{
					var forLoop = methodNode.Block
						.ForEach(typeof(MultiKeyArrayOfKeys<EventBean>), "row", Ref("resultSet"));
					forLoop.IncrementRef("countInputRows")
						.DeclareVar<EventBean[]>("eventsPerStream", ExprDotName(Ref("row"), "Array"))
						.ExprDotMethod(
							MEMBER_AGGREGATIONSVC,
							"SetCurrentAccess",
							ArrayAtIndex(Ref("groupByKeys"), Ref("countInputRows")),
							Ref("cpid"),
							ConstantNull());

					if (forge.OptionalHavingNode != null) {
						forLoop.IfCondition(
								Not(
									LocalMethod(
										instance.Methods.GetMethod("EvaluateHavingClause"),
										ExprForgeCodegenNames.REF_EPS,
										REF_ISNEWDATA,
										MEMBER_AGENTINSTANCECONTEXT)))
							.BlockContinue();
					}

					forLoop.AssignArrayElement(
							"events",
							Ref("countOutputRows"),
							ExprDotMethod(MEMBER_SELECTEXPRPROCESSOR, "Process", ExprForgeCodegenNames.REF_EPS, REF_ISNEWDATA, REF_ISSYNTHESIZE, MEMBER_AGENTINSTANCECONTEXT))
						.AssignArrayElement("keys", Ref("countOutputRows"), ArrayAtIndex(Ref("groupByKeys"), Ref("countInputRows")));

					if (forge.IsSorting) {
						forLoop.AssignArrayElement("currentGenerators", Ref("countOutputRows"), Ref("eventsPerStream"));
					}

					forLoop.IncrementRef("countOutputRows")
						.BlockEnd();
				}

				ResultSetProcessorUtil.OutputFromCountMaySortCodegen(
					methodNode.Block,
					Ref("countOutputRows"),
					Ref("events"),
					Ref("keys"),
					Ref("currentGenerators"),
					forge.IsSorting);
			};

			return instance.Methods.AddMethod(
				typeof(EventBean[]),
				"generateOutputEventsJoin",
				CodegenNamedParam.From(
                    typeof(ISet<MultiKeyArrayOfKeys<EventBean>>), "resultSet",
					typeof(object[]), "groupByKeys",
					typeof(bool), NAME_ISNEWDATA,
					typeof(bool), NAME_ISSYNTHESIZE),
				typeof(ResultSetProcessorAggregateGroupedImpl),
				classScope,
				code);
		}

		public static void GetEnumeratorViewCodegen(
			ResultSetProcessorAggregateGroupedForge forge,
			CodegenClassScope classScope,
			CodegenMethod method,
			CodegenInstanceAux instance)
		{
			if (!forge.IsHistoricalOnly) {
				method.Block.MethodReturn(
					LocalMethod(ObtainEnumeratorCodegen(forge, method, classScope, instance), REF_VIEWABLE));
				return;
			}

			method.Block
				.ExprDotMethod(MEMBER_AGGREGATIONSVC, "ClearResults", MEMBER_AGENTINSTANCECONTEXT)
				.DeclareVar<IEnumerator<EventBean>>("enumerator", ExprDotMethod(REF_VIEWABLE, "GetEnumerator"))
				.DeclareVar<EventBean[]>("eventsPerStream", NewArrayByLength(typeof(EventBean), Constant(1)));

			{
				method.Block
					.WhileLoop(ExprDotMethod(Ref("enumerator"), "MoveNext"))
					.AssignArrayElement(Ref("eventsPerStream"), Constant(0), ExprDotName(Ref("enumerator"), "Current"))
					.DeclareVar<object>("groupKey", LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()))
					.ExprDotMethod(MEMBER_AGGREGATIONSVC, "ApplyEnter", Ref("eventsPerStream"), Ref("groupKey"), MEMBER_AGENTINSTANCECONTEXT)
					.BlockEnd();
			}

			method.Block.DeclareVar<ArrayDeque<EventBean>>(
					"deque",
					StaticMethod(
						typeof(ResultSetProcessorUtil),
						METHOD_ITERATORTODEQUE,
						LocalMethod(ObtainEnumeratorCodegen(forge, method, classScope, instance), REF_VIEWABLE)))
				.ExprDotMethod(MEMBER_AGGREGATIONSVC, "ClearResults", MEMBER_AGENTINSTANCECONTEXT)
				.MethodReturn(ExprDotMethod(Ref("deque"), "GetEnumerator"));
		}

		private static CodegenMethod ObtainEnumeratorCodegen(
			ResultSetProcessorAggregateGroupedForge forge,
			CodegenMethod parent,
			CodegenClassScope classScope,
			CodegenInstanceAux instance)
		{
			var iterator = parent
				.MakeChild(typeof(IEnumerator<EventBean>), typeof(ResultSetProcessorAggregateGroupedImpl), classScope)
				.AddParam(typeof(Viewable), NAME_VIEWABLE);
			if (!forge.IsSorting) {
				iterator.Block.MethodReturn(
					StaticMethod(
						typeof(ResultSetProcessorAggregateGroupedIterator),
						"Create",
						ExprDotMethod(REF_VIEWABLE, "GetEnumerator"),
						Ref("this"),
						MEMBER_AGGREGATIONSVC,
						MEMBER_AGENTINSTANCECONTEXT));
				return iterator;
			}

			// Pull all parent events, generate order keys
			iterator.Block
				.DeclareVar<EventBean[]>("eventsPerStream", NewArrayByLength(typeof(EventBean), Constant(1)))
				.DeclareVar<IList<EventBean>>("outgoingEvents", NewInstance(typeof(List<EventBean>)))
				.DeclareVar<IList<object>>("orderKeys", NewInstance(typeof(List<object>)));

			{
				var forLoop = iterator.Block.ForEach(typeof(EventBean), "candidate", REF_VIEWABLE);
				forLoop.AssignArrayElement(Ref("eventsPerStream"), Constant(0), Ref("candidate"))
					.DeclareVar<object>("groupKey", LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()))
					.ExprDotMethod(
						MEMBER_AGGREGATIONSVC,
						"SetCurrentAccess",
						Ref("groupKey"),
						ExprDotName(MEMBER_AGENTINSTANCECONTEXT, "AgentInstanceId"),
						ConstantNull());

				if (forge.OptionalHavingNode != null) {
					forLoop.IfCondition(
							Not(
								LocalMethod(
									instance.Methods.GetMethod("EvaluateHavingClause"),
									ExprForgeCodegenNames.REF_EPS,
									ConstantTrue(),
									MEMBER_AGENTINSTANCECONTEXT)))
						.BlockContinue();
				}

				forLoop.ExprDotMethod(
						Ref("outgoingEvents"),
						"Add",
						ExprDotMethod(
							MEMBER_SELECTEXPRPROCESSOR,
							"Process",
							Ref("eventsPerStream"),
							ConstantTrue(),
							ConstantTrue(),
							MEMBER_AGENTINSTANCECONTEXT))
					.ExprDotMethod(
						Ref("orderKeys"),
						"Add",
						ExprDotMethod(
							MEMBER_ORDERBYPROCESSOR, 
							"GetSortKey",
							Ref("eventsPerStream"),
							ConstantTrue(),
							MEMBER_AGENTINSTANCECONTEXT));
			}

			iterator.Block.MethodReturn(
				StaticMethod(
					typeof(ResultSetProcessorUtil),
					METHOD_ORDEROUTGOINGGETITERATOR,
					Ref("outgoingEvents"),
					Ref("orderKeys"),
					MEMBER_ORDERBYPROCESSOR,
					MEMBER_AGENTINSTANCECONTEXT));
			return iterator;
		}

		public static void GetEnumeratorJoinCodegen(
			ResultSetProcessorAggregateGroupedForge forge,
			CodegenClassScope classScope,
			CodegenMethod method,
			CodegenInstanceAux instance)
		{
			var generateOutputEventsJoin = GenerateOutputEventsJoinCodegen(forge, classScope, instance);

			method.Block
				.DeclareVar<object[]>(
					"groupByKeys",
					LocalMethod(forge.GenerateGroupKeyArrayJoin, REF_JOINSET, ConstantTrue()))
				.DeclareVar<EventBean[]>(
					"result",
					LocalMethod(generateOutputEventsJoin, REF_JOINSET, Ref("groupByKeys"), ConstantTrue(), ConstantTrue()))
				.MethodReturn(
					NewInstance<ArrayEventEnumerator>(Ref("result")));
		}

		public static void ClearMethodCodegen(CodegenMethod method)
		{
			method.Block.ExprDotMethod(MEMBER_AGGREGATIONSVC, "ClearResults", MEMBER_AGENTINSTANCECONTEXT);
		}

		public static void ProcessOutputLimitedJoinCodegen(
			ResultSetProcessorAggregateGroupedForge forge,
			CodegenClassScope classScope,
			CodegenMethod method,
			CodegenInstanceAux instance)
		{
			var outputLimitLimitType = forge.OutputLimitSpec.DisplayLimit;
			if (outputLimitLimitType == OutputLimitLimitType.DEFAULT) {
				ProcessOutputLimitedJoinDefaultCodegen(forge, classScope, method, instance);
			}
			else if (outputLimitLimitType == OutputLimitLimitType.ALL) {
				ProcessOutputLimitedJoinAllCodegen(forge, classScope, method, instance);
			}
			else if (outputLimitLimitType == OutputLimitLimitType.FIRST) {
				ProcessOutputLimitedJoinFirstCodegen(forge, classScope, method, instance);
			}
			else if (outputLimitLimitType == OutputLimitLimitType.LAST) {
				ProcessOutputLimitedJoinLastCodegen(forge, classScope, method, instance);
			}
			else {
				throw new IllegalStateException("Unrecognized output limit " + outputLimitLimitType);
			}
		}

		public static void ProcessOutputLimitedViewCodegen(
			ResultSetProcessorAggregateGroupedForge forge,
			CodegenClassScope classScope,
			CodegenMethod method,
			CodegenInstanceAux instance)
		{
			var outputLimitLimitType = forge.OutputLimitSpec.DisplayLimit;
			if (outputLimitLimitType == OutputLimitLimitType.DEFAULT) {
				ProcessOutputLimitedViewDefaultCodegen(forge, classScope, method, instance);
			}
			else if (outputLimitLimitType == OutputLimitLimitType.ALL) {
				ProcessOutputLimitedViewAllCodegen(forge, classScope, method, instance);
			}
			else if (outputLimitLimitType == OutputLimitLimitType.FIRST) {
				ProcessOutputLimitedViewFirstCodegen(forge, classScope, method, instance);
			}
			else if (outputLimitLimitType == OutputLimitLimitType.LAST) {
				ProcessOutputLimitedViewLastCodegen(forge, classScope, method, instance);
			}
			else {
				throw new IllegalStateException("Unrecognized output limited type " + outputLimitLimitType);
			}
		}

		public static void StopMethodCodegen(
			CodegenMethod method,
			CodegenInstanceAux instance)
		{
			if (instance.HasMember(NAME_OUTPUTALLGROUPREPS)) {
				method.Block.ExprDotMethod(Member(NAME_OUTPUTALLGROUPREPS), "Destroy");
			}

			if (instance.HasMember(NAME_OUTPUTALLHELPER)) {
				method.Block.ExprDotMethod(Member(NAME_OUTPUTALLHELPER), "Destroy");
			}

			if (instance.HasMember(NAME_OUTPUTLASTHELPER)) {
				method.Block.ExprDotMethod(Member(NAME_OUTPUTLASTHELPER), "Destroy");
			}

			if (instance.HasMember(NAME_OUTPUTFIRSTHELPER)) {
				method.Block.ExprDotMethod(Member(NAME_OUTPUTFIRSTHELPER), "Destroy");
			}
		}

		internal static CodegenMethod GenerateOutputBatchedJoinUnkeyedCodegen(
			ResultSetProcessorAggregateGroupedForge forge,
			CodegenClassScope classScope,
			CodegenInstanceAux instance)
		{
			Consumer<CodegenMethod> code = methodNode => {
				methodNode.Block.IfCondition(EqualsNull(Ref("outputEvents")))
					.BlockReturnNoValue()
					.DeclareVar<int>("count", Constant(0))
					.DeclareVarNoInit(typeof(EventBean[]), "eventsPerStream");

				{
					var forEach = methodNode.Block
						.ForEach(typeof(MultiKeyArrayOfKeys<EventBean>), "row", Ref("outputEvents"));
					
					forEach.ExprDotMethod(
							MEMBER_AGGREGATIONSVC,
							"SetCurrentAccess",
							ArrayAtIndex(Ref("groupByKeys"), Ref("count")),
							ExprDotName(MEMBER_AGENTINSTANCECONTEXT, "AgentInstanceId"),
							ConstantNull())
						.AssignRef("eventsPerStream", ExprDotName(Ref("row"), "Array"));

					if (forge.OptionalHavingNode != null) {
						forEach.IfCondition(
								Not(
									LocalMethod(
										instance.Methods.GetMethod("EvaluateHavingClause"),
										Ref("eventsPerStream"),
										REF_ISNEWDATA,
										MEMBER_AGENTINSTANCECONTEXT)))
							.IncrementRef("count")
							.BlockContinue();
					}

					forEach.ExprDotMethod(
						Ref("resultEvents"),
						"Add",
						ExprDotMethod(
							MEMBER_SELECTEXPRPROCESSOR,
							"Process",
							Ref("eventsPerStream"),
							REF_ISNEWDATA,
							REF_ISSYNTHESIZE,
							MEMBER_AGENTINSTANCECONTEXT));

					if (forge.IsSorting) {
						forEach.ExprDotMethod(
							Ref("optSortKeys"),
							"Add",
							ExprDotMethod(
								MEMBER_ORDERBYPROCESSOR,
								"GetSortKey",
								Ref("eventsPerStream"),
								REF_ISNEWDATA,
								MEMBER_AGENTINSTANCECONTEXT));
					}

					forEach.IncrementRef("count");
				}
			};
			
			return instance.Methods.AddMethod(
				typeof(void),
				"GenerateOutputBatchedJoinUnkeyed",
				CodegenNamedParam.From(
					typeof(ISet<MultiKeyArrayOfKeys<EventBean>>), "outputEvents",
					typeof(object[]), "groupByKeys",
					typeof(bool), NAME_ISNEWDATA,
					typeof(bool), NAME_ISSYNTHESIZE,
					typeof(ICollection<EventBean>), "resultEvents",
					typeof(IList<object>), "optSortKeys"),
				typeof(ResultSetProcessorAggregateGrouped),
				classScope,
				code);
		}

		internal static void GenerateOutputBatchedSingleCodegen(
			ResultSetProcessorAggregateGroupedForge forge,
			CodegenClassScope classScope,
			CodegenInstanceAux instance)
		{
			Consumer<CodegenMethod> code = methodNode => {
				methodNode.Block.ExprDotMethod(
					MEMBER_AGGREGATIONSVC,
					"SetCurrentAccess",
					Ref("groupByKey"),
					ExprDotName(MEMBER_AGENTINSTANCECONTEXT, "AgentInstanceId"),
					ConstantNull());

				if (forge.OptionalHavingNode != null) {
					methodNode.Block.IfCondition(
							Not(
								LocalMethod(
									instance.Methods.GetMethod("EvaluateHavingClause"),
									ExprForgeCodegenNames.REF_EPS,
									REF_ISNEWDATA,
									MEMBER_AGENTINSTANCECONTEXT)))
						.BlockReturn(ConstantNull());
				}

				methodNode.Block.MethodReturn(
					ExprDotMethod(
						MEMBER_SELECTEXPRPROCESSOR,
						"Process",
						Ref("eventsPerStream"),
						REF_ISNEWDATA,
						REF_ISSYNTHESIZE,
						MEMBER_AGENTINSTANCECONTEXT));
			};
			
			instance.Methods.AddMethod(
				typeof(EventBean),
				"GenerateOutputBatchedSingle",
				CodegenNamedParam.From(
					typeof(object), "groupByKey",
					typeof(EventBean[]), ExprForgeCodegenNames.NAME_EPS,
					typeof(bool), NAME_ISNEWDATA,
					typeof(bool), NAME_ISSYNTHESIZE),
				typeof(ResultSetProcessorUtil),
				classScope,
				code);
		}

		internal static CodegenMethod GenerateOutputBatchedViewPerKeyCodegen(
			ResultSetProcessorAggregateGroupedForge forge,
			CodegenClassScope classScope,
			CodegenInstanceAux instance)
		{
			Consumer<CodegenMethod> code = methodNode => {
				methodNode.Block.IfCondition(EqualsNull(Ref("outputEvents")))
					.BlockReturnNoValue()
					.DeclareVar<int>("count", Constant(0));

				{
					var forEach = methodNode.Block
						.ForEach(typeof(EventBean), "outputEvent", Ref("outputEvents"));
					
					forEach
						.DeclareVar<object>("groupKey", ArrayAtIndex(Ref("groupByKeys"), Ref("count")))
						.ExprDotMethod(
							MEMBER_AGGREGATIONSVC,
							"SetCurrentAccess",
							Ref("groupKey"),
							ExprDotName(MEMBER_AGENTINSTANCECONTEXT, "AgentInstanceId"),
							ConstantNull())
						.AssignArrayElement(Ref("eventsPerStream"), Constant(0), ArrayAtIndex(Ref("outputEvents"), Ref("count")));

					if (forge.OptionalHavingNode != null) {
						forEach.IfCondition(
								Not(
									LocalMethod(
										instance.Methods.GetMethod("EvaluateHavingClause"),
										ExprForgeCodegenNames.REF_EPS,
										REF_ISNEWDATA,
										MEMBER_AGENTINSTANCECONTEXT)))
							.BlockContinue();
					}

					forEach.ExprDotMethod(
						Ref("resultEvents"),
						"Put",
						Ref("groupKey"),
						ExprDotMethod(
							MEMBER_SELECTEXPRPROCESSOR,
							"Process",
							Ref("eventsPerStream"),
							REF_ISNEWDATA,
							REF_ISSYNTHESIZE,
							MEMBER_AGENTINSTANCECONTEXT));

					if (forge.IsSorting) {
						forEach.ExprDotMethod(
							Ref("optSortKeys"),
							"Put",
							Ref("groupKey"),
							ExprDotMethod(
								MEMBER_ORDERBYPROCESSOR,
								"GetSortKey",
								Ref("eventsPerStream"),
								REF_ISNEWDATA,
								MEMBER_AGENTINSTANCECONTEXT));
					}

					forEach.IncrementRef("count");
				}
			};
			
			return instance.Methods.AddMethod(
				typeof(void),
				"GenerateOutputBatchedViewPerKey",
				CodegenNamedParam.From(
					typeof(EventBean[]), "outputEvents",
					typeof(object[]), "groupByKeys",
					typeof(bool), NAME_ISNEWDATA,
					typeof(bool), NAME_ISSYNTHESIZE,
					typeof(IDictionary<object, EventBean>), "resultEvents",
					typeof(IDictionary<object, object>), "optSortKeys",
					typeof(EventBean[]), "eventsPerStream"),
				typeof(ResultSetProcessorAggregateGrouped),
				classScope,
				code);
		}

		internal static CodegenMethod GenerateOutputBatchedJoinPerKeyCodegen(
			ResultSetProcessorAggregateGroupedForge forge,
			CodegenClassScope classScope,
			CodegenInstanceAux instance)
		{
			Consumer<CodegenMethod> code = methodNode => {
				methodNode.Block.IfCondition(EqualsNull(Ref("outputEvents")))
					.BlockReturnNoValue()
					.DeclareVar<int>("count", Constant(0));

				{
					var forEach = methodNode.Block
						.ForEach(typeof(MultiKeyArrayOfKeys<EventBean>), "row", Ref("outputEvents"));
					
					forEach
						.DeclareVar<object>("groupKey", ArrayAtIndex(Ref("groupByKeys"), Ref("count")))
						.ExprDotMethod(
							MEMBER_AGGREGATIONSVC,
							"SetCurrentAccess",
							Ref("groupKey"),
							ExprDotName(MEMBER_AGENTINSTANCECONTEXT, "AgentInstanceId"),
							ConstantNull())
						.DeclareVar<EventBean[]>(
							"eventsPerStream",
                            ExprDotName(Ref("row"), "Array"));

					if (forge.OptionalHavingNode != null) {
						forEach.IfCondition(
								Not(
									LocalMethod(
										instance.Methods.GetMethod("EvaluateHavingClause"),
										ExprForgeCodegenNames.REF_EPS, 
										REF_ISNEWDATA, 
										MEMBER_AGENTINSTANCECONTEXT)))
							.BlockContinue();
					}

					forEach.ExprDotMethod(
						Ref("resultEvents"),
						"Put",
						Ref("groupKey"),
						ExprDotMethod(
							MEMBER_SELECTEXPRPROCESSOR,
							"Process",
							Ref("eventsPerStream"),
							REF_ISNEWDATA,
							REF_ISSYNTHESIZE,
							MEMBER_AGENTINSTANCECONTEXT));

					if (forge.IsSorting) {
						forEach.ExprDotMethod(
							Ref("optSortKeys"),
							"Put",
							Ref("groupKey"),
							ExprDotMethod(
								MEMBER_ORDERBYPROCESSOR, 
								"GetSortKey", 
								Ref("eventsPerStream"),
								REF_ISNEWDATA, 
								MEMBER_AGENTINSTANCECONTEXT));
					}

					forEach.IncrementRef("count");
				}
			};
			return instance.Methods.AddMethod(
				typeof(void),
				"GenerateOutputBatchedJoinPerKey",
				CodegenNamedParam.From(
					typeof(ISet<MultiKeyArrayOfKeys<EventBean>>), "outputEvents",
					typeof(object[]), "groupByKeys",
					typeof(bool), NAME_ISNEWDATA,
					typeof(bool), NAME_ISSYNTHESIZE,
					typeof(IDictionary<object, EventBean>), "resultEvents",
					typeof(IDictionary<object, object>), "optSortKeys"),
				typeof(ResultSetProcessorAggregateGrouped),
				classScope,
				code);
		}

		internal static void RemovedAggregationGroupKeyCodegen(
			CodegenClassScope classScope,
			CodegenInstanceAux instance)
		{
			Consumer<CodegenMethod> code = method => {
				if (instance.HasMember(NAME_OUTPUTALLGROUPREPS)) {
					method.Block.ExprDotMethod(Member(NAME_OUTPUTALLGROUPREPS), "Remove", Ref("key"));
				}

				if (instance.HasMember(NAME_OUTPUTALLHELPER)) {
					method.Block.ExprDotMethod(Member(NAME_OUTPUTALLHELPER), "Remove", Ref("key"));
				}

				if (instance.HasMember(NAME_OUTPUTLASTHELPER)) {
					method.Block.ExprDotMethod(Member(NAME_OUTPUTLASTHELPER), "Remove", Ref("key"));
				}

				if (instance.HasMember(NAME_OUTPUTFIRSTHELPER)) {
					method.Block.ExprDotMethod(Member(NAME_OUTPUTFIRSTHELPER), "Remove", Ref("key"));
				}
			};
			instance.Methods.AddMethod(
				typeof(void),
				"RemovedAggregationGroupKey",
				CodegenNamedParam.From(typeof(object), "key"),
				typeof(ResultSetProcessorRowPerGroupImpl),
				classScope,
				code);
		}

		public static void ProcessOutputLimitedLastAllNonBufferedViewCodegen(
			ResultSetProcessorAggregateGroupedForge forge,
			CodegenClassScope classScope,
			CodegenMethod method,
			CodegenInstanceAux instance)
		{
			ProcessOutputLimitedLastAllNonBufferedCodegen(forge, "ProcessView", classScope, method, instance);
		}

		private static void ProcessOutputLimitedLastAllNonBufferedCodegen(
			ResultSetProcessorAggregateGroupedForge forge,
			string methodName,
			CodegenClassScope classScope,
			CodegenMethod method,
			CodegenInstanceAux instance)
		{
			var factory = classScope.AddOrGetDefaultFieldSharable(ResultSetProcessorHelperFactoryField.INSTANCE);
			var groupKeyTypes = Constant(forge.GroupKeyTypes);
			var groupKeyMKSerde = forge.MultiKeyClassRef.GetExprMKSerde(method, classScope);

			if (forge.IsOutputAll) {
				CodegenExpression eventTypes = classScope.AddDefaultFieldUnshared(
					true,
					typeof(EventType[]),
					EventTypeUtility.ResolveTypeArrayCodegen(forge.EventTypes, EPStatementInitServicesConstants.REF));
				instance.AddMember(NAME_OUTPUTALLHELPER, typeof(ResultSetProcessorAggregateGroupedOutputAllHelper));
				instance.ServiceCtor.Block.AssignRef(
					NAME_OUTPUTALLHELPER,
					ExprDotMethod(
						factory,
						"MakeRSAggregateGroupedOutputAll",
						MEMBER_AGENTINSTANCECONTEXT,
						Ref("this"),
						groupKeyTypes,
						groupKeyMKSerde,
						eventTypes));
				method.Block.ExprDotMethod(
					Member(NAME_OUTPUTALLHELPER),
					methodName,
					REF_NEWDATA,
					REF_OLDDATA,
					REF_ISSYNTHESIZE);
			}
			else if (forge.IsOutputLast) {
				instance.AddMember(NAME_OUTPUTLASTHELPER, typeof(ResultSetProcessorAggregateGroupedOutputLastHelper));
				instance.ServiceCtor.Block.AssignRef(
					NAME_OUTPUTLASTHELPER,
					ExprDotMethod(
						factory,
						"MakeRSAggregateGroupedOutputLastOpt",
						MEMBER_AGENTINSTANCECONTEXT,
						Ref("this"),
						groupKeyTypes,
						groupKeyMKSerde));
				method.Block.ExprDotMethod(
					Member(NAME_OUTPUTLASTHELPER),
					methodName,
					REF_NEWDATA,
					REF_OLDDATA,
					REF_ISSYNTHESIZE);
			}
		}

		public static void ProcessOutputLimitedLastAllNonBufferedJoinCodegen(
			ResultSetProcessorAggregateGroupedForge forge,
			CodegenClassScope classScope,
			CodegenMethod method,
			CodegenInstanceAux instance)
		{
			ProcessOutputLimitedLastAllNonBufferedCodegen(forge, "ProcessJoin", classScope, method, instance);
		}

		public static void ContinueOutputLimitedLastAllNonBufferedViewCodegen(
			ResultSetProcessorAggregateGroupedForge forge,
			CodegenMethod method)
		{
			if (forge.IsOutputAll) {
				method.Block.MethodReturn(ExprDotMethod(Member(NAME_OUTPUTALLHELPER), "OutputView", REF_ISSYNTHESIZE));
			}
			else if (forge.IsOutputLast) {
				method.Block.MethodReturn(ExprDotMethod(Member(NAME_OUTPUTLASTHELPER), "OutputView", REF_ISSYNTHESIZE));
			}
			else {
				method.Block.MethodReturn(ConstantNull());
			}
		}

		public static void ContinueOutputLimitedLastAllNonBufferedJoinCodegen(
			ResultSetProcessorAggregateGroupedForge forge,
			CodegenMethod method)
		{
			if (forge.IsOutputAll) {
				method.Block.MethodReturn(ExprDotMethod(Member(NAME_OUTPUTALLHELPER), "OutputJoin", REF_ISSYNTHESIZE));
			}
			else if (forge.IsOutputLast) {
				method.Block.MethodReturn(ExprDotMethod(Member(NAME_OUTPUTLASTHELPER), "OutputJoin", REF_ISSYNTHESIZE));
			}
			else {
				method.Block.MethodReturn(ConstantNull());
			}
		}

		private static void ProcessOutputLimitedJoinLastCodegen(
			ResultSetProcessorAggregateGroupedForge forge,
			CodegenClassScope classScope,
			CodegenMethod method,
			CodegenInstanceAux instance)
		{
			var generateOutputBatchedJoinPerKey = GenerateOutputBatchedJoinPerKeyCodegen(forge, classScope, instance);

			method.Block
				.DeclareVar<IDictionary<object, EventBean>>(
					"lastPerGroupNew", 
					NewInstance(typeof(LinkedHashMap<object, EventBean>)))
				.DeclareVar<IDictionary<object, EventBean>>(
					"lastPerGroupOld",
					forge.IsSelectRStream ? NewInstance(typeof(LinkedHashMap<object, EventBean>)) : ConstantNull());

			method.Block
				.DeclareVar<IDictionary<object, object>>("newEventsSortKey", ConstantNull())
				.DeclareVar<IDictionary<object, object>>("oldEventsSortKey", ConstantNull());
			if (forge.IsSorting) {
				method.Block
					.AssignRef("newEventsSortKey", NewInstance(typeof(LinkedHashMap<object, object>)))
					.AssignRef("oldEventsSortKey", forge.IsSelectRStream ? NewInstance(typeof(LinkedHashMap<object, object>)) : ConstantNull());
			}

			{
				var forEach = method.Block.ForEach(typeof(UniformPair<ISet<MultiKeyArrayOfKeys<EventBean>>>), "pair", REF_JOINEVENTSSET);
				forEach
					.DeclareVar<ISet<MultiKeyArrayOfKeys<EventBean>>>("newData", ExprDotName(Ref("pair"), "First"))
					.DeclareVar<ISet<MultiKeyArrayOfKeys<EventBean>>>("oldData", ExprDotName(Ref("pair"), "Second"))
					.DeclareVar<object[]>("newDataMultiKey", LocalMethod(forge.GenerateGroupKeyArrayJoin, Ref("newData"), ConstantTrue()))
					.DeclareVar<object[]>("oldDataMultiKey", LocalMethod(forge.GenerateGroupKeyArrayJoin, Ref("oldData"), ConstantFalse()));

				forEach.StaticMethod(
					typeof(ResultSetProcessorGroupedUtil),
					ResultSetProcessorGroupedUtil.METHOD_APPLYAGGJOINRESULTKEYEDJOIN,
					MEMBER_AGGREGATIONSVC,
					MEMBER_AGENTINSTANCECONTEXT,
					Ref("newData"),
					Ref("newDataMultiKey"),
					Ref("oldData"),
					Ref("oldDataMultiKey"));

				// generate old events using select expressions
				if (forge.IsSelectRStream) {
					forEach.LocalMethod(
						generateOutputBatchedJoinPerKey,
						Ref("oldData"),
						Ref("oldDataMultiKey"),
						ConstantFalse(),
						REF_ISSYNTHESIZE,
						Ref("lastPerGroupOld"),
						Ref("oldEventsSortKey"));
				}

				forEach.LocalMethod(
					generateOutputBatchedJoinPerKey,
					Ref("newData"),
					Ref("newDataMultiKey"),
					ConstantTrue(),
					REF_ISSYNTHESIZE,
					Ref("lastPerGroupNew"),
					Ref("newEventsSortKey"));
			}

			method.Block.DeclareVar<EventBean[]>(
					"newEventsArr",
					StaticMethod(typeof(CollectionUtil), METHOD_TOARRAYNULLFOREMPTYVALUEEVENTS, Ref("lastPerGroupNew")))
				.DeclareVar<EventBean[]>(
					"oldEventsArr",
					forge.IsSelectRStream
						? StaticMethod(typeof(CollectionUtil), METHOD_TOARRAYNULLFOREMPTYVALUEEVENTS, Ref("lastPerGroupOld"))
						: ConstantNull());

			if (forge.IsSorting) {
				method.Block.DeclareVar<object[]>(
						"sortKeysNew",
						StaticMethod(
							typeof(CollectionUtil),
							METHOD_TOARRAYNULLFOREMPTYVALUEVALUES,
							Ref("newEventsSortKey")))
					.AssignRef(
						"newEventsArr",
						ExprDotMethod(
							MEMBER_ORDERBYPROCESSOR,
							"SortWOrderKeys",
							Ref("newEventsArr"),
							Ref("sortKeysNew"),
							MEMBER_AGENTINSTANCECONTEXT));
				if (forge.IsSelectRStream) {
					method.Block.DeclareVar<object[]>(
							"sortKeysOld",
							StaticMethod(
								typeof(CollectionUtil),
								METHOD_TOARRAYNULLFOREMPTYVALUEVALUES,
								Ref("oldEventsSortKey")))
						.AssignRef(
							"oldEventsArr",
							ExprDotMethod(
								MEMBER_ORDERBYPROCESSOR,
								"SortWOrderKeys",
								Ref("oldEventsArr"),
								Ref("sortKeysOld"),
								MEMBER_AGENTINSTANCECONTEXT));
				}
			}

			method.Block.MethodReturn(StaticMethod(typeof(ResultSetProcessorUtil), METHOD_TOPAIRNULLIFALLNULL, Ref("newEventsArr"), Ref("oldEventsArr")));
		}

		private static void ProcessOutputLimitedJoinFirstCodegen(
			ResultSetProcessorAggregateGroupedForge forge,
			CodegenClassScope classScope,
			CodegenMethod method,
			CodegenInstanceAux instance)
		{
			var generateOutputBatchedAddToList = GenerateOutputBatchedAddToListCodegen(forge, classScope, instance);

			var helperFactory = classScope.AddOrGetDefaultFieldSharable(ResultSetProcessorHelperFactoryField.INSTANCE);
			var outputFactory = classScope.AddDefaultFieldUnshared(
				true,
				typeof(OutputConditionPolledFactory),
				forge.OptionalOutputFirstConditionFactory.Make(
					classScope.NamespaceScope.InitMethod,
					classScope));
			var groupKeyTypes = Constant(forge.GroupKeyTypes);
			var groupKeyMKSerde = forge.MultiKeyClassRef.GetExprMKSerde(method, classScope);
			instance.AddMember(NAME_OUTPUTFIRSTHELPER, typeof(ResultSetProcessorGroupedOutputFirstHelper));
			instance.ServiceCtor.Block.AssignRef(
				NAME_OUTPUTFIRSTHELPER,
				ExprDotMethod(
					helperFactory,
					"MakeRSGroupedOutputFirst",
					MEMBER_AGENTINSTANCECONTEXT,
					groupKeyTypes,
					outputFactory,
					ConstantNull(),
					Constant(-1),
					groupKeyMKSerde));

			method.Block.DeclareVar<IList<EventBean>>("newEvents", NewInstance<List<EventBean>>());
			method.Block.DeclareVar<IList<object>>("newEventsSortKey", ConstantNull());
			if (forge.IsSorting) {
				method.Block.AssignRef("newEventsSortKey", NewInstance(typeof(List<object>)));
			}

			method.Block.DeclareVar<IDictionary<object, EventBean[]>>(
				"workCollection",
				NewInstance<LinkedHashMap<object, EventBean[]>>());

			if (forge.OptionalHavingNode == null) {
				{
					var forEach = method.Block
						.ForEach(typeof(UniformPair<ISet<MultiKeyArrayOfKeys<EventBean>>>), "pair", REF_JOINEVENTSSET);
					forEach
						.DeclareVar<ISet<MultiKeyArrayOfKeys<EventBean>>>(
							"newData", ExprDotName(Ref("pair"), "First"))
						.DeclareVar<ISet<MultiKeyArrayOfKeys<EventBean>>>(
							"oldData", ExprDotName(Ref("pair"), "Second"))
						.DeclareVar<object[]>(
							"newDataMultiKey",
							LocalMethod(forge.GenerateGroupKeyArrayJoin, Ref("newData"), ConstantTrue()))
						.DeclareVar<object[]>(
							"oldDataMultiKey",
							LocalMethod(forge.GenerateGroupKeyArrayJoin, Ref("oldData"), ConstantFalse()));

					{
						var ifNewData = forEach.IfCondition(NotEqualsNull(Ref("newData")))
							.DeclareVar<int>("count", Constant(0));
						{
							var forloop = ifNewData.ForEach(typeof(MultiKeyArrayOfKeys<EventBean>), "aNewData", Ref("newData"));
							forloop
								.DeclareVar<object>("mk", ArrayAtIndex(Ref("newDataMultiKey"), Ref("count")))
								.DeclareVar<EventBean[]>(
									"eventsPerStream",
									Cast(typeof(EventBean[]), ExprDotName(Ref("aNewData"), "Array")))
								.DeclareVar<OutputConditionPolled>(
									"outputStateGroup",
									ExprDotMethod(
										Member(NAME_OUTPUTFIRSTHELPER),
										"GetOrAllocate",
										Ref("mk"),
										MEMBER_AGENTINSTANCECONTEXT,
										outputFactory))
								.DeclareVar<bool>(
									"pass",
									ExprDotMethod(
										Ref("outputStateGroup"),
										"UpdateOutputCondition",
										Constant(1),
										Constant(0)));
							var ifPass = forloop.IfCondition(Ref("pass"));
							ifPass.ExprDotMethod(Ref("workCollection"), "Put", Ref("mk"), Ref("eventsPerStream"));
							forloop.ExprDotMethod(
									MEMBER_AGGREGATIONSVC,
									"ApplyEnter",
									Ref("eventsPerStream"),
									Ref("mk"),
									MEMBER_AGENTINSTANCECONTEXT)
								.IncrementRef("count");
						}
					}
					{
						var ifOldData = forEach.IfCondition(NotEqualsNull(Ref("oldData")))
							.DeclareVar<int>("count", Constant(0));
						{
							var forloop = ifOldData
								.ForEach(typeof(MultiKeyArrayOfKeys<EventBean>), "aOldData", Ref("oldData"));
							forloop
								.DeclareVar<object>(
									"mk",
									ArrayAtIndex(Ref("oldDataMultiKey"), Ref("count")))
								.DeclareVar<EventBean[]>(
									"eventsPerStream",
									ExprDotName(Ref("aOldData"), "Array"))
								.DeclareVar<OutputConditionPolled>(
									"outputStateGroup",
									ExprDotMethod(
										Member(NAME_OUTPUTFIRSTHELPER),
										"GetOrAllocate",
										Ref("mk"),
										MEMBER_AGENTINSTANCECONTEXT,
										outputFactory))
								.DeclareVar<bool>(
									"pass",
									ExprDotMethod(
										Ref("outputStateGroup"),
										"UpdateOutputCondition",
										Constant(0),
										Constant(1)));
							var ifPass = forloop.IfCondition(Ref("pass"));
							ifPass.ExprDotMethod(Ref("workCollection"), "Put", Ref("mk"), Ref("eventsPerStream"));
							forloop.ExprDotMethod(
									MEMBER_AGGREGATIONSVC,
									"ApplyLeave",
									Ref("eventsPerStream"),
									Ref("mk"),
									MEMBER_AGENTINSTANCECONTEXT)
								.IncrementRef("count");
						}
					}

					forEach.LocalMethod(
						generateOutputBatchedAddToList,
						Ref("workCollection"),
						ConstantFalse(),
						REF_ISSYNTHESIZE,
						Ref("newEvents"),
						Ref("newEventsSortKey"));
				}
			}
			else {
				// having clause present, having clause evaluates at the level of individual posts
				{
					var forEach = method.Block.ForEach(typeof(UniformPair<ISet<MultiKeyArrayOfKeys<EventBean>>>), "pair", REF_JOINEVENTSSET);
					forEach
						.DeclareVar<ISet<MultiKeyArrayOfKeys<EventBean>>>(
							"newData", ExprDotName(Ref("pair"), "First"))
						.DeclareVar<ISet<MultiKeyArrayOfKeys<EventBean>>>(
							"oldData", ExprDotName(Ref("pair"), "Second"))
						.DeclareVar<object[]>(
							"newDataMultiKey", LocalMethod(forge.GenerateGroupKeyArrayJoin, Ref("newData"), ConstantTrue()))
						.DeclareVar<object[]>(
							"oldDataMultiKey", LocalMethod(forge.GenerateGroupKeyArrayJoin, Ref("oldData"), ConstantFalse()))
						.StaticMethod(
							typeof(ResultSetProcessorGroupedUtil),
							ResultSetProcessorGroupedUtil.METHOD_APPLYAGGJOINRESULTKEYEDJOIN,
							MEMBER_AGGREGATIONSVC,
							MEMBER_AGENTINSTANCECONTEXT,
							Ref("newData"),
							Ref("newDataMultiKey"),
							Ref("oldData"),
							Ref("oldDataMultiKey"));

					{
						var ifNewData = forEach.IfCondition(NotEqualsNull(Ref("newData")))
							.DeclareVar<int>("count", Constant(0));
						{
							var forloop = ifNewData.ForEach(typeof(MultiKeyArrayOfKeys<EventBean>), "aNewData", Ref("newData"));
							forloop
								.DeclareVar<object>("mk", ArrayAtIndex(Ref("newDataMultiKey"), Ref("count")))
								.DeclareVar<EventBean[]>(
									"eventsPerStream",
									ExprDotName(Ref("aNewData"), "Array"))
								.ExprDotMethod(
									MEMBER_AGGREGATIONSVC,
									"SetCurrentAccess",
									Ref("mk"),
									ExprDotName(MEMBER_AGENTINSTANCECONTEXT, "AgentInstanceId"),
									ConstantNull())
								.IfCondition(
									Not(
										LocalMethod(
											instance.Methods.GetMethod("EvaluateHavingClause"),
											Ref("eventsPerStream"),
											ConstantTrue(),
											MEMBER_AGENTINSTANCECONTEXT)))
								.IncrementRef("count")
								.BlockContinue();

							forloop
								.DeclareVar<OutputConditionPolled>(
									"outputStateGroup",
									ExprDotMethod(
										Member(NAME_OUTPUTFIRSTHELPER),
										"GetOrAllocate",
										Ref("mk"),
										MEMBER_AGENTINSTANCECONTEXT,
										outputFactory))
								.DeclareVar<bool>(
									"pass",
									ExprDotMethod(
										Ref("outputStateGroup"),
										"UpdateOutputCondition",
										Constant(1),
										Constant(0)));
							
							forloop.IfCondition(Ref("pass"))
								.ExprDotMethod(Ref("workCollection"), "Put", Ref("mk"), Ref("eventsPerStream"));
						}
					}

					{
						var ifOldData = forEach.IfCondition(NotEqualsNull(Ref("oldData")))
							.DeclareVar<int>("count", Constant(0));
						{
							var forloop = ifOldData.ForEach(typeof(MultiKeyArrayOfKeys<EventBean>), "aOldData", Ref("oldData"));
							forloop.DeclareVar<object>("mk", ArrayAtIndex(Ref("oldDataMultiKey"), Ref("count")))
								.DeclareVar<EventBean[]>(
									"eventsPerStream",
									ExprDotName(Ref("aOldData"), "Array"))
								.ExprDotMethod(
									MEMBER_AGGREGATIONSVC,
									"SetCurrentAccess",
									Ref("mk"),
									ExprDotName(MEMBER_AGENTINSTANCECONTEXT, "AgentInstanceId"),
									ConstantNull())
								.IfCondition(
									Not(
										LocalMethod(
											instance.Methods.GetMethod("EvaluateHavingClause"),
											Ref("eventsPerStream"),
											ConstantFalse(),
											MEMBER_AGENTINSTANCECONTEXT)))
								.IncrementRef("count")
								.BlockContinue();

							forloop.DeclareVar<OutputConditionPolled>(
									"outputStateGroup",
									ExprDotMethod(
										Member(NAME_OUTPUTFIRSTHELPER),
										"GetOrAllocate",
										Ref("mk"),
										MEMBER_AGENTINSTANCECONTEXT,
										outputFactory))
								.DeclareVar<bool>(
									"pass",
									ExprDotMethod(
										Ref("outputStateGroup"),
										"UpdateOutputCondition",
										Constant(0),
										Constant(1)));
							forloop.IfCondition(Ref("pass"))
								.ExprDotMethod(
									Ref("workCollection"),
									"Put",
									Ref("mk"),
									Ref("eventsPerStream"));
						}
					}

					forEach.LocalMethod(
						generateOutputBatchedAddToList,
						Ref("workCollection"),
						ConstantFalse(),
						REF_ISSYNTHESIZE,
						Ref("newEvents"),
						Ref("newEventsSortKey"));
				}
			}

			method.Block.DeclareVar<EventBean[]>(
				"newEventsArr",
				StaticMethod(typeof(CollectionUtil), METHOD_TOARRAYNULLFOREMPTYEVENTS, Ref("newEvents")));

			if (forge.IsSorting) {
				method.Block.DeclareVar<object[]>(
						"sortKeysNew",
						StaticMethod(
							typeof(CollectionUtil),
							METHOD_TOARRAYNULLFOREMPTYOBJECTS,
							Ref("newEventsSortKey")))
					.AssignRef(
						"newEventsArr",
						ExprDotMethod(
							MEMBER_ORDERBYPROCESSOR,
							"SortWOrderKeys",
							Ref("newEventsArr"),
							Ref("sortKeysNew"),
							MEMBER_AGENTINSTANCECONTEXT));
			}

			method.Block.MethodReturn(
				StaticMethod(
					typeof(ResultSetProcessorUtil),
					METHOD_TOPAIRNULLIFALLNULL,
					Ref("newEventsArr"),
					ConstantNull()));
		}

		private static void ProcessOutputLimitedJoinAllCodegen(
			ResultSetProcessorAggregateGroupedForge forge,
			CodegenClassScope classScope,
			CodegenMethod method,
			CodegenInstanceAux instance)
		{
			var generateOutputBatchedJoinUnkeyed = GenerateOutputBatchedJoinUnkeyedCodegen(forge, classScope, instance);
			var generateOutputBatchedAddToListSingle = GenerateOutputBatchedAddToListSingleCodegen(forge, classScope, instance);

			var helperFactory = classScope.AddOrGetDefaultFieldSharable(ResultSetProcessorHelperFactoryField.INSTANCE);
			var groupKeyTypes = Constant(forge.GroupKeyTypes);
			var groupKeyMKSerde = forge.MultiKeyClassRef.GetExprMKSerde(method, classScope);
			var eventTypes = classScope.AddDefaultFieldUnshared(
				true,
				typeof(EventType[]),
				EventTypeUtility.ResolveTypeArrayCodegen(forge.EventTypes, EPStatementInitServicesConstants.REF));
			instance.AddMember(NAME_OUTPUTALLGROUPREPS, typeof(ResultSetProcessorGroupedOutputAllGroupReps));
			instance.ServiceCtor.Block.AssignRef(
				NAME_OUTPUTALLGROUPREPS,
				ExprDotMethod(
					helperFactory,
					"MakeRSGroupedOutputAllNoOpt",
					MEMBER_AGENTINSTANCECONTEXT,
					groupKeyTypes,
					groupKeyMKSerde,
					eventTypes));

			PrefixCodegenNewOldEvents(method.Block, forge.IsSorting, forge.IsSelectRStream);

			method.Block.DeclareVar<IDictionary<object, object>>(
				"workCollection",
				NewInstance(typeof(LinkedHashMap<object, object>)));

			{
				var forEach = method.Block.ForEach(typeof(UniformPair<ISet<MultiKeyArrayOfKeys<EventBean>>>), "pair", REF_JOINEVENTSSET);
				forEach
					.DeclareVar<ISet<MultiKeyArrayOfKeys<EventBean>>>("newData", ExprDotName(Ref("pair"), "First"))
					.DeclareVar<ISet<MultiKeyArrayOfKeys<EventBean>>>("oldData", ExprDotName(Ref("pair"), "Second"))
					.DeclareVar<object[]>("newDataMultiKey", LocalMethod(forge.GenerateGroupKeyArrayJoin, Ref("newData"), ConstantTrue()))
					.DeclareVar<object[]>("oldDataMultiKey", LocalMethod(forge.GenerateGroupKeyArrayJoin, Ref("oldData"), ConstantFalse()));

				if (forge.IsUnidirectional) {
					forEach.ExprDotMethod(Ref("this"), "Clear");
				}

				{
					var ifNewData = forEach.IfCondition(NotEqualsNull(Ref("newData")))
						.DeclareVar<int>("count", Constant(0));

					{
						ifNewData.ForEach(typeof(MultiKeyArrayOfKeys<EventBean>), "aNewData", Ref("newData"))
							.DeclareVar<object>(
								"mk", ArrayAtIndex(Ref("newDataMultiKey"), Ref("count")))
							.DeclareVar<EventBean[]>(
								"eventsPerStream", ExprDotName(Ref("aNewData"), "Array"))
							.ExprDotMethod(MEMBER_AGGREGATIONSVC, "ApplyEnter", Ref("eventsPerStream"), Ref("mk"), MEMBER_AGENTINSTANCECONTEXT)
							.IncrementRef("count")
							.ExprDotMethod(Ref("workCollection"), "Put", Ref("mk"), Ref("eventsPerStream"))
							.ExprDotMethod(Member(NAME_OUTPUTALLGROUPREPS), "Put", Ref("mk"), Ref("eventsPerStream"));
					}

					var ifOldData = forEach.IfCondition(NotEqualsNull(Ref("oldData")))
						.DeclareVar<int>("count", Constant(0));
					{
						ifOldData.ForEach(typeof(MultiKeyArrayOfKeys<EventBean>), "anOldData", Ref("oldData"))
							.DeclareVar<object>("mk", ArrayAtIndex(Ref("oldDataMultiKey"), Ref("count")))
							.DeclareVar<EventBean[]>("eventsPerStream", ExprDotName(Ref("anOldData"), "Array"))
							.ExprDotMethod(
								MEMBER_AGGREGATIONSVC,
								"ApplyLeave",
								Ref("eventsPerStream"),
								Ref("mk"),
								MEMBER_AGENTINSTANCECONTEXT)
							.IncrementRef("count");
					}
				}

				if (forge.IsSelectRStream) {
					forEach.LocalMethod(
						generateOutputBatchedJoinUnkeyed,
						Ref("oldData"),
						Ref("oldDataMultiKey"),
						ConstantFalse(),
						REF_ISSYNTHESIZE,
						Ref("oldEvents"),
						Ref("oldEventsSortKey"));
				}

				forEach.LocalMethod(
					generateOutputBatchedJoinUnkeyed,
					Ref("newData"),
					Ref("newDataMultiKey"),
					ConstantTrue(),
					REF_ISSYNTHESIZE,
					Ref("newEvents"),
					Ref("newEventsSortKey"));
			}

			method.Block.DeclareVar<IEnumerator<KeyValuePair<object, EventBean[]>>>(
				"entryEnumerator", 
				ExprDotMethod(Member(NAME_OUTPUTALLGROUPREPS), "EntryEnumerator"));
			{
				method.Block.WhileLoop(ExprDotMethod(Ref("entryEnumerator"), "MoveNext"))
					.DeclareVar<KeyValuePair<object, EventBean[]>>(
						"entry",
						ExprDotName(Ref("entryEnumerator"), "Current"))
					.IfCondition(
						Not(
							ExprDotMethod(
								Ref("workCollection"),
								"CheckedContainsKey",
								ExprDotName(Ref("entry"), "Key"))))
					.LocalMethod(
						generateOutputBatchedAddToListSingle,
						ExprDotName(Ref("entry"), "Key"),
						ExprDotName(Ref("entry"), "Value"),
						ConstantTrue(),
						REF_ISSYNTHESIZE,
						Ref("newEvents"),
						Ref("newEventsSortKey"));

			}

			FinalizeOutputMaySortMayRStreamCodegen(
				method.Block,
				Ref("newEvents"),
				Ref("newEventsSortKey"),
				Ref("oldEvents"),
				Ref("oldEventsSortKey"),
				forge.IsSelectRStream,
				forge.IsSorting);
		}

		private static void ProcessOutputLimitedJoinDefaultCodegen(
			ResultSetProcessorAggregateGroupedForge forge,
			CodegenClassScope classScope,
			CodegenMethod method,
			CodegenInstanceAux instance)
		{
			var generateOutputBatchedJoinUnkeyed = GenerateOutputBatchedJoinUnkeyedCodegen(forge, classScope, instance);

			ResultSetProcessorUtil.PrefixCodegenNewOldEvents(method.Block, forge.IsSorting, forge.IsSelectRStream);

			{
				var forEach = method.Block.ForEach(
					typeof(UniformPair<ISet<MultiKeyArrayOfKeys<EventBean>>>), "pair", REF_JOINEVENTSSET);
				forEach
					.DeclareVar<ISet<MultiKeyArrayOfKeys<EventBean>>>("newData", ExprDotName(Ref("pair"), "First"))
					.DeclareVar<ISet<MultiKeyArrayOfKeys<EventBean>>>("oldData", ExprDotName(Ref("pair"), "Second"))
					.DeclareVar<object[]>("newDataMultiKey", LocalMethod(forge.GenerateGroupKeyArrayJoin, Ref("newData"), ConstantTrue()))
					.DeclareVar<object[]>("oldDataMultiKey", LocalMethod(forge.GenerateGroupKeyArrayJoin, Ref("oldData"), ConstantFalse()));

				if (forge.IsUnidirectional) {
					forEach.ExprDotMethod(Ref("this"), "Clear");
				}

				forEach.StaticMethod(
					typeof(ResultSetProcessorGroupedUtil),
					ResultSetProcessorGroupedUtil.METHOD_APPLYAGGJOINRESULTKEYEDJOIN,
					MEMBER_AGGREGATIONSVC,
					MEMBER_AGENTINSTANCECONTEXT,
					Ref("newData"),
					Ref("newDataMultiKey"),
					Ref("oldData"),
					Ref("oldDataMultiKey"));

				// generate old events using select expressions
				if (forge.IsSelectRStream) {
					forEach.LocalMethod(
						generateOutputBatchedJoinUnkeyed,
						Ref("oldData"),
						Ref("oldDataMultiKey"),
						ConstantFalse(),
						REF_ISSYNTHESIZE,
						Ref("oldEvents"),
						Ref("oldEventsSortKey"));
				}

				forEach.LocalMethod(
					generateOutputBatchedJoinUnkeyed,
					Ref("newData"),
					Ref("newDataMultiKey"),
					ConstantTrue(),
					REF_ISSYNTHESIZE,
					Ref("newEvents"),
					Ref("newEventsSortKey"));
			}

			FinalizeOutputMaySortMayRStreamCodegen(
				method.Block,
				Ref("newEvents"),
				Ref("newEventsSortKey"),
				Ref("oldEvents"),
				Ref("oldEventsSortKey"),
				forge.IsSelectRStream,
				forge.IsSorting);
		}

		private static void ProcessOutputLimitedViewLastCodegen(
			ResultSetProcessorAggregateGroupedForge forge,
			CodegenClassScope classScope,
			CodegenMethod method,
			CodegenInstanceAux instance)
		{
			var generateOutputBatchedViewPerKey = GenerateOutputBatchedViewPerKeyCodegen(forge, classScope, instance);

			method.Block
				.DeclareVar<IDictionary<object, EventBean>>(
					"lastPerGroupNew",
					NewInstance(typeof(LinkedHashMap<object, EventBean>)))
				.DeclareVar<IDictionary<object, EventBean>>(
					"lastPerGroupOld",
					forge.IsSelectRStream ? NewInstance(typeof(LinkedHashMap<object, EventBean>)) : ConstantNull());

			method.Block
				.DeclareVar<IDictionary<object, object>>("newEventsSortKey", ConstantNull())
				.DeclareVar<IDictionary<object, object>>("oldEventsSortKey", ConstantNull());
			if (forge.IsSorting) {
				method.Block
					.AssignRef("newEventsSortKey", NewInstance(typeof(LinkedHashMap<object, object>)))
					.AssignRef("oldEventsSortKey", forge.IsSelectRStream ? NewInstance(typeof(LinkedHashMap<object, object>)) : ConstantNull());
			}

			method.Block.DeclareVar<EventBean[]>("eventsPerStream", NewArrayByLength(typeof(EventBean), Constant(1)));

			{
				var forEach = method.Block.ForEach(typeof(UniformPair<EventBean[]>), "pair", REF_VIEWEVENTSLIST);
				forEach
					.DeclareVar<EventBean[]>("newData", ExprDotName(Ref("pair"), "First"))
					.DeclareVar<EventBean[]>("oldData", ExprDotName(Ref("pair"), "Second"))
					.DeclareVar<object[]>("newDataMultiKey", LocalMethod(forge.GenerateGroupKeyArrayView, Ref("newData"), ConstantTrue()))
					.DeclareVar<object[]>("oldDataMultiKey", LocalMethod(forge.GenerateGroupKeyArrayView, Ref("oldData"), ConstantFalse()));

				forEach.StaticMethod(
					typeof(ResultSetProcessorGroupedUtil),
					ResultSetProcessorGroupedUtil.METHOD_APPLYAGGVIEWRESULTKEYEDVIEW,
					MEMBER_AGGREGATIONSVC,
					MEMBER_AGENTINSTANCECONTEXT,
					Ref("newData"),
					Ref("newDataMultiKey"),
					Ref("oldData"),
					Ref("oldDataMultiKey"),
					Ref("eventsPerStream"));

				// generate old events using select expressions
				if (forge.IsSelectRStream) {
					forEach.LocalMethod(
						generateOutputBatchedViewPerKey,
						Ref("oldData"),
						Ref("oldDataMultiKey"),
						ConstantFalse(),
						REF_ISSYNTHESIZE,
						Ref("lastPerGroupOld"),
						Ref("oldEventsSortKey"),
						Ref("eventsPerStream"));
				}

				forEach.LocalMethod(
					generateOutputBatchedViewPerKey,
					Ref("newData"),
					Ref("newDataMultiKey"),
					ConstantTrue(),
					REF_ISSYNTHESIZE,
					Ref("lastPerGroupNew"),
					Ref("newEventsSortKey"),
					Ref("eventsPerStream"));
			}

			method.Block.DeclareVar<EventBean[]>(
					"newEventsArr",
					StaticMethod(typeof(CollectionUtil), METHOD_TOARRAYNULLFOREMPTYVALUEEVENTS, Ref("lastPerGroupNew")))
				.DeclareVar<EventBean[]>(
					"oldEventsArr",
					forge.IsSelectRStream
						? StaticMethod(typeof(CollectionUtil), METHOD_TOARRAYNULLFOREMPTYVALUEEVENTS, Ref("lastPerGroupOld"))
						: ConstantNull());

			if (forge.IsSorting) {
				method.Block.DeclareVar<object[]>(
						"sortKeysNew",
						StaticMethod(typeof(CollectionUtil), METHOD_TOARRAYNULLFOREMPTYVALUEVALUES, Ref("newEventsSortKey")))
					.AssignRef(
						"newEventsArr",
						ExprDotMethod(MEMBER_ORDERBYPROCESSOR, "SortWOrderKeys", Ref("newEventsArr"), Ref("sortKeysNew"), MEMBER_AGENTINSTANCECONTEXT));
				if (forge.IsSelectRStream) {
					method.Block.DeclareVar<object[]>(
							"sortKeysOld",
							StaticMethod(typeof(CollectionUtil), METHOD_TOARRAYNULLFOREMPTYVALUEVALUES, Ref("oldEventsSortKey")))
						.AssignRef(
							"oldEventsArr",
							ExprDotMethod(MEMBER_ORDERBYPROCESSOR, "SortWOrderKeys", Ref("oldEventsArr"), Ref("sortKeysOld"), MEMBER_AGENTINSTANCECONTEXT));
				}
			}

			method.Block.MethodReturn(StaticMethod(typeof(ResultSetProcessorUtil), METHOD_TOPAIRNULLIFALLNULL, Ref("newEventsArr"), Ref("oldEventsArr")));
		}

		private static void ProcessOutputLimitedViewFirstCodegen(
			ResultSetProcessorAggregateGroupedForge forge,
			CodegenClassScope classScope,
			CodegenMethod method,
			CodegenInstanceAux instance)
		{
			var generateOutputBatchedAddToList = GenerateOutputBatchedAddToListCodegen(forge, classScope, instance);

			var helperFactory = classScope.AddOrGetDefaultFieldSharable(ResultSetProcessorHelperFactoryField.INSTANCE);
			var outputFactory = classScope.AddDefaultFieldUnshared(
				true,
				typeof(OutputConditionPolledFactory),
				forge.OptionalOutputFirstConditionFactory.Make(classScope.NamespaceScope.InitMethod, classScope));
			var groupKeyTypes = Constant(forge.GroupKeyTypes);
			var groupKeyMKSerde = forge.MultiKeyClassRef.GetExprMKSerde(method, classScope);
			instance.AddMember(NAME_OUTPUTFIRSTHELPER, typeof(ResultSetProcessorGroupedOutputFirstHelper));
			instance.ServiceCtor.Block.AssignRef(
				NAME_OUTPUTFIRSTHELPER,
				ExprDotMethod(
					helperFactory,
					"MakeRSGroupedOutputFirst",
					MEMBER_AGENTINSTANCECONTEXT,
					groupKeyTypes,
					outputFactory,
					ConstantNull(),
					Constant(-1),
					groupKeyMKSerde));

			method.Block.DeclareVar<IList<EventBean>>("newEvents", NewInstance(typeof(List<EventBean>)));
			method.Block.DeclareVar<IList<object>>("newEventsSortKey", ConstantNull());
			if (forge.IsSorting) {
				method.Block.AssignRef("newEventsSortKey", NewInstance(typeof(List<object>)));
			}

			method.Block
				.DeclareVar<IDictionary<object, EventBean[]>>(
					"workCollection",
					NewInstance(typeof(LinkedHashMap<object, EventBean[]>)))
				.DeclareVar<EventBean[]>(
					"eventsPerStream",
					NewArrayByLength(typeof(EventBean), Constant(1)));

			if (forge.OptionalHavingNode == null) {
				{
					var forEach = method.Block.ForEach(typeof(UniformPair<EventBean[]>), "pair", REF_VIEWEVENTSLIST);
					forEach
						.DeclareVar<EventBean[]>("newData", ExprDotName(Ref("pair"), "First"))
						.DeclareVar<EventBean[]>("oldData", ExprDotName(Ref("pair"), "Second"))
						.DeclareVar<object[]>("newDataMultiKey", LocalMethod(forge.GenerateGroupKeyArrayView, Ref("newData"), ConstantTrue()))
						.DeclareVar<object[]>("oldDataMultiKey", LocalMethod(forge.GenerateGroupKeyArrayView, Ref("oldData"), ConstantFalse()));
					{
						var ifNewData = forEach.IfCondition(NotEqualsNull(Ref("newData")));
						{
							var forloop = ifNewData.ForLoopIntSimple("i", ArrayLength(Ref("newData")));
							forloop.AssignArrayElement("eventsPerStream", Constant(0), ArrayAtIndex(Ref("newData"), Ref("i")))
								.DeclareVar<object>("mk", ArrayAtIndex(Ref("newDataMultiKey"), Ref("i")))
								.DeclareVar<OutputConditionPolled>(
									"outputStateGroup",
									ExprDotMethod(
										Member(NAME_OUTPUTFIRSTHELPER),
										"GetOrAllocate",
										Ref("mk"),
										MEMBER_AGENTINSTANCECONTEXT,
										outputFactory))
								.DeclareVar<bool>(
									"pass",
									ExprDotMethod(
										Ref("outputStateGroup"),
										"UpdateOutputCondition",
										Constant(1),
										Constant(0)));
							var ifPass = forloop.IfCondition(Ref("pass"));
							ifPass.ExprDotMethod(
								Ref("workCollection"),
								"Put",
								Ref("mk"),
								NewArrayWithInit(typeof(EventBean), ArrayAtIndex(Ref("newData"), Ref("i"))));
							forloop.ExprDotMethod(
								MEMBER_AGGREGATIONSVC,
								"ApplyEnter",
								Ref("eventsPerStream"),
								Ref("mk"),
								MEMBER_AGENTINSTANCECONTEXT);
						}
					}
					{
						var ifOldData = forEach.IfCondition(NotEqualsNull(Ref("oldData")));
						{
							var forloop = ifOldData.ForLoopIntSimple("i", ArrayLength(Ref("oldData")));
							forloop.AssignArrayElement("eventsPerStream", Constant(0), ArrayAtIndex(Ref("oldData"), Ref("i")))
								.DeclareVar<object>("mk", ArrayAtIndex(Ref("oldDataMultiKey"), Ref("i")))
								.DeclareVar<OutputConditionPolled>(
									"outputStateGroup",
									ExprDotMethod(
										Member(NAME_OUTPUTFIRSTHELPER),
										"GetOrAllocate",
										Ref("mk"),
										MEMBER_AGENTINSTANCECONTEXT,
										outputFactory))
								.DeclareVar<bool>(
									"pass",
									ExprDotMethod(
										Ref("outputStateGroup"),
                                        "UpdateOutputCondition",
										Constant(0),
										Constant(1)));
							var ifPass = forloop.IfCondition(Ref("pass"));
							ifPass.ExprDotMethod(
								Ref("workCollection"),
								"Put",
								Ref("mk"),
								NewArrayWithInit(typeof(EventBean), ArrayAtIndex(Ref("oldData"), Ref("i"))));
							forloop.ExprDotMethod(
								MEMBER_AGGREGATIONSVC,
								"ApplyLeave",
								Ref("eventsPerStream"),
								Ref("mk"),
								MEMBER_AGENTINSTANCECONTEXT);
						}
					}

					forEach.LocalMethod(
						generateOutputBatchedAddToList,
						Ref("workCollection"),
						ConstantFalse(),
						REF_ISSYNTHESIZE,
						Ref("newEvents"),
						Ref("newEventsSortKey"));
				}
			}
			else {
				// having clause present, having clause evaluates at the level of individual posts
				{
					var forEach = method.Block.ForEach(typeof(UniformPair<EventBean[]>), "pair", REF_VIEWEVENTSLIST);
					forEach
						.DeclareVar<EventBean[]>(
							"newData",
							ExprDotName(Ref("pair"), "First"))
						.DeclareVar<EventBean[]>(
							"oldData",
							ExprDotName(Ref("pair"), "Second"))
						.DeclareVar<object[]>(
							"newDataMultiKey",
							LocalMethod(forge.GenerateGroupKeyArrayView, Ref("newData"), ConstantTrue()))
						.DeclareVar<object[]>(
							"oldDataMultiKey",
							LocalMethod(forge.GenerateGroupKeyArrayView, Ref("oldData"), ConstantFalse()))
						.StaticMethod(
							typeof(ResultSetProcessorGroupedUtil),
							ResultSetProcessorGroupedUtil.METHOD_APPLYAGGVIEWRESULTKEYEDVIEW,
							MEMBER_AGGREGATIONSVC,
							MEMBER_AGENTINSTANCECONTEXT,
							Ref("newData"),
							Ref("newDataMultiKey"),
							Ref("oldData"),
							Ref("oldDataMultiKey"),
							Ref("eventsPerStream"));

					{
						var ifNewData = forEach.IfCondition(NotEqualsNull(Ref("newData")));
						{
							var forloop = ifNewData.ForLoopIntSimple("i", ArrayLength(Ref("newData")));
							forloop.DeclareVar<object>("mk", ArrayAtIndex(Ref("newDataMultiKey"), Ref("i")))
								.AssignArrayElement(
									"eventsPerStream",
									Constant(0),
									ArrayAtIndex(Ref("newData"), Ref("i")))
								.ExprDotMethod(
									MEMBER_AGGREGATIONSVC,
									"SetCurrentAccess",
									Ref("mk"),
									ExprDotName(MEMBER_AGENTINSTANCECONTEXT, "AgentInstanceId"),
									ConstantNull())
								.IfCondition(
									Not(
										LocalMethod(
											instance.Methods.GetMethod("EvaluateHavingClause"),
											Ref("eventsPerStream"),
											ConstantTrue(),
											MEMBER_AGENTINSTANCECONTEXT)))
								.BlockContinue();

							forloop.DeclareVar<OutputConditionPolled>(
									"outputStateGroup",
									ExprDotMethod(
										Member(NAME_OUTPUTFIRSTHELPER),
										"GetOrAllocate",
										Ref("mk"),
										MEMBER_AGENTINSTANCECONTEXT,
										outputFactory))
								.DeclareVar<bool>(
									"pass",
									ExprDotMethod(
										Ref("outputStateGroup"),
										"UpdateOutputCondition",
										Constant(1),
										Constant(0)));
							forloop.IfCondition(Ref("pass"))
								.ExprDotMethod(
									Ref("workCollection"),
									"Put",
									Ref("mk"),
									NewArrayWithInit(typeof(EventBean), ArrayAtIndex(Ref("newData"), Ref("i"))));
						}
					}

					{
						var ifOldData = forEach.IfCondition(NotEqualsNull(Ref("oldData")));
						{
							var forloop = ifOldData.ForLoopIntSimple("i", ArrayLength(Ref("oldData")));
							forloop.DeclareVar<object>("mk", ArrayAtIndex(Ref("oldDataMultiKey"), Ref("i")))
								.AssignArrayElement(
									"eventsPerStream",
									Constant(0),
									ArrayAtIndex(Ref("oldData"), Ref("i")))
								.ExprDotMethod(
									MEMBER_AGGREGATIONSVC,
									"SetCurrentAccess",
									Ref("mk"),
									ExprDotName(MEMBER_AGENTINSTANCECONTEXT, "AgentInstanceId"),
									ConstantNull())
								.IfCondition(
									Not(
										LocalMethod(
											instance.Methods.GetMethod("EvaluateHavingClause"),
											Ref("eventsPerStream"),
											ConstantFalse(),
											MEMBER_AGENTINSTANCECONTEXT)))
								.BlockContinue();

							forloop.DeclareVar<OutputConditionPolled>(
									"outputStateGroup",
									ExprDotMethod(
										Member(NAME_OUTPUTFIRSTHELPER),
										"GetOrAllocate",
										Ref("mk"),
										MEMBER_AGENTINSTANCECONTEXT,
										outputFactory))
								.DeclareVar<bool>(
									"pass",
									ExprDotMethod(Ref("outputStateGroup"), "UpdateOutputCondition", Constant(0), Constant(1)));
							forloop.IfCondition(Ref("pass"))
								.ExprDotMethod(
									Ref("workCollection"),
									"Put",
									Ref("mk"),
									NewArrayWithInit(typeof(EventBean), ArrayAtIndex(Ref("oldData"), Ref("i"))));
						}
					}

					forEach.LocalMethod(
						generateOutputBatchedAddToList,
						Ref("workCollection"),
						ConstantFalse(),
						REF_ISSYNTHESIZE,
						Ref("newEvents"),
						Ref("newEventsSortKey"));
				}
			}

			method.Block.DeclareVar<EventBean[]>(
				"newEventsArr",
				StaticMethod(typeof(CollectionUtil), METHOD_TOARRAYNULLFOREMPTYEVENTS, Ref("newEvents")));

			if (forge.IsSorting) {
				method.Block
					.DeclareVar<object[]>(
						"sortKeysNew",
						StaticMethod(typeof(CollectionUtil), METHOD_TOARRAYNULLFOREMPTYOBJECTS, Ref("newEventsSortKey")))
					.AssignRef(
						"newEventsArr",
						ExprDotMethod(
							MEMBER_ORDERBYPROCESSOR,
							"SortWOrderKeys",
							Ref("newEventsArr"),
							Ref("sortKeysNew"),
							MEMBER_AGENTINSTANCECONTEXT));
			}

			method.Block.MethodReturn(StaticMethod(typeof(ResultSetProcessorUtil), METHOD_TOPAIRNULLIFALLNULL, Ref("newEventsArr"), ConstantNull()));
		}

		private static void ProcessOutputLimitedViewAllCodegen(
			ResultSetProcessorAggregateGroupedForge forge,
			CodegenClassScope classScope,
			CodegenMethod method,
			CodegenInstanceAux instance)
		{
			var generateOutputBatchedViewUnkeyed = GenerateOutputBatchedViewUnkeyedCodegen(forge, classScope, instance);
			var generateOutputBatchedAddToListSingle =
				GenerateOutputBatchedAddToListSingleCodegen(forge, classScope, instance);

			var helperFactory = classScope.AddOrGetDefaultFieldSharable(ResultSetProcessorHelperFactoryField.INSTANCE);
			var groupKeyTypes = Constant(forge.GroupKeyTypes);
			var groupKeyMKSerde = forge.MultiKeyClassRef.GetExprMKSerde(method, classScope);
			CodegenExpression eventTypes = classScope.AddDefaultFieldUnshared(
				true,
				typeof(EventType[]),
				EventTypeUtility.ResolveTypeArrayCodegen(forge.EventTypes, EPStatementInitServicesConstants.REF));
			instance.AddMember(NAME_OUTPUTALLGROUPREPS, typeof(ResultSetProcessorGroupedOutputAllGroupReps));
			instance.ServiceCtor.Block.AssignRef(
				NAME_OUTPUTALLGROUPREPS,
				ExprDotMethod(
					helperFactory,
					"MakeRSGroupedOutputAllNoOpt",
					MEMBER_AGENTINSTANCECONTEXT,
					groupKeyTypes,
					groupKeyMKSerde,
					eventTypes));

			PrefixCodegenNewOldEvents(method.Block, forge.IsSorting, forge.IsSelectRStream);

			method.Block
				.DeclareVar<IDictionary<object, object>>("workCollection", NewInstance(typeof(LinkedHashMap<object, object>)))
				.DeclareVar<EventBean[]>("eventsPerStream", NewArrayByLength(typeof(EventBean), Constant(1)));

			{
				var forEach = method.Block.ForEach(typeof(UniformPair<EventBean[]>), "pair", REF_VIEWEVENTSLIST);
				forEach
					.DeclareVar<EventBean[]>(
						"newData",
						ExprDotName(Ref("pair"), "First"))
					.DeclareVar<EventBean[]>(
						"oldData",
						ExprDotName(Ref("pair"), "Second"))
					.DeclareVar<object[]>(
						"newDataMultiKey",
						LocalMethod(forge.GenerateGroupKeyArrayView, Ref("newData"), ConstantTrue()))
					.DeclareVar<object[]>(
						"oldDataMultiKey",
						LocalMethod(forge.GenerateGroupKeyArrayView, Ref("oldData"), ConstantFalse()));

				{
					var ifNewData = forEach.IfCondition(NotEqualsNull(Ref("newData")))
						.DeclareVar<int>("count", Constant(0));

					{
						ifNewData.ForEach(typeof(EventBean), "aNewData", Ref("newData"))
							.DeclareVar<object>("mk", ArrayAtIndex(Ref("newDataMultiKey"), Ref("count")))
							.AssignArrayElement(Ref("eventsPerStream"), Constant(0), Ref("aNewData"))
							.ExprDotMethod(MEMBER_AGGREGATIONSVC, "ApplyEnter", Ref("eventsPerStream"), Ref("mk"), MEMBER_AGENTINSTANCECONTEXT)
							.IncrementRef("count")
							.ExprDotMethod(Ref("workCollection"), "Put", Ref("mk"), Ref("eventsPerStream"))
							.ExprDotMethod(Member(NAME_OUTPUTALLGROUPREPS), "Put", Ref("mk"), NewArrayWithInit(typeof(EventBean), Ref("aNewData")));
					}

					var ifOldData = forEach.IfCondition(NotEqualsNull(Ref("oldData")))
						.DeclareVar<int>("count", Constant(0));
					{
						ifOldData.ForEach(typeof(EventBean), "anOldData", Ref("oldData"))
							.DeclareVar<object>("mk", ArrayAtIndex(Ref("oldDataMultiKey"), Ref("count")))
							.AssignArrayElement(Ref("eventsPerStream"), Constant(0), Ref("anOldData"))
							.ExprDotMethod(MEMBER_AGGREGATIONSVC, "ApplyLeave", Ref("eventsPerStream"), Ref("mk"), MEMBER_AGENTINSTANCECONTEXT)
							.IncrementRef("count");
					}
				}

				if (forge.IsSelectRStream) {
					forEach.LocalMethod(
						generateOutputBatchedViewUnkeyed,
						Ref("oldData"),
						Ref("oldDataMultiKey"),
						ConstantFalse(),
						REF_ISSYNTHESIZE,
						Ref("oldEvents"),
						Ref("oldEventsSortKey"),
						Ref("eventsPerStream"));
				}

				forEach.LocalMethod(
					generateOutputBatchedViewUnkeyed,
					Ref("newData"),
					Ref("newDataMultiKey"),
					ConstantTrue(),
					REF_ISSYNTHESIZE,
					Ref("newEvents"),
					Ref("newEventsSortKey"),
					Ref("eventsPerStream"));
			}

			method.Block.DeclareVar<IEnumerator<KeyValuePair<object, EventBean[]>>>(
				"entryEnumerator",
				ExprDotMethod(Member(NAME_OUTPUTALLGROUPREPS), "EntryEnumerator"));
			{
				method.Block.WhileLoop(ExprDotMethod(Ref("entryEnumerator"), "MoveNext"))
					.DeclareVar<KeyValuePair<object, EventBean[]>>(
						"entry",
						ExprDotName(Ref("entryEnumerator"), "Current"))
					.IfCondition(
						Not(ExprDotMethod(Ref("workCollection"), "ContainsKey", ExprDotName(Ref("entry"), "Key"))))
					.LocalMethod(
						generateOutputBatchedAddToListSingle,
                        ExprDotName(Ref("entry"), "Key"),
                        ExprDotName(Ref("entry"), "Value"),
						ConstantTrue(),
						REF_ISSYNTHESIZE,
						Ref("newEvents"),
						Ref("newEventsSortKey"));

			}

			FinalizeOutputMaySortMayRStreamCodegen(
				method.Block,
				Ref("newEvents"),
				Ref("newEventsSortKey"),
				Ref("oldEvents"),
				Ref("oldEventsSortKey"),
				forge.IsSelectRStream,
				forge.IsSorting);
		}

		private static void ProcessOutputLimitedViewDefaultCodegen(
			ResultSetProcessorAggregateGroupedForge forge,
			CodegenClassScope classScope,
			CodegenMethod method,
			CodegenInstanceAux instance)
		{
			var generateOutputBatchedViewUnkeyed = GenerateOutputBatchedViewUnkeyedCodegen(forge, classScope, instance);

			ResultSetProcessorUtil.PrefixCodegenNewOldEvents(method.Block, forge.IsSorting, forge.IsSelectRStream);

			method.Block.DeclareVar<EventBean[]>("eventsPerStream", NewArrayByLength(typeof(EventBean), Constant(1)));

			{
				var forEach = method.Block.ForEach(typeof(UniformPair<EventBean[]>), "pair", REF_VIEWEVENTSLIST);
				
				forEach
					.DeclareVar<EventBean[]>("newData", ExprDotName(Ref("pair"), "First"))
					.DeclareVar<EventBean[]>("oldData", ExprDotName(Ref("pair"), "Second"))
					.DeclareVar<object[]>("newDataMultiKey", LocalMethod(forge.GenerateGroupKeyArrayView, Ref("newData"), ConstantTrue()))
					.DeclareVar<object[]>("oldDataMultiKey", LocalMethod(forge.GenerateGroupKeyArrayView, Ref("oldData"), ConstantFalse()));

				forEach.StaticMethod(
					typeof(ResultSetProcessorGroupedUtil),
					ResultSetProcessorGroupedUtil.METHOD_APPLYAGGVIEWRESULTKEYEDVIEW,
					MEMBER_AGGREGATIONSVC,
					MEMBER_AGENTINSTANCECONTEXT,
					Ref("newData"),
					Ref("newDataMultiKey"),
					Ref("oldData"),
					Ref("oldDataMultiKey"),
					Ref("eventsPerStream"));

				// generate old events using select expressions
				if (forge.IsSelectRStream) {
					forEach.LocalMethod(
						generateOutputBatchedViewUnkeyed,
						Ref("oldData"),
						Ref("oldDataMultiKey"),
						ConstantFalse(),
						REF_ISSYNTHESIZE,
						Ref("oldEvents"),
						Ref("oldEventsSortKey"),
						Ref("eventsPerStream"));
				}

				forEach.LocalMethod(
					generateOutputBatchedViewUnkeyed,
					Ref("newData"),
					Ref("newDataMultiKey"),
					ConstantTrue(),
					REF_ISSYNTHESIZE,
					Ref("newEvents"),
					Ref("newEventsSortKey"),
					Ref("eventsPerStream"));
			}

			FinalizeOutputMaySortMayRStreamCodegen(
				method.Block,
				Ref("newEvents"),
				Ref("newEventsSortKey"),
				Ref("oldEvents"),
				Ref("oldEventsSortKey"),
				forge.IsSelectRStream,
				forge.IsSorting);
		}

		private static CodegenMethod GenerateOutputBatchedAddToListCodegen(
			ResultSetProcessorAggregateGroupedForge forge,
			CodegenClassScope classScope,
			CodegenInstanceAux instance)
		{
			var generateOutputBatchedAddToListSingle = GenerateOutputBatchedAddToListSingleCodegen(forge, classScope, instance);

			Consumer<CodegenMethod> code = methodNode => {
				methodNode.Block.ForEach(
						typeof(KeyValuePair<object, EventBean[]>),
						"entry",
						Ref("keysAndEvents"))
					.LocalMethod(
						generateOutputBatchedAddToListSingle,
                        ExprDotName(Ref("entry"), "Key"),
                        ExprDotName(Ref("entry"), "Value"),
						REF_ISNEWDATA,
						REF_ISSYNTHESIZE,
						Ref("resultEvents"),
						Ref("optSortKeys"));
			};

			return instance.Methods.AddMethod(
				typeof(void),
				"GenerateOutputBatchedAddToList",
				CodegenNamedParam.From(
                    typeof(IDictionary<object, EventBean[]>), "keysAndEvents",
					typeof(bool), NAME_ISNEWDATA,
					typeof(bool), NAME_ISSYNTHESIZE,
                    typeof(IList<EventBean>), "resultEvents",
                    typeof(IList<object>), "optSortKeys"),
				typeof(ResultSetProcessorAggregateGroupedImpl),
				classScope,
				code);
		}

		private static CodegenMethod GenerateOutputBatchedAddToListSingleCodegen(
			ResultSetProcessorAggregateGroupedForge forge,
			CodegenClassScope classScope,
			CodegenInstanceAux instance)
		{
			Consumer<CodegenMethod> code = methodNode => {
				{
					methodNode.Block.ExprDotMethod(
						MEMBER_AGGREGATIONSVC,
						"SetCurrentAccess",
						Ref("key"),
						ExprDotName(MEMBER_AGENTINSTANCECONTEXT, "AgentInstanceId"),
						ConstantNull());

					if (forge.OptionalHavingNode != null) {
						methodNode.Block.IfCondition(
								Not(
									LocalMethod(
                                        instance.Methods.GetMethod("EvaluateHavingClause"),
										ExprForgeCodegenNames.REF_EPS,
										REF_ISNEWDATA,
										MEMBER_AGENTINSTANCECONTEXT)))
							.BlockReturnNoValue();
					}

					methodNode.Block
						.ExprDotMethod(
							Ref("resultEvents"),
							"Add",
							ExprDotMethod(
								MEMBER_SELECTEXPRPROCESSOR,
								"Process",
								ExprForgeCodegenNames.REF_EPS,
								REF_ISNEWDATA,
								REF_ISSYNTHESIZE,
								MEMBER_AGENTINSTANCECONTEXT));

					if (forge.IsSorting) {
						methodNode.Block
							.ExprDotMethod(
								Ref("optSortKeys"),
								"Add",
								ExprDotMethod(
									MEMBER_ORDERBYPROCESSOR,
									"GetSortKey",
									ExprForgeCodegenNames.REF_EPS,
									REF_ISNEWDATA,
									MEMBER_AGENTINSTANCECONTEXT));
					}
				}
			};

			return instance.Methods.AddMethod(
				typeof(void),
				"GenerateOutputBatchedAddToListSingle",
				CodegenNamedParam.From(
					typeof(object), "key",
					typeof(EventBean[]), "eventsPerStream",
					typeof(bool), NAME_ISNEWDATA,
					typeof(bool), NAME_ISSYNTHESIZE,
					typeof(IList<EventBean>), "resultEvents",
					typeof(IList<object>), "optSortKeys"),
				typeof(ResultSetProcessorAggregateGroupedImpl),
				classScope,
				code);
		}

		public static CodegenMethod GenerateOutputBatchedViewUnkeyedCodegen(
			ResultSetProcessorAggregateGroupedForge forge,
			CodegenClassScope classScope,
			CodegenInstanceAux instance)
		{
			Consumer<CodegenMethod> code = methodNode => {
				methodNode.Block.IfCondition(EqualsNull(Ref("outputEvents")))
					.BlockReturnNoValue()
					.DeclareVar<int>("count", Constant(0));

				{
					var forEach = methodNode.Block.ForEach(typeof(EventBean), "outputEvent", Ref("outputEvents"));
					forEach.ExprDotMethod(
							MEMBER_AGGREGATIONSVC,
							"SetCurrentAccess",
							ArrayAtIndex(Ref("groupByKeys"), Ref("count")),
							ExprDotName(MEMBER_AGENTINSTANCECONTEXT, "AgentInstanceId"),
							ConstantNull())
						.AssignArrayElement(Ref("eventsPerStream"), Constant(0), ArrayAtIndex(Ref("outputEvents"), Ref("count")));

					if (forge.OptionalHavingNode != null) {
						forEach.IfCondition(
								Not(
									LocalMethod(
                                        instance.Methods.GetMethod("EvaluateHavingClause"),
										Ref("eventsPerStream"),
										REF_ISNEWDATA,
										MEMBER_AGENTINSTANCECONTEXT)))
							.IncrementRef("count")
							.BlockContinue();
					}

					forEach.ExprDotMethod(
						Ref("resultEvents"),
						"Add",
						ExprDotMethod(
							MEMBER_SELECTEXPRPROCESSOR,
							"Process",
							Ref("eventsPerStream"),
							REF_ISNEWDATA,
							REF_ISSYNTHESIZE,
							MEMBER_AGENTINSTANCECONTEXT));

					if (forge.IsSorting) {
						forEach.ExprDotMethod(
							Ref("optSortKeys"),
							"Add",
							ExprDotMethod(
								MEMBER_ORDERBYPROCESSOR,
								"GetSortKey",
								Ref("eventsPerStream"),
								REF_ISNEWDATA, 
								MEMBER_AGENTINSTANCECONTEXT));
					}

					forEach.IncrementRef("count");
				}
			};
			return instance.Methods.AddMethod(
				typeof(void),
				"GenerateOutputBatchedViewUnkeyed",
				CodegenNamedParam.From(
					typeof(EventBean[]), "outputEvents",
					typeof(object[]), "groupByKeys",
					typeof(bool), NAME_ISNEWDATA,
					typeof(bool), NAME_ISSYNTHESIZE,
					typeof(ICollection<EventBean>), "resultEvents",
					typeof(IList<object>), "optSortKeys",
					typeof(EventBean[]), "eventsPerStream"),
				typeof(ResultSetProcessorAggregateGrouped),
				classScope,
				code);
		}

		private static CodegenMethod ProcessViewResultPairDepthOneCodegen(
			ResultSetProcessorAggregateGroupedForge forge,
			CodegenClassScope classScope,
			CodegenInstanceAux instance)
		{
			var shortcutEvalGivenKey = ShortcutEvalGivenKeyCodegen(forge.OptionalHavingNode, classScope, instance);
			var generateGroupKeySingle = forge.GenerateGroupKeySingle;

			Consumer<CodegenMethod> code = methodNode => {
				methodNode.Block
					.DeclareVar<object>(
						"newGroupKey",
						LocalMethod(generateGroupKeySingle, REF_NEWDATA, ConstantTrue()))
					.DeclareVar<object>(
						"oldGroupKey",
						LocalMethod(generateGroupKeySingle, REF_OLDDATA, ConstantFalse()))
					.ExprDotMethod(
						MEMBER_AGGREGATIONSVC,
						"ApplyEnter",
						REF_NEWDATA,
						Ref("newGroupKey"),
						MEMBER_AGENTINSTANCECONTEXT)
					.ExprDotMethod(
						MEMBER_AGGREGATIONSVC,
						"ApplyLeave",
						REF_OLDDATA,
						Ref("oldGroupKey"),
						MEMBER_AGENTINSTANCECONTEXT)
					.DeclareVar<EventBean>(
						"istream",
						LocalMethod(shortcutEvalGivenKey, REF_NEWDATA, Ref("newGroupKey"), ConstantTrue(), REF_ISSYNTHESIZE));
				if (!forge.IsSelectRStream) {
					methodNode.Block.MethodReturn(StaticMethod(typeof(ResultSetProcessorUtil), "ToPairNullIfNullIStream", Ref("istream")));
				}
				else {
					methodNode.Block.DeclareVar<EventBean>(
							"rstream",
							LocalMethod(
								shortcutEvalGivenKey,
								REF_OLDDATA,
								Ref("oldGroupKey"),
								ConstantFalse(),
								REF_ISSYNTHESIZE))
						.MethodReturn(
							StaticMethod(
								typeof(ResultSetProcessorUtil),
								"ToPairNullIfAllNullSingle",
								Ref("istream"),
								Ref("rstream")));
				}
			};

			return instance.Methods.AddMethod(
				typeof(UniformPair<EventBean[]>),
				"ProcessViewResultPairDepthOne",
				CodegenNamedParam.From(
					typeof(EventBean[]),
					NAME_NEWDATA,
					typeof(EventBean[]),
					NAME_OLDDATA,
					typeof(bool),
					NAME_ISSYNTHESIZE),
				typeof(ResultSetProcessorRowPerGroupImpl),
				classScope,
				code);
		}

		private static CodegenMethod ProcessViewResultNewDepthOneCodegen(
			ResultSetProcessorAggregateGroupedForge forge,
			CodegenClassScope classScope,
			CodegenInstanceAux instance)
		{
			var shortcutEvalGivenKey = ShortcutEvalGivenKeyCodegen(forge.OptionalHavingNode, classScope, instance);

			Consumer<CodegenMethod> code = methodNode => {
				methodNode.Block
					.DeclareVar<object>(
						"groupKey",
						LocalMethod(forge.GenerateGroupKeySingle, REF_NEWDATA, ConstantTrue()))
					.ExprDotMethod(
						MEMBER_AGGREGATIONSVC,
						"ApplyEnter",
						REF_NEWDATA,
						Ref("groupKey"),
						MEMBER_AGENTINSTANCECONTEXT)
					.DeclareVar<EventBean>(
						"istream",
						LocalMethod(
							shortcutEvalGivenKey,
							REF_NEWDATA,
							Ref("groupKey"),
							ConstantTrue(),
							REF_ISSYNTHESIZE))
					.MethodReturn(
                        StaticMethod(typeof(ResultSetProcessorUtil), "ToPairNullIfNullIStream", Ref("istream")));
			};

			return instance.Methods.AddMethod(
                typeof(UniformPair<EventBean[]>),
                "ProcessViewResultNewDepthOneCodegen",
				CodegenNamedParam.From(
					typeof(EventBean[]), NAME_NEWDATA,
					typeof(bool), NAME_ISSYNTHESIZE),
				typeof(ResultSetProcessorRowPerGroupImpl),
				classScope,
				code);
		}

		private static CodegenMethod ShortcutEvalGivenKeyCodegen(
			ExprForge optionalHavingNode,
			CodegenClassScope classScope,
			CodegenInstanceAux instance)
		{
			Consumer<CodegenMethod> code = methodNode => {
				methodNode.Block.ExprDotMethod(
					MEMBER_AGGREGATIONSVC,
					"SetCurrentAccess",
					Ref("groupKey"),
					ExprDotName(MEMBER_AGENTINSTANCECONTEXT, "AgentInstanceId"),
					ConstantNull());
				if (optionalHavingNode != null) {
					methodNode.Block.IfCondition(
							Not(
								LocalMethod(
                                    instance.Methods.GetMethod("EvaluateHavingClause"),
									ExprForgeCodegenNames.REF_EPS,
									REF_ISNEWDATA,
									MEMBER_AGENTINSTANCECONTEXT)))
						.BlockReturn(ConstantNull());
				}

				methodNode.Block.MethodReturn(
					ExprDotMethod(
						MEMBER_SELECTEXPRPROCESSOR,
						"Process",
						ExprForgeCodegenNames.REF_EPS,
						REF_ISNEWDATA,
						REF_ISSYNTHESIZE,
						MEMBER_AGENTINSTANCECONTEXT));
			};

			return instance.Methods.AddMethod(
				typeof(EventBean),
                "ShortcutEvalGivenKey",
				CodegenNamedParam.From(
					typeof(EventBean[]), ExprForgeCodegenNames.NAME_EPS,
					typeof(object), "groupKey",
					typeof(bool), NAME_ISNEWDATA,
					typeof(bool), NAME_ISSYNTHESIZE),
				typeof(ResultSetProcessorRowPerGroupImpl),
				classScope,
				code);
		}
	}
} // end of namespace
